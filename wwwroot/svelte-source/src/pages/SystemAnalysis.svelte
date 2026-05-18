<!--
  SystemAnalysis.svelte
  System Analysis page — uptime, storage, agents, CPU/Memory charts, storage I/O, top processes.
-->
<script>
  import { onMount } from 'svelte';
  import GlassCard from '../components/shared/GlassCard.svelte';
  import ProgressBar from '../components/shared/ProgressBar.svelte';

  let cpuData = Array(15).fill(0);
  let memData = Array(15).fill(0);

  let globalUptime = "Loading...";
  let storageUsed = 0;
  let storageTotal = 0;
  let storagePercent = 0;
  let activeThreads = 0;
  let runningProcesses = 0;

  let storageSegments = [];
  let processes = [];
  let hardwareSensors = [];

  $: currentCpu = cpuData.length > 0 ? Math.round(cpuData[cpuData.length - 1]) : 0;
  $: isCpuSpiking = currentCpu > 80;
  $: memAvg = memData.length > 0 ? Math.round(memData.reduce((a, b) => a + b, 0) / memData.length) : 0;
  $: currentMem = memData.length > 0 ? Math.round(memData[memData.length - 1]) : 0;

  onMount(() => {
    fetchData();
    const interval = setInterval(fetchData, 2000);
    return () => clearInterval(interval);
  });

  async function fetchData() {
    try {
      const res = await fetch('http://localhost:5000/api/system/full');
      if (res.ok) {
        const data = await res.json();
        
        globalUptime = data.uptime ?? data.Uptime ?? globalUptime;
        
        const storage = data.storage ?? data.Storage ?? {};
        storageUsed = storage.used ?? storage.Used ?? 0;
        storageTotal = storage.total ?? storage.Total ?? 0;
        storagePercent = storage.percent ?? storage.Percent ?? 0;

        const sysStats = data.systemStats ?? data.SystemStats ?? {};
        activeThreads = sysStats.threads ?? sysStats.Threads ?? 0;
        runningProcesses = sysStats.processes ?? sysStats.Processes ?? 0;

        cpuData.shift();
        cpuData.push(data.currentCpuPercent ?? data.CurrentCpuPercent ?? 0);
        cpuData = [...cpuData];

        memData.shift();
        memData.push(data.currentRamPercent ?? data.CurrentRamPercent ?? 0);
        memData = [...memData];

        storageSegments = [
          { value: storageUsed, color: 'var(--primary)', label: 'Used Space' },
          { value: storage.free ?? storage.Free ?? Math.max(0, storageTotal - storageUsed), color: 'var(--surface-container-highest)', label: 'Free Space' }
        ];

        processes = data.topProcesses ?? data.TopProcesses ?? [];
        hardwareSensors = data.hardwareSensors ?? data.HardwareSensors ?? [];
      }
    } catch (err) {
      console.error("API Error:", err);
    }
  }
</script>

