import { writable } from 'svelte/store';

export const user = writable(null);
export const isAuthenticated = writable(false);
export const showTimeoutModal = writable(false);

const SESSION_TIMEOUT_MS = 10 * 60 * 1000; // 10 minutes
/** @type {ReturnType<typeof setTimeout>} */
let timeoutTimer;
/** @param {any} userData */
export function login(userData) {
    user.set(userData);
    isAuthenticated.set(true);
    resetSessionTimeout();
}

export function logout() {
    user.set(null);
    isAuthenticated.set(false);
    showTimeoutModal.set(false);
    clearTimeout(timeoutTimer);
}

export function resetSessionTimeout() {
    let isAuth;
    isAuthenticated.subscribe(v => isAuth = v)();
    
    if (!isAuth) return;

    clearTimeout(timeoutTimer);
    timeoutTimer = setTimeout(() => {
        showTimeoutModal.set(true);
    }, SESSION_TIMEOUT_MS);
}
