using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Watchman.Data;
using Watchman.Models;
using Watchman.Services;

namespace Watchman.Workers
{
    public class TrafficAnalyzerWorker : BackgroundService
    {
        private readonly SecurityStateStore _stateStore;
        private readonly ILogger<TrafficAnalyzerWorker> _logger;

        // Veritabanına yazılmayı bekleyen tehditlerin RAM'deki güvenli kuyruğu
        private readonly ConcurrentQueue<ThreatEvent> _dbWriteQueue = new();

        // Saldırı tespiti için geçici SYN paket sayaçları
        private readonly ConcurrentDictionary<string, int> _synPacketCounts = new();
        private DateTime _lastClearTime = DateTime.UtcNow;

        private const string DbPassword = "Watchman!9Qd7npDk";

        // Diğer servislerin kart değişikliğini bildirebileceği statik bayrak
        public static bool NetworkInterfaceChanged { get; set; } = false;

        private long _currentSecondBytes = 0;

        public TrafficAnalyzerWorker(SecurityStateStore stateStore, ILogger<TrafficAnalyzerWorker> logger)
        {
            _stateStore = stateStore;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[NIDS] Traffic Analyzer başlatılıyor...");
            _ = Task.Run(() => ProcessDatabaseWritesAsync(stoppingToken), stoppingToken);
            _ = Task.Run(() => MonitorNetworkHealthAsync(stoppingToken), stoppingToken);

            try
            {
                var devices = CaptureDeviceList.Instance;
                if (devices.Count < 1)
                {
                    _logger.LogError("[NIDS] HATA: Hiçbir ağ kartı bulunamadı. Npcap kurulu mu?");
                    return;
                }

                // İlk açılışta kartı seç ve başlat
                int activeInterfaceId = await GetActiveInterfaceIdAsync(stoppingToken);
                if (activeInterfaceId < 0 || activeInterfaceId >= devices.Count) activeInterfaceId = 0;

                var currentDevice = devices[activeInterfaceId];
                currentDevice.OnPacketArrival += Device_OnPacketArrival;
                currentDevice.Open(DeviceModes.Promiscuous, 1000);

                _logger.LogInformation($"[NIDS] >>> DİNLENİYOR: [{activeInterfaceId}] {currentDevice.Description} <<<");
                currentDevice.StartCapture();

                // Sonsuz Döngü
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);

                    // HOT-SWAP KONTROLÜ: Arayüzden kart değiştirildi mi?
                    if (NetworkInterfaceChanged)
                    {
                        NetworkInterfaceChanged = false;
                        _logger.LogWarning("[NIDS] Ağ kartı değişikliği algılandı. Dinleme aktarılıyor...");

                        // Eski kartı güvenlice kapat ve olay dinleyicisini sil (Memory Leak olmaması için kritik)
                        currentDevice.StopCapture();
                        currentDevice.Close();
                        currentDevice.OnPacketArrival -= Device_OnPacketArrival;

                        // Yeni kartı DB'den oku ve başlat
                        activeInterfaceId = await GetActiveInterfaceIdAsync(stoppingToken);
                        if (activeInterfaceId >= 0 && activeInterfaceId < devices.Count)
                        {
                            currentDevice = devices[activeInterfaceId];
                            currentDevice.OnPacketArrival += Device_OnPacketArrival;
                            currentDevice.Open(DeviceModes.Promiscuous, 1000);
                            currentDevice.StartCapture();
                            _logger.LogInformation($"[NIDS] >>> YENİ KART BAŞARIYLA AKTİF EDİLDİ: [{activeInterfaceId}] {currentDevice.Description} <<<");
                        }
                    }

                    // Saniyede bir kez SYN DoS sayaçlarını, genel hız metriklerini ve BANT GENİŞLİĞİNİ güncelle/sıfırla
                    if ((DateTime.UtcNow - _lastClearTime).TotalSeconds >= 1)
                    {
                        // 1. Saniyelik byte birikimini atomik okuyup Mbps cinsine çeviriyoruz (Byte * 8 bit / 1M)
                        long bytesCaptured = Interlocked.Read(ref _currentSecondBytes);
                        double mbps = (bytesCaptured * 8.0) / 1_000_000.0;

                        // 2. Hesaplanan canlı değeri StateStore üzerine yuvarlayarak yazıyoruz
                        _stateStore.Metrics.ThroughputBandwidth = Math.Round(mbps, 2);

                        // 3. Bir sonraki saniyenin temiz ölçümü için tüm sayaçları ve zaman damgasını sıfırlıyoruz
                        Interlocked.Exchange(ref _currentSecondBytes, 0);
                        _synPacketCounts.Clear();
                        _stateStore.Metrics.GlobalPacketRate = 0;
                        _stateStore.Metrics.ProtocolDistribution.Clear();
                        _lastClearTime = DateTime.UtcNow;
                    }
                }

