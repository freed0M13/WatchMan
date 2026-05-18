<script>
  import { createEventDispatcher } from "svelte";

  const dispatch = createEventDispatcher();

  let username = "";
  let password = "";
  let rememberMe = false;
  let isLoading = false;

  let errorMessage = "";

  async function handleLogin() {
    if (!username || !password) return;
    isLoading = true;
    errorMessage = "";

    try {
      const response = await fetch("http://localhost:5000/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Username: username, Password: password })
      });

      if (response.ok) {
        const data = await response.json();
        const rawRole = data.role || data.Role || 'READ_ONLY';
        localStorage.setItem("userRole", rawRole.toUpperCase());
        localStorage.setItem("userName", data.username || data.Username || username);
        window.location.href = '/';
      } else {
        errorMessage = "Geçersiz şifre veya kullanıcı adı";
      }
    } catch (err) {
      errorMessage = "Bağlantı hatası";
    } finally {
      isLoading = false;
    }
  }
</script>

<div class="login-page">
  <div class="login-card">
    <!-- Shield Icon -->
    <div class="login-icon">
      <svg
        width="36"
        height="36"
        viewBox="0 0 24 24"
        fill="none"
        stroke="var(--primary)"
        stroke-width="1.5"
      >
        <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
        <rect
          x="9"
          y="9"
          width="6"
          height="7"
          rx="1"
          stroke="var(--primary)"
          stroke-width="1.5"
        />
        <circle cx="12" cy="13" r="1" fill="var(--primary)" />
        <line
          x1="12"
          y1="14"
          x2="12"
          y2="15"
          stroke="var(--primary)"
          stroke-width="1.5"
        />
      </svg>
    </div>

    <!-- Header -->
    <h1 class="login-title">Welcome back</h1>
    <p class="login-subtitle">Sign in to WatchMan</p>

    <!-- Form -->
    <form class="login-form" on:submit|preventDefault={handleLogin}>
      {#if errorMessage}
        <div class="error-msg">{errorMessage}</div>
      {/if}

      <div class="field">
          <label class="field-label" for="login-username">USERNAME</label>
          <div class="input-wrapper">
            <svg
              class="input-icon"
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
            >
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
              <circle cx="12" cy="7" r="4" />
            </svg>
            <input
              id="login-username"
              type="text"
              placeholder="admin_user"
              bind:value={username}
              autocomplete="username"
            />
          </div>
        </div>

        <div class="field">
          <label class="field-label" for="login-password">PASSWORD</label>
          <div class="input-wrapper">
            <svg
              class="input-icon"
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
            >
              <path d="M2 12s3 3 5 3 5-6 5-6" />
              <path d="M22 12s-3-3-5-3-5 6-5 6" />
            </svg>
            <input
              id="login-password"
              type="password"
              placeholder="••••••••"
              bind:value={password}
              autocomplete="current-password"
            />
          </div>
        </div>

        <div class="login-options">
          <label class="remember-label">
            <input type="checkbox" bind:checked={rememberMe} />
            <span class="custom-check">
              {#if rememberMe}✓{/if}
            </span>
            Remember me
          </label>
          <button type="button" class="forgot-link">Forgot password?</button>
        </div>

        <button type="submit" class="login-btn" disabled={isLoading}>
          {#if isLoading}
            Signing in...
          {:else}
            Sign In
            <svg
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
            >
              <path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4" />
              <polyline points="10,17 15,12 10,7" />
              <line x1="15" y1="12" x2="3" y2="12" />
            </svg>
          {/if}
        </button>
      </form>

    <!-- Footer -->
    <p class="login-footer">
      <svg
        width="12"
        height="12"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        stroke-width="2"
      >
        <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
      </svg>
      Protected by 2FA & End-to-End Encryption
    </p>
  </div>
</div>

<style>
  .login-page {
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--background);
    padding: var(--space-lg);
  }

  .login-card {
    width: 100%;
    max-width: 420px;
    background: var(--surface-container);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-lg);
    padding: var(--space-2xl) var(--space-xl);
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-sm);
    animation: fade-in 0.4s ease;
  }

  .login-icon {
    width: 64px;
    height: 64px;
    border-radius: 50%;
    background: var(--surface-container-highest);
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: var(--space-sm);
  }

  .login-title {
    font: var(--text-headline-md);
    color: var(--on-surface);
    text-align: center;
  }

  .login-subtitle {
    font: var(--text-body-md);
    color: var(--on-surface-variant);
    text-align: center;
    margin-bottom: var(--space-md);
  }

  .login-form {
    width: 100%;
    display: flex;
    flex-direction: column;
    gap: var(--space-md);
  }

  .error-msg {
    color: var(--error);
    background: rgba(239, 68, 68, 0.1);
    border: 1px solid rgba(239, 68, 68, 0.2);
    padding: 10px;
    border-radius: var(--radius-sm);
    font: var(--text-body-sm);
    text-align: center;
  }

  .field {
    display: flex;
    flex-direction: column;
    gap: var(--space-xs);
  }

  .field-label {
    font: var(--text-mono-label);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: var(--on-surface-variant);
  }

  .input-wrapper {
    position: relative;
    display: flex;
    align-items: center;
  }

  .input-icon {
    position: absolute;
    left: 12px;
    color: var(--outline);
    pointer-events: none;
  }

  .input-wrapper input {
    width: 100%;
    padding: 10px 12px 10px 38px;
    background: var(--surface-container-lowest);
    border: 1px solid var(--panel-border);
    border-radius: var(--radius-md);
    color: var(--on-surface);
    font-size: 14px;
  }

  .input-wrapper input:focus {
    border-color: var(--primary);
    box-shadow: inset 0 0 0 1px rgba(192, 193, 255, 0.15);
    outline: none;
  }

  .login-options {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .remember-label {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
    cursor: pointer;
  }

  .remember-label input[type="checkbox"] {
    display: none;
  }

  .custom-check {
    width: 16px;
    height: 16px;
    border: 1px solid var(--outline-variant);
    border-radius: 3px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: 10px;
    color: var(--primary);
    transition:
      background var(--transition-fast),
      border-color var(--transition-fast);
  }

  .remember-label input:checked ~ .custom-check {
    border-color: var(--primary);
    background: rgba(192, 193, 255, 0.1);
  }

  .forgot-link {
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
    transition: color var(--transition-fast);
  }

  .forgot-link:hover,
  .back-link:hover {
    color: var(--primary);
  }

  .back-link {
    font: var(--text-body-sm);
    color: var(--on-surface-variant);
    background: none;
    border: none;
    cursor: pointer;
    text-align: center;
    width: 100%;
    margin-top: var(--space-sm);
  }

  .field-hint {
    font: var(--text-body-sm);
    color: var(--outline);
    margin-top: 4px;
    text-align: center;
  }

  .login-btn {
    width: 100%;
    padding: 12px;
    background: var(--primary-container);
    color: #fff;
    border: none;
    border-radius: var(--radius-md);
    font: var(--text-body-md);
    font-weight: 600;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-sm);
    transition:
      opacity var(--transition-fast),
      transform var(--transition-fast);
  }

  .login-btn:hover:not(:disabled) {
    opacity: 0.9;
  }

  .login-btn:active:not(:disabled) {
    transform: scale(0.98);
  }

  .login-btn:disabled {
    opacity: 0.6;
    cursor: wait;
  }

  .login-footer {
    font: var(--text-mono-label);
    color: var(--outline);
    display: flex;
    align-items: center;
    gap: 6px;
    margin-top: var(--space-md);
  }
</style>
