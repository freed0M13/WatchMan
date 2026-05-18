using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Watchman.Web
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private const string DbPassword = "Watchman!9Qd7npDk";
        private readonly string _dbPath = Path.Combine(AppContext.BaseDirectory, "monitor.db");
        private string ConnectionString => new SqliteConnectionStringBuilder { DataSource = _dbPath, Password = DbPassword }.ToString();

        // 1. TÜM VERİTABANI LOGLARINI BİRLEŞTİREREK GETİR
        // 1. TÜM VERİTABANI LOGLARINI BİRLEŞTİREREK GETİR (Çökmelere Karşı Güvenli Hale Getirildi)
        // 1. TÜM VERİTABANI LOGLARINI BİRLEŞTİREREK GETİR (Gelişmiş Alanlar Eklendi)
        [HttpGet]
        public IActionResult GetUnifiedLogs()
        {
            var unifiedLogs = new List<object>();

            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                using var command = connection.CreateCommand();
                // Tüm tabloları uyumlu hale getirmek için yeni sütunları UNION ALL'a ekliyoruz
                command.CommandText = @"
                    SELECT 
                        'EVT-A' || Id as LogId, CreatedAt as Timestamp, Severity, 'Alert' as Source, Description as Message,
                        SourceIp, TargetPort, AlertType, Module 
                    FROM ALERTS
                    UNION ALL
                    SELECT 
                        'EVT-S' || Id as LogId, CreatedAt as Timestamp, 'info' as Severity, 'System' as Source, Message,
                        '127.0.0.1' as SourceIp, NULL as TargetPort, 'System Event' as AlertType, Provider as Module 
                    FROM SYSTEM_LOGS
                    UNION ALL
                    SELECT 
                        'EVT-T' || Id as LogId, Timestamp, 'info' as Severity, 'Traffic' as Source, 
                        'Packets: ' || TotalPackets || ' | Bytes: ' || TotalBytes || ' | Dropped: ' || DroppedPackets as Message,
                        '0.0.0.0' as SourceIp, NULL as TargetPort, 'Traffic Stats' as AlertType, 'NetworkWorker' as Module 
                    FROM TRAFFIC_STATS
                    ORDER BY Timestamp DESC 
                    LIMIT 200;";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string rawId = reader.GetValue(0)?.ToString() ?? "EVT-UNKNOWN";
                    string rawTimestamp = reader.GetValue(1)?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string rawSeverity = reader.GetValue(2)?.ToString()?.ToLower() ?? "info";
                    string rawSource = reader.GetValue(3)?.ToString() ?? "System";
                    string rawMessage = reader.GetValue(4)?.ToString() ?? string.Empty;

                    // Yeni Eklenen Alanların Güvenli Okunması
                    string sourceIp = reader.GetValue(5)?.ToString() ?? string.Empty;
                    string targetPort = reader.IsDBNull(6) ? string.Empty : reader.GetValue(6)?.ToString();
                    string alertType = reader.GetValue(7)?.ToString() ?? "Generic";
                    string module = reader.GetValue(8)?.ToString() ?? "Kernel";

                    // İstediğin gibi IP ve Port'u birleştiriyoruz (Örn: 192.168.1.10:80)
                    string formattedHostIp = sourceIp;
                    if (!string.IsNullOrEmpty(targetPort))
                    {
                        formattedHostIp = $"{sourceIp}:{targetPort}";
                    }

                    unifiedLogs.Add(new
                    {
                        Id = rawId,
                        Timestamp = rawTimestamp,
                        Severity = rawSeverity,
                        Source = rawSource,
                        Message = rawMessage,
                        Description = rawMessage,
                        HostIp = formattedHostIp, // Birleşik IP:PORT verisi
                        AlertType = alertType,
                        Module = module
                    });
                }

                return Ok(unifiedLogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Log akışı okunamadı.", Details = ex.Message });
            }
        }

        // 2. SEÇİLEN LOGLARI VERİTABANINDAN SİL
        [HttpPost("delete-bulk")]
        public IActionResult DeleteLogs([FromBody] List<string> logIds)
        {
            if (logIds == null || !logIds.Any()) return BadRequest("Silinecek log seçilmedi.");

            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                foreach (var logId in logIds)
                {
                    string actualId = new string(logId.Where(char.IsDigit).ToArray());
                    string tableType = logId.Substring(0, 5); // EVT-A, EVT-S, EVT-T

                    string targetTable = tableType switch
                    {
                        "EVT-A" => "ALERTS",
                        "EVT-S" => "SYSTEM_LOGS",
                        "EVT-T" => "TRAFFIC_STATS",
                        _ => null
                    };

                    if (targetTable != null)
                    {
                        using var command = connection.CreateCommand();
                        command.CommandText = $"DELETE FROM {targetTable} WHERE Id = @id";
                        command.Parameters.AddWithValue("@id", actualId);
                        command.ExecuteNonQuery();
                    }
                }

                return Ok(new { Message = "Seçilen tüm loglar veritabanından kalıcı olarak silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Loglar silinirken hata oluştu.", Details = ex.Message });
            }
        }

        // 3. LOGLARI APP_SETTINGS'TEKİ URL'E GÖNDER (REMOTE LOG UPLOAD)
        [HttpPost("send-remote")]
        public async Task<IActionResult> SendToRemoteServer([FromBody] List<string> logIds)
        {
            if (logIds == null || !logIds.Any()) return BadRequest("Gönderilecek log seçilmedi.");

            try
            {
                string remoteUrl = ""; // Fallback varsayılan

                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = "SELECT SettingValue FROM APP_SETTINGS WHERE SettingKey = 'RemoteLogServerUrl'";
                    var result = cmd.ExecuteScalar();
                    if (result != null) remoteUrl = result.ToString();
                }

                // Gönderilecek log detaylarını topla
                var allLogs = (GetUnifiedLogs() as OkObjectResult)?.Value as List<object>;
                var selectedLogDetails = allLogs?.Where(l => logIds.Contains((string)l.GetType().GetProperty("Id")?.GetValue(l))).ToList();

                // HttpClient ile uzak sunucuya POST atıyoruz
                using var client = new HttpClient();
                var response = await client.PostAsJsonAsync(remoteUrl, selectedLogDetails);

                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { Message = $"Seçilen {logIds.Count} log, başarıyla {remoteUrl} adresine transfer edildi." });
                }

                return StatusCode((int)response.StatusCode, new { Error = "Uzak sunucu log transferini reddetti." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Uzak sunucuya bağlanılamadı.", Details = ex.Message });
            }
        }
    }
}