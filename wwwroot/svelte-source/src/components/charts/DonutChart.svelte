<!--
  DonutChart.svelte
  SVG donut/ring chart for Protocol Distribution, Storage I/O.
  Props: segments, size, strokeWidth, centerLabel, centerValue
-->
<script>
  /**
   * @typedef {{ value: number, color: string, label?: string }} Segment
   */

  /** @type {Segment[]} */
  export let segments = [];
  /** @type {number} */
  export let size = 160;
  /** @type {number} */
  export let strokeWidth = 20;
  /** @type {string} */
  export let centerLabel = '';
  /** @type {string} */
  export let centerValue = '';

  const radius = (size - strokeWidth) / 2;
  const circumference = 2 * Math.PI * radius;
  const cx = size / 2;
  const cy = size / 2;

  $: total = segments.reduce((sum, s) => sum + s.value, 0);

  $: arcs = (() => {
    let offset = 0;
    return segments.map(seg => {
      const pct = seg.value / (total || 1);
      const dash = pct * circumference;
      const gap = circumference - dash;
      const rotation = offset * 360 - 90;
      offset += pct;
      return { ...seg, dash, gap, rotation, pct };
    });
  })();
</script>

<div class="donut-wrapper" style="width: {size}px; height: {size}px;">
  <svg viewBox="0 0 {size} {size}" class="donut-svg">
    <!-- Background ring -->
    <circle
      {cx} {cy} r={radius}
      fill="none"
      stroke="var(--surface-container-highest)"
      stroke-width={strokeWidth}
    />

    <!-- Data segments -->
    {#each arcs as arc}
      <circle
        {cx} {cy} r={radius}
        fill="none"
        stroke={arc.color}
        stroke-width={strokeWidth}
        stroke-dasharray="{arc.dash} {arc.gap}"
        stroke-linecap="butt"
        transform="rotate({arc.rotation} {cx} {cy})"
        class="donut-segment"
      />
    {/each}
  </svg>

  {#if centerValue || centerLabel}
    <div class="donut-center">
      <span class="center-value">{centerValue}</span>
      {#if centerLabel}
        <span class="center-label">{centerLabel}</span>
      {/if}
    </div>
  {/if}
</div>

<style>
  .donut-wrapper {
    position: relative;
    display: inline-flex;
    align-items: center;
    justify-content: center;
  }

  .donut-svg {
    width: 100%;
    height: 100%;
    transform: scaleY(-1) scaleX(1);
  }

  .donut-segment {
    transition: stroke-dasharray var(--transition-slow);
  }

  .donut-center {
    position: absolute;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    text-align: center;
  }

  .center-value {
    font: var(--text-title-sm);
    color: var(--on-surface);
  }

  .center-label {
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
  }
</style>
