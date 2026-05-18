<!--
  StatusBadge.svelte
  Severity/status chip: CRITICAL, ERROR, WARN, INFO, SUCCESS, PERMANENT, etc.
  Props: level, label, pulse
-->
<script>
  /** @type {'critical'|'error'|'warn'|'info'|'success'|'permanent'|'inactive'} */
  export let level = 'info';
  /** @type {string} Custom label override */
  export let label = '';
  /** @type {boolean} Enable pulsing glow for critical */
  export let pulse = false;

  const defaults = {
    critical: 'CRITICAL',
    error:    'ERROR',
    warn:     'WARN',
    info:     'INFO',
    success:  'SUCCESS',
    permanent:'PERMANENT',
    inactive: 'INACTIVE'
  };

  $: displayLabel = label || defaults[level] || level.toUpperCase();
</script>

<span
  class="badge badge-{level}"
  class:pulse={pulse || level === 'critical'}
>
  <span class="badge-dot"></span>
  {displayLabel}
</span>

<style>
  .badge {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 3px 10px;
    border-radius: var(--radius-xs);
    font: var(--text-mono-label);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    white-space: nowrap;
    user-select: none;
  }

  .badge-dot {
    width: 6px;
    height: 6px;
    border-radius: 50%;
    flex-shrink: 0;
  }

  /* Critical */
  .badge-critical {
    background: rgba(239, 68, 68, 0.15);
    color: #fca5a5;
    border: 1px solid rgba(239, 68, 68, 0.3);
  }
  .badge-critical .badge-dot { background: #ef4444; }

  /* Error */
  .badge-error {
    background: rgba(239, 68, 68, 0.12);
    color: #fca5a5;
    border: 1px solid rgba(239, 68, 68, 0.25);
  }
  .badge-error .badge-dot { background: #ef4444; }

  /* Warn */
  .badge-warn {
    background: rgba(251, 191, 36, 0.12);
    color: #fde68a;
    border: 1px solid rgba(251, 191, 36, 0.25);
  }
  .badge-warn .badge-dot { background: #fbbf24; }

  /* Info */
  .badge-info {
    background: rgba(96, 165, 250, 0.12);
    color: #93c5fd;
    border: 1px solid rgba(96, 165, 250, 0.25);
  }
  .badge-info .badge-dot { background: #60a5fa; }

  /* Success */
  .badge-success {
    background: rgba(74, 222, 128, 0.12);
    color: #86efac;
    border: 1px solid rgba(74, 222, 128, 0.25);
  }
  .badge-success .badge-dot { background: #4ade80; }

  /* Permanent */
  .badge-permanent {
    background: rgba(74, 222, 128, 0.1);
    color: #86efac;
    border: 1px solid rgba(74, 222, 128, 0.2);
  }
  .badge-permanent .badge-dot { background: #4ade80; }

  /* Inactive */
  .badge-inactive {
    background: rgba(144, 143, 160, 0.1);
    color: var(--outline);
    border: 1px solid rgba(144, 143, 160, 0.2);
  }
  .badge-inactive .badge-dot { background: var(--outline); }

  /* Pulse animation */
  .pulse {
    animation: pulse-glow 2s ease-in-out infinite;
  }
</style>
