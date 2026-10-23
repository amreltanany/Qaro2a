/* Lenis smooth scroll + GSAP scroll reveals. Fails soft if the CDN scripts are blocked. */
(function () {
    'use strict';

    var lenis = null;
    if (typeof Lenis !== 'undefined') {
        lenis = new Lenis({
            duration: 1.2,
            easing: function (t) { return Math.min(1, 1.001 - Math.pow(2, -10 * t)); }
        });
        window.__lenis = lenis;
        var raf = function (time) { lenis.raf(time); requestAnimationFrame(raf); };
        requestAnimationFrame(raf);
    }

    // Keep smooth scroll stopped while the home intro loader (html.loading) is on screen.
    var root = document.documentElement;
    if (lenis && root.classList.contains('loading')) {
        lenis.stop();
        new MutationObserver(function (_, obs) {
            if (!root.classList.contains('loading')) {
                if (!root.classList.contains('sh-lock')) lenis.start();
                obs.disconnect();
            }
        }).observe(root, { attributes: true, attributeFilter: ['class'] });
    }

    if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;
    gsap.registerPlugin(ScrollTrigger);

    // Clip-path reveal for below-the-fold blocks (plays once).
    gsap.utils.toArray('.home-publish-banner > a, .home-about-video').forEach(function (el) {
        gsap.fromTo(el,
            { clipPath: 'inset(0 0 100% 0)' },
            { clipPath: 'inset(0 0 0% 0)', duration: 1.4, ease: 'expo.out',
              scrollTrigger: { trigger: el, start: 'top 85%', once: true } });
    });

    window.addEventListener('load', function () { ScrollTrigger.refresh(); });
})();
