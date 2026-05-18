/** @param {(page: string) => void} onNavigate */
export function setupShortcuts(onNavigate) {
  if (typeof window === 'undefined') return () => {};

  /** @type {string | null} */
  let lastKey = null;
  let lastKeyTime = 0;

  /** @param {KeyboardEvent} e */
  function handleKeydown(e) {
    // Ignore input fields
    const target = /** @type {HTMLElement} */ (e.target);
    if (target && (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA')) return;

    const currentKey = e.key.toLowerCase();
    const currentTime = new Date().getTime();

    if (lastKey === 'g' && (currentTime - lastKeyTime < 1000)) {
      if (currentKey === 'd') onNavigate('dashboard');
      else if (currentKey === 'n') onNavigate('network');
      else if (currentKey === 's') onNavigate('system');
      else if (currentKey === 'l') onNavigate('logs');
      else if (currentKey === 'u') onNavigate('users');
      else if (currentKey === 'r') onNavigate('rules');
      else if (currentKey === ',') onNavigate('settings');
      
      lastKey = null;
      return;
    }

    lastKey = currentKey;
    lastKeyTime = currentTime;
  }

  window.addEventListener('keydown', handleKeydown);
  
  return () => {
    window.removeEventListener('keydown', handleKeydown);
  };
}
