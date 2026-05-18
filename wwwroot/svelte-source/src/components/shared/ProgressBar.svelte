<!--
  ProgressBar.svelte
  Horizontal progress indicator for CPU, RAM, Disk, Storage metrics.
  Props: value, max, label, color, showLabel
-->
<script>
  /** @type {number} Current value */
  export let value = 0;
  /** @type {number} Maximum value */
  export let max = 100;
  /** @type {string} Metric label */
  export let label = '';
  /** @type {string} Bar color */
  export let color = 'var(--primary)';
  /** @type {boolean} Show percentage label */
  export let showLabel = true;

  $: percent = Math.min(Math.round((value / max) * 100), 100);
  $: barColor = percent > 80 ? 'var(--error)' : percent > 60 ? 'var(--tertiary)' : color;
</script>

<div class="progress-wrapper">
  {#if label || showLabel}
    <div class="progress-meta">
      {#if label}
        <span class="progress-label">{label}</span>
      {/if}
      {#if showLabel}
        <span class="progress-value" style="color: {barColor}">{percent}%</span>
      {/if}
    </div>
  {/if}
  <div class="progress-track">
    <div
      class="progress-fill"
      style="width: {percent}%; background: {barColor};"
    ></div>
  </div>
</div>

<style>
  .progress-wrapper {
    display: flex;
    flex-direction: column;
    gap: var(--space-xs);
    width: 100%;
  }

  .progress-meta {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .progress-label {
    font: var(--text-mono-label);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: var(--on-surface-variant);
  }

  .progress-value {
    font: var(--text-mono-label);
    font-weight: 600;
  }

  .progress-track {
    width: 100%;
    height: 6px;
    background: var(--surface-container-highest);
    border-radius: var(--radius-full);
    overflow: hidden;
  }

  .progress-fill {
    height: 100%;
    border-radius: var(--radius-full);
    transition: width var(--transition-slow);
  }
</style>
