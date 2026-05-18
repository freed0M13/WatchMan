<!--
  UserManagement.svelte
  Identity Access Management page — personnel table + IAM audit trail.
-->
<script>
  import GlassCard from '../components/shared/GlassCard.svelte';
  import { onMount } from 'svelte';

  let users = [];
  let showProvisionModal = false;
  let provUsername = '';
  let provPassword = '';
  let provRole = 'READ_ONLY';
  let isLoading = false;

  let showPasswordModal = false;
  let selectedUserForPassword = '';
  let newPasswordForUser = '';

  async function fetchUsers() {
    try {
      const res = await fetch('http://localhost:5000/api/auth/users');
      if (res.ok) {
        users = await res.json();
      }
    } catch (err) {
      console.error('Error fetching users:', err);
    }
  }

  onMount(() => {
    fetchUsers();
  });

  async function provisionUser() {
    if (!provUsername || !provPassword) return;
    isLoading = true;
    try {
      const res = await fetch('http://localhost:5000/api/auth/users', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Username: provUsername, Password: provPassword, Role: provRole })
      });
      if (res.ok) {
        showProvisionModal = false;
        provUsername = '';
        provPassword = '';
        provRole = 'READ_ONLY';
        fetchUsers();
      } else {
        alert("Provisioning failed!");
      }
    } catch (err) {
      console.error(err);
    } finally {
      isLoading = false;
    }
  }

  async function deleteUser(username) {
    if (!confirm(`Are you sure you want to delete user ${username}?`)) return;
    try {
      const res = await fetch(`http://localhost:5000/api/auth/users/${username}`, { method: 'DELETE' });
      if (res.ok) fetchUsers();
      else alert("Delete failed!");
    } catch (err) {
      console.error(err);
    }
  }

  async function resetPassword() {
    if (!newPasswordForUser) return;
    isLoading = true;
    try {
      const res = await fetch('http://localhost:5000/api/auth/users/reset-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Username: selectedUserForPassword, NewPassword: newPasswordForUser })
      });
      if (res.ok) {
        showPasswordModal = false;
        selectedUserForPassword = '';
        newPasswordForUser = '';
      } else {
        alert("Password reset failed!");
      }
    } catch (err) {
      console.error(err);
    } finally {
      isLoading = false;
    }
  }
</script>

