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

    // Keep scroll locked while the home preloader is on screen.
    var preloader = document.querySelector('.preloader');
    if (lenis && preloader && !preloader.classList.contains('loaded')) {
        lenis.stop();
        new MutationObserver(function (_, obs) {
            if (preloader.classList.contains('loaded')) {
                if (!document.documentElement.classList.contains('sh-lock')) lenis.start();
                obs.disconnect();
            }
        }).observe(preloader, { attributes: true, attributeFilter: ['class'] });
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

    // Gentle scrubbed parallax on the hero image.
    // Animate the wrapper, not the <img>: the image already carries AOS (data-aos) transforms.
    var heroImg = document.querySelector('.home-hero > .w-100');
    if (heroImg) {
        gsap.fromTo(heroImg,
            { scale: 1.06, yPercent: 0 },
            { scale: 1.06, yPercent: 2.8, ease: 'none',
              scrollTrigger: { trigger: '.home-hero', start: 'top top', end: 'bottom top', scrub: true } });
    }

    window.addEventListener('load', function () { ScrollTrigger.refresh(); });
})();
