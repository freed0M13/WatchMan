<!--
  DashboardOverview.svelte
  Main dashboard page — KPI cards, Network Traffic chart, Live Threat Feed, System Resources.
-->
<script>
  import StatCard from "../components/shared/StatCard.svelte";
  import GlassCard from "../components/shared/GlassCard.svelte";
  import ProgressBar from "../components/shared/ProgressBar.svelte";
  import AreaChart from "../components/charts/AreaChart.svelte";
  import ThreatFeedItem from "../components/domain/ThreatFeedItem.svelte";
  import Heatmap from "../components/charts/Heatmap.svelte";

  /* ---------- Live Data States ---------- */
  let globalPacketRate = 0;
  let throughputBandwidth = 0;
  let threats = [];
  let topTalkers = [];

  // Trend States
  let prevAlerts = 0, alertsTrend = "0%", alertsTrendDir = "neutral";
  let prevPPS = 0, ppsTrend = "0%", ppsTrendDir = "neutral";
  let prevBandwidth = 0, bwTrend = "0%", bwTrendDir = "neutral";

  let systemHealthStatus = "Checking...";
  let systemHealthUptime = "";
  let cpuUsage = 0;
  let ramAvailable = 0;
  let diskUsage = 0;
  const ramTotal = 16384; // Mock assumption 16GB

  function calcTrend(oldVal, newVal) {
    if (oldVal === 0 && newVal === 0) return { str: "0%", dir: "neutral" };
    const pct = ((newVal - oldVal) / (oldVal || 1)) * 100;
    const sign = pct > 0 ? "+" : "";
    const dir = pct > 0 ? "up" : (pct < 0 ? "down" : "neutral");
    return { str: `${sign}${pct.toFixed(1)}%`, dir };
  }

  let trafficDataCurrent = Array(20).fill(0);
  const trafficDataPrevious = [
    12, 10, 18, 20, 30, 40, 45, 40, 50, 60, 65, 70, 60, 65, 70, 75, 80, 75, 70,
    65,
  ];

  $: activeTrafficData = comparisonMode
    ? trafficDataPrevious
    : trafficDataCurrent;

  $: stats = [
    {
      title: "Total Alerts",
      value: threats.length.toLocaleString(),
      icon: `<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--warning)" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>`,
      trend: alertsTrend,
      trendLabel: "vs last period",
      trendDirection: alertsTrendDir,
    },
    {
      title: "Packet Rate (PPS)",
      value: globalPacketRate.toLocaleString(),
      icon: `<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--primary)" stroke-width="2"><polyline points="22,12 18,12 15,21 9,3 6,12 2,12"/></svg>`,
      trend: ppsTrend,
      trendLabel: "vs last period",
      trendDirection: ppsTrendDir,
    },
    {
      title: "Bandwidth",
      value: throughputBandwidth.toLocaleString() + " Mbps",
      icon: `<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--tertiary)" stroke-width="2"><rect x="2" y="2" width="20" height="8" rx="2"/><rect x="2" y="14" width="20" height="8" rx="2"/><circle cx="6" cy="6" r="1"/><circle cx="6" cy="18" r="1"/></svg>`,
      trend: bwTrend,
      trendLabel: "vs last period",
      trendDirection: bwTrendDir,
    },
    {
      title: "System Health",
      value: systemHealthStatus,
      icon: `<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--success)" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 6v6l4 2"/></svg>`,
      trend: "",
      trendLabel: systemHealthUptime
        ? `Uptime: ${systemHealthUptime}`
        : "Checking...",
      trendDirection: "neutral",
    },
  ];

  let comparisonMode = false;
  let lastUpdated = 0;

  import { onMount } from "svelte";

  async function fetchLiveData() {
    try {
      const [metricsRes, threatsRes, healthRes] = await Promise.all([
        fetch("http://localhost:5000/api/networkanalysis/metrics"),
        fetch("http://localhost:5000/api/networkanalysis/threats"),
        fetch("http://localhost:5000/api/system/health"),
      ]);

      if (metricsRes.ok) {
        const metrics = await metricsRes.json();
        const pps =
          metrics.globalPacketRate?.value ?? metrics.globalPacketRate ?? 0;
        const bw =
          metrics.throughputBandwidth?.value ??
          metrics.throughputBandwidth ??
          0;

        const ppsCalc = calcTrend(prevPPS, pps);
        ppsTrend = ppsCalc.str;
        ppsTrendDir = ppsCalc.dir;
        prevPPS = pps;

        const bwCalc = calcTrend(prevBandwidth, bw);
        bwTrend = bwCalc.str;
        bwTrendDir = bwCalc.dir;
        prevBandwidth = bw;

        globalPacketRate = pps;
        throughputBandwidth = bw;
        topTalkers = metrics.topTalkers ?? metrics.TopTalkers ?? [];

        trafficDataCurrent.shift();
        trafficDataCurrent.push(pps);
        trafficDataCurrent = [...trafficDataCurrent];
      }

      if (threatsRes.ok) {
        const threatsData = await threatsRes.json();

        const currentAlerts = threatsData.length;
        const alertsCalc = calcTrend(prevAlerts, currentAlerts);
        alertsTrend = alertsCalc.str;
        alertsTrendDir = alertsCalc.dir;
        prevAlerts = currentAlerts;

        threats = threatsData.map((t) => {
          const date = new Date(t.time);
          const timeStr = date.toLocaleTimeString([], {
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit",
          });
          return {
            title: t.type,
            source: "SRC: " + t.sourceIp,
            time: timeStr,
          };
        });
      }

      if (healthRes.ok) {
        const health = await healthRes.json();
        systemHealthStatus = health.status ?? health.Status ?? "Online";
        systemHealthUptime = health.uptime ?? health.Uptime ?? "";

        cpuUsage = health.cpuUsagePercent ?? health.CpuUsagePercent ?? 0;
        ramAvailable = health.ramAvailableMB ?? health.RamAvailableMB ?? 0;
        diskUsage = health.diskUsagePercent ?? health.DiskUsagePercent ?? 0;
      }

      lastUpdated = 0;
    } catch (err) {
      console.error("API Error:", err);
    }
  }

  onMount(() => {
    fetchLiveData();

    const pollInterval = setInterval(fetchLiveData, 2000);
    const secInterval = setInterval(() => {
      lastUpdated++;
    }, 1000);

    return () => {
      clearInterval(pollInterval);
      clearInterval(secInterval);
    };
  });

  function handleRefresh() {
    fetchLiveData();
  }

  $: resources = [
    { label: "CPU Utilization", value: cpuUsage, color: "var(--primary)" },
    {
      label: `RAM Allocation (Avail: ${ramAvailable} MB)`,
      value:
        Math.max(
          0,
          Math.min(
            100,
            Math.round(((ramTotal - ramAvailable) / ramTotal) * 100),
          ),
        ) || 0,
      color: "var(--tertiary)",
    },
    { label: "Disk Usage", value: diskUsage, color: "var(--secondary)" },
  ];
