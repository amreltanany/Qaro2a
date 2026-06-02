(function () {
    var preloader = document.querySelector('.preloader');
    if (!preloader) return;

    var player = document.getElementById('preloaderLottie');
    var hasHidden = false;
    var startAt = Date.now();
    var minVisibleMs = 1200;

    var animationInstance = null;

    function stopAnimationSafely() {
        try {
            if (animationInstance && typeof animationInstance.destroy === 'function') {
                animationInstance.destroy();
            }
        } catch (e) { }
    }

    function hidePreloader() {
        if (hasHidden) return;
        hasHidden = true;

        var elapsed = Date.now() - startAt;
        var delay = Math.max(0, minVisibleMs - elapsed);
        setTimeout(function () {
            stopAnimationSafely();
            if (preloader) preloader.classList.add('loaded');
        }, delay);
    }

    function initLottie() {
        if (!player) return;
        if (!window.lottie || typeof window.lottie.loadAnimation !== 'function') return;

        var src = player.getAttribute('data-src');
        if (!src) return;

        player.classList.add('is-ready');

        try {
            animationInstance = window.lottie.loadAnimation({
                container: player,
                renderer: 'svg',
                loop: true,
                autoplay: true,
                path: src
            });
            animationInstance.setSpeed(2.2);
        } catch (e) { }
    }

    // Ensure animation is attempted whenever scripts are ready.
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initLottie, { once: true });
    } else {
        initLottie();
    }

    // Hide after main page load, but keep at least minVisibleMs to show animation.
    window.addEventListener('load', hidePreloader, { once: true });
    // Hard fallback so it never blocks forever.
    setTimeout(hidePreloader, 4500);
})();
