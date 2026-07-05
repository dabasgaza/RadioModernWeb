(function () {
  'use strict';

  if (typeof flatpickr === 'undefined') return;

  var arLocale = null;
  try { arLocale = flatpickr.l10ns.ar; } catch (e) {}

  document.addEventListener('DOMContentLoaded', function () {

    document.querySelectorAll('.datepicker:not([data-flatpickr])').forEach(function (el) {
      el.setAttribute('data-flatpickr', '1');
      flatpickr(el, {
        locale: arLocale || 'ar',
        rtl: true,
        dateFormat: 'Y-m-d',
        altInput: true,
        altFormat: 'j F Y',
        allowInput: true,
        disableMobile: true
      });
    });

    document.querySelectorAll('.timepicker:not([data-flatpickr])').forEach(function (el) {
      el.setAttribute('data-flatpickr', '1');
      flatpickr(el, {
        locale: arLocale || 'ar',
        rtl: true,
        enableTime: true,
        noCalendar: true,
        dateFormat: 'H:i',
        altInput: true,
        altFormat: 'h:i K',
        allowInput: true,
        disableMobile: true
      });
    });

    document.querySelectorAll('.datetimepicker:not([data-flatpickr])').forEach(function (el) {
      el.setAttribute('data-flatpickr', '1');
      flatpickr(el, {
        locale: arLocale || 'ar',
        rtl: true,
        enableTime: true,
        dateFormat: 'Y-m-dTH:i',
        altInput: true,
        altFormat: 'j F Y الساعة h:i K',
        allowInput: true,
        disableMobile: true
      });
    });
  });

  window.reinitDatepickers = function (container) {
    if (!container) return;
    container.querySelectorAll('.timepicker:not([data-flatpickr])').forEach(function (el) {
      el.setAttribute('data-flatpickr', '1');
      flatpickr(el, {
        locale: arLocale || 'ar',
        rtl: true,
        enableTime: true,
        noCalendar: true,
        dateFormat: 'H:i',
        altInput: true,
        altFormat: 'h:i K',
        allowInput: true,
        disableMobile: true
      });
    });
  };
})();
