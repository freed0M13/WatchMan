<!--
  TopBar.svelte
  Top navigation bar with search, notifications, and action icons.
  Props: searchPlaceholder, notificationCount
-->
<script>
  import SearchInput from "../shared/SearchInput.svelte";
  import { theme } from "../../lib/theme.js";

  /** @type {string} */
  export let searchPlaceholder = "Search logs, IPs...";
  /** @type {number} */
  export let notificationCount = 0;
  /** @type {string} */
  export let shortcutHint = "";
  /** @type {{ name: string, role: string }} */
  export let user = { name: "S. Analyst", role: "ID: 8492-AX" };

  let isConnected = true;

  function handleLogout() {
    localStorage.clear();
    window.location.href = '/';
  }
</script>

<header class="topbar">
  <div class="topbar-search">
    <SearchInput placeholder={searchPlaceholder} {shortcutHint} />
  </div>

  <div class="topbar-actions">
    <!-- Connection Status -->
    <div
      class="connection-status"
      title={isConnected ? "Connected" : "Disconnected"}
    >
      <span class="status-dot" class:live={isConnected}></span>
      <span class="status-text">{isConnected ? "Live" : "Disconnected"}</span>
    </div>
    <!-- User Profile & Logout -->
    <div class="user-profile">
      <div class="user-avatar">{user.name.charAt(0)}</div>
      <div class="user-info">
        <span class="user-name">{user.name}</span>
        <button class="logout-btn" on:click={handleLogout}>Logout</button>
      </div>
    </div>
  </div>
</header>

<style>
  .topbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    height: var(--topbar-height);
    padding: 0 var(--container-padding);
    background: var(--surface);
    border-bottom: 1px solid var(--panel-border);
    position: sticky;
    top: 0;
    z-index: 50;
  }

  .topbar-search {
    flex: 1;
    max-width: 480px;
  }

  .topbar-actions {
    display: flex;
    align-items: center;
    gap: var(--space-xs);
  }

  .topbar-btn {
    position: relative;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 40px;
    border-radius: var(--radius-default);
    color: var(--on-surface-variant);
    transition:
      background var(--transition-fast),
      color var(--transition-fast);
  }

  .topbar-btn:hover {
    background: var(--surface-container);
    color: var(--on-surface);
  }

  .notification-badge {
    position: absolute;
    top: 6px;
    right: 6px;
    min-width: 16px;
    height: 16px;
    padding: 0 4px;
    background: var(--error);
    color: #fff;
    font-size: 10px;
    font-weight: 700;
    border-radius: var(--radius-full);
    display: flex;
    align-items: center;
    justify-content: center;
  }

  /* Connection Status */
  .connection-status {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 4px 10px;
    border-radius: var(--radius-full);
    background: var(--surface-container);
    margin-right: var(--space-md);
  }

  .status-dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: var(--error);
  }

  .status-dot.live {
    background: var(--success);
    box-shadow: 0 0 8px var(--success);
    animation: pulse-glow 2s infinite;
  }

  .status-text {
    font: var(--text-mono-label);
    color: var(--on-surface-variant);
  }

  /* Notification Dropdown */
  .notification-wrapper {
    position: relative;
  }

  .notification-dropdown {
    position: absolute;
    top: 100%;
    right: 0;
    margin-top: 8px;
    width: 320px;
    background: var(--panel-bg);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-lg);
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
    display: flex;
    flex-direction: column;
    overflow: hidden;
  }

  .dropdown-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: var(--space-md);
    border-bottom: 1px solid var(--panel-border);
  }

  .dropdown-header h4 {
    font: var(--text-title-sm);
  }

  .text-btn {
    font: var(--text-body-sm);
    color: var(--primary);
  }

  .text-btn:hover {
    text-decoration: underline;
  }

  .dropdown-body {
    max-height: 300px;
    overflow-y: auto;
  }

  .notif-item {
    display: flex;
    gap: var(--space-md);
    padding: var(--space-md);
    border-bottom: 1px solid var(--panel-border);
    background: var(--surface-container-lowest);
    transition: background var(--transition-fast);
  }

  .notif-item:hover {
    background: var(--surface-container-low);
  }

  .notif-dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: var(--critical);
    margin-top: 6px;
    flex-shrink: 0;
  }

  .notif-content h5 {
    font: var(--text-body-md);
    font-weight: 600;
    margin-bottom: 2px;
  }

  .notif-content p {
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
    margin-bottom: 4px;
    line-height: 1.4;
  }

  .notif-time {
    font: var(--text-mono-label);
    color: var(--outline);
  }

  /* User Profile */
  .user-profile {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
    margin-left: var(--space-md);
    padding-left: var(--space-md);
    border-left: 1px solid var(--panel-border);
  }

  .user-avatar {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    background: var(--surface-container-highest);
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 600;
    font-size: 14px;
    color: var(--primary);
  }

  .user-info {
    display: flex;
    flex-direction: column;
  }

  .user-name {
    font: var(--text-body-sm);
    font-weight: 600;
  }

  .logout-btn {
    font: var(--text-body-sm);
    color: var(--outline);
    text-align: left;
    padding: 0;
  }

  .logout-btn:hover {
    color: var(--error);
  }
</style>
