<!--
  SystemLogs.svelte
  System Logs page — log table with filter, detail panel, actions.
-->
<script>
  import GlassCard from '../components/shared/GlassCard.svelte';
  import StatusBadge from '../components/shared/StatusBadge.svelte';
  import LogDetailPanel from '../components/domain/LogDetailPanel.svelte';
  import { fly } from 'svelte/transition';
  import { onMount } from 'svelte';

  let selectedIndex = -1;
  let selectedSet = new Set();
  let showDetail = false;
  let toastMessage = "";

  /** @type {any[]} */
  let logs = [];
  let selectedLog = null;

  async function fetchLogs() {
    try {
      const res = await fetch('http://localhost:5000/api/logs');
      if (res.ok) {
        logs = await res.json();
      }
    } catch (err) {
      console.error('API Error:', err);
    }
  }

  onMount(() => {
    fetchLogs();
    const interval = setInterval(fetchLogs, 3000);
    return () => clearInterval(interval);
  });

  /** @param {number} i */
  function selectLog(i) {
    selectedIndex = i;
    const l = logs[i];
    selectedLog = {
      id: l.id || l.Id,
      severity: l.severity || l.Severity,
      date: (l.timestamp || l.Timestamp || '').split('T')[0] || (l.timestamp || l.Timestamp || '').split(' ')[0],
      time: (l.timestamp || l.Timestamp || '').split('T')[1] || (l.timestamp || l.Timestamp || '').split(' ')[1] || l.timestamp || l.Timestamp,
      source: l.source || l.Source,
      hostIp: l.hostIp || l.HostIp,
      alertType: l.alertType || l.AlertType,
      module: l.module || l.Module,
      processId: l.processId || l.ProcessId,
      rawMessage: l.rawMessage || l.RawMessage,
      json: l.json || l.Json,
      relatedEvents: l.relatedEvents || l.RelatedEvents
    };
    showDetail = true;
  }

  async function deleteSelected() {
    if (selectedSet.size === 0) return;
    const ids = Array.from(selectedSet).map(idx => logs[idx].id || logs[idx].Id);
    try {
      const res = await fetch('http://localhost:5000/api/logs/delete-bulk', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(ids)
      });
      if (res.ok) {
        selectedSet.clear();
        selectedSet = selectedSet;
        fetchLogs();
      }
    } catch (err) {
      console.error('Delete failed:', err);
    }
  }

  async function sendRemote() {
    if (selectedSet.size === 0) return;
    const ids = Array.from(selectedSet).map(idx => logs[idx].id || logs[idx].Id);
    try {
      const res = await fetch('http://localhost:5000/api/logs/send-remote', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(ids)
      });
      if (res.ok) {
        selectedSet.clear();
        selectedSet = selectedSet;
        toastMessage = "Logs sent to remote successfully.";
        setTimeout(() => toastMessage = "", 3000);
      }
    } catch (err) {
      console.error('Send remote failed:', err);
    }
  }

  /**
   * @param {number} i
   * @param {Event} event
   */
  function toggleSelection(i, event) {
    event.stopPropagation();
    if (selectedSet.has(i)) {
      selectedSet.delete(i);
    } else {
      selectedSet.add(i);
    }
    selectedSet = selectedSet; // trigger reactivity
  }

  function toggleAll() {
    if (selectedSet.size === logs.length) {
      selectedSet.clear();
    } else {
      logs.forEach((_, i) => selectedSet.add(i));
    }
    selectedSet = selectedSet;
  }
</script>

