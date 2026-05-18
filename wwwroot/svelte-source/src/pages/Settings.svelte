<!--
  Settings.svelte
  Settings page — Security Thresholds, System Logging & Backups configuration.
-->
<script>
  import GlassCard from "../components/shared/GlassCard.svelte";
  import ToggleSwitch from "../components/shared/ToggleSwitch.svelte";

  let dbBackupInterval = "Daily";
  let theme = "dark";

  let interfaces = [];
  let activeNetworkInterface = "0";
  let isLoading = true;
  let toastMessage = "";
  let userRole = "READ_ONLY";

  let original = { activeNetworkInterface, dbBackupInterval, theme };

  $: isDirty =
    activeNetworkInterface !== original.activeNetworkInterface ||
    dbBackupInterval !== original.dbBackupInterval ||
    theme !== original.theme;

  $: if (typeof document !== "undefined")
    document.documentElement.setAttribute("data-theme", theme);

  async function handleSave() {
    try {
      const response = await fetch("http://localhost:5000/api/settings", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          ActiveNetworkInterface: activeNetworkInterface,
          DbBackupInterval: dbBackupInterval,
          Theme: theme,
        }),
      });
      if (response.ok) {
        original = { activeNetworkInterface, dbBackupInterval, theme };
        toastMessage = "Settings saved successfully";
        setTimeout(() => (toastMessage = ""), 3000);
      } else {
        toastMessage = "Save failed";
        setTimeout(() => (toastMessage = ""), 3000);
      }
    } catch (err) {
      console.error(err);
      toastMessage = "Save error";
      setTimeout(() => (toastMessage = ""), 3000);
    }
  }

  function handleDiscard() {
    activeNetworkInterface = original.activeNetworkInterface;
    dbBackupInterval = original.dbBackupInterval;
    theme = original.theme;
  }

  // Warn on beforeunload
  import { onMount, onDestroy } from "svelte";

  /** @param {BeforeUnloadEvent} e */
  function handleBeforeUnload(e) {
    if (isDirty) {
      e.preventDefault();
      e.returnValue = "";
    }
  }

  onMount(async () => {
    userRole = (localStorage.getItem("userRole") || "READ_ONLY").toUpperCase();
    window.addEventListener("beforeunload", handleBeforeUnload);

    try {
      const interfacesRes = await fetch(
        "http://localhost:5000/api/settings/interfaces",
      );
      if (interfacesRes.ok) {
        interfaces = await interfacesRes.json();
      }

      const settingsRes = await fetch("http://localhost:5000/api/settings");
      if (settingsRes.ok) {
        const settings = await settingsRes.json();
        if (settings.ActiveNetworkInterface)
          activeNetworkInterface = settings.ActiveNetworkInterface;
        if (settings.DbBackupInterval)
          dbBackupInterval = settings.DbBackupInterval;
        if (settings.Theme) theme = settings.Theme;

        original = { activeNetworkInterface, dbBackupInterval, theme };
      }
    } catch (err) {
      console.error("API error:", err);
    } finally {
      isLoading = false;
    }
  });

  onDestroy(() => {
    window.removeEventListener("beforeunload", handleBeforeUnload);
  });

  const settingsTabs = [
    { id: "general", label: "General Settings", active: true },
  ];

  let activeTab = "general";
</script>

