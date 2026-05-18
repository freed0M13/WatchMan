<!--
  ThreatFeedItem.svelte
  Live Threat Feed card for Dashboard page.
  Props: title, source, time, onAcknowledge
-->
<script>
  import { createEventDispatcher } from 'svelte';

  const dispatch = createEventDispatcher();

  /** @type {string} */
  export let title = '';
  /** @type {string} e.g. "SRC: 192.168.1.104" */
  export let source = '';
  /** @type {string} e.g. "Just now" */
  export let time = '';

  function acknowledge() {
    dispatch('acknowledge');
  }
</script>

<div class="threat-item">
  <div class="threat-header">
    <span class="threat-title">{title}</span>
    <span class="threat-time">{time}</span>
  </div>
  <span class="threat-source">{source}</span>
  <button class="threat-btn" on:click={acknowledge}>ACKNOWLEDGE</button>
</div>

<style>
  .threat-item {
    display: flex;
    flex-direction: column;
    gap: var(--space-sm);
    padding: var(--space-md);
    border-left: 3px solid var(--error);
    background: rgba(239, 68, 68, 0.04);
    border-radius: 0 var(--radius-sm) var(--radius-sm) 0;
    transition: background var(--transition-fast);
  }

  .threat-item:hover {
    background: rgba(239, 68, 68, 0.08);
  }

  .threat-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
  }

  .threat-title {
    font: var(--text-mono-code);
    font-weight: 700;
    color: var(--error);
  }

  .threat-time {
    font: var(--text-mono-label);
    color: var(--outline);
    white-space: nowrap;
  }

  .threat-source {
    font: var(--text-mono-label);
    color: var(--on-surface-variant);
  }

  .threat-btn {
    align-self: flex-start;
    padding: 4px 12px;
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-sm);
    font: var(--text-mono-label);
    color: var(--on-surface-variant);
    background: transparent;
    transition: background var(--transition-fast), border-color var(--transition-fast);
  }

  .threat-btn:hover {
    background: var(--surface-container-highest);
    border-color: var(--outline);
  }
</style>
