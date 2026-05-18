<script>
  import { createEventDispatcher, onMount } from 'svelte';
  const dispatch = createEventDispatcher();
  
  export let isOpen = false;
  let searchInput = '';
  /** @type {HTMLInputElement} */
  let inputElement;
  
  const commands = [
    { id: 'dashboard', label: 'Go to Dashboard', shortcut: 'G D' },
    { id: 'network', label: 'Go to Network Analysis', shortcut: 'G N' },
    { id: 'system', label: 'Go to System Analysis', shortcut: 'G S' },
    { id: 'logs', label: 'Go to Logs', shortcut: 'G L' },
    { id: 'users', label: 'Go to User Management', shortcut: 'G U' },
    { id: 'rules', label: 'Go to Rules & Whitelist', shortcut: 'G R' },
    { id: 'settings', label: 'Go to Settings', shortcut: 'G ,' }
  ];

  $: filteredCommands = commands.filter(c => 
    c.label.toLowerCase().includes(searchInput.toLowerCase())
  );

  /** @param {KeyboardEvent} e */
  function handleKeydown(e) {
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
      e.preventDefault();
      isOpen = !isOpen;
      if (isOpen) {
        searchInput = '';
        setTimeout(() => inputElement?.focus(), 50);
      }
    }
    if (e.key === 'Escape' && isOpen) {
      isOpen = false;
    }
  }

  /** @param {string} id */
  function executeCommand(id) {
    isOpen = false;
    dispatch('navigate', id);
  }
</script>

<svelte:window on:keydown={handleKeydown} />

{#if isOpen}
  <!-- svelte-ignore a11y-click-events-have-key-events -->
  <!-- svelte-ignore a11y-no-static-element-interactions -->
  <div class="palette-backdrop" on:click={() => isOpen = false}>
    <div class="palette-content animate-fade-in" on:click|stopPropagation>
      <div class="palette-search">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--outline)" stroke-width="2">
          <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
        </svg>
        <input bind:this={inputElement} type="text" bind:value={searchInput} placeholder="Type a command or search..." />
      </div>
      <div class="palette-results">
        {#each filteredCommands as cmd}
          <button class="palette-item" on:click={() => executeCommand(cmd.id)}>
            <span class="cmd-label">{cmd.label}</span>
            <span class="cmd-shortcut">{cmd.shortcut}</span>
          </button>
        {/each}
        {#if filteredCommands.length === 0}
          <div class="palette-empty">No results found.</div>
        {/if}
      </div>
    </div>
  </div>
{/if}

<style>
  .palette-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    background: rgba(0, 0, 0, 0.4);
    backdrop-filter: blur(2px);
    z-index: 2000;
    display: flex;
    justify-content: center;
    padding-top: 10vh;
  }

  .palette-content {
    width: 90%;
    max-width: 600px;
    background: var(--surface-container);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-lg);
    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
    display: flex;
    flex-direction: column;
    overflow: hidden;
    height: fit-content;
  }

  .palette-search {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
    padding: var(--space-md);
    border-bottom: 1px solid var(--panel-border);
  }

  .palette-search input {
    flex: 1;
    background: transparent;
    border: none;
    outline: none;
    color: var(--on-surface);
    font: var(--text-body-md);
    font-size: 16px;
    padding: 0;
    box-shadow: none;
  }

  .palette-results {
    max-height: 300px;
    overflow-y: auto;
    padding: var(--space-xs) 0;
  }

  .palette-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
    padding: 12px var(--space-md);
    background: transparent;
    border: none;
    color: var(--on-surface);
    font: var(--text-body-md);
    cursor: pointer;
    transition: background var(--transition-fast);
  }

  .palette-item:hover {
    background: var(--surface-container-high);
  }

  .cmd-shortcut {
    font: var(--text-mono-label);
    color: var(--outline);
    background: var(--surface-container-lowest);
    padding: 2px 6px;
    border-radius: var(--radius-sm);
    border: 1px solid var(--panel-border);
  }

  .palette-empty {
    padding: var(--space-md);
    text-align: center;
    color: var(--on-surface-variant);
    font: var(--text-body-md);
  }
</style>
