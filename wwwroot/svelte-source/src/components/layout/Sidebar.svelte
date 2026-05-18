<script>
  import { createEventDispatcher, onMount } from "svelte";

  const dispatch = createEventDispatcher();

  /** @type {string} */
  export let currentPage = "dashboard";

  /** @type {boolean} */
  export let isCollapsed = false;

  let userRole = "READ_ONLY";

  onMount(() => {
    // Sayfa yüklendiğinde kullanıcının rolünü alıyoruz
    userRole = (localStorage.getItem("userRole") || "READ_ONLY").toUpperCase();
  });

  const navItems = [
    {
      id: "dashboard",
      label: "Dashboard",
      icon: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>`,
    },
    {
      id: "network",
      label: "Network",
      icon: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="5" r="2"/><circle cx="5" cy="19" r="2"/><circle cx="19" cy="19" r="2"/><path d="M12 7v4M7.5 17.5L10.5 13M16.5 17.5L13.5 13"/><circle cx="12" cy="12" r="1.5"/></svg>`,
    },
    {
      id: "system",
      label: "System",
      icon: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="3" width="20" height="14" rx="2"/><path d="M8 21h8M12 17v4"/></svg>`,
    },
    {
      id: "logs",
      label: "Logs",
      icon: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14,2 14,8 20,8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>`,
    },
    {
      id: "users",
      label: "Users",
      icon: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>`,
    },
    {
      id: "settings",
      label: "Settings",
      icon: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/></svg>`,
    },
  ];

  /** @param {string} pageId */
  function navigate(pageId) {
    dispatch("navigate", pageId);
  }
</script>

<aside class="sidebar">
  <div class="sidebar-brand">
    <div class="brand-icon">
      <svg
        width="28"
        height="28"
        viewBox="0 0 24 24"
        fill="none"
        stroke="var(--primary)"
        stroke-width="2"
      >
        <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
        <path d="M9 12l2 2 4-4" stroke="var(--primary)" stroke-width="2" />
      </svg>
    </div>
    <div class="brand-text">
      <span class="brand-name">WatchMan</span>
      <span class="brand-sub">NIDS/HIDS PANEL</span>
    </div>
  </div>

  <nav class="sidebar-nav">
    {#each navItems as item}
      {#if item.id !== "users" || userRole === "SYS_ADMIN"}
        <button
          class="nav-item"
          class:active={currentPage === item.id}
          on:click={() => navigate(item.id)}
        >
          <span class="nav-icon">{@html item.icon}</span>
          <span class="nav-label">{item.label}</span>
        </button>
      {/if}
    {/each}
  </nav>

  <button
    class="collapse-btn"
    on:click={() => dispatch("toggleCollapse")}
    title="Toggle Sidebar"
  >
    {#if isCollapsed}
      <svg
        width="20"
        height="20"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        stroke-width="2"
        ><polyline points="13 17 18 12 13 7" /><polyline
          points="6 17 11 12 6 7"
        /></svg
      >
    {:else}
      <svg
        width="20"
        height="20"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        stroke-width="2"
        ><polyline points="11 17 6 12 11 7" /><polyline
          points="18 17 13 12 18 7"
        /></svg
      >
    {/if}
  </button>
</aside>

<style>
  .sidebar {
    width: var(--sidebar-width);
    height: 100vh;
    background: var(--surface-container-low);
    border-right: 1px solid var(--panel-border);
    display: flex;
    flex-direction: column;
    position: fixed;
    top: 0;
    left: 0;
    z-index: 100;
    overflow-y: auto;
    transition: width var(--transition-normal);
  }

  /* Brand */
  .sidebar-brand {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
    padding: var(--space-lg) var(--space-md);
  }

  .brand-icon {
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .brand-text {
    display: flex;
    flex-direction: column;
  }

  .brand-name {
    font: var(--text-title-sm);
    font-size: 18px;
    color: var(--on-surface);
    font-weight: 700;
  }

  .brand-sub {
    font: var(--text-mono-label);
    color: var(--outline);
    letter-spacing: 0.05em;
  }

  :global([data-sidebar-collapsed="true"]) .brand-text {
    display: none;
  }
  :global([data-sidebar-collapsed="true"]) .sidebar-brand {
    justify-content: center;
    padding: var(--space-lg) 0;
  }

  /* Navigation */
  .sidebar-nav {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 2px;
    padding: var(--space-sm) var(--space-sm);
  }

  .nav-item {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
    padding: 10px var(--space-md);
    border-radius: var(--radius-default);
    color: var(--on-surface-variant);
    font: var(--text-body-md);
    font-weight: 500;
    transition:
      background var(--transition-fast),
      color var(--transition-fast);
    width: 100%;
    text-align: left;
    background: transparent;
    border: none;
    cursor: pointer;
  }

  .nav-item:hover {
    background: var(--surface-container);
    color: var(--on-surface);
  }

  .nav-item.active {
    background: var(--primary-container);
    color: var(--on-primary-container);
    color: #fff;
  }

  .nav-icon {
    display: flex;
    align-items: center;
    flex-shrink: 0;
  }

  :global([data-sidebar-collapsed="true"]) .nav-label {
    display: none;
  }
  :global([data-sidebar-collapsed="true"]) .nav-item {
    justify-content: center;
    padding: 10px 0;
  }

  /* Collapse Toggle */
  .collapse-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: var(--space-md);
    border-top: 1px solid var(--panel-border);
    margin-top: auto;
    color: var(--outline);
    transition:
      color var(--transition-fast),
      background var(--transition-fast);
    background: transparent;
    border: none;
    border-top: 1px solid var(--panel-border);
    cursor: pointer;
    width: 100%;
  }

  .collapse-btn:hover {
    color: var(--on-surface);
    background: var(--surface-container);
  }
</style>
