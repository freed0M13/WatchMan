<script>
  import { createEventDispatcher } from 'svelte';
  import { fly } from 'svelte/transition';
  
  export let isOpen = false;
  /** @type {any} */
  export let ipData = null; // { ip, country, asn, status, risk, history, etc. }
  
  const dispatch = createEventDispatcher();
  
  function closeDrawer() {
    isOpen = false;
    dispatch('close');
  }
</script>

{#if isOpen && ipData}
  <!-- svelte-ignore a11y-click-events-have-key-events -->
  <!-- svelte-ignore a11y-no-static-element-interactions -->
  <div class="drawer-backdrop" on:click={closeDrawer}></div>
  <div class="drawer-panel" transition:fly={{ x: 400, duration: 300 }}>
    <div class="drawer-header">
      <div class="header-info">
        <h3>{ipData.ip}</h3>
        <span class="badge {ipData.risk === 'Critical' ? 'critical' : 'warning'}">{ipData.risk} Risk</span>
      </div>
      <button class="close-btn" aria-label="Close" on:click={closeDrawer}>
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
      </button>
    </div>
    
    <div class="drawer-content">
      <div class="info-grid">
        <div class="info-box">
          <span class="label">Location</span>
          <span class="value">{ipData.country || 'Unknown'}</span>
        </div>
        <div class="info-box">
          <span class="label">ASN</span>
          <span class="value">{ipData.asn || 'N/A'}</span>
        </div>
        <div class="info-box">
          <span class="label">Status</span>
          <span class="value">{ipData.status || 'Active'}</span>
        </div>
      </div>
      
      <div class="actions">
        <button class="action-btn block-btn">Block IP</button>
        <button class="action-btn whitelist-btn">Add to Whitelist</button>
      </div>
      
      <div class="history-section">
        <h4>Recent Activity</h4>
        <ul class="activity-list">
          {#each ipData.history || [] as item}
            <li>
              <span class="time">{item.time}</span>
              <span class="event">{item.event}</span>
            </li>
          {/each}
          {#if !(ipData.history) || ipData.history.length === 0}
             <li class="empty-msg">No recent activity</li>
          {/if}
        </ul>
      </div>
    </div>
  </div>
{/if}

<style>
  .drawer-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    background: rgba(0, 0, 0, 0.4);
    z-index: 1000;
  }
  
  .drawer-panel {
    position: fixed;
    top: 0;
    right: 0;
    width: 400px;
    max-width: 90vw;
    height: 100vh;
    background: var(--surface-container);
    border-left: 1px solid var(--panel-border);
    z-index: 1010;
    display: flex;
    flex-direction: column;
    box-shadow: -10px 0 30px rgba(0, 0, 0, 0.5);
  }
  
  .drawer-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: var(--space-lg);
    border-bottom: 1px solid var(--panel-border);
  }
  
  .header-info h3 {
    font: var(--text-headline-md);
    margin-bottom: 4px;
    font-variant-numeric: tabular-nums;
  }
  
  .badge {
    font: var(--text-mono-label);
    padding: 2px 6px;
    border-radius: var(--radius-sm);
  }
  
  .badge.critical { background: rgba(239, 68, 68, 0.2); color: var(--error); }
  .badge.warning { background: rgba(251, 191, 36, 0.2); color: var(--warning); }
  
  .close-btn { color: var(--outline); transition: color var(--transition-fast); }
  .close-btn:hover { color: var(--on-surface); }
  
  .drawer-content {
    padding: var(--space-lg);
    overflow-y: auto;
    flex: 1;
  }
  
  .info-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-md);
    margin-bottom: var(--space-xl);
  }
  
  .info-box {
    display: flex;
    flex-direction: column;
    background: var(--surface-container-lowest);
    padding: var(--space-md);
    border-radius: var(--radius-md);
    border: 1px solid var(--panel-border);
  }
  
  .info-box .label { font: var(--text-mono-label); color: var(--outline); margin-bottom: 4px; }
  .info-box .value { font: var(--text-body-md); font-weight: 600; color: var(--on-surface); }
  
  .actions {
    display: flex;
    gap: var(--space-md);
    margin-bottom: var(--space-xl);
  }
  
  .action-btn {
    flex: 1;
    padding: 10px;
    border-radius: var(--radius-md);
    font: var(--text-body-md);
    font-weight: 600;
    transition: opacity var(--transition-fast);
  }
  .action-btn:hover { opacity: 0.9; }
  
  .block-btn { background: var(--error); color: #fff; border: none; cursor: pointer; }
  .whitelist-btn { background: var(--surface-container-lowest); color: var(--on-surface); border: 1px solid var(--outline-variant); cursor: pointer; }
  
  .history-section h4 {
    font: var(--text-title-sm);
    margin-bottom: var(--space-md);
    padding-bottom: var(--space-sm);
    border-bottom: 1px solid var(--panel-border);
  }
  
  .activity-list { list-style: none; padding: 0; }
  .activity-list li {
    display: flex;
    justify-content: space-between;
    padding: var(--space-sm) 0;
    border-bottom: 1px dashed var(--panel-border);
  }
  .activity-list .time { font: var(--text-mono-label); color: var(--outline); }
  .activity-list .event { font: var(--text-body-sm); color: var(--on-surface); }
  .empty-msg { font: var(--text-body-sm); color: var(--outline); font-style: italic; justify-content: center !important; border: none !important; }
</style>
