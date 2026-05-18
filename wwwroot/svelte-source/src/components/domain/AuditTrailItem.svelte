<!--
  AuditTrailItem.svelte
  IAM Audit Trail entry for User Management page.
  Props: actor, action, time, detail, isSystem
-->
<script>
  /** @type {string} */
  export let actor = '';
  /** @type {string} */
  export let action = '';
  /** @type {string} */
  export let time = '';
  /** @type {string} */
  export let detail = '';
  /** @type {boolean} */
  export let isSystem = false;
</script>

<div class="audit-item">
  <div class="audit-dot" class:system={isSystem}></div>
  <div class="audit-content">
    <div class="audit-header">
      <div class="audit-meta">
        <span class="audit-actor" class:system-label={isSystem}>
          {actor}
        </span>
        <span class="audit-time">{time}</span>
      </div>
      <span class="audit-action">{action}</span>
    </div>
    {#if detail}
      <pre class="audit-detail">{detail}</pre>
    {/if}
  </div>
</div>

<style>
  .audit-item {
    display: flex;
    gap: var(--space-sm);
    align-items: flex-start;
    padding: var(--space-md) 0;
    border-bottom: 1px solid var(--panel-border-light);
  }

  .audit-dot {
    width: 10px;
    height: 10px;
    border-radius: 50%;
    border: 2px solid var(--outline-variant);
    background: transparent;
    margin-top: 4px;
    flex-shrink: 0;
  }

  .audit-dot.system {
    border-color: var(--error);
    background: var(--error);
  }

  .audit-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: var(--space-xs);
  }

  .audit-header {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .audit-meta {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .audit-actor {
    font: var(--text-mono-label);
    color: var(--on-surface-variant);
  }

  .system-label {
    color: var(--error);
  }

  .audit-time {
    font: var(--text-mono-label);
    color: var(--outline);
  }

  .audit-action {
    font: var(--text-body-md);
    font-weight: 600;
    color: var(--on-surface);
  }

  .audit-detail {
    font: var(--text-mono-code);
    font-size: 12px;
    padding: var(--space-sm);
    background: var(--surface-container-lowest);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-sm);
    color: var(--on-surface-variant);
    white-space: pre-wrap;
    word-break: break-all;
  }
</style>
