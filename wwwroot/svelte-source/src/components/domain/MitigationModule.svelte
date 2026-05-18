<!--
  MitigationModule.svelte
  Automated Threat Mitigation module card for Rules page.
  Props: name, description, icon, enabled, status, metric
-->
<script>
  import ToggleSwitch from '../shared/ToggleSwitch.svelte';

  /** @type {string} */
  export let name = '';
  /** @type {string} */
  export let description = '';
  /** @type {string} SVG icon HTML */
  export let icon = '';
  /** @type {boolean} */
  export let enabled = false;
  /** @type {string} e.g. "ACTIVE & ENFORCING" */
  export let status = '';
  /** @type {string} e.g. "1.2k drops/min" */
  export let metric = '';
  /** @type {'active'|'critical'|'inactive'} */
  export let statusType = 'inactive';
</script>

<div class="module-card" class:enabled>
  <div class="module-header">
    <span class="module-icon">{@html icon}</span>
    <ToggleSwitch bind:checked={enabled} />
  </div>

  <h4 class="module-name">{name}</h4>
  <p class="module-desc">{description}</p>

  <div class="module-footer">
    <span class="module-status" class:status-active={statusType === 'active'} class:status-critical={statusType === 'critical'}>
      <span class="status-dot"></span>
      {status}
    </span>
    {#if metric}
      <span class="module-metric">{metric}</span>
    {/if}
  </div>
</div>

<style>
  .module-card {
    background: var(--surface-container);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-default);
    padding: var(--space-lg);
    display: flex;
    flex-direction: column;
    gap: var(--space-sm);
    transition: border-color var(--transition-normal);
  }

  .module-card.enabled {
    border-color: var(--outline-variant);
  }

  .module-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .module-icon {
    width: 40px;
    height: 40px;
    border-radius: var(--radius-default);
    background: var(--surface-container-highest);
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--primary);
  }

  .module-name {
    font: var(--text-title-sm);
    color: var(--on-surface);
    margin-top: var(--space-sm);
  }

  .module-desc {
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
    line-height: 1.5;
  }

  .module-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: auto;
    padding-top: var(--space-sm);
  }

  .module-status {
    font: var(--text-mono-label);
    color: var(--outline);
    display: flex;
    align-items: center;
    gap: 6px;
  }

  .status-dot {
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: var(--outline);
  }

  .status-active {
    color: var(--success);
  }
  .status-active .status-dot {
    background: var(--success);
  }

  .status-critical {
    color: var(--error);
  }
  .status-critical .status-dot {
    background: var(--error);
    animation: pulse-glow 2s ease-in-out infinite;
  }

  .module-metric {
    font: var(--text-mono-label);
    color: var(--on-surface-variant);
  }
</style>
