<!--
  BarChart.svelte
  Horizontal bar chart for Top Talkers (IP).
  Props: items, maxValue
-->
<script>
  /**
   * @typedef {{ label: string, value: number, color?: string, highlight?: boolean }} BarItem
   */

  /** @type {BarItem[]} */
  export let items = [];
  /** @type {number} Override max value, defaults to largest item */
  export let maxValue = 0;

  $: computedMax = maxValue || Math.max(...items.map(i => i.value), 1);

  const defaultColors = [
    'var(--primary)',
    'var(--secondary)',
    'var(--error)',
    'var(--tertiary)'
  ];
</script>

<div class="bar-chart">
  {#each items as item, i}
    <div class="bar-row" class:highlight={item.highlight}>
      <span class="bar-label" class:flagged={item.highlight}>
        {item.label}
      </span>
      <div class="bar-track">
        <div
          class="bar-fill"
          style="width: {(item.value / computedMax) * 100}%; background: {item.color || defaultColors[i % defaultColors.length]};"
        ></div>
      </div>
      <span class="bar-value">{item.value.toFixed(2)}</span>
    </div>
  {/each}
</div>

<style>
  .bar-chart {
    display: flex;
    flex-direction: column;
    gap: var(--space-md);
  }

  .bar-row {
    display: flex;
    align-items: center;
    gap: var(--space-md);
  }

  .bar-label {
    font: var(--text-mono-code);
    color: var(--on-surface-variant);
    min-width: 120px;
    flex-shrink: 0;
  }

  .bar-label.flagged {
    color: var(--error);
    padding: 2px 6px;
    background: rgba(239, 68, 68, 0.1);
    border: 1px solid rgba(239, 68, 68, 0.3);
    border-radius: var(--radius-xs);
  }

  .bar-track {
    flex: 1;
    height: 10px;
    background: var(--surface-container-highest);
    border-radius: var(--radius-full);
    overflow: hidden;
  }

  .bar-fill {
    height: 100%;
    border-radius: var(--radius-full);
    transition: width var(--transition-slow);
  }

  .bar-value {
    font: var(--text-mono-code);
    color: var(--on-surface-variant);
    min-width: 40px;
    text-align: right;
  }
</style>
