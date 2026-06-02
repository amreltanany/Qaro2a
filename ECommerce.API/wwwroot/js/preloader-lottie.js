(function () {
    var player = document.getElementById('preloaderLottie');
    if (!player) return;

    // Book animation is active on frames 0–89; the file's out-point is 150 (empty hold after 89).
    var loopStart = 0;
    var loopEnd = 89;

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
})();
