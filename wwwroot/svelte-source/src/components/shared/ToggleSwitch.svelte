<!--
  ToggleSwitch.svelte
  On/Off toggle for Rules mitigation modules and Settings.
  Props: checked, disabled, onChange
-->
<script>
  /** @type {boolean} */
  export let checked = false;
  /** @type {boolean} */
  export let disabled = false;

  function toggle() {
    if (!disabled) {
      checked = !checked;
    }
  }
</script>

<button
  class="toggle"
  class:checked
  class:disabled
  role="switch"
  aria-checked={checked}
  aria-label="Toggle setting"
  on:click={toggle}
>
  <span class="toggle-track">
    <span class="toggle-thumb"></span>
  </span>
</button>

<style>
  .toggle {
    display: inline-flex;
    padding: 2px;
    cursor: pointer;
  }

  .toggle.disabled {
    opacity: 0.4;
    cursor: not-allowed;
  }

  .toggle-track {
    width: 44px;
    height: 24px;
    background: var(--surface-container-highest);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-full);
    position: relative;
    transition: background var(--transition-fast), border-color var(--transition-fast);
  }

  .checked .toggle-track {
    background: var(--primary-container);
    border-color: var(--primary);
  }

  .toggle-thumb {
    position: absolute;
    top: 2px;
    left: 2px;
    width: 18px;
    height: 18px;
    background: var(--outline);
    border-radius: 50%;
    transition: transform var(--transition-fast), background var(--transition-fast);
  }

  .checked .toggle-thumb {
    transform: translateX(20px);
    background: var(--on-surface);
  }
</style>
