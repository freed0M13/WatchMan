<!--
  GaugeRing.svelte
  Circular gauge/ring for Packet Rate and Bandwidth Utilization.
  Props: value, max, size, strokeWidth, color, label, unit
-->
<script>
  /** @type {number} */
  export let value = 0;
  /** @type {number} */
  export let max = 100;
  /** @type {number} */
  export let size = 120;
  /** @type {number} */
  export let strokeWidth = 10;
  /** @type {string} */
  export let color = 'var(--primary)';
  /** @type {string} */
  export let label = '';

  const radius = (size - strokeWidth) / 2;
  const circumference = 2 * Math.PI * radius;
  const cx = size / 2;
  const cy = size / 2;

  $: pct = Math.min(value / (max || 1), 1);
  $: dashLen = pct * circumference;
  $: gapLen = circumference - dashLen;
  $: displayColor = pct > 0.9 ? 'var(--error)' : pct > 0.7 ? 'var(--tertiary)' : color;
</script>

<div class="gauge-wrapper" style="width: {size}px; height: {size}px;">
  <svg viewBox="0 0 {size} {size}">
    <!-- Background ring -->
    <circle
      {cx} {cy} r={radius}
      fill="none"
      stroke="var(--surface-container-highest)"
      stroke-width={strokeWidth}
    />
    <!-- Value arc -->
    <circle
      {cx} {cy} r={radius}
      fill="none"
      stroke={displayColor}
      stroke-width={strokeWidth}
      stroke-dasharray="{dashLen} {gapLen}"
      stroke-linecap="round"
      transform="rotate(-90 {cx} {cy})"
      class="gauge-arc"
    />
  </svg>

  <div class="gauge-center">
    {#if label}
      <span class="gauge-label">{label}</span>
    {:else}
      <span class="gauge-value">{Math.round(pct * 100)}%</span>
    {/if}
  </div>
</div>

<style>
  .gauge-wrapper {
    position: relative;
    display: inline-flex;
    align-items: center;
    justify-content: center;
  }

  svg {
    width: 100%;
    height: 100%;
  }

  .gauge-arc {
    transition: stroke-dasharray var(--transition-slow), stroke var(--transition-normal);
  }

  .gauge-center {
    position: absolute;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
  }

  .gauge-value {
    font: var(--text-title-sm);
    color: var(--on-surface);
  }

  .gauge-label {
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
    text-align: center;
  }
</style>
