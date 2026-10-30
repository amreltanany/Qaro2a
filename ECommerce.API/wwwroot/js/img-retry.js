/* A picture that fails to load (server restarting, a dropped connection, a slow network) is tried again by itself,
   so nobody has to refresh the page to see a cover. Same-origin pictures only; each one is retried 5 times with growing waits. */
(function () {
    var WAITS = [1000, 3000, 6000, 10000, 15000];
    function retry(img, reset) {
        var src = img.getAttribute('data-orig') || img.getAttribute('src');
        if (!src || /^data:|^blob:/.test(src)) return;
        if (/^https?:\/\//i.test(src) && src.indexOf(location.origin) !== 0) return;
        if (!img.getAttribute('data-orig')) img.setAttribute('data-orig', src);
        var n = reset ? 0 : (parseInt(img.getAttribute('data-retries'), 10) || 0);
        if (n >= WAITS.length) { img.setAttribute('data-gave-up', '1'); return; }
        img.setAttribute('data-retries', String(n + 1));
        clearTimeout(img.__retryTimer);
        img.__retryTimer = setTimeout(function () {
            img.src = src + (src.indexOf('?') > -1 ? '&' : '?') + 'r=' + (n + 1);
        }, WAITS[n]);
    }
    // 'error' does not bubble, so listen in the capture phase
    document.addEventListener('error', function (e) {
        var t = e.target;
        if (t && t.tagName === 'IMG') retry(t, false);
    }, true);
    // back online / tab visible again: give up-and-gone pictures a fresh start
    function again() {
        document.querySelectorAll('img[data-gave-up]').forEach(function (img) { img.removeAttribute('data-gave-up'); retry(img, true); });
    }
    window.addEventListener('online', again);
    document.addEventListener('visibilitychange', function () { if (!document.hidden) again(); });
})();
