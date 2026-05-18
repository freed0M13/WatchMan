<!--
  NetworkAnalysis.svelte
  Network Traffic Analysis page.
-->
<script>
  import { onMount } from "svelte";
  import GlassCard from "../components/shared/GlassCard.svelte";
  import GaugeRing from "../components/charts/GaugeRing.svelte";
  import AreaChart from "../components/charts/AreaChart.svelte";
  import BarChart from "../components/charts/BarChart.svelte";
  import DonutChart from "../components/charts/DonutChart.svelte";
  import IPDetailDrawer from "../components/shared/IPDetailDrawer.svelte";

  let isIpDrawerOpen = false;
  /** @type {any} */
  let selectedIpData = null;

  let globalPacketRate = 0;
  let throughputBandwidth = 0;
  let oscilloscopeData = Array(15).fill(0);
  let latencyHistory = Array(15).fill(0);
  let packetLossHistory = Array(15).fill(0);
  let currentLatencyMs = 0;
  let packetLossPercent = 0;
  let topTalkers = [];
  let protocolSegments = [];
  let donutCenterValue = "TCP";
  let donutCenterLabel = "0%";

  function formatBytes(bytes) {
    if (bytes > 1073741824) return (bytes / 1073741824).toFixed(1) + " GB";
    if (bytes > 1048576) return (bytes / 1048576).toFixed(1) + " MB";
    return (bytes / 1024).toFixed(1) + " KB";
  }

  onMount(() => {
    fetchData();
    const interval = setInterval(fetchData, 2000);
    return () => clearInterval(interval);
  });

  async function fetchData() {
    try {
      const metricsRes = await fetch(
        "http://localhost:5000/api/networkanalysis/metrics",
      );

      if (metricsRes.ok) {
        const metrics = await metricsRes.json();

        globalPacketRate =
          metrics.globalPacketRate?.value ?? metrics.globalPacketRate ?? 0;
        throughputBandwidth =
          metrics.throughputBandwidth?.value ??
          metrics.throughputBandwidth ??
          0;
        currentLatencyMs =
          metrics.currentLatencyMs?.value ?? metrics.currentLatencyMs ?? 0;
        packetLossPercent =
          metrics.packetLossPercent?.value ?? metrics.packetLossPercent ?? 0;

        oscilloscopeData.shift();
        oscilloscopeData.push(throughputBandwidth);
        oscilloscopeData = [...oscilloscopeData];

        latencyHistory.shift();
        latencyHistory.push(currentLatencyMs);
        latencyHistory = [...latencyHistory];

        packetLossHistory.shift();
        packetLossHistory.push(packetLossPercent);
        packetLossHistory = [...packetLossHistory];

        const talkers = metrics.topTalkers ?? metrics.TopTalkers ?? [];
        const colors = [
          "var(--primary)",
          "var(--secondary)",
          "var(--tertiary)",
          "var(--error)",
          "var(--warning)",
        ];
        topTalkers = talkers.slice(0, 5).map((t, i) => ({
          label: t.ip ?? t.Ip,
          rawValue: t.volumeGB ?? t.VolumeGB ?? 0,
          value: formatBytes(t.volumeGB ?? t.VolumeGB ?? 0),
          color: colors[i % colors.length],
          highlight: (t.risk || "").toLowerCase() === "critical",
        }));

        const dist =
          metrics.protocolDistribution ?? metrics.ProtocolDistribution ?? {};

        let tcpVal = dist.TCP || 0;
        let udpVal = dist.UDP || 0;
        let icmpVal = dist.ICMP || 0;

        if (tcpVal === 0 && udpVal === 0 && icmpVal === 0) {
          tcpVal = 1; // Prevent empty DonutChart crash
        }

        protocolSegments = [
          { label: "TCP", value: tcpVal, color: "var(--primary)" },
          {
            label: "UDP",
            value: udpVal,
            color: "var(--accent, var(--success))",
          },
          { label: "ICMP", value: icmpVal, color: "var(--warning)" },
        ];

        const sorted = [...protocolSegments].sort((a, b) => b.value - a.value);
        donutCenterValue = sorted[0].label;
        let total = protocolSegments.reduce((acc, curr) => acc + curr.value, 0);
        donutCenterLabel =
          Math.round((sorted[0].value / (total || 1)) * 100) + "%";
      }
    } catch (err) {
      console.error("API Error:", err);
    }
  }

  /** @param {string} ip */
  function handleIpClick(ip) {
    selectedIpData = {
      ip,
      country: "Unknown",
      asn: "AS00000",
      status: "Active",
      risk: ip === "45.33.22.11" ? "Critical" : "Low",
      history: [
        { time: "2m ago", event: "Connection established" },
        { time: "10m ago", event: "Port scan detected" },
      ],
    };
    isIpDrawerOpen = true;
  }
