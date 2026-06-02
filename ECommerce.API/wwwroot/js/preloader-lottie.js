(function () {
    var player = document.getElementById('preloaderLottie');
    if (!player) return;

    var preloader = document.querySelector('.preloader');
    var hasHidden = false;

    // Book animation is active on frames 0–89; the file's out-point is 150 (empty hold after 89).
    var loopStart = 0;
    var loopEnd = 89;

    function stopAnimationSafely() {
        try { if (typeof player.stop === 'function') player.stop(); } catch (e) { }
        try { if (typeof player.pause === 'function') player.pause(); } catch (e) { }
        try { if (typeof player.setSpeed === 'function') player.setSpeed(1); } catch (e) { }

        // Attempt to stop underlying lottie instance (if available).
        try {
            var anim = player.getLottie && player.getLottie();
            if (anim && typeof anim.then === 'function') {
                anim.then(function (a) {
                    try { if (a && typeof a.stop === 'function') a.stop(); } catch (e) { }
                }).catch(function () { });
            } else if (anim && typeof anim.stop === 'function') {
                anim.stop();
            }
        } catch (e) { }
    }

    function hidePreloader() {
        if (hasHidden) return;
        hasHidden = true;

        stopAnimationSafely();
        if (preloader) preloader.classList.add('loaded');
    }

    function applyLoop(animation) {
        if (!animation) {
            player.play();
            return;
        }
        if (typeof animation.setLoop === 'function') {
            animation.setLoop(true);
        }
        if (typeof animation.playSegments === 'function') {
            animation.playSegments([loopStart, loopEnd], true);
            return;
        }
        player.play();
    }

    function startTightLoop() {
        player.setSpeed(2.5);
        if (typeof player.setLooping === 'function') {
            player.setLooping(true);
        }

        if (!player.getLottie) {
            player.play();
            return;
        }

        var lottieInstance = player.getLottie();
        if (lottieInstance && typeof lottieInstance.then === 'function') {
            lottieInstance.then(applyLoop).catch(function () { player.play(); });
            return;
        }
        applyLoop(lottieInstance);
    }

    player.addEventListener('ready', startTightLoop);

    // Performance: don't keep the fixed overlay blocking the page.
    // Hide quickly after DOM is ready, and again on full window load (idempotent).
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            setTimeout(hidePreloader, 120);
        });
    } else {
        setTimeout(hidePreloader, 120);
    }
    window.addEventListener('load', hidePreloader);
})();
