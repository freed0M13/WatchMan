<!--
  App.svelte
  Root component — handles page routing and layout orchestration.
-->
<script>
  import MainLayout from "./components/layout/MainLayout.svelte";
  import Login from "./pages/Login.svelte";
  import DashboardOverview from "./pages/DashboardOverview.svelte";
  import NetworkAnalysis from "./pages/NetworkAnalysis.svelte";
  import SystemAnalysis from "./pages/SystemAnalysis.svelte";
  import SystemLogs from "./pages/SystemLogs.svelte";
  import UserManagement from "./pages/UserManagement.svelte";
  //import RulesWhitelist from './pages/RulesWhitelist.svelte';
  import Settings from "./pages/Settings.svelte";
  import SessionTimeoutModal from "./components/ui/SessionTimeoutModal.svelte";
  import CommandPalette from "./components/ui/CommandPalette.svelte";
  import {
    user,
    isAuthenticated,
    login,
    logout,
    showTimeoutModal,
    resetSessionTimeout,
  } from "./lib/auth.js";
  import { t, currentLanguage } from "./lib/i18n.js";
  import { setupShortcuts } from "./lib/shortcuts.js";
  import { onMount } from "svelte";

  let currentPage = "dashboard";
  let isLoggedIn = false;

  /** @type {Record<string, string>} */
  const searchPlaceholders = {
    dashboard: "Search logs, IPs...",
    network: "Search IP, Hash, CVE...",
    system: "Search systems, processes, metrics...",
    logs: "Global search (Ctrl+K)...",
    users: "Search users, roles, or audit events...",
    rules: "Search rules, IPs, policies...",
    settings: "Search resources...",
  };

  /** @type {Record<string, string>} */
  const shortcutHints = {
    logs: "⌘ + K",
    users: "⌘ + K",
  };

  // Role-Based Access Control (RBAC) Guard
  /** @param {CustomEvent<string>} event */
  function handleNavigate(event) {
    const targetPage = event.detail;
    const currentRole = (localStorage.getItem("userRole") || "READ_ONLY").toUpperCase();

    if (targetPage === "users" && currentRole !== "SYS_ADMIN") {
      currentPage = "dashboard";
      return;
    }

    currentPage = targetPage;
  }

  // Handle browser routing mock
  onMount(() => {
    const savedRole = localStorage.getItem("userRole");
    if (savedRole) {
      isLoggedIn = true;
      login({ name: localStorage.getItem("userName") || "User", role: savedRole });
    }

    const checkHash = () => {
      const hash = window.location.hash.replace("#", "");
      if (hash === "status") {
        currentPage = "status";
      } else if (hash) {
        currentPage = hash;
      }
    };
    checkHash();
    window.addEventListener("hashchange", checkHash);

    return () => {
      window.removeEventListener("hashchange", checkHash);
    };
  });

  /** @param {CustomEvent<{username: string}>} event */
  function handleLogin(event) {
    const { username } = event.detail;
    let role = "L3 Analyst";
    if (username === "admin" || username === "admin_user") role = "SYS_ADMIN";
    else if (username === "viewer") role = "VIEWER";

    login({
      name: username,
      role: role,
    });
    currentPage = "dashboard";
  }

  function handleActivity() {
    if ($isAuthenticated) {
      resetSessionTimeout();
    }
  }

  function handleLogout() {
    logout();
  }

  onMount(() => {
    return setupShortcuts((/** @type {string} */ page) =>
      handleNavigate(/** @type {any} */ ({ detail: page })),
    );
  });
</script>

<svelte:window
  on:mousemove={handleActivity}
  on:keydown={handleActivity}
  on:scroll={handleActivity}
/>

{#if !isLoggedIn && !$isAuthenticated}
  <Login on:login={handleLogin} />
{:else}
  <MainLayout
    {currentPage}
    searchPlaceholder={$t("search") !== "search"
      ? $t("search")
      : (searchPlaceholders[currentPage] ?? "Search...")}
    shortcutHint={shortcutHints[currentPage] ?? ""}
    user={$user || { name: "", role: "" }}
    notificationCount={currentPage === "network" ? 3 : 0}
    on:navigate={handleNavigate}
  >
    {#if currentPage === "dashboard"}
      <DashboardOverview />
    {:else if currentPage === "network"}
      <NetworkAnalysis />
    {:else if currentPage === "system"}
      <SystemAnalysis />
    {:else if currentPage === "logs"}
      <SystemLogs />
    {:else if currentPage === "users"}
      <UserManagement />
    {:else if currentPage === "rules"}
      <RulesWhitelist />
    {:else if currentPage === "settings"}
      <Settings />
    {/if}
  </MainLayout>

  <SessionTimeoutModal
    isOpen={$showTimeoutModal}
    on:extend={() => {
      $showTimeoutModal = false;
      resetSessionTimeout();
    }}
    on:logout={handleLogout}
  />
  <CommandPalette on:navigate={handleNavigate} />
{/if}
