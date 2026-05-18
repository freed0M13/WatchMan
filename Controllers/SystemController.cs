using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System;
using System.Linq; // Yeni eklenen sorgular (OrderByDescending, Sum vb.) için gerekli
using System.Management; // WMI üzerinden sıcaklık okumak için gerekli
using System.Collections.Generic; // List<object> için gerekli

namespace Watchman.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        // Sayaçları static tanımlıyoruz ki her istekte yeniden RAM tüketmesin
        private static readonly PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private static readonly PerformanceCounter _ramAvailableCounter = new PerformanceCounter("Memory", "Available MBytes");

        [HttpGet("health")]
        public IActionResult GetSystemHealth()
        {
            try
            {
                // 1. CPU Kullanımı
                // Not: PerformanceCounter ilk çağrıldığında 0 dönebilir, sonraki saniyelerde gerçek değeri okur.
                float cpuUsage = _cpuCounter.NextValue();

                // 2. RAM Kullanımı (Boşta kalan RAM)
                float availableRamMB = _ramAvailableCounter.NextValue();

                // 3. Disk Kullanımı (C: Sürücüsü örneği)
                DriveInfo drive = new DriveInfo("C");
                long totalDiskGB = drive.TotalSize / 1024 / 1024 / 1024;
                long freeDiskGB = drive.AvailableFreeSpace / 1024 / 1024 / 1024;
                double diskUsagePercent = 100 - ((double)freeDiskGB / totalDiskGB * 100);

                // 4. Uptime (Sistemin ne zamandır açık olduğu)
                TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);

                // Svelte arayüzüne gönderilecek JSON paketi
                var healthMetrics = new
                {
                    Status = "Online",
                    Uptime = uptime.ToString(@"dd\.hh\:mm\:ss"),
                    CpuUsagePercent = Math.Round(cpuUsage, 1),
                    RamAvailableMB = Math.Round(availableRamMB, 0),
                    DiskUsagePercent = Math.Round(diskUsagePercent, 1),
                    TotalDiskGB = totalDiskGB
                };

                return Ok(healthMetrics);
            }
            catch (Exception ex)
            {
                // Windows servis yetkileri veya başka bir hatada uygulamanın çökmesini önlemek için
                return StatusCode(500, new { Error = "Sistem metrikleri okunamadı.", Details = ex.Message });
            }
        }

        [HttpGet("processes")]
        public IActionResult GetTopProcesses()
        {
            try
            {
                // Sistemdeki tüm çalışan işlemleri al
                var processes = Process.GetProcesses();

                // RAM kullanımına (WorkingSet64) göre büyükten küçüğe sıralayıp ilk 5'i al
                var topProcesses = processes
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(5)
                    .Select(p => new
                    {
                        PID = p.Id,
                        Command = p.ProcessName,
                        // Bayt cinsinden olan RAM'i Megabayt'a (MB) çevir
                        MemoryMB = Math.Round(p.WorkingSet64 / 1024.0 / 1024.0, 1)
                    })
                    .ToList();

                return Ok(topProcesses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "İşlem listesi okunamadı.", Details = ex.Message });
            }
        }

        // --- YENİ EKLENEN API: SystemAnalysis.svelte İçin Kapsamlı Donanım Verisi ---
        [HttpGet("full")]
        public IActionResult GetFullSystemDetails()
        {
            try
            {
                // 1. Uptime
                var uptimeSpan = TimeSpan.FromMilliseconds(Environment.TickCount64);
                string uptimeStr = $"{(int)uptimeSpan.TotalDays}d {uptimeSpan.Hours}h {uptimeSpan.Minutes}m";

                // 2. Storage (Disk Kullanımı - C:)
                DriveInfo drive = new DriveInfo("C");
                double totalStorageGB = Math.Round(drive.TotalSize / 1024.0 / 1024.0 / 1024.0, 1);
                double freeStorageGB = Math.Round(drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0, 1);
                double usedStorageGB = totalStorageGB - freeStorageGB;
                double diskUsagePercent = Math.Round((usedStorageGB / totalStorageGB) * 100, 1);

                // 3. System Stats (Thread ve Process Sayıları)
                var allProcesses = Process.GetProcesses();
                int totalThreads = allProcesses.Sum(p => p.Threads.Count);
                int totalProcesses = allProcesses.Length;

                // 4. Top Processes (Svelte arayüzündeki tablo için)
                var topProcesses = allProcesses
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(4)
                    .Select(p => new
                    {
                        pid = p.Id,
                        command = p.ProcessName,
                        user = "SYSTEM",
                        cpu = 0, // Performance counter olmadan anlık CPU yüzdesi zordur, arayüzde gizlenebilir veya 0 bırakılabilir.
                        memory = Math.Round(p.WorkingSet64 / 1024.0 / 1024.0, 1) + " MB"
                    }).ToList();

                // 5. Grafikler İçin Canlı CPU ve RAM Yüzdesi Hesaplama
                float currentCpuPercent = _cpuCounter.NextValue();

                // Toplam fiziksel RAM'i GC üzerinden alıp anlık yüzdeyi hesaplıyoruz
                var gcInfo = GC.GetGCMemoryInfo();
                double totalRamMB = gcInfo.TotalAvailableMemoryBytes / 1024.0 / 1024.0;
                float availableRamMB = _ramAvailableCounter.NextValue();
                double usedRamMB = totalRamMB - availableRamMB;
                double currentRamPercent = totalRamMB > 0 ? Math.Round((usedRamMB / totalRamMB) * 100, 1) : 0;

                return Ok(new
                {
                    Uptime = uptimeStr,
                    Storage = new { Total = totalStorageGB, Used = usedStorageGB, Free = freeStorageGB, Percent = diskUsagePercent },
                    SystemStats = new { Threads = totalThreads, Processes = totalProcesses },
                    TopProcesses = topProcesses,
                    CurrentCpuPercent = Math.Round(currentCpuPercent, 1),
                    CurrentRamPercent = currentRamPercent,
                    HardwareSensors = GetHardwareSensors() // BURASI EKLENDİ
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Sistem detayları okunamadı.", Details = ex.Message });
            }
        }

        // --- YENİ EKLENEN METOT: Sadece Gerçek Sensör Verilerini Okur ---
        private List<object> GetHardwareSensors()
        {
            var sensors = new List<object>();

            try
            {
                // Yalnızca MSAcpi üzerinden okunan GERÇEK verileri alır. 
                // Not: Uygulamanın yönetici yetkisi (Run as Administrator) ile çalışması gerekebilir.
                using (var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        // InstanceName genelde "ACPI\ThermalZone\THM0_0" gibi donanım yolunu verir.
                        // Arayüzde çok uzun ve karmaşık durmaması için ufak bir metin temizliği yapıyoruz.
                        string rawName = obj["InstanceName"]?.ToString() ?? "Unknown Thermal Zone";
                        string componentName = rawName.Replace("ACPI\\ThermalZone\\", "Zone: ");

                        // WMI sıcaklığı Kelvin'in 10 katı olarak döndürür. Celcius'a çeviriyoruz.
                        double kelvinTens = Convert.ToDouble(obj["CurrentTemperature"]);
                        float tempC = (float)Math.Round((kelvinTens / 10.0) - 273.15, 1);

                        // Sensör okuması bazen kapalı portlar için 0 veya absürt yüksek değerler (örneğin 2732 vb) verebilir.
                        // Sadece geçerli ve mantıklı bir sıcaklık aralığındaysa listeye dahil ediyoruz.
                        if (tempC > 0 && tempC < 150)
                        {
                            sensors.Add(new
                            {
                                Component = componentName,
                                Temp = tempC,
                                Status = tempC > 85 ? "Critical" : "Normal"
                            });
                        }
                    }
                }
            }
            catch
            {
                // Yetki yoksa veya anakart desteklemiyorsa liste boş döner. Mock veri (sahte veri) kesinlikle eklenmez.
            }

            return sensors;
        }
    }
}