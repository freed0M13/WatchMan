using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using SharpPcap;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Watchman.Web // Web klasörüne göre güncellenmiş namespace
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private const string DbPassword = "Watchman!9Qd7npDk";
        private readonly string _dbPath = Path.Combine(AppContext.BaseDirectory, "monitor.db");
        private string ConnectionString => new SqliteConnectionStringBuilder { DataSource = _dbPath, Password = DbPassword }.ToString();

        // 1. SİSTEMDEKİ TÜM AĞ KARTLARINI LİSTELE (Dropdown için)
        [HttpGet("interfaces")]
        public IActionResult GetNetworkInterfaces()
        {
            try
            {
                var devices = CaptureDeviceList.Instance;
                var interfaceList = new List<object>();

                for (int i = 0; i < devices.Count; i++)
                {
                    var device = devices[i];
                    interfaceList.Add(new
                    {
                        Id = i,
                        Name = device.Name,
                        Description = device.Description,
                        MacAddress = device.MacAddress?.ToString() ?? "Bilinmiyor"
                    });
                }

                return Ok(interfaceList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Ağ kartları okunamadı.", Details = ex.Message });
            }
        }

        // 2. MEVCUT AYARLARI GETİR
        [HttpGet]
        public IActionResult GetSettings()
        {
            var settings = new Dictionary<string, string>();

            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT SettingKey, SettingValue FROM APP_SETTINGS";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    settings.Add(reader.GetString(0), reader.GetString(1));
                }

                // Eğer DB'de ayarlar yoksa varsayılan değerleri frontend'e gönder
                if (!settings.ContainsKey("ActiveNetworkInterface")) settings.Add("ActiveNetworkInterface", "0");
                if (!settings.ContainsKey("DbBackupInterval")) settings.Add("DbBackupInterval", "Daily");
                if (!settings.ContainsKey("Theme")) settings.Add("Theme", "dark");

                return Ok(settings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Ayarlar okunamadı.", Details = ex.Message });
            }
        }

        // 3. AYARLARI KAYDET (Form Submit)
        [HttpPost]
        public IActionResult SaveSettings([FromBody] Dictionary<string, string> newSettings)
        {
            if (newSettings == null || !newSettings.Any()) return BadRequest("Ayar verisi boş olamaz.");

            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                foreach (var setting in newSettings)
                {
                    using var command = connection.CreateCommand();

                    // UPSERT mantığı: Key varsa günceller, yoksa yeni satır ekler
                    command.CommandText = @"
                        INSERT INTO APP_SETTINGS (SettingKey, SettingValue) 
                        VALUES (@key, @value) 
                        ON CONFLICT(SettingKey) DO UPDATE SET SettingValue = @value;";

                    command.Parameters.AddWithValue("@key", setting.Key);
                    command.Parameters.AddWithValue("@value", setting.Value);
                    command.ExecuteNonQuery();
                }

                // TETİKLEYİCİ: Ağ kartı değiştiyse Worker'a haber ver
                if (newSettings.ContainsKey("ActiveNetworkInterface"))
                {
                    // Watchman.Workers.TrafficAnalyzerWorker sınıfında bu değişkenin 'public static volatile bool' olduğuna emin ol.
                    Watchman.Workers.TrafficAnalyzerWorker.NetworkInterfaceChanged = true;
                }

                return Ok(new { Message = "Ayarlar başarıyla veritabanına kaydedildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Ayarlar kaydedilirken bir hata oluştu.", Details = ex.Message });
            }
        }
    }
}