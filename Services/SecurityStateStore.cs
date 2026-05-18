// Services/SecurityStateStore.cs
using System.Collections.Concurrent;
using Watchman.Models;

namespace Watchman.Services
{
    public class SecurityStateStore
    {
        private int _threatIdCounter = 0;

        // Son tespit edilen tehditleri tutan thread-safe kuyruk (Örn: Son 100 tehdit)
        public ConcurrentQueue<ThreatEvent> LiveThreats { get; } = new();

        // Anlık ağ metrikleri
        public NetworkMetrics Metrics { get; } = new();
        public int CurrentLatencyMs { get; set; } = 0;
        public double PacketLossPercent { get; set; } = 0;

        public void AddThreat(string type, string sourceIp, string severity, string module)
        {
            var newThreat = new ThreatEvent
            {
                Id = Interlocked.Increment(ref _threatIdCounter),
                Time = DateTime.UtcNow,
                Type = type,
                SourceIp = sourceIp,
                Severity = severity,
                Module = module
            };

            LiveThreats.Enqueue(newThreat);

            // Belleğin şişmemesi için eski tehditleri listeden çıkarıyoruz (Kuyruk limiti: 100)
            while (LiveThreats.Count > 100)
            {
                LiveThreats.TryDequeue(out _);
            }
        }
    }
}