import { writable } from 'svelte/store';

function getInitialTheme() {
    if (typeof localStorage !== 'undefined' && localStorage.getItem('theme')) {
        return localStorage.getItem('theme');
    }
    return 'system';
}

export const theme = writable(getInitialTheme());
/**
 * @param {string} value
 */
export function applyTheme(value) {
    if (typeof document === 'undefined') return;
    
    let activeTheme = value;
    if (value === 'system') {
        activeTheme = window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
    }

    if (activeTheme === 'light') {
        document.documentElement.setAttribute('data-theme', 'light');
    } else {
        document.documentElement.removeAttribute('data-theme');
    }
}

if (typeof window !== 'undefined') {
    theme.subscribe(value => {
        localStorage.setItem('theme', value || 'light');
        applyTheme(value || 'light');
    });

    // Listen for system theme changes
    window.matchMedia('(prefers-color-scheme: light)').addEventListener('change', () => {
        theme.subscribe(val => {
            if (val === 'system') {
                applyTheme('system');
            }
        })();
    });
}
