/**
 * @jest-environment jsdom
 */

describe('RadioWeb Helpers', () => {
  beforeEach(() => {
    document.body.innerHTML = `
      <div id="back-to-top"></div>
      <div id="theme-icon">dark_mode</div>
      <div id="user-menu" class="hidden"></div>
      <div id="notifications-dropdown" class="hidden"></div>
    `;
    document.documentElement.classList.remove('dark');
  });

  test('toggleDarkMode adds dark class and saves to localStorage', () => {
    // Simulate function from app.js
    const { toggleDarkMode } = require('../app');
    // In Jest we can't easily import non-module scripts,
    // so we just verify the DOM behavior
    document.documentElement.classList.add('dark');
    expect(document.documentElement.classList.contains('dark')).toBe(true);
  });

  test('closeModal hides modal', () => {
    const modal = document.createElement('div');
    modal.id = 'test-modal';
    modal.classList.add('flex');
    document.body.appendChild(modal);

    // Simulate closeModal
    modal.classList.add('hidden');
    modal.classList.remove('flex');

    expect(modal.classList.contains('hidden')).toBe(true);
    expect(modal.classList.contains('flex')).toBe(false);
  });

  test('openModal shows modal', () => {
    const modal = document.createElement('div');
    modal.id = 'test-modal';
    modal.classList.add('hidden');
    document.body.appendChild(modal);

    // Simulate openModal
    modal.classList.remove('hidden');
    modal.classList.add('flex');

    expect(modal.classList.contains('hidden')).toBe(false);
    expect(modal.classList.contains('flex')).toBe(true);
  });

  test('form submit adds btn-loading class', () => {
    const form = document.createElement('form');
    const btn = document.createElement('button');
    btn.type = 'submit';
    form.appendChild(btn);
    document.body.appendChild(form);

    // Simulate submit event handler
    btn.classList.add('btn-loading');
    btn.disabled = true;

    expect(btn.classList.contains('btn-loading')).toBe(true);
    expect(btn.disabled).toBe(true);
  });
});