</script>

<div class="network-page">
  <div class="page-header">
    <div>
      <h1>Network Traffic Analysis</h1>
      <span class="mono-label">REAL-TIME MONITORING</span>
    </div>
  </div>

  <!-- Top Stats -->
  <div class="top-stats">
    <GlassCard>
      <div class="stat-gauge-row">
        <div class="stat-info">
          <span class="mono-label">PACKET RATE</span>
          <div class="big-value">
            <span class="num">{globalPacketRate}</span><span class="unit"
              >pps</span
            >
          </div>
          <span class="trend-up">↗ Live</span>
        </div>
        <GaugeRing
          value={globalPacketRate}
          max={10000}
          size={100}
          strokeWidth={8}
          color="var(--primary)"
        />
      </div>
    </GlassCard>
    <GlassCard>
      <div class="stat-gauge-row">
        <div class="stat-info">
          <span class="mono-label">BANDWIDTH UTILIZATION</span>
          <div class="big-value">
            <span class="num">{throughputBandwidth}</span><span class="unit"
              >Mbps</span
            >
          </div>
          {#if throughputBandwidth > 80}
            <span
              class="warning-tag"
              style="color: var(--error); border-color: var(--error); background: rgba(239,68,68,0.1);"
              >⚠ High Traffic</span
            >
          {:else}
            <span
              class="warning-tag"
              style="color: var(--warning); border-color: rgba(251,191,36,0.5); background: rgba(251,191,36,0.1);"
              >⚠ Live</span
            >
          {/if}
        </div>
        <GaugeRing
          value={throughputBandwidth}
          max={1000}
          size={100}
          strokeWidth={8}
          color="var(--error)"
          label={throughputBandwidth}
        />
      </div>
    </GlassCard>
  </div>

  <!-- Oscilloscope -->
  <GlassCard>
    <div class="section-bar">
      <span class="mono-label">REAL-TIME THROUGHPUT OSCILLOSCOPE</span>
    </div>
    <AreaChart
      data={oscilloscopeData}
      height={120}
      color="var(--primary)"
      fillOpacity={0.1}
      gradientId="oscillo"
    />
  </GlassCard>

  <!-- Bottom Grid -->
  <div class="bottom-grid">
    <GlassCard>
      <span class="mono-label">TOP TALKERS (IP)</span>
      <div class="talker-list">
        {#each topTalkers as talker}
          <button
            class="talker-btn"
            on:click={() => handleIpClick(talker.label)}
          >
            <div class="talker-info">
              <span class="talker-ip">{talker.label}</span>
              <span class="talker-val">{talker.value}</span>
            </div>
            <div class="talker-bar-bg">
              <div
                class="talker-bar-fill"
                style="width: {(talker.rawValue /
                  (topTalkers[0]?.rawValue || 1)) *
                  100}%; background: {talker.color}"
              ></div>
            </div>
          </button>
        {/each}
      </div>
    </GlassCard>
    <GlassCard>
      <span class="mono-label">PROTOCOL DIST</span>
      <div class="proto-center">
        <DonutChart
          segments={protocolSegments}
          size={220}
          strokeWidth={24}
          centerValue={donutCenterValue}
          centerLabel={donutCenterLabel}
        />
        <div class="legend">
          {#each protocolSegments as s}
            <span class="legend-item"
              ><span class="dot" style="background:{s.color}"
              ></span>{s.label}</span
            >
          {/each}
        </div>
      </div>
    </GlassCard>
    <GlassCard>
      <span
        class="mono-label"
        style="display:block; margin-bottom: var(--space-md);"
        >NETWORK LINK QUALITY</span
      >
      <div class="link-quality-block">
        <div class="link-quality-head">
          <span class="link-label">NETWORK LATENCY</span>
          <span class="link-val">{currentLatencyMs} ms</span>
        </div>
        <AreaChart
          data={latencyHistory}
          height={70}
          color="var(--primary)"
          fillOpacity={0.1}
          gradientId="lat-oscillo"
        />
      </div>
      <div class="link-quality-block" style="margin-top: var(--space-md);">
        <div class="link-quality-head">
          <span class="link-label">PACKET LOSS</span>
          <span class="link-val">{packetLossPercent} %</span>
        </div>
        <AreaChart
          data={packetLossHistory}
          height={70}
          color="var(--error)"
          fillOpacity={0.1}
          gradientId="pl-oscillo"
        />
      </div>
    </GlassCard>
  </div>
</div>

<IPDetailDrawer
  isOpen={isIpDrawerOpen}
  ipData={selectedIpData}
  on:close={() => (isIpDrawerOpen = false)}
/>

<style>
  .network-page {
    display: flex;
    flex-direction: column;
    gap: var(--space-lg);
  }
  .page-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
  }
  .page-header h1 {
    font: var(--text-headline-md);
    color: var(--on-surface);
  }
  .header-right {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
  }
  .filter-btn {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 8px 14px;
    background: var(--surface-container);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-default);
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
    cursor: pointer;
  }
  .filter-btn:hover {
    border-color: var(--outline);
  }
  .filter-btn.sm {
    padding: 6px 10px;
    font-size: 12px;
  }
  .mono-label {
    font: var(--text-mono-label);
    color: var(--outline);
    letter-spacing: 0.05em;
    text-transform: uppercase;
  }
  .top-stats {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--gutter);
  }
  .stat-gauge-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  .stat-info {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }
  .big-value {
    display: flex;
    align-items: baseline;
    gap: 4px;
  }
  .num {
    font: var(--text-headline-lg);
    font-size: 2rem;
    font-weight: 600;
    line-height: 1;
    color: var(--on-surface);
  }
  .unit {
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
    margin-left: 4px;
    font-weight: 500;
  }
  .trend-up {
    font: var(--text-body-sm);
    color: var(--success);
    display: flex;
    align-items: center;
    gap: 4px;
  }
  .warning-tag {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    font: var(--text-body-sm);
    color: var(--warning);
    padding: 4px 8px;
    background: rgba(251, 191, 36, 0.08);
    border: 1px solid rgba(251, 191, 36, 0.2);
    border-radius: var(--radius-sm);
  }
  .section-bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: var(--space-md);
  }
  .bottom-grid {
    display: grid;
    grid-template-columns: 1fr 1fr 1fr;
    gap: var(--gutter);
  }
  .proto-center {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: var(--space-md);
    margin-top: var(--space-md);
    flex: 1;
  }
  .legend {
    display: flex;
    gap: var(--space-md);
  }
  .legend-item {
    display: flex;
    align-items: center;
    gap: 6px;
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
  }
  .dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
  }
  .link-quality-block {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }
  .link-quality-head {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  .link-label {
    font: var(--text-mono-label);
    color: var(--on-surface-variant);
  }
  .link-val {
    font: var(--text-body-md);
    color: var(--on-surface);
    font-weight: 600;
  }

  /* Talker List */
  .talker-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
    margin-top: 16px;
  }
  .talker-btn {
    display: flex;
    flex-direction: column;
    gap: 6px;
    width: 100%;
    text-align: left;
    background: transparent;
    border: none;
    cursor: pointer;
    padding: 4px;
    border-radius: var(--radius-sm);
    transition: background var(--transition-fast);
  }
  .talker-btn:hover {
    background: var(--surface-container-highest);
  }
  .talker-info {
    display: flex;
    justify-content: space-between;
    align-items: center;
    width: 100%;
  }
  .talker-ip {
    font: var(--text-mono-code);
    color: var(--on-surface);
  }
  .talker-val {
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
  }
  .talker-bar-bg {
    width: 100%;
    height: 6px;
    background: var(--surface-container);
    border-radius: 3px;
    overflow: hidden;
  }
  .talker-bar-fill {
    height: 100%;
    border-radius: 3px;
    transition: width 1s ease-out;
  }

  @media (max-width: 1200px) {
    .top-stats,
    .bottom-grid {
      grid-template-columns: 1fr;
    }
  }
</style>
