import { writable, derived } from 'svelte/store';

const translations = {
  en: {
    dashboard: 'Dashboard',
    network: 'Network',
    system: 'System Logs',
    users: 'Users',
    settings: 'Settings',
    logout: 'Logout',
    search: 'Search...',
    welcome: 'Welcome'
  },
  tr: {
    dashboard: 'Panel',
    network: 'Ağ Analizi',
    system: 'Sistem Logları',
    users: 'Kullanıcı Yönetimi',
    settings: 'Ayarlar',
    logout: 'Çıkış Yap',
    search: 'Ara...',
    welcome: 'Hoşgeldiniz'
  }
};

export const currentLanguage = writable('en');

export const t = derived(currentLanguage, ($lang) => (/** @type {string} */ key) => {
  return /** @type {any} */(translations)[$lang]?.[key] || key;
});