                currentDevice.StopCapture();
                currentDevice.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError($"[NIDS] Ağ dinleme hatası: {ex.Message}");
            }
        }

        // Veritabanı okuma işlemini temizlik açısından ufak bir metoda ayırdık
        private async Task<int> GetActiveInterfaceIdAsync(CancellationToken token)
        {
            try
            {
                string dbPath = Path.Combine(AppContext.BaseDirectory, "monitor.db");
                var connStr = new SqliteConnectionStringBuilder { DataSource = dbPath, Password = DbPassword }.ToString();
                using var connection = new SqliteConnection(connStr);
                await connection.OpenAsync(token);

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT SettingValue FROM APP_SETTINGS WHERE SettingKey = 'ActiveNetworkInterface'";
                var result = await command.ExecuteScalarAsync(token);

                if (result != null && int.TryParse(result.ToString(), out int parsedId)) return parsedId;
            }
            catch { }
            return 0; // Hata durumunda varsayılan ID
        }

        // AĞDAN GEÇEN HER PAKETTE TETİKLENEN METOT (Milisaniyede yüzlerce kez çalışabilir)
        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                var rawPacket = e.GetPacket();
                var parsedPacket = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

                // UI (Dashboard) için genel hız sayacını artır
                _stateStore.Metrics.GlobalPacketRate++;

                // KRİTİK GÜNCELLEME: Saniyede akan toplam byte miktarını thread-safe (güvenli) şekilde sayıyoruz
                Interlocked.Add(ref _currentSecondBytes, rawPacket.Data.Length);

                var ipPacket = parsedPacket.Extract<IPPacket>();
                if (ipPacket != null)
                {
                    string sourceIp = ipPacket.SourceAddress.ToString();

                    // Top Talkers (En çok veri harcayanlar) tablosunu güncelle
                    _stateStore.Metrics.TopTalkers.AddOrUpdate(sourceIp, rawPacket.Data.Length, (key, old) => old + rawPacket.Data.Length);

                    // Protokol sayıcı
                    if (parsedPacket.Extract<TcpPacket>() != null)
                        _stateStore.Metrics.ProtocolDistribution.AddOrUpdate("TCP", 1, (k, v) => v + 1);
                    else if (parsedPacket.Extract<UdpPacket>() != null)
                        _stateStore.Metrics.ProtocolDistribution.AddOrUpdate("UDP", 1, (k, v) => v + 1);
                    else if (parsedPacket.Extract<IcmpV4Packet>() != null || parsedPacket.Extract<IcmpV6Packet>() != null)
                        _stateStore.Metrics.ProtocolDistribution.AddOrUpdate("ICMP", 1, (k, v) => v + 1);

                    var tcpPacket = parsedPacket.Extract<TcpPacket>();
                    if (tcpPacket != null)
                    {
                        // --- DDoS / SYN FLOOD KORUMASI ---
                        if (tcpPacket.Synchronize && !tcpPacket.Acknowledgment)
                        {
                            int count = _synPacketCounts.AddOrUpdate(sourceIp, 1, (key, old) => old + 1);

                            // Saniyede aynı IP'den 100 SYN paketi gelirse saldırı say
                            if (count == 100)
                            {
                                RegisterThreat("DoS / SYN Flood Attempt", sourceIp, "CRITICAL", "DDoS Protection", tcpPacket.DestinationPort);
                            }
                        }

                        // --- DPI (DERİN PAKET İNCELEMESİ) / PROXY TESPİTİ ---
                        if ((tcpPacket.DestinationPort == 80 || tcpPacket.DestinationPort == 8080) && tcpPacket.HasPayloadData)
                        {
                            string payload = System.Text.Encoding.ASCII.GetString(tcpPacket.PayloadData);

                            // HTTP paketinin içinde Proxy veya VPN izi (X-Forwarded-For) arıyoruz
                            if (payload.Contains("X-Forwarded-For:") || payload.Contains("Via:"))
                            {
                                RegisterThreat("Proxy/VPN Connection Deteced", sourceIp, "MEDIUM", "Deep Packet Inspection", tcpPacket.DestinationPort);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Yüksek hızda bozuk paketler (malformed packets) çökmeye sebep olmasın diye yutulur
            }
        }

        // Gerçek Ağ Gecikmesi ve Paket Kaybı Ölçüm Motoru
        private async Task MonitorNetworkHealthAsync(CancellationToken token)
        {
            using var ping = new System.Net.NetworkInformation.Ping();
            string targetHost = "8.8.8.8"; // Kesintisiz ölçüm için Google DNS
            int pingCount = 0;
            int failedPings = 0;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var reply = await ping.SendPingAsync(targetHost, 1000);
                    pingCount++;

                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        _stateStore.CurrentLatencyMs = (int)reply.RoundtripTime;
                    }
                    else
                    {
                        failedPings++;
                    }

                    // Her 10 saniyede bir paket kayıp oranını (yüzde) hesapla ve sıfırla
                    if (pingCount >= 10)
                    {
                        _stateStore.PacketLossPercent = Math.Round((double)failedPings / pingCount * 100, 1);
                        pingCount = 0;
                        failedPings = 0;
                    }
                }
                catch
                {
                    failedPings++;
                }

                await Task.Delay(1000, token); // Saniyede 1 kez ping at
            }
        }

        // Tehdidi hem Web UI için RAM'e hem de veritabanı yazıcısı için kuyruğa atar
        private void RegisterThreat(string type, string sourceIp, string severity, string module, int targetPort)
        {
            // 1. Dashboard (Svelte) için RAM'deki StateStore'a gönder
            _stateStore.AddThreat(type, sourceIp, severity, module);

            // 2. Kalıcı saklama için (SQLite) yazım kuyruğuna gönder
            var threat = new ThreatEvent
            {
                Type = type,
                SourceIp = sourceIp,
                Severity = severity,
                Module = module,
                Time = DateTime.UtcNow
            };
            _dbWriteQueue.Enqueue(threat);
        }

        // VERİTABANI YAZICI İŞ PARÇACIĞI (Kuyruğu eritir)
        private async Task ProcessDatabaseWritesAsync(CancellationToken token)
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "monitor.db");
            var connStr = new SqliteConnectionStringBuilder { DataSource = dbPath, Password = DbPassword }.ToString();

            while (!token.IsCancellationRequested)
            {
                // Eğer veritabanı rotasyon (silme/yedekleme) modundaysa VEYA kuyruk boşsa bekle
                if (DatabaseArchiver.IsDatabaseRotating || _dbWriteQueue.IsEmpty)
                {
                    await Task.Delay(200, token);
                    continue;
                }

                // Kuyrukta tehdit varsa al ve kalıcı olarak şifreli veritabanına yaz
                if (_dbWriteQueue.TryDequeue(out var threat))
                {
                    try
                    {
                        using var connection = new SqliteConnection(connStr);
                        await connection.OpenAsync(token);

                        using var command = connection.CreateCommand();
                        command.CommandText = @"
                            INSERT INTO ALERTS (AlertType, Severity, SourceIp, Module, TargetPort, CreatedAt) 
                            VALUES (@type, @severity, @sourceIp, @module, @port, @time)";

                        command.Parameters.AddWithValue("@type", threat.Type);
                        command.Parameters.AddWithValue("@severity", threat.Severity);
                        command.Parameters.AddWithValue("@sourceIp", threat.SourceIp);
                        command.Parameters.AddWithValue("@module", threat.Module);
                        command.Parameters.AddWithValue("@port", 80); // Basitlik için, dinamik yapılabilir
                        command.Parameters.AddWithValue("@time", threat.Time.ToString("yyyy-MM-dd HH:mm:ss"));

                        await command.ExecuteNonQueryAsync(token);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[NIDS] Veritabanı yazma hatası: {ex.Message}");
                        // Başarısız olursa veriyi kaybetmemek için kuyruğa geri atabiliriz
                        _dbWriteQueue.Enqueue(threat);
                        await Task.Delay(1000, token); // DB kilitliyse biraz bekle
                    }
                }
            }
        }
    }
}