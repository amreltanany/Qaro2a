/* Draws each .arrow divider once, when it scrolls into view (or is scrolled past). */
(function () {
    'use strict';
    var arrows = Array.prototype.slice.call(document.querySelectorAll('.arrow'));
    if (!arrows.length) return;

    var root = document.documentElement;
    var waiting = [];

    function draw(a) {
        // Hold the drawing until the home intro loader (html.loading) is gone.
        if (root.classList.contains('loading')) {
            if (waiting.indexOf(a) < 0) waiting.push(a);
            return;
        }
        a.classList.add('is-drawn');
    }

    if (root.classList.contains('loading')) {
        new MutationObserver(function (_, obs) {
            if (root.classList.contains('loading')) return;
            obs.disconnect();
            waiting.forEach(function (a) { a.classList.add('is-drawn'); });
            waiting = [];
        }).observe(root, { attributes: true, attributeFilter: ['class'] });
    }

    if (!('IntersectionObserver' in window)) {
        arrows.forEach(draw);
        return;
    }

    var io = new IntersectionObserver(function (entries) {
        entries.forEach(function (e) {
            // boundingClientRect.top < 0: scrolled past it quickly, still end up drawn.
            if (e.isIntersecting || e.boundingClientRect.top < 0) {
                draw(e.target);
                io.unobserve(e.target);
            }
        });
    }, { threshold: 0.25 });
    arrows.forEach(function (a) { io.observe(a); });

    // IntersectionObserver reports nothing when a fast jump goes from "below the
    // viewport" straight to "above it", so also catch scrolled-past arrows here.
    function checkPast() {
        arrows = arrows.filter(function (a) {
            if (a.classList.contains('is-drawn') || waiting.indexOf(a) >= 0) return false;
            if (a.getBoundingClientRect().bottom < 0) { draw(a); io.unobserve(a); return false; }
            return true;
        });
        if (!arrows.length) window.removeEventListener('scroll', checkPast);
    }
    window.addEventListener('scroll', checkPast, { passive: true });
})();
