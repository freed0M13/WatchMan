<script>
  export let title = 'No data available';
  export let message = 'There is currently no data to display in this view.';
</script>

<div class="empty-state">
  <div class="empty-icon">
    <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
      <circle cx="12" cy="12" r="10"/>
      <line x1="12" y1="8" x2="12" y2="12"/>
      <line x1="12" y1="16" x2="12.01" y2="16"/>
    </svg>
  </div>
  <h3>{title}</h3>
  <p>{message}</p>
  <slot />
</div>

<style>
  .empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: var(--space-2xl);
    text-align: center;
    background: var(--surface-container-lowest);
    border: 1px dashed var(--outline-variant);
    border-radius: var(--radius-lg);
  }
  .empty-icon {
    color: var(--outline);
    margin-bottom: var(--space-md);
  }
  h3 { font: var(--text-title-sm); color: var(--on-surface); margin-bottom: var(--space-xs); }
  p { font: var(--text-body-md); color: var(--on-surface-variant); max-width: 300px; margin-bottom: var(--space-md); }
</style>
