/* Custom tooltips: replaces the browser's native `title` bubble on buttons and icon links. */
(function () {
    'use strict';
    var tip = document.createElement('div');
    tip.className = 'qt-tip';
    tip.setAttribute('role', 'tooltip');
    tip.innerHTML = '<span class="qt-tip__text"></span><i class="qt-tip__arrow"></i>';
    var textEl = tip.firstChild;
    var current = null;

    function mount() { if (!tip.parentNode) document.body.appendChild(tip); }

    function targetOf(el) {
        return el && el.closest ? el.closest('[title], [data-qt]') : null;
    }

    function show(el) {
        if (!el || el === current) return;
        // take the text out of `title` so the native bubble never appears
        if (el.hasAttribute('title')) {
            var t = el.getAttribute('title');
            if (t && t.trim()) { el.setAttribute('data-qt', t.trim()); }
            el.removeAttribute('title');
        }
        var text = el.getAttribute('data-qt');
        if (!text) return;
        mount();
        current = el;
        textEl.textContent = text;
        tip.classList.remove('is-below');
        tip.style.left = '0px'; tip.style.top = '0px';
        var r = el.getBoundingClientRect();
        var w = tip.offsetWidth, h = tip.offsetHeight;
        var gap = 10;
        var left = r.left + r.width / 2 - w / 2;
        var maxLeft = document.documentElement.clientWidth - w - 8;
        left = Math.max(8, Math.min(left, maxLeft));
        var top = r.top - h - gap;
        if (top < 8) { top = r.bottom + gap; tip.classList.add('is-below'); }
        tip.style.left = left + 'px';
        tip.style.top = top + 'px';
        tip.style.setProperty('--arrow-x', (r.left + r.width / 2 - left) + 'px');
        requestAnimationFrame(function () { tip.classList.add('is-visible'); });
    }

    function hide() {
        tip.classList.remove('is-visible');
        current = null;
    }

    document.addEventListener('mouseover', function (e) {
        if (e.pointerType === 'touch') return;
        var el = targetOf(e.target);
        if (!el) { hide(); return; }
        show(el);
    });
    document.addEventListener('mouseout', function (e) {
        if (!current) return;
        var to = e.relatedTarget;
        if (to && current.contains(to)) return;
        hide();
    });
    document.addEventListener('focusin', function (e) {
        var el = targetOf(e.target);
        if (el && e.target.matches && e.target.matches(':focus-visible')) show(el);
    });
    document.addEventListener('focusout', hide);
    document.addEventListener('mousedown', hide);
    window.addEventListener('scroll', hide, { passive: true });
    window.addEventListener('resize', hide);
})();
