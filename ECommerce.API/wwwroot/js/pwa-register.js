/* Registers the service worker (installable app + offline page). Failing to register changes nothing for the visitor. */
if ('serviceWorker' in navigator) {
    window.addEventListener('load', function () {
        navigator.serviceWorker.register('/sw.js', { scope: '/' }).catch(function () { });
    });
}