<div class="settings-page">
  <!-- Left Nav -->
  <aside class="settings-nav">
    <span class="nav-section-label">CONFIGURATION</span>
    {#each settingsTabs as tab}
      <button
        class="settings-tab"
        class:active={activeTab === tab.id}
        on:click={() => (activeTab = tab.id)}
      >
        {tab.label}
        {#if activeTab === tab.id}
          <svg
            width="12"
            height="12"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"><polyline points="9,18 15,12 9,6" /></svg
          >
        {/if}
      </button>
    {/each}
  </aside>

  <!-- Content Area -->
  <div class="settings-content">
    <div class="content-header">
      <h1>General Settings</h1>
      <p class="subtitle">System preferences and interface configurations.</p>
    </div>

    {#if isDirty}
      <div class="unsaved-banner animate-fade-in">
        <svg
          width="20"
          height="20"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          ><circle cx="12" cy="12" r="10" /><line
            x1="12"
            y1="8"
            x2="12"
            y2="12"
          /><line x1="12" y1="16" x2="12.01" y2="16" /></svg
        >
        <span
          >You have unsaved changes. Remember to save or discard before leaving.</span
        >
      </div>
    {/if}

    {#if isLoading}
      <div class="loading-skeleton">
        <div class="skeleton-spinner"></div>
        <div class="skeleton-text">
          Data is being retrieved from the server...
        </div>
      </div>
    {:else}
      <!-- System & Appearance -->
      <GlassCard
        title="System & Appearance"
        icon='<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14,2 14,8 20,8"/></svg>'
      >
        <div class="settings-list">
          {#if userRole === 'SYS_ADMIN'}
          <div class="setting-row">
            <div class="setting-info">
              <span class="setting-name">Active Network Interface</span>
              <span class="setting-desc"
                >The adapter used to monitor network traffic</span
              >
            </div>
            <div class="select-wrap">
              <select bind:value={activeNetworkInterface}>
                {#each interfaces as iface}
                  <option value={iface.id.toString()}
                    >{iface.description}</option
                  >
                {/each}
              </select>
              <svg
                class="sel-icon"
                width="12"
                height="12"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2"><polyline points="6,9 12,15 18,9" /></svg
              >
            </div>
          </div>

          <div class="setting-row">
            <div class="setting-info">
              <span class="setting-name">Database Backup Interval</span>
              <span class="setting-desc"
                >Set the database backup frequency.</span
              >
            </div>
            <div class="select-wrap">
              <select bind:value={dbBackupInterval}>
                <option value="Hourly">Hourly</option>
                <option value="Daily">Daily</option>
                <option value="Weekly">Weekly</option>
                <option value="Never">Never</option>
              </select>
              <svg
                class="sel-icon"
                width="12"
                height="12"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2"><polyline points="6,9 12,15 18,9" /></svg
              >
            </div>
          </div>
          {/if}

          <div class="setting-row">
            <div class="setting-info">
              <span class="setting-name">Interface Theme</span>
              <span class="setting-desc"
                >Change the appearance of the interface.</span
              >
            </div>
            <div class="select-wrap">
              <select bind:value={theme}>
                <option value="dark">Dark</option>
                <option value="light">Light</option>
              </select>
              <svg
                class="sel-icon"
                width="12"
                height="12"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2"><polyline points="6,9 12,15 18,9" /></svg
              >
            </div>
          </div>
        </div>
      </GlassCard>
    {/if}

    <!-- Action Buttons -->
    <div class="settings-actions">
      <button
        class="btn-ghost"
        on:click={handleDiscard}
        disabled={!isDirty || isLoading}>Discard</button
      >
      <button
        class="btn-primary"
        on:click={handleSave}
        disabled={!isDirty || isLoading}>Save Changes</button
      >
    </div>
  </div>
</div>

{#if toastMessage}
  <div class="toast animate-slide-up">
    <svg
      width="20"
      height="20"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"><polyline points="20 6 9 17 4 12"></polyline></svg
    >
    <span>{toastMessage}</span>
  </div>
{/if}

<style>
  .toast {
    position: fixed;
    bottom: var(--space-lg);
    right: var(--space-lg);
    background: var(--surface-container-high);
    border: 1px solid #10b981;
    color: #10b981;
    padding: var(--space-md) var(--space-lg);
    border-radius: var(--radius-md);
    display: flex;
    align-items: center;
    gap: var(--space-sm);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5);
    z-index: 1000;
    font: var(--text-body-md);
    font-weight: 600;
  }
  .animate-slide-up {
    animation: slideUp 0.3s ease-out;
  }
  @keyframes slideUp {
    from {
      transform: translateY(100%);
      opacity: 0;
    }
    to {
      transform: translateY(0);
      opacity: 1;
    }
  }

  .loading-skeleton {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 300px;
    gap: var(--space-md);
    color: var(--primary);
  }
  .skeleton-spinner {
    width: 40px;
    height: 40px;
    border: 3px solid rgba(0, 255, 170, 0.1);
    border-top-color: var(--primary);
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }
  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }
  .skeleton-text {
    font: var(--text-mono-label);
    color: var(--primary);
    letter-spacing: 0.05em;
    animation: pulse 1.5s infinite;
  }
  @keyframes pulse {
    0%,
    100% {
      opacity: 1;
    }
    50% {
      opacity: 0.5;
    }
  }
  .settings-page {
    display: flex;
    gap: 0;
    margin: calc(var(--container-padding) * -1);
    min-height: calc(100vh - var(--topbar-height));
  }

  /* Left Nav */
  .settings-nav {
    width: 240px;
    padding: var(--space-lg);
    border-right: 1px solid var(--panel-border);
    display: flex;
    flex-direction: column;
    flex-shrink: 0;
  }
  .nav-section-label {
    font: var(--text-mono-label);
    color: var(--outline);
    letter-spacing: 0.05em;
    text-transform: uppercase;
    margin-bottom: var(--space-md);
  }
  .settings-tab {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 10px var(--space-md);
    border-radius: var(--radius-default);
    font: var(--text-body-md);
    color: var(--on-surface-variant);
    text-align: left;
    width: 100%;
    transition:
      background var(--transition-fast),
      color var(--transition-fast);
  }
  .settings-tab:hover {
    background: var(--surface-container);
    color: var(--on-surface);
  }
  .settings-tab.active {
    background: var(--surface-container-high);
    color: var(--on-surface);
    font-weight: 600;
  }
  .nav-bottom {
    margin-top: auto;
    padding-top: var(--space-lg);
  }
  .about-link {
    font: var(--text-body-md);
    color: var(--on-surface-variant);
  }

  /* Content */
  .settings-content {
    flex: 1;
    padding: var(--container-padding);
    display: flex;
    flex-direction: column;
    gap: var(--space-lg);
  }
  .content-header h1 {
    font: var(--text-headline-md);
    color: var(--on-surface);
  }
  .subtitle {
    font: var(--text-body-md);
    color: var(--on-surface-variant);
    margin-top: 4px;
  }

  .live-tune-badge {
    font: var(--text-mono-label);
    padding: 4px 10px;
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-sm);
    color: var(--on-surface-variant);
  }

  .threshold-list,
  .settings-list {
    display: flex;
    flex-direction: column;
  }
  .threshold-row,
  .setting-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: var(--space-md) 0;
    border-bottom: 1px solid var(--panel-border-light);
  }
  .threshold-row:last-child,
  .setting-row:last-child {
    border-bottom: none;
  }
  .threshold-info,
  .setting-info {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }
  .threshold-name,
  .setting-name {
    font: var(--text-body-md);
    font-weight: 600;
    color: var(--on-surface);
    display: flex;
    align-items: center;
    gap: 6px;
  }
  .threshold-desc,
  .setting-desc {
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
  }
  .info-icon {
    font-size: 14px;
    color: var(--outline);
    cursor: help;
  }

  .threshold-input {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
  }
  .threshold-input input {
    width: 100px;
    padding: 8px 12px;
    background: var(--surface-container-lowest);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-md);
    color: var(--on-surface);
    font: var(--text-mono-code);
    text-align: right;
  }
  .threshold-input input:focus {
    border-color: var(--primary);
    outline: none;
  }
  .input-unit {
    font: var(--text-mono-label);
    color: var(--on-surface-variant);
  }

  .select-wrap {
    position: relative;
  }
  .select-wrap select {
    appearance: none;
    padding: 8px 32px 8px 12px;
    background: var(--surface-container-lowest);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-md);
    color: var(--on-surface);
    font: var(--text-body-md);
    min-width: 120px;
  }
  .select-wrap select:focus {
    border-color: var(--primary);
    outline: none;
  }
  .sel-icon {
    position: absolute;
    right: 10px;
    top: 50%;
    transform: translateY(-50%);
    pointer-events: none;
    color: var(--outline);
  }

  .settings-actions {
    display: flex;
    justify-content: flex-end;
    gap: var(--space-sm);
    padding-top: var(--space-md);
    border-top: 1px solid var(--panel-border);
  }
  .btn-ghost {
    padding: 10px 24px;
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-default);
    font: var(--text-body-md);
    font-weight: 600;
    color: var(--on-surface);
    cursor: pointer;
  }
  .btn-ghost:hover:not(:disabled) {
    background: var(--surface-container);
  }
  .btn-primary {
    padding: 10px 24px;
    background: var(--primary-container);
    color: #fff;
    border-radius: var(--radius-default);
    font: var(--text-body-md);
    font-weight: 600;
    cursor: pointer;
    border: none;
  }
  .btn-primary:hover:not(:disabled) {
    opacity: 0.9;
  }
  .btn-ghost:disabled,
  .btn-primary:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .unsaved-banner {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
    padding: var(--space-md);
    background: rgba(251, 191, 36, 0.1);
    border: 1px solid var(--warning);
    border-radius: var(--radius-md);
    color: var(--warning);
    font: var(--text-body-md);
    font-weight: 600;
  }

  @media (max-width: 900px) {
    .settings-page {
      flex-direction: column;
    }
    .settings-nav {
      width: 100%;
      border-right: none;
      border-bottom: 1px solid var(--panel-border);
    }
  }
</style>