</script>

<div class="dashboard">
  <!-- Page Header -->
  <div class="page-header">
    <div>
      <h1 class="page-title">Dashboard Overview</h1>
      <span class="page-subtitle">SYSTEM STATUS: <strong>NOMINAL</strong></span>
    </div>
    <div class="header-actions">
      <span class="last-updated">Last updated: {lastUpdated}s ago</span>
      <button
        class="icon-btn"
        on:click={handleRefresh}
        title="Refresh Dashboard"
      >
        <svg
          width="16"
          height="16"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          ><path
            d="M21.5 2v6h-6M21.34 15.57a10 10 0 1 1-.92-10.26l5.58 5.58"
          /></svg
        >
      </button>
      <button class="filter-btn">
        <svg
          width="14"
          height="14"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          ><rect x="3" y="4" width="18" height="18" rx="2" /><line
            x1="16"
            y1="2"
            x2="16"
            y2="6"
          /><line x1="8" y1="2" x2="8" y2="6" /><line
            x1="3"
            y1="10"
            x2="21"
            y2="10"
          /></svg
        >
        Last 24 Hours
        <svg
          width="12"
          height="12"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"><polyline points="6,9 12,15 18,9" /></svg
        >
      </button>
    </div>
  </div>

  <!-- KPI Cards -->
  <div class="stat-grid">
    {#each stats as stat}
      <StatCard
        title={stat.title}
        value={stat.value}
        icon={stat.icon}
        trend={stat.trend}
        trendLabel={stat.trendLabel}
        trendDirection={stat.trendDirection}
      />
    {/each}
  </div>

  <!-- System Resources -->
  <GlassCard title="System Resources">
    <div class="resources-grid">
      {#each resources as res}
        <ProgressBar label={res.label} value={res.value} color={res.color} />
      {/each}
    </div>
  </GlassCard>

  <!-- Main Content Grid -->
  <div class="content-grid">
    <!-- Network Traffic -->
    <div class="traffic-section">
      <GlassCard title="Network Traffic">
        <AreaChart
          data={activeTrafficData}
          height={220}
          color="var(--primary)"
          gradientId="dashTraffic"
        />
      </GlassCard>
    </div>

    <!-- Live Threat Feed -->
    <div class="threat-section">
      <GlassCard>
        <div slot="actions"></div>
        <div class="threat-header-custom">
          <span class="live-dot"></span>
          <h3>Live Threat Feed</h3>
        </div>
        <div class="threat-list">
          {#each threats as threat}
            <ThreatFeedItem
              title={threat.title}
              source={threat.source}
              time={threat.time}
            />
          {/each}
        </div>
      </GlassCard>
    </div>
  </div>

  <!-- Incident Heatmap -->
  <GlassCard title="Alert Intensity Heatmap">
    <Heatmap data={topTalkers} />
  </GlassCard>
</div>

<style>
  .dashboard {
    display: flex;
    flex-direction: column;
    gap: var(--space-lg);
  }

  .page-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
  }

  .page-title {
    font: var(--text-headline-md);
    color: var(--on-surface);
  }

  .page-subtitle {
    font: var(--text-mono-label);
    color: var(--outline);
    letter-spacing: 0.05em;
  }

  .page-subtitle strong {
    color: var(--success);
  }

  .header-actions {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
  }

  .last-updated {
    font: var(--text-mono-label);
    color: var(--outline);
    margin-right: var(--space-sm);
  }

  .filter-btn {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
    padding: 8px 14px;
    background: var(--surface-container);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-default);
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
    transition: border-color var(--transition-fast);
  }

  .filter-btn:hover {
    border-color: var(--outline);
  }

  .stat-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: var(--gutter);
  }

  .content-grid {
    display: grid;
    grid-template-columns: 1fr 340px;
    gap: var(--gutter);
  }

  .threat-header-custom {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
    margin-bottom: var(--space-md);
  }

  .threat-header-custom h3 {
    font: var(--text-title-sm);
    color: var(--on-surface);
  }

  .live-dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: var(--error);
    animation: pulse-glow 2s ease-in-out infinite;
  }

  .threat-list {
    display: flex;
    flex-direction: column;
    gap: var(--space-md);
  }

  .resources-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: var(--space-xl);
  }

  .icon-btn {
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: var(--radius-sm);
    color: var(--on-surface-variant);
    transition: background var(--transition-fast);
  }

  .icon-btn:hover {
    background: var(--surface-container-highest);
  }

  .chart-actions {
    display: flex;
    align-items: center;
  }

  .toggle-switch {
    display: flex;
    align-items: center;
    gap: 8px;
    cursor: pointer;
  }

  .toggle-switch input {
    display: none;
  }

  .toggle-switch .slider {
    width: 32px;
    height: 18px;
    background: var(--surface-container-highest);
    border-radius: 9px;
    position: relative;
    transition: background var(--transition-fast);
  }

  .toggle-switch .slider::after {
    content: "";
    position: absolute;
    top: 2px;
    left: 2px;
    width: 14px;
    height: 14px;
    background: #fff;
    border-radius: 50%;
    transition: transform var(--transition-fast);
  }

  .toggle-switch input:checked + .slider {
    background: var(--primary);
  }

  .toggle-switch input:checked + .slider::after {
    transform: translateX(14px);
  }

  .toggle-switch .label-text {
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
  }

  @media (max-width: 1200px) {
    .stat-grid {
      grid-template-columns: repeat(2, 1fr);
    }
    .content-grid {
      grid-template-columns: 1fr;
    }
    .resources-grid {
      grid-template-columns: 1fr;
    }
  }
</style>
