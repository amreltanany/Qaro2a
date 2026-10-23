/* scroll-3d hero: extraY = scrollY * (1 - speed). Background 0.8x, product 0.84x plus a scale-up. */
(function () {
    'use strict';
    var stage = document.getElementById('hero3d');
    if (!stage) return;
    var bg = stage.querySelector('.hero3d__bg');
    var product = stage.querySelector('.hero3d__product');
    if (!bg || !product) return;

    function update() {
        var h = stage.offsetHeight;
        var y = Math.min(Math.max(window.scrollY || 0, 0), h);
        var p = h ? y / h : 0;
        bg.style.setProperty('--y', (y * 0.2).toFixed(1) + 'px');
        product.style.setProperty('--y', (y * 0.16).toFixed(1) + 'px');
        product.style.setProperty('--s', (1 + 0.1 * (1 - Math.pow(1 - p, 2))).toFixed(4));
    }

    // Update synchronously in the scroll handler (rAF lags behind in throttled panes).
    window.addEventListener('scroll', update, { passive: true });
    window.addEventListener('resize', update);
    update();
})();
