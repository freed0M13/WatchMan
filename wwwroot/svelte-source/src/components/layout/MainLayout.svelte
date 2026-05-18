<!--
  MainLayout.svelte
  Root layout wrapper: Sidebar + TopBar + scrollable content area.
  Props: currentPage, searchPlaceholder, shortcutHint, user, notificationCount
-->
<script>
  import Sidebar from './Sidebar.svelte';
  import TopBar from './TopBar.svelte';

  /** @type {string} */
  export let currentPage = 'dashboard';
  /** @type {string} */
  export let searchPlaceholder = 'Search logs, IPs...';
  /** @type {string} */
  export let shortcutHint = '';
  /** @type {{ name: string, role: string }} */
  export let user = { name: 'S. Analyst', role: 'ID: 8492-AX' };
  /** @type {number} */
  export let notificationCount = 0;

  let isSidebarCollapsed = false;

  function handleToggleCollapse() {
    isSidebarCollapsed = !isSidebarCollapsed;
  }
</script>

<div class="layout" data-sidebar-collapsed={isSidebarCollapsed} style="--sidebar-width: {isSidebarCollapsed ? '80px' : '220px'}">
  <Sidebar {currentPage} isCollapsed={isSidebarCollapsed} on:toggleCollapse={handleToggleCollapse} on:navigate />

  <div class="layout-main">
    <TopBar {searchPlaceholder} {shortcutHint} {notificationCount} {user} />

    <main class="layout-content">
      <slot />
    </main>
  </div>
</div>

<style>
  .layout {
    display: flex;
    min-height: 100vh;
  }

  .layout-main {
    flex: 1;
    margin-left: var(--sidebar-width);
    display: flex;
    flex-direction: column;
    min-height: 100vh;
    transition: margin-left var(--transition-normal);
  }

  .layout-content {
    flex: 1;
    padding: var(--container-padding);
    overflow-y: auto;
  }
</style>