<div class="users-page">
  <div class="page-header">
    <div>
      <h1>Identity Access Management</h1>
      <p class="subtitle">Manage system users, roles, and review authentication logs.</p>
    </div>
    <button class="provision-btn" on:click={() => showProvisionModal = true}>
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="8.5" cy="7" r="4"/><line x1="20" y1="8" x2="20" y2="14"/><line x1="23" y1="11" x2="17" y2="11"/></svg>
      Provision User
    </button>
  </div>

  <div class="content-grid">
    <!-- Active Personnel Table -->
    <GlassCard title="Active Personnel" icon='<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>'>
      <svelte:fragment slot="actions">
        <button class="icon-btn" title="Filter"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="4" y1="21" x2="4" y2="14"/><line x1="4" y1="10" x2="4" y2="3"/><line x1="12" y1="21" x2="12" y2="12"/><line x1="12" y1="8" x2="12" y2="3"/><line x1="20" y1="21" x2="20" y2="16"/><line x1="20" y1="12" x2="20" y2="3"/></svg></button>
        <button class="icon-btn" title="Export"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7,10 12,15 17,10"/><line x1="12" y1="15" x2="12" y2="3"/></svg></button>
      </svelte:fragment>

      <table class="user-table">
        <thead><tr><th>STATUS</th><th>IDENTITY</th><th>ROLE AUTHORITY</th><th>LAST AUTH EVENT</th><th>ACT.</th></tr></thead>
        <tbody>
          {#each users as u}
            <tr>
              <td><span class="status-dot" class:online={u.status === 'online' || u.Status === 'online'} class:offline={u.status !== 'online' && u.Status !== 'online'}></span></td>
              <td class="identity-cell">
                <div class="user-avatar">{(u.Username || u.username || u.Name || u.name || '?').charAt(0).toUpperCase()}</div>
                <div class="user-info">
                  <span class="user-name">{u.Username || u.username || u.Name || u.name}</span>
                </div>
              </td>
              <td><span class="role-badge" style="border-color: var(--primary); color: var(--primary)">{u.Role || u.role}</span></td>
              <td><span class="mono">{u.LastAuth || u.lastAuth || 'Hiç giriş yapmadı'}</span></td>
              <td class="action-cell">
                <button class="icon-btn-sm" title="Reset Password" on:click={() => { showPasswordModal = true; selectedUserForPassword = u.Username || u.username || u.Name || u.name; }}>🔑</button>
                <button class="icon-btn-sm delete-btn" title="Delete User" on:click={() => deleteUser(u.Username || u.username || u.Name || u.name)}>🗑</button>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
      <div class="table-footer">
        <span>Showing 1-4 of 142 identities</span>
        <div class="pagination"><button class="page-btn">‹</button><button class="page-btn">›</button></div>
      </div>
    </GlassCard>

  </div>

  {#if showProvisionModal}
    <div class="modal-backdrop" on:click|self={() => showProvisionModal = false}>
      <div class="modal-card">
        <h2 class="modal-title">Provision New User</h2>
        <p class="modal-subtitle">Create a new identity and assign access roles.</p>
        
        <form class="modal-form" on:submit|preventDefault={provisionUser}>
          <div class="field">
            <label>USERNAME</label>
            <input type="text" bind:value={provUsername} placeholder="e.g. system_admin" required />
          </div>
          <div class="field">
            <label>PASSWORD</label>
            <input type="password" bind:value={provPassword} placeholder="••••••••" required />
          </div>
          <div class="field">
            <label>ROLE AUTHORITY</label>
            <select bind:value={provRole}>
              <option value="SYS_ADMIN">SYS_ADMIN</option>
              <option value="L1_ANALYST">L1_ANALYST</option>
              <option value="READ_ONLY">READ_ONLY</option>
            </select>
          </div>
          
          <div class="modal-actions">
            <button type="button" class="btn-ghost" on:click={() => showProvisionModal = false}>Cancel</button>
            <button type="submit" class="btn-primary" disabled={isLoading}>{isLoading ? 'Saving...' : 'Save'}</button>
          </div>
        </form>
      </div>
    </div>
  {/if}

  {#if showPasswordModal}
    <div class="modal-backdrop" on:click|self={() => showPasswordModal = false}>
      <div class="modal-card">
        <h2 class="modal-title">Reset Password</h2>
        <p class="modal-subtitle">Set a new password for {selectedUserForPassword}.</p>
        
        <form class="modal-form" on:submit|preventDefault={resetPassword}>
          <div class="field">
            <label>NEW PASSWORD</label>
            <input type="password" bind:value={newPasswordForUser} placeholder="••••••••" required />
          </div>
          
          <div class="modal-actions">
            <button type="button" class="btn-ghost" on:click={() => showPasswordModal = false}>Cancel</button>
            <button type="submit" class="btn-primary" disabled={isLoading}>{isLoading ? 'Saving...' : 'Şifreyi Güncelle'}</button>
          </div>
        </form>
      </div>
    </div>
  {/if}
</div>

<style>
  .users-page { display:flex; flex-direction:column; gap:var(--space-lg); }
  .page-header { display:flex; justify-content:space-between; align-items:flex-start; }
  .page-header h1 { font:var(--text-headline-md); color:var(--on-surface); }
  .subtitle { font:var(--text-body-md); color:var(--on-surface-variant); margin-top:4px; }
  .provision-btn { display:flex; align-items:center; gap:var(--space-sm); padding:10px 20px; background:var(--primary-container); color:#fff; border-radius:var(--radius-default); font:var(--text-body-md); font-weight:600; }
  .provision-btn:hover { opacity:0.9; }
  .content-grid { display:grid; grid-template-columns:1fr; gap:var(--gutter); align-items:start; }
  .icon-btn { width:32px; height:32px; display:flex; align-items:center; justify-content:center; border-radius:var(--radius-sm); color:var(--on-surface-variant); }
  .icon-btn:hover { background:var(--surface-container-highest); }
  .link-btn { font:var(--text-mono-label); color:var(--primary); cursor:pointer; }
  .user-table { width:100%; border-collapse:collapse; }
  .user-table th { font:var(--text-mono-label); color:var(--outline); text-align:left; padding:var(--space-sm) var(--space-md); border-bottom:1px solid var(--panel-border); text-transform:uppercase; letter-spacing:0.05em; }
  .user-table td { padding:var(--space-md); border-bottom:1px solid var(--panel-border-light); vertical-align:middle; }
  .status-dot { width:10px; height:10px; border-radius:50%; display:inline-block; }
  .status-dot.online { background:var(--success); }
  .status-dot.offline { background:var(--error); }
  .identity-cell { display:flex; align-items:center; gap:var(--space-sm); }
  .user-avatar { width:36px; height:36px; border-radius:50%; background:var(--surface-container-highest); display:flex; align-items:center; justify-content:center; font-weight:600; color:var(--primary); font-size:14px; flex-shrink:0; }
  .user-info { display:flex; flex-direction:column; }
  .user-name { font:var(--text-body-md); font-weight:600; color:var(--on-surface); }
  .user-email { font:var(--text-body-sm); color:var(--on-surface-variant); }
  .role-badge { font:var(--text-mono-label); padding:3px 8px; border:1px solid; border-radius:var(--radius-xs); }
  .mono { font:var(--text-mono-code); }
  .auth-cell { display:flex; flex-direction:column; gap:2px; }
  .auth-cell span { white-space:pre-line; }
  .ip-text { font-size:11px; color:var(--outline); }
  .action-cell { display:flex; gap: 8px; align-items:center; }
  .icon-btn-sm { font-size:16px; background:none; border:none; color:var(--outline); cursor:pointer; padding:4px; transition: transform 0.2s; }
  .icon-btn-sm:hover { transform: scale(1.1); }
  .delete-btn:hover { color: var(--error); }
  .table-footer { display:flex; justify-content:space-between; align-items:center; padding-top:var(--space-md); font:var(--text-body-sm); color:var(--on-surface-variant); }
  .pagination { display:flex; gap:var(--space-xs); }
  .page-btn { width:28px; height:28px; display:flex; align-items:center; justify-content:center; border:1px solid var(--panel-border); border-radius:var(--radius-sm); color:var(--on-surface-variant); cursor:pointer; }
  .page-btn:hover { background:var(--surface-container-highest); }
  .audit-list { display:flex; flex-direction:column; }
  @media(max-width:1200px) { .content-grid { grid-template-columns:1fr; } }

  .modal-backdrop { position:fixed; top:0; left:0; width:100%; height:100%; background:rgba(0,0,0,0.6); display:flex; align-items:center; justify-content:center; z-index:9999; backdrop-filter:blur(4px); }
  .modal-card { background:var(--surface-container); border:1px solid var(--panel-border); border-radius:var(--radius-lg); padding:var(--space-2xl); width:400px; box-shadow:0 10px 30px rgba(0,0,0,0.5); }
  .modal-title { font:var(--text-headline-md); color:var(--on-surface); margin-bottom:4px; }
  .modal-subtitle { font:var(--text-body-md); color:var(--on-surface-variant); margin-bottom:var(--space-xl); }
  .modal-form { display:flex; flex-direction:column; gap:var(--space-md); }
  .field { display:flex; flex-direction:column; gap:6px; }
  .field label { font:var(--text-mono-label); color:var(--on-surface-variant); letter-spacing:0.05em; }
  .field input, .field select { padding:10px 12px; background:var(--surface-container-lowest); border:1px solid var(--panel-border); border-radius:var(--radius-md); color:var(--on-surface); font:var(--text-body-md); width:100%; }
  .field input:focus, .field select:focus { border-color:var(--primary); outline:none; }
  .modal-actions { display:flex; justify-content:flex-end; gap:var(--space-sm); margin-top:var(--space-md); }
  .btn-ghost { padding:10px 20px; border:none; background:none; color:var(--on-surface); font-weight:600; cursor:pointer; }
  .btn-ghost:hover { background:var(--surface-container-highest); border-radius:var(--radius-default); }
  .btn-primary { padding:10px 20px; background:var(--primary-container); color:#fff; border:none; border-radius:var(--radius-default); font-weight:600; cursor:pointer; }
  .btn-primary:hover:not(:disabled) { opacity:0.9; }
</style>
