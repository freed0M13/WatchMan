<!--
  SearchInput.svelte
  Context-aware search input used in TopBar across all pages.
  Props: placeholder, value, shortcutHint
-->
<script>
  /** @type {string} */
  export let placeholder = 'Search...';
  /** @type {string} */
  export let value = '';
  /** @type {string} e.g. "Ctrl+K" */
  export let shortcutHint = '';
</script>

<div class="search-wrapper">
  <svg class="search-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
    <circle cx="11" cy="11" r="8"/>
    <path d="M21 21l-4.35-4.35"/>
  </svg>

  <input
    type="text"
    class="search-input"
    {placeholder}
    bind:value
    on:input
    on:focus
  />

  {#if shortcutHint}
    <div class="shortcut-hint">
      {#each shortcutHint.split('+') as key, i}
        {#if i > 0}<span class="shortcut-plus">+</span>{/if}
        <kbd>{key.trim()}</kbd>
      {/each}
    </div>
  {/if}
</div>

<style>
  .search-wrapper {
    position: relative;
    display: flex;
    align-items: center;
    width: 100%;
    max-width: 420px;
  }

  .search-icon {
    position: absolute;
    left: 12px;
    color: var(--outline);
    pointer-events: none;
  }

  .search-input {
    width: 100%;
    padding: 8px 12px 8px 36px;
    background: var(--surface-container-low);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-default);
    color: var(--on-surface);
    font-size: 13px;
    transition: border-color var(--transition-fast), background var(--transition-fast);
  }

  .search-input::placeholder {
    color: var(--outline);
  }

  .search-input:focus {
    background: var(--surface-container);
    border-color: var(--primary);
    outline: none;
  }

  .shortcut-hint {
    position: absolute;
    right: 10px;
    display: flex;
    align-items: center;
    gap: 2px;
    pointer-events: none;
  }

  kbd {
    font: var(--text-mono-label);
    padding: 2px 5px;
    background: var(--surface-container-highest);
    border: 1px solid var(--outline-variant);
    border-radius: 3px;
    color: var(--outline);
    font-size: 10px;
  }

  .shortcut-plus {
    color: var(--outline);
    font-size: 10px;
  }
</style>
