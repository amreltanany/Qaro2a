/* "Install the app": a SweetAlert for phone visitors and a button in the footer.
   - Android (Chrome / Samsung Internet): the install button runs the browser's real install prompt.
   - iPhone (Safari): iOS has no install prompt, so the alert shows the two steps (Share, Add to Home Screen).
   The popup is polite: phones only, never inside the installed app, not on cart / checkout / login pages, it waits a few seconds,
   and after "Not now" it stays quiet for 14 days (three refusals = never again). The footer button is always there for anyone who wants it. */
(function () {
    'use strict';
    var KEY = 'qaro2a_install_prompt', QUIET_DAYS = 14, MAX_REFUSALS = 3;

    var ua = navigator.userAgent || '';
    var isIOS = /iphone|ipad|ipod/i.test(ua);
    var isAndroid = /android/i.test(ua);
    var isPhone = isIOS || isAndroid;
    var iosSafari = isIOS && /safari/i.test(ua) && !/crios|fxios|edgios|opios/i.test(ua);
    var standalone = (window.matchMedia && window.matchMedia('(display-mode: standalone)').matches) || window.navigator.standalone === true;
    if (standalone) return;                                                      // already installed and open as an app: nothing to offer

    var ar = (document.documentElement.lang || '').toLowerCase().indexOf('ar') === 0;
    var T = ar ? {
        title: 'ثبّت تطبيق قروءة', text: 'ادخل متجر الكتب بضغطة من شاشتك الرئيسية، وبدون شريط المتصفح.',
        install: 'تثبيت', later: 'ليس الآن', ok: 'حسنًا',
        iosIntro: 'ثبّت قروءة على الآيفون في خطوتين:', step1: 'اضغط زر المشاركة', step2: 'اختر «إضافة إلى الشاشة الرئيسية»',
        iosOther: 'لتثبيت التطبيق على الآيفون افتح الموقع من متصفح <b>Safari</b> ثم اتبع الخطوتين.',
        androidIntro: 'ثبّت قروءة من قائمة المتصفح:', aStep1: 'اضغط على القائمة (⋮) أعلى المتصفح', aStep2: 'اختر «تثبيت التطبيق» أو «إضافة إلى الشاشة الرئيسية»',
        desktopIntro: 'افتح الموقع من Chrome أو Edge ثم اضغط على أيقونة التثبيت في شريط العنوان.'
    } : {
        title: 'Install the Qaro2a app', text: 'Open the bookstore in one tap from your home screen, with no browser bar.',
        install: 'Install', later: 'Not now', ok: 'Got it',
        iosIntro: 'Install Qaro2a on your iPhone in two steps:', step1: 'Tap the Share button', step2: 'Choose “Add to Home Screen”',
        iosOther: 'To install the app on iPhone, open the site in <b>Safari</b> and follow the two steps.',
        androidIntro: 'Install Qaro2a from the browser menu:', aStep1: 'Tap the menu (⋮) at the top of the browser', aStep2: 'Choose “Install app” or “Add to Home screen”',
        desktopIntro: 'Open the site in Chrome or Edge and click the install icon in the address bar.'
    };

    function read() { try { return JSON.parse(localStorage.getItem(KEY) || '{}') || {}; } catch (e) { return {}; } }
    function write(v) { try { localStorage.setItem(KEY, JSON.stringify(v)); } catch (e) { } }
    function remember(outcome) {
        var s = read();
        if (outcome === 'installed') s.installed = true;
        else { s.refusals = (s.refusals || 0) + 1; s.last = Date.now(); }
        write(s);
    }

    var deferred = null, shown = false;

    /* ---------- footer button ---------- */
    function buttons() { return document.querySelectorAll('[data-install-app]'); }
    function refreshButtons() {
        var visible = isPhone || !!deferred;                                     // desktop: only when the browser really offers installing
        buttons().forEach(function (b) { b.hidden = !visible; });
    }
    function steps(items) {
        return '<ol class="qaro-install-steps">' + items.map(function (i) { return '<li><i class="bi ' + i[0] + '" aria-hidden="true"></i> ' + i[1] + '</li>'; }).join('') + '</ol>';
    }
    function howToHtml() {
        if (isIOS && !iosSafari) return '<p class="qaro-install-text">' + T.iosOther + '</p>' + steps([['bi-box-arrow-up', T.step1], ['bi-plus-square', T.step2]]);
        if (isIOS) return '<p class="qaro-install-text">' + T.iosIntro + '</p>' + steps([['bi-box-arrow-up', T.step1], ['bi-plus-square', T.step2]]);
        if (isAndroid) return '<p class="qaro-install-text">' + T.androidIntro + '</p>' + steps([['bi-three-dots-vertical', T.aStep1], ['bi-phone', T.aStep2]]);
        return '<p class="qaro-install-text">' + T.desktopIntro + '</p>';
    }
    function popupBase() {
        return {
            title: T.title, imageUrl: '/icons/icon-192.png', imageWidth: 88, imageHeight: 88, imageAlt: 'Qaro2a',
            position: isPhone ? 'bottom' : 'center', backdrop: 'rgba(0, 52, 110, .35)',
            confirmButtonColor: '#0270B8', cancelButtonColor: '#6c7a89',
            allowOutsideClick: true, allowEscapeKey: true, focusConfirm: false,
            customClass: { popup: 'qaro-install-popup', image: 'qaro-install-icon' }
        };
    }
    function runNativePrompt() {
        var ev = deferred; deferred = null; refreshButtons();
        try {
            ev.prompt();
            Promise.resolve(ev.userChoice).then(function (c) {
                if (c && c.outcome === 'accepted') { remember('installed'); buttons().forEach(function (b) { b.hidden = true; }); }
            });
        } catch (e) { }
    }
    function onFooterClick() {
        if (deferred) { runNativePrompt(); return; }                             // the browser offers installing: go straight to it
        if (!window.Swal) return;
        window.Swal.fire(Object.assign(popupBase(), { html: howToHtml(), confirmButtonText: T.ok }));
    }
    function bindButtons() {
        buttons().forEach(function (b) { b.addEventListener('click', onFooterClick); });
        refreshButtons();
    }

    /* ---------- the popup ---------- */
    var path = location.pathname.toLowerCase();
    var popupAllowed = isPhone
        && !(isIOS && !iosSafari)
        && !/^\/(cart|checkout|track\/placed|home\/login|home\/register|admin|dashboard)/.test(path);
    var st = read();
    if (st.installed || (st.refusals || 0) >= MAX_REFUSALS) popupAllowed = false;
    if (st.last && Date.now() - st.last < QUIET_DAYS * 86400000) popupAllowed = false;
    var delay = (typeof window.__qaroInstallDelay === 'number') ? window.__qaroInstallDelay : 5000;

    function showPopup() {
        if (shown || !popupAllowed || !window.Swal) return;
        if (window.Swal.isVisible && window.Swal.isVisible()) return;           // never on top of another message
        if (!isIOS && !deferred) return;                                         // Android: only when the browser really offers installing
        shown = true;
        if (isIOS) {
            window.Swal.fire(Object.assign(popupBase(), { html: howToHtml(), confirmButtonText: T.ok, showCancelButton: true, cancelButtonText: T.later, reverseButtons: ar }))
                .then(function (r) { remember(r.isConfirmed ? 'installed' : 'later'); });
            return;
        }
        window.Swal.fire(Object.assign(popupBase(), { text: T.text, confirmButtonText: T.install, showCancelButton: true, cancelButtonText: T.later, reverseButtons: ar }))
            .then(function (r) {
                if (!r.isConfirmed || !deferred) { remember('later'); return; }
                runNativePrompt();
            });
    }
    function schedule() {
        if (!popupAllowed) return;
        var waited = 0;
        setTimeout(function tick() {
            if (window.Swal) { showPopup(); return; }
            if ((waited += 500) < 12000) setTimeout(tick, 500);                  // SweetAlert loads deferred from the CDN
        }, delay);
    }

    window.addEventListener('beforeinstallprompt', function (e) {
        e.preventDefault();                                                       // our own message replaces the browser's mini bar
        deferred = e;
        refreshButtons();
        schedule();
    });
    window.addEventListener('appinstalled', function () { remember('installed'); deferred = null; buttons().forEach(function (b) { b.hidden = true; }); });
    if (isIOS) window.addEventListener('load', schedule);
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', bindButtons); else bindButtons();
})();
