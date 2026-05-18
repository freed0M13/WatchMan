namespace Watchman.Models
{
    public class ThreatEvent
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string Type { get; set; } = string.Empty; // DDoS, Port Scan, vb.
        public string SourceIp { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // CRITICAL, HIGH, vb.
        public string Module { get; set; } = string.Empty; // DPI, Anomaly Engine, vb.
    }
}