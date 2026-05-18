<!--
  AreaChart.svelte
  Pure SVG area/line chart for Network Traffic, CPU Utilization, Memory Allocation.
  Props: data, width, height, color, fillOpacity, showGrid, label
-->
<script>
  /** @type {number[]} */
  export let data = [];
  /** @type {number} */
  export let width = 600;
  /** @type {number} */
  export let height = 200;
  /** @type {string} */
  export let color = 'var(--primary)';
  /** @type {number} */
  export let fillOpacity = 0.15;
  /** @type {boolean} */
  export let showGrid = true;
  /** @type {string} */
  export let gradientId = 'areaGrad';

  const padding = { top: 10, right: 10, bottom: 10, left: 10 };

  $: chartW = width - padding.left - padding.right;
  $: chartH = height - padding.top - padding.bottom;
  $: maxVal = Math.max(...data, 1);

  $: points = data.map((d, i) => ({
    x: padding.left + (i / Math.max(data.length - 1, 1)) * chartW,
    y: padding.top + chartH - (d / maxVal) * chartH
  }));

  $: linePath = points.map((p, i) => {
    if (i === 0) return `M ${p.x},${p.y}`;
    const prev = points[i - 1];
    const cpx1 = prev.x + (p.x - prev.x) * 0.4;
    const cpx2 = prev.x + (p.x - prev.x) * 0.6;
    return `C ${cpx1},${prev.y} ${cpx2},${p.y} ${p.x},${p.y}`;
  }).join(' ');

  $: areaPath = linePath +
    ` L ${points[points.length - 1]?.x ?? 0},${padding.top + chartH}` +
    ` L ${padding.left},${padding.top + chartH} Z`;

  $: gridLines = showGrid
    ? [0.25, 0.5, 0.75].map(f => padding.top + chartH * (1 - f))
    : [];
</script>

<svg viewBox="0 0 {width} {height}" class="area-chart" preserveAspectRatio="none">
  <defs>
    <linearGradient id={gradientId} x1="0" y1="0" x2="0" y2="1">
      <stop offset="0%" stop-color={color} stop-opacity={fillOpacity} />
      <stop offset="100%" stop-color={color} stop-opacity="0.01" />
    </linearGradient>
  </defs>

  <!-- Grid Lines -->
  {#if showGrid}
    {#each gridLines as y}
      <line
        x1={padding.left} y1={y}
        x2={padding.left + chartW} y2={y}
        stroke="var(--panel-border)" stroke-width="1"
        stroke-dasharray="4,4"
      />
    {/each}
  {/if}

  <!-- Area Fill -->
  {#if points.length > 1}
    <path d={areaPath} fill="url(#{gradientId})" />
    <path d={linePath} fill="none" stroke={color} stroke-width="2" />
  {/if}
</svg>

<style>
  .area-chart {
    width: 100%;
    height: auto;
    display: block;
  }
</style>
