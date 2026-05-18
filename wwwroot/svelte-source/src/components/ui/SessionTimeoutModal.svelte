<script>
  import { createEventDispatcher } from 'svelte';
  
  const dispatch = createEventDispatcher();
  
  export let isOpen = false;
  
  function handleStayLoggedIn() {
    dispatch('extend');
  }
  
  function handleLogout() {
    dispatch('logout');
  }
</script>

{#if isOpen}
  <div class="modal-backdrop animate-fade-in">
    <div class="modal-content animate-slide-in">
      <div class="modal-icon">
        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="var(--warning)" stroke-width="2">
          <circle cx="12" cy="12" r="10"/>
          <polyline points="12 6 12 12 16 14"/>
        </svg>
      </div>
      <h3 class="modal-title">Session Timeout Warning</h3>
      <p class="modal-body">
        You have been inactive for a while. For your security, you will be logged out soon. Do you want to stay logged in?
      </p>
      <div class="modal-actions">
        <button class="btn-secondary" on:click={handleLogout}>Log Out</button>
        <button class="btn-primary" on:click={handleStayLoggedIn}>Stay Logged In</button>
      </div>
    </div>
  </div>
{/if}

<style>
  .modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    background: rgba(0, 0, 0, 0.6);
    backdrop-filter: blur(4px);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
  }

  .modal-content {
    background: var(--surface-container);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-lg);
    padding: var(--space-xl);
    max-width: 400px;
    width: 90%;
    text-align: center;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-md);
    box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4);
  }

  .modal-icon {
    width: 56px;
    height: 56px;
    border-radius: 50%;
    background: rgba(251, 191, 36, 0.1);
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: var(--space-sm);
  }

  .modal-title {
    font: var(--text-headline-md);
    font-size: 20px;
    color: var(--on-surface);
  }

  .modal-body {
    font: var(--text-body-md);
    color: var(--on-surface-variant);
    line-height: 1.5;
  }

  .modal-actions {
    display: flex;
    gap: var(--space-md);
    width: 100%;
    margin-top: var(--space-md);
  }

  .btn-primary, .btn-secondary {
    flex: 1;
    padding: 10px 16px;
    border-radius: var(--radius-md);
    font: var(--text-body-md);
    font-weight: 600;
    cursor: pointer;
    transition: all var(--transition-fast);
  }

  .btn-primary {
    background: var(--primary);
    color: var(--on-primary);
    border: none;
  }

  .btn-primary:hover {
    background: var(--primary-container);
    color: var(--on-primary-container);
  }

  .btn-secondary {
    background: transparent;
    color: var(--on-surface);
    border: 1px solid var(--outline);
  }

  .btn-secondary:hover {
    background: var(--surface-container-high);
  }
</style>
