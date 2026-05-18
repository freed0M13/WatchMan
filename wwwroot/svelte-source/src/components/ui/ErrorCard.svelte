<script>
  export let title = 'Failed to load data';
  export let message = 'An error occurred while fetching the requested information.';
  import { createEventDispatcher } from 'svelte';
  const dispatch = createEventDispatcher();
</script>

<div class="error-card">
  <div class="error-icon">
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
      <circle cx="12" cy="12" r="10"/>
      <line x1="12" y1="8" x2="12" y2="12"/>
      <line x1="12" y1="16" x2="12.01" y2="16"/>
    </svg>
  </div>
  <div class="error-content">
    <h4>{title}</h4>
    <p>{message}</p>
  </div>
  <button class="retry-btn" on:click={() => dispatch('retry')}>Retry</button>
</div>

<style>
  .error-card {
    display: flex;
    align-items: center;
    gap: var(--space-md);
    background: rgba(239, 68, 68, 0.1);
    border: 1px solid var(--error);
    border-radius: var(--radius-md);
    padding: var(--space-md);
  }
  .error-icon { color: var(--error); }
  .error-content { flex: 1; }
  h4 { font: var(--text-title-sm); color: var(--error); margin-bottom: 4px; }
  p { font: var(--text-body-sm); color: var(--on-surface); }
  .retry-btn {
    padding: 8px 16px;
    background: var(--error);
    color: #fff;
    border-radius: var(--radius-sm);
    font: var(--text-body-sm);
    font-weight: 600;
    cursor: pointer;
    border: none;
    transition: opacity var(--transition-fast);
  }
  .retry-btn:hover {
    opacity: 0.9;
  }
</style>