<div class="logs-page">
  <div class="logs-main">
    <div class="page-header">
      <div>
        <h1>System Logs</h1>
        <p class="subtitle">Real-time event stream and historical audit trail.</p>
      </div>
    </div>

    <!-- Filters -->
    <div class="filter-row">
      <div class="filter-input-wrap">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="4" y1="21" x2="4" y2="14"/><line x1="4" y1="10" x2="4" y2="3"/><line x1="12" y1="21" x2="12" y2="12"/><line x1="12" y1="8" x2="12" y2="3"/><line x1="20" y1="21" x2="20" y2="16"/><line x1="20" y1="12" x2="20" y2="3"/></svg>
        <input type="text" placeholder="Filter message..." class="filter-input" />
      </div>
      <button class="filter-btn">📅 Last 24 Hours</button>
    </div>

    <!-- Log Table -->
    <div class="log-table-wrap">
      <table class="log-table">
        <thead>
          <tr>
            <th style="width:40px; cursor:pointer;" on:click={toggleAll}>
              <span class="checkbox" class:checked={selectedSet.size === logs.length}>{selectedSet.size === logs.length ? '✓' : selectedSet.size > 0 ? '-' : ''}</span>
            </th>
            <th>LOG ID</th>
            <th>TIMESTAMP</th>
            <th>SEVERITY</th>
            <th>HOST IP</th>
            <th>SOURCE</th>
          </tr>
        </thead>
        <tbody>
          {#each logs as log, i (log.id || log.Id || i)}
            <tr class:selected={selectedIndex === i || selectedSet.has(i)} on:click={() => selectLog(i)} in:fly={{ y: 20, duration: 300, delay: i * 50 }}>
              <td on:click={(e) => toggleSelection(i, e)}><span class="checkbox" class:checked={selectedSet.has(i)}>{selectedSet.has(i) ? '✓' : ''}</span></td>
              <td class="mono">{log.id || log.Id}</td>
              <td class="mono">{log.timestamp || log.Timestamp}</td>
              <td><StatusBadge level={log.severity || log.Severity} /></td>
              <td class="mono">{log.hostIp || log.HostIp || 'N/A'}</td>
              <td>
                <span class="source-badge {(log.source || log.Source || '').toLowerCase()}">
                  {log.source || log.Source}
                </span>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    </div>

    <!-- Bottom Bar -->
    <div class="bottom-bar">
      <span class="showing">Showing {logs.length} entries</span>
      {#if selectedSet.size > 0}
        <div class="bottom-actions animate-fade-in">
          <span class="selected-count">{selectedSet.size} Log{selectedSet.size > 1 ? 's' : ''} selected</span>
          <button class="action-btn outline" on:click={deleteSelected}>Delete</button>
          <button class="action-btn accent" on:click={sendRemote}>⚡ Send to Remote</button>
        </div>
      {/if}
    </div>
  </div>

  <!-- Detail Panel -->
  <LogDetailPanel log={selectedLog} visible={showDetail} on:close={() => showDetail = false} />
</div>

{#if toastMessage}
  <div class="toast animate-slide-up">
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="20 6 9 17 4 12"></polyline></svg>
    <span>{toastMessage}</span>
  </div>
{/if}

<style>
  .logs-page { display:flex; height:calc(100vh - var(--topbar-height)); overflow:hidden; margin: calc(var(--container-padding) * -1); }
  .logs-main { flex:1; display:flex; flex-direction:column; padding:var(--container-padding); overflow:hidden; }
  .page-header h1 { font:var(--text-headline-md); color:var(--on-surface); }
  .subtitle { font:var(--text-body-md); color:var(--on-surface-variant); margin-top:4px; }
  .filter-row { display:flex; gap:var(--space-sm); margin:var(--space-lg) 0; }
  .filter-input-wrap { flex:1; position:relative; display:flex; align-items:center; }
  .filter-input-wrap svg { position:absolute; left:12px; color:var(--outline); }
  .filter-input { width:100%; padding:10px 12px 10px 36px; background:var(--surface-container); border:1px solid var(--panel-border); border-radius:var(--radius-default); color:var(--on-surface); font-size:14px; }
  .filter-input:focus { border-color:var(--primary); outline:none; }
  .filter-btn { display:flex; align-items:center; gap:6px; padding:8px 14px; background:var(--surface-container); border:1px solid var(--panel-border); border-radius:var(--radius-default); font:var(--text-body-sm); color:var(--on-surface-variant); cursor:pointer; white-space:nowrap; }
  .log-table-wrap { flex:1; overflow:auto; }
  .log-table { width:100%; border-collapse:collapse; }
  .log-table th { font:var(--text-mono-label); color:var(--outline); text-align:left; padding:var(--space-sm) var(--space-md); border-bottom:1px solid var(--panel-border); text-transform:uppercase; letter-spacing:0.05em; position:sticky; top:0; background:var(--surface); z-index:1; }
  .log-table td { padding:var(--space-sm) var(--space-md); border-bottom:1px solid var(--panel-border-light); vertical-align:middle; }
  .log-table td.mono { font:var(--text-mono-code); }
  .log-table tbody tr { cursor:pointer; transition:background var(--transition-fast); }
  .log-table tbody tr:hover { background:rgba(255,255,255,0.03); }
  .log-table tbody tr.selected { background:rgba(192,193,255,0.08); }
  .checkbox { display:inline-flex; align-items:center; justify-content:center; width:18px; height:18px; border:1px solid var(--outline-variant); border-radius:3px; font-size:11px; color:var(--primary); }
  .checkbox.checked { background:rgba(192,193,255,0.15); border-color:var(--primary); }
  .bottom-bar { display:flex; justify-content:space-between; align-items:center; padding:var(--space-md) 0; border-top:1px solid var(--panel-border); min-height: 60px; }
  .showing { font:var(--text-body-sm); color:var(--on-surface-variant); }
  .bottom-actions { display:flex; align-items:center; gap:var(--space-sm); }
  .selected-count { font:var(--text-body-sm); color:var(--on-surface-variant); padding:6px 12px; background:var(--surface-container); border-radius:var(--radius-default); }
  .action-btn { padding:8px 16px; border-radius:var(--radius-md); font:var(--text-body-md); font-weight:600; cursor:pointer; border: none; }
  .action-btn.accent { background:var(--primary-container); color:#fff; }
  .action-btn.outline { background:transparent; border: 1px solid var(--error); color:var(--error); }
  .action-btn.ghost { background:var(--surface-container-highest); color:var(--on-surface); }

  .source-badge { display:inline-block; padding:3px 8px; border-radius:var(--radius-sm); font:var(--text-mono-label); color:#fff; }
  .source-badge.alert { background:rgba(239, 68, 68, 0.2); border:1px solid var(--error); color:var(--error); }
  .source-badge.system { background:rgba(59, 130, 246, 0.2); border:1px solid #3b82f6; color:#3b82f6; }
  .source-badge.traffic { background:rgba(139, 92, 246, 0.2); border:1px solid #8b5cf6; color:#8b5cf6; }

  .toast { position:fixed; bottom:var(--space-lg); right:var(--space-lg); background:var(--surface-container-high); border:1px solid var(--success); color:var(--success); padding:var(--space-md) var(--space-lg); border-radius:var(--radius-md); display:flex; align-items:center; gap:var(--space-sm); box-shadow:0 4px 12px rgba(0,0,0,0.5); z-index:1000; font:var(--text-body-md); font-weight:600; }
  .animate-slide-up { animation:slideUp 0.3s ease-out; }
  @keyframes slideUp { from { transform:translateY(100%); opacity:0; } to { transform:translateY(0); opacity:1; } }
</style>
