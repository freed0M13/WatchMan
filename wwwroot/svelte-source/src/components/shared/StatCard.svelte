<!--
  StatCard.svelte
  Reusable KPI statistic card used across Dashboard, Network, System pages.
  Props: title, value, unit, icon, trend, trendLabel, trendDirection, accentColor
-->
<script>
  /** @type {string} */
  export let title = '';
  /** @type {string} */
  export let value = '';
  /** @type {string} */
  export let unit = '';
  /** @type {string} SVG icon markup */
  export let icon = '';
  /** @type {string} e.g. "+14%" */
  export let trend = '';
  /** @type {string} e.g. "vs last period" */
  export let trendLabel = '';
  /** @type {'up'|'down'|'neutral'} */
  export let trendDirection = 'neutral';
  /** @type {string} optional accent color override */
  export let accentColor = '';
</script>

<div class="stat-card">
  <div class="stat-header">
    <span class="stat-title">{title}</span>
    {#if icon}
      <span class="stat-icon" style={accentColor ? `color: ${accentColor}` : ''}>
        {@html icon}
      </span>
    {/if}
  </div>

  <div class="stat-value">
    <span class="value">{value}</span>
    {#if unit}
      <span class="unit">{unit}</span>
    {/if}
  </div>

  {#if trend}
    <div class="stat-trend" class:trend-up={trendDirection === 'up'} class:trend-down={trendDirection === 'down'}>
      <span class="trend-icon">
        {#if trendDirection === 'up'}↗{:else if trendDirection === 'down'}↘{:else}→{/if}
      </span>
      <span class="trend-value">{trend}</span>
      {#if trendLabel}
        <span class="trend-label">{trendLabel}</span>
      {/if}
    </div>
  {/if}
</div>

<style>
  .stat-card {
    background: var(--surface-container);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-default);
    padding: var(--space-lg);
    display: flex;
    flex-direction: column;
    gap: var(--space-sm);
    transition: border-color var(--transition-normal), box-shadow var(--transition-normal);
  }

  .stat-card:hover {
    border-color: var(--outline-variant);
    box-shadow: var(--glow-primary);
  }

  .stat-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
  }

  .stat-title {
    font: var(--text-mono-label);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: var(--on-surface-variant);
  }

  .stat-icon {
    font-size: 20px;
    color: var(--primary);
    opacity: 0.8;
  }

  .stat-value {
    display: flex;
    align-items: baseline;
    gap: var(--space-xs);
  }

  .value {
    font: var(--text-display-lg);
    color: var(--on-surface);
    letter-spacing: -0.02em;
  }

  .unit {
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
  }

  .stat-trend {
    display: flex;
    align-items: center;
    gap: var(--space-xs);
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
  }

  .trend-up { color: var(--success); }
  .trend-down { color: var(--error); }

  .trend-value {
    font-weight: 500;
  }

  .trend-label {
    color: var(--on-surface-variant);
  }
</style>
