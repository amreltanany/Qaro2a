/* Top-rated books: an endless ticker that keeps gliding by itself, and the visitor can take over by swiping, dragging or scrolling.
   Several copies of the books sit in one row; the position is kept inside the middle copy, so it never reaches an end. */
(function () {
    'use strict';
    function init() {
        var slider = document.querySelector('.top-rated-slider');
        var track = slider && slider.querySelector('.top-rated-track');
        if (!track || track.getAttribute('data-ticker') === '1') return;
        track.setAttribute('data-ticker', '1');

        var items = Array.prototype.slice.call(track.children);
        var single = track.scrollWidth;                                   // width of one copy of the books
        var copies = 3 + Math.ceil(slider.clientWidth / Math.max(single, 1));
        for (var k = 1; k < copies; k++) {
            items.forEach(function (it) {
                var c = it.cloneNode(true);
                c.setAttribute('aria-hidden', 'true');
                c.querySelectorAll('a,button').forEach(function (x) { x.tabIndex = -1; });
                track.appendChild(c);
            });
        }

        var SPEED = 40;                                                   // pixels per second
        var rtl = getComputedStyle(slider).direction === 'rtl';
        var dir = rtl ? -1 : 1;                                           // scrollLeft is negative in right-to-left pages
        var half = function () { return track.scrollWidth / copies; };    // width of one copy
        var pos = 0, paused = false, touching = false, resumeTimer = null, last = null, dragging = null, settle = null;

        function current() { return Math.abs(slider.scrollLeft); }
        function place(v) { slider.scrollLeft = dir * v; }
        function wrap() {                                                 // keep the position inside the middle copy
            var h = half(), s = current();
            if (s >= 2 * h) place(s - h); else if (s < h) place(s + h);
        }
        function pause() { paused = true; clearTimeout(resumeTimer); }
        function resumeSoon(ms) {
            clearTimeout(resumeTimer);
            resumeTimer = setTimeout(function () { paused = false; wrap(); pos = current(); }, ms);
        }

        place(half());                                                    // start at the beginning of the middle copy
        pos = current();

        function frame(t) {
            if (last == null) last = t;
            var dt = Math.min((t - last) / 1000, 0.1); last = t;
            if (!paused && !touching && !dragging) {
                pos += SPEED * dt;
                var h = half();
                if (pos >= 2 * h) pos -= h;
                place(pos);
            } else {
                pos = current();                                          // follow the hand, so it resumes from where it was left
            }
            requestAnimationFrame(frame);
        }
        requestAnimationFrame(frame);

        // phone / tablet: finger down = stop; 2.5 s after the finger lifts it glides again
        slider.addEventListener('touchstart', function () { touching = true; pause(); }, { passive: true });
        slider.addEventListener('touchend', function () { touching = false; resumeSoon(2500); }, { passive: true });
        slider.addEventListener('touchcancel', function () { touching = false; resumeSoon(2500); }, { passive: true });
        // mouse wheel / trackpad
        slider.addEventListener('wheel', function () { pause(); resumeSoon(2500); }, { passive: true });
        // keyboard focus on a book
        slider.addEventListener('focusin', pause);
        slider.addEventListener('focusout', function () { resumeSoon(1500); });
        // mouse: hovering pauses (as before); dragging with the mouse scrolls it
        slider.addEventListener('mouseenter', pause);
        slider.addEventListener('mouseleave', function () { if (!dragging) resumeSoon(300); });
        slider.addEventListener('pointerdown', function (e) {
            if (e.pointerType !== 'mouse' || e.button !== 0) return;
            dragging = { x: e.clientX, left: current(), moved: false };
            pause();
        });
        window.addEventListener('pointermove', function (e) {
            if (!dragging) return;
            var dx = e.clientX - dragging.x;
            if (Math.abs(dx) > 5 && !dragging.moved) { dragging.moved = true; slider.classList.add('is-dragging'); }
            if (dragging.moved) place(dragging.left - dir * dx);
        });
        window.addEventListener('pointerup', function () {
            if (!dragging) return;
            var moved = dragging.moved; dragging = null;
            slider.classList.remove('is-dragging');
            if (moved) wrap();
            resumeSoon(moved ? 2500 : 300);
        });
        // after a hand scroll settles, jump back into the middle copy (the copies are identical, so nothing visibly moves)
        slider.addEventListener('scroll', function () {
            if (!paused && !touching && !dragging) return;
            clearTimeout(settle);
            settle = setTimeout(function () { if (!touching && !dragging) wrap(); }, 160);
        }, { passive: true });
        window.addEventListener('resize', function () { wrap(); pos = current(); });
    }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init); else init();
})();
