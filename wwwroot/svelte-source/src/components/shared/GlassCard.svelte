<!--
  GlassCard.svelte
  Glassmorphism panel wrapper with optional header and actions.
  Props: title, icon, headerRight
-->
<script>
  /** @type {string} */
  export let title = '';
  /** @type {string} SVG icon HTML */
  export let icon = '';
  /** @type {string} optional CSS class */
  export let className = '';
</script>

<div class="glass-card {className}">
  {#if title}
    <div class="card-header">
      <div class="card-title">
        {#if icon}
          <span class="card-icon">{@html icon}</span>
        {/if}
        <h3>{title}</h3>
      </div>
      <div class="card-actions">
        <slot name="actions" />
      </div>
    </div>
  {/if}
  <div class="card-body">
    <slot />
  </div>
</div>

<style>
  .glass-card {
    background: var(--surface-container);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-default);
    overflow: hidden;
    transition: border-color var(--transition-normal);
  }

  .glass-card:hover {
    border-color: var(--outline-variant);
  }

  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: var(--space-md) var(--space-lg);
    border-bottom: 1px solid var(--panel-border);
  }

  .card-title {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
  }

  .card-title h3 {
    font: var(--text-title-sm);
    color: var(--on-surface);
  }

  .card-icon {
    color: var(--primary);
    display: flex;
    align-items: center;
  }

  .card-actions {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
  }

  .card-body {
    padding: var(--space-lg);
  }
</style>