<div class="system-page">
  <div class="page-header">
    <div>
      <h1>System Analysis</h1>
      <p class="subtitle">Real-time resource utilization and node health.</p>
    </div>
    <div class="header-right">
      <span class="auto-refresh"><span class="live-dot"></span> Auto-refresh: 5s</span>
      <button class="icon-btn">↻</button>
    </div>
  </div>

  <!-- Top Stats -->
  <div class="top-stats">
    <GlassCard>
      <div class="stat-col">
        <div class="stat-top"><span class="mono-label">Global Uptime</span>
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="var(--success)" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 6v6l4 2"/></svg>
        </div>
        <div class="big-value"><span class="num" style="font-size: 1.5rem;">{globalUptime}</span><span class="up-arrow">↑</span></div>
        <span class="meta">Live System Data</span>
      </div>
    </GlassCard>
    <GlassCard>
      <div class="stat-col">
        <div class="stat-top"><span class="mono-label">Local Disk Storage</span>
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="2" width="20" height="8" rx="2"/><rect x="2" y="14" width="20" height="8" rx="2"/></svg>
        </div>
        <div class="big-value"><span class="num">{storageUsed}</span><span class="unit">GB / {storageTotal} GB</span></div>
        <ProgressBar value={storagePercent} label="" showLabel={false} color="var(--primary)" />
        <div class="storage-meta"><span>{storagePercent}% Utilized</span></div>
      </div>
    </GlassCard>
    <GlassCard>
      <div class="stat-col">
        <div class="stat-top"><span class="mono-label">System Processes & Threads</span>
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="var(--info)" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/></svg>
        </div>
        <div class="agents-row">
          <div><span class="num">{activeThreads}</span><span class="meta">Active Threads</span></div>
          <div class="proc-box"><span class="proc-num">{runningProcesses}</span><span class="meta">Running Processes</span></div>
        </div>
      </div>
    </GlassCard>
  </div>

  <!-- Charts Row -->
  <div class="charts-row">
    <GlassCard>
      <div class="chart-head">
        <span class="chart-title">⊙ CPU Utilization</span>
        {#if isCpuSpiking}
          <span class="spike-badge">Spike Detected</span>
        {/if}
      </div>
      <ProgressBar value={currentCpu} label="{currentCpu}% Utilized" color="var(--primary)" />
    </GlassCard>
    <GlassCard>
      <div class="chart-head">
        <span class="chart-title">◐ Memory Allocation</span>
        <span class="avg-label">Avg {memAvg}%</span>
      </div>
      <ProgressBar value={currentMem} label="{currentMem}% Allocated" color="var(--tertiary)" />
    </GlassCard>
  </div>

  <!-- Bottom Row -->
  <div class="bottom-row">
    <GlassCard>
      <span class="chart-title">Hardware Sensor Telemetry</span>
      <div class="table-container" style="margin-top: var(--space-md);">
        {#if hardwareSensors.length === 0}
          <p class="meta" style="padding: var(--space-md) 0;">Sensör verisi okunamadı. Yönetici yetkisi (Administrator) gerekebilir.</p>
        {:else}
          <table class="proc-table">
            <thead>
              <tr><th>Component</th><th>Temperature</th><th>Status</th></tr>
            </thead>
            <tbody>
              {#each hardwareSensors as sensor}
                <tr>
                  <td class="mono">{sensor.Component || sensor.component}</td>
                  <td class="mono" style="color: {(sensor.Temp || sensor.temp) > 80 ? 'var(--error)' : 'var(--on-surface)'}">
                    {sensor.Temp || sensor.temp} °C
                  </td>
                  <td class="mono" style="color: {(sensor.Status || sensor.status) === 'OK' ? 'var(--success)' : 'var(--warning)'}">
                    {sensor.Status || sensor.status}
                  </td>
                </tr>
              {/each}
            </tbody>
          </table>
        {/if}
      </div>
    </GlassCard>
    <GlassCard>
      <div class="chart-head"><span class="chart-title">Top Processes</span></div>
      <table class="proc-table">
        <thead><tr><th>PID</th><th>Command</th><th>User</th><th>Threads</th><th>Memory</th></tr></thead>
        <tbody>
          {#each processes as p}
            <tr>
              <td class="mono">{p.pid ?? p.Pid}</td>
              <td class="mono">{p.command ?? p.Command}</td>
              <td class="mono">{p.user ?? p.User ?? 'SYSTEM'}</td>
              <td><span class="cpu-val" class:high={(p.threads ?? p.Threads) > 50}>{p.threads ?? p.Threads ?? 0}</span>
                <span class="cpu-bar"><span class="cpu-fill" style="width:{(p.threads ?? p.Threads ?? 0)}%; background:{(p.threads ?? p.Threads ?? 0) > 50 ? 'var(--error)' : 'var(--primary)'}"></span></span>
              </td>
              <td><span class="mem-val">{p.memory ?? p.Memory ?? '0 MB'}</span>
                <span class="mem-dot" style="background:var(--tertiary)"></span>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    </GlassCard>
  </div>
</div>

<style>
  .system-page { display:flex; flex-direction:column; gap:var(--space-lg); }
  .page-header { display:flex; justify-content:space-between; align-items:flex-start; }
  .page-header h1 { font:var(--text-headline-md); color:var(--on-surface); }
  .subtitle { font:var(--text-body-md); color:var(--on-surface-variant); margin-top:4px; }
  .header-right { display:flex; align-items:center; gap:var(--space-sm); }
  .auto-refresh { font:var(--text-mono-label); color:var(--on-surface-variant); display:flex; align-items:center; gap:6px; padding:6px 12px; border:1px solid var(--panel-border); border-radius:var(--radius-default); }
  .live-dot { width:6px; height:6px; border-radius:50%; background:var(--error); animation:pulse-glow 2s ease-in-out infinite; }
  .icon-btn { width:32px; height:32px; display:flex; align-items:center; justify-content:center; border-radius:var(--radius-sm); color:var(--on-surface-variant); font-size:16px; }
  .icon-btn:hover { background:var(--surface-container-highest); }
  .mono-label { font:var(--text-mono-label); color:var(--outline); letter-spacing:0.05em; text-transform:uppercase; }
  .top-stats { display:grid; grid-template-columns:1fr 1fr 1fr; gap:var(--gutter); }
  .stat-col { display:flex; flex-direction:column; gap:var(--space-sm); }
  .stat-top { display:flex; justify-content:space-between; align-items:center; }
  .big-value { display:flex; align-items:baseline; gap:4px; }
  .num { font:var(--text-display-lg); color:var(--on-surface); }
  .unit { font:var(--text-body-sm); color:var(--on-surface-variant); }
  .up-arrow { color:var(--success); font-size:18px; }
  .meta { font:var(--text-body-sm); color:var(--on-surface-variant); }
  .storage-meta { display:flex; justify-content:space-between; font:var(--text-mono-label); color:var(--on-surface-variant); }
  .agents-row { display:flex; gap:var(--space-xl); align-items:flex-end; }
  .agents-row > div { display:flex; flex-direction:column; }
  .proc-box { padding:6px 10px; background:rgba(96,165,250,0.1); border:1px solid rgba(96,165,250,0.3); border-radius:var(--radius-sm); }
  .proc-num { font:var(--text-title-sm); font-size:24px; color:var(--info); }
  .charts-row { display:grid; grid-template-columns:1fr 1fr; gap:var(--gutter); }
  .chart-head { display:flex; justify-content:space-between; align-items:center; margin-bottom:var(--space-md); }
  .chart-title { font:var(--text-title-sm); font-size:14px; color:var(--on-surface); }
  .spike-badge { font:var(--text-mono-label); padding:4px 10px; background:rgba(239,68,68,0.12); border:1px solid rgba(239,68,68,0.25); border-radius:var(--radius-sm); color:#fca5a5; }
  .avg-label { font:var(--text-mono-label); color:var(--on-surface-variant); }
  .bottom-row { display:grid; grid-template-columns:1fr 1.5fr; gap:var(--gutter); }
  .donut-section { display:flex; flex-direction:column; align-items:center; gap:var(--space-lg); margin-top:var(--space-md); }
  .donut-legend { display:flex; flex-direction:column; gap:var(--space-sm); width:100%; }
  .legend-row { display:flex; align-items:center; gap:8px; font:var(--text-body-sm); color:var(--on-surface-variant); }
  .dot { width:8px; height:8px; border-radius:50%; flex-shrink:0; }
  .legend-val { margin-left:auto; font:var(--text-mono-code); }
  .link-btn { font:var(--text-mono-label); color:var(--primary); cursor:pointer; }
  .proc-table { width:100%; border-collapse:collapse; margin-top:var(--space-sm); }
  .proc-table th { font:var(--text-mono-label); color:var(--outline); text-align:left; padding:var(--space-sm); border-bottom:1px solid var(--panel-border); text-transform:uppercase; letter-spacing:0.05em; }
  .proc-table td { padding:var(--space-sm); font:var(--text-body-md); color:var(--on-surface); border-bottom:1px solid var(--panel-border-light); vertical-align:middle; }
  .proc-table td.mono { font:var(--text-mono-code); }
  .cpu-val { font:var(--text-mono-code); margin-right:6px; }
  .cpu-val.high { color:var(--error); }
  .cpu-bar { display:inline-block; width:60px; height:4px; background:var(--surface-container-highest); border-radius:var(--radius-full); overflow:hidden; vertical-align:middle; }
  .cpu-fill { display:block; height:100%; border-radius:var(--radius-full); }
  .mem-val { font:var(--text-mono-code); margin-right:6px; }
  .mem-dot { display:inline-block; width:6px; height:6px; border-radius:50%; vertical-align:middle; }
  @media(max-width:1200px) { .top-stats,.charts-row { grid-template-columns:1fr; } .bottom-row { grid-template-columns:1fr; } }
</style>
