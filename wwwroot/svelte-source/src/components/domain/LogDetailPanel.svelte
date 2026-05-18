<!--
  LogDetailPanel.svelte
  Right side panel for System Logs page showing selected log details.
  Props: log, visible
-->
<script>
  import { createEventDispatcher } from "svelte";
  import StatusBadge from "../shared/StatusBadge.svelte";

  const dispatch = createEventDispatcher();

  /** @type {any} */
  export let log = null;
  /** @type {boolean} */
  export let visible = false;

  function close() {
    dispatch("close");
  }

  function copyJson() {
    if (log?.json) {
      navigator.clipboard.writeText(JSON.stringify(log.json, null, 2));
    }
  }
</script>

{#if visible && log}
  <aside class="log-detail animate-slide-in">
    <div class="detail-header">
      <div class="detail-title">
        <svg
          width="18"
          height="18"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
        >
          <path
            d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"
          />
          <polyline points="14,2 14,8 20,8" />
        </svg>
        <span>Log Details</span>
      </div>
      <button class="close-btn" on:click={close}>✕</button>
    </div>

    <!-- Severity & ID -->
    <div class="detail-badge-row">
      <StatusBadge level={log.severity?.toLowerCase() ?? "info"} />
      <span class="log-id">{log.id}</span>
    </div>

    <!-- Meta Grid -->
    <div class="meta-grid">
      <div class="meta-item">
        <span class="meta-label">TIMESTAMP</span>
        <span class="meta-value">{log.date}</span>
        <span class="meta-value">{log.time}</span>
      </div>
      <div class="meta-item">
        <span class="meta-label">SOURCE</span>
        <span class="meta-value">{log.source}</span>
      </div>
      <div class="meta-item">
        <span class="meta-label">HOST IP</span>
        <span class="meta-value mono">{log.hostIp ?? "—"}</span>
      </div>
      <div class="meta-item">
        <span class="meta-label">PROCESS ID</span>
        <span class="meta-value mono">{log.processId ?? "—"}</span>
      </div>
    </div>

    <!-- New Meta Info Badges -->
    <div class="meta-badges">
      <div class="meta-badge">
        <span class="badge-label">Alert Type</span>
        <span class="badge-value">{log.alertType || "N/A"}</span>
      </div>
      <div class="meta-badge">
        <span class="badge-label">Triggered Module</span>
        <span class="badge-value">{log.module || "N/A"}</span>
      </div>
    </div>

    <!-- Raw Message -->
    {#if log.rawMessage}
      <div class="detail-section">
        <h4>
          <svg
            width="14"
            height="14"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"><path d="M4 6h16M4 12h16M4 18h10" /></svg
          >
          Raw Message
        </h4>
        <pre class="raw-message">{log.rawMessage}</pre>
      </div>
    {/if}

    <!-- JSON Payload -->
    {#if log.json}
      <div class="detail-section">
        <div class="section-header">
          <h4>
            <svg
              width="14"
              height="14"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"><circle cx="12" cy="12" r="10" /></svg
            >
            JSON Payload
          </h4>
          <button class="copy-btn" on:click={copyJson}>
            <svg
              width="12"
              height="12"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
              ><rect x="9" y="9" width="13" height="13" rx="2" /><path
                d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"
              /></svg
            >
            Copy
          </button>
        </div>
        <pre class="json-payload">{JSON.stringify(log.json, null, 2)}</pre>
      </div>
    {/if}

    <!-- Related Events -->
    {#if log.relatedEvents?.length}
      <div class="detail-section">
        <h4>
          <svg
            width="14"
            height="14"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            ><polyline points="22,12 18,12 15,21 9,3 6,12 2,12" /></svg
          >
          Related Events Context
        </h4>
        <div class="events-timeline">
          {#each log.relatedEvents as event}
            <div class="timeline-item">
              <span class="timeline-dot" class:current={event.current}></span>
              <div class="timeline-content">
                <span class="timeline-time">{event.time}</span>
                <span class="timeline-desc">{event.description}</span>
              </div>
            </div>
          {/each}
        </div>
      </div>
    {/if}
  </aside>
{/if}

<style>
  .log-detail {
    width: 380px;
    min-width: 380px;
    height: 100%;
    background: var(--surface-container);
    border-left: 1px solid var(--panel-border);
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: var(--space-md);
    padding: var(--space-lg);
  }

  .detail-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .detail-title {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
    font: var(--text-title-sm);
    color: var(--on-surface);
  }

  .close-btn {
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: var(--radius-sm);
    color: var(--outline);
    transition: background var(--transition-fast);
  }

  .close-btn:hover {
    background: var(--surface-container-highest);
  }

  .detail-badge-row {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
  }

  .log-id {
    font: var(--text-mono-code);
    color: var(--on-surface-variant);
  }

  .meta-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-md);
    padding: var(--space-md);
    background: var(--surface-container-low);
    border-radius: var(--radius-default);
    border: 1px solid var(--panel-border);
  }

  .meta-label {
    font: var(--text-mono-label);
    color: var(--outline);
    display: block;
    margin-bottom: 4px;
  }

  .meta-value {
    font: var(--text-body-md);
    color: var(--on-surface);
    display: block;
  }

  .meta-value.mono {
    font: var(--text-mono-code);
  }

  .meta-badges {
    display: flex;
    gap: var(--space-sm);
  }

  .meta-badge {
    flex: 1;
    display: flex;
    flex-direction: column;
    padding: var(--space-sm) var(--space-md);
    background: var(--surface-container-lowest);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-sm);
    gap: 4px;
  }

  .badge-label {
    font: var(--text-mono-label);
    color: var(--outline);
    text-transform: uppercase;
  }

  .badge-value {
    font: var(--text-mono-code);
    color: var(--primary);
  }

  .detail-section h4 {
    font: var(--text-body-md);
    font-weight: 600;
    color: var(--on-surface);
    display: flex;
    align-items: center;
    gap: var(--space-sm);
    margin-bottom: var(--space-sm);
  }

  .section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: var(--space-sm);
  }

  .section-header h4 {
    margin-bottom: 0;
  }

  .copy-btn {
    display: flex;
    align-items: center;
    gap: 4px;
    font: var(--text-mono-label);
    color: var(--primary);
    padding: 4px 8px;
    border-radius: var(--radius-sm);
    transition: background var(--transition-fast);
  }

  .copy-btn:hover {
    background: rgba(192, 193, 255, 0.1);
  }

  .raw-message {
    font: var(--text-mono-code);
    padding: var(--space-md);
    background: var(--surface-container-low);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-default);
    color: var(--tertiary);
    white-space: pre-wrap;
    word-break: break-all;
    overflow-x: auto;
  }

  .json-payload {
    font: var(--text-mono-code);
    padding: var(--space-md);
    background: var(--surface-container-lowest);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-default);
    color: var(--tertiary);
    white-space: pre-wrap;
    word-break: break-all;
    overflow-x: auto;
    max-height: 300px;
  }

  .events-timeline {
    display: flex;
    flex-direction: column;
    gap: var(--space-md);
    padding-left: var(--space-sm);
  }

  .timeline-item {
    display: flex;
    gap: var(--space-sm);
    align-items: flex-start;
  }

  .timeline-dot {
    width: 10px;
    height: 10px;
    border-radius: 50%;
    background: var(--outline-variant);
    margin-top: 4px;
    flex-shrink: 0;
  }

  .timeline-dot.current {
    background: var(--primary);
  }

  .timeline-content {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .timeline-time {
    font: var(--text-mono-label);
    color: var(--outline);
  }

  .timeline-desc {
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
  }

  .detail-actions {
    display: flex;
    gap: var(--space-sm);
    margin-top: auto;
    padding-top: var(--space-md);
    border-top: 1px solid var(--panel-border);
  }

  .action-btn {
    flex: 1;
    padding: 10px 16px;
    border-radius: var(--radius-md);
    font: var(--text-body-md);
    font-weight: 600;
    text-align: center;
    transition: background var(--transition-fast);
  }

  .action-btn.ghost {
    background: var(--surface-container-highest);
    color: var(--on-surface);
  }

  .action-btn.ghost:hover {
    background: var(--surface-bright);
  }

  .action-btn.primary {
    background: var(--primary-container);
    color: #fff;
  }

  .action-btn.primary:hover {
    opacity: 0.9;
  }
</style>
