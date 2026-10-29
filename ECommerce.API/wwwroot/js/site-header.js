/* Floating header behaviour: scroll states + mobile drawer. */
(function () {
    'use strict';
    var header = document.getElementById('siteHeader');
    if (!header) return;

    var toggle = header.querySelector('.site-header__toggle');
    var drawer = header.querySelector('.site-header__drawer');
    var root = document.documentElement;
    var lastY = window.scrollY || 0;
    var goingUp = false;

    function onScroll() {
        var y = window.scrollY || window.pageYOffset || 0;
        var scrolled = y > window.innerHeight * 0.4;
        if (y < lastY - 2) { goingUp = true; lastY = y; }
        else if (y > lastY + 2) { goingUp = false; lastY = y; }
        if (!scrolled) goingUp = false;
        header.classList.toggle('is-scrolled', scrolled);
        header.classList.toggle('is-up', scrolled && goingUp);
    }

    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();

    function setOpen(open) {
        if (!toggle) return;
        header.classList.toggle('is-open', open);
        toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
        root.classList.toggle('sh-lock', open);
        var lenis = window.__lenis;
        if (lenis) { open ? lenis.stop() : lenis.start(); }
    }

    if (toggle && drawer) {
        toggle.addEventListener('click', function () {
            setOpen(!header.classList.contains('is-open'));
        });
        drawer.addEventListener('click', function (e) {
            if (e.target === drawer || e.target.closest('a')) setOpen(false);
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && header.classList.contains('is-open')) {
                setOpen(false);
                toggle.focus();
            }
        });
        window.addEventListener('resize', function () {
            if (window.innerWidth > 860 && header.classList.contains('is-open')) setOpen(false);
        });
    }

    // Search popup (the legacy handler is bound to .navbar, which this header does not use)
    header.addEventListener('click', function (e) {
        var btn = e.target.closest('.search-button');
        if (!btn) return;
        e.preventDefault();
        var popup = document.querySelector('.search-popup');
        if (popup) popup.classList.toggle('is-visible');
        if (header.classList.contains('is-open')) setOpen(false);
    });

    // Cart count badge: hide at zero, and let other scripts refresh it (window.updateCartBadge).
    function paintBadge(el, n) {
        el.textContent = n;
        el.setAttribute('data-count', n);
    }
    document.querySelectorAll('[data-cart-count]').forEach(function (el) { paintBadge(el, parseInt(el.textContent, 10) || 0); });
    window.updateCartBadge = function (n) {
        document.querySelectorAll('[data-cart-count]').forEach(function (el) {
            paintBadge(el, n);
            el.classList.remove('is-pop'); void el.offsetWidth; el.classList.add('is-pop');
        });
    };
})();
