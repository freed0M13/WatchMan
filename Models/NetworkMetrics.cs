using System.Collections.Concurrent;

namespace Watchman.Models
{
    public class NetworkMetrics
    {
        public int GlobalPacketRate { get; set; } // pps
        public double ThroughputBandwidth { get; set; } // Mbps
        public ConcurrentDictionary<string, double> TopTalkers { get; set; } = new();
        public ConcurrentDictionary<string, int> ProtocolDistribution { get; set; } = new();
    }
}