using Microsoft.AspNetCore.Mvc;
using System.Management; // Donanım sıcaklığı (WMI) için gerekli
using System.Diagnostics;
using Watchman.Services;

namespace Watchman.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NetworkAnalysisController : ControllerBase
    {
        private readonly SecurityStateStore _stateStore;

        // Singleton servis controller'a enjekte ediliyor
        public NetworkAnalysisController(SecurityStateStore stateStore)
        {
            _stateStore = stateStore;
        }

        // GET: api/networkanalysis/metrics
        [HttpGet("metrics")]
        public IActionResult GetNetworkMetrics()
        {
            var metrics = new
            {
                GlobalPacketRate = new { Value = _stateStore.Metrics.GlobalPacketRate, Unit = "pps" },
                ThroughputBandwidth = new { Value = _stateStore.Metrics.ThroughputBandwidth, Unit = "Mbps" },
                CurrentLatencyMs = _stateStore.CurrentLatencyMs,       // YENİ
                PacketLossPercent = _stateStore.PacketLossPercent,     // YENİ
                TopTalkers = _stateStore.Metrics.TopTalkers.Select(x => new { Ip = x.Key, VolumeGB = Math.Round(x.Value, 2) }),
                ProtocolDistribution = _stateStore.Metrics.ProtocolDistribution
            };

            return Ok(metrics);
        }

        // GET: api/networkanalysis/threats
        [HttpGet("threats")]
        public IActionResult GetLiveThreats()
        {
            // Arka plan servisinin ConcurrentQueue'ya yazdığı gerçek tehditler listelenir
            var threats = _stateStore.LiveThreats.OrderByDescending(t => t.Time).ToList();
            return Ok(threats);
        }

        // GET: api/networkanalysis/hardware
        // Sistem Sıcaklığı ve Detaylı Donanım İncelemesi
        [HttpGet("hardware")]
        public IActionResult GetHardwareStatus()
        {
            float cpuTemp = GetCpuTemperature();

            var hardware = new
            {
                CpuTemperature = cpuTemp > 0 ? $"{cpuTemp} °C" : "Sensör Okunamadı (Yetki/Donanım Kısıtı)",
                Warning = cpuTemp > 85 ? "Aşırı Isınma Tespit Edildi!" : "Normal"
            };

            return Ok(hardware);
        }

        // WMI (Windows Management Instrumentation) ile İşlemci Sıcaklığı Okuma Modülü
        private float GetCpuTemperature()
        {
            try
            {
                // MSAcpi_ThermalZoneTemperature üzerinden okuma yapılır. 
                // Not: Uygulamanın Administrator olarak çalışması gerekir ve her anakart desteklemez.
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        // WMI sıcaklığı Kelvin'in 10 katı olarak döndürür. Celcius'a çeviriyoruz.
                        double tempK = Convert.ToDouble(obj["CurrentTemperature"]) / 10;
                        return (float)Math.Round(tempK - 273.15, 1);
                    }
                }
            }
            catch
            {
                // Eğer anakart desteklemiyorsa veya Admin yetkisi yoksa 0 döneriz.
                return 0;
            }
            return 0;
        }
    }
}