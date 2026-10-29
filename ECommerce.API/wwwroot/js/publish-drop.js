(function () {
    var zone = document.getElementById('pubDrop');
    if (!zone) return;
    var input = document.getElementById('pdfFile');
    var ball = document.getElementById('pubBall');
    var hoop = document.getElementById('pubHoop');
    var file = zone.querySelector('.pub-file');
    var nameEl = zone.querySelector('[data-file-name]');
    var sizeEl = zone.querySelector('[data-file-size]');
    var bar = zone.querySelector('.pub-file__bar i');
    var status = zone.querySelector('.pub-file__status');
    var T = JSON.parse(zone.getAttribute('data-text'));
    var MAX = 5 * 1024 * 1024;

    function fmt(b) { return b > 1048576 ? (b / 1048576).toFixed(1) + ' MB' : Math.max(1, Math.round(b / 1024)) + ' KB'; }

    function show(f, ok, msg) {
        file.classList.add('is-visible');
        nameEl.textContent = f ? f.name : '';
        sizeEl.textContent = f ? fmt(f.size) : '';
        status.textContent = msg;
        zone.classList.toggle('is-done', ok);
        zone.classList.toggle('is-error', !ok);
        bar.style.transition = 'none'; bar.style.width = '0';
        if (ok) requestAnimationFrame(function () { requestAnimationFrame(function () { bar.style.transition = ''; bar.style.width = '100%'; }); });
    }

    function onFile() {
        var f = input.files && input.files[0];
        if (!f) { file.classList.remove('is-visible'); zone.classList.remove('is-done', 'is-error'); return; }
        if (!/\.pdf$/i.test(f.name) && f.type !== 'application/pdf') { input.value = ''; show(f, false, T.notPdf); return; }
        if (f.size > MAX) { input.value = ''; show(f, false, T.tooBig); return; }
        show(f, true, T.ready);
        swish();
    }
    input.addEventListener('change', onFile);
    ['dragenter', 'dragover'].forEach(function (e) { input.addEventListener(e, function () { zone.classList.add('is-over'); }); });
    ['dragleave', 'drop'].forEach(function (e) { input.addEventListener(e, function () { zone.classList.remove('is-over'); }); });

    function swish() { hoop.classList.remove('is-swish'); void hoop.getBoundingClientRect(); hoop.classList.add('is-swish'); }

    // ---- shoot the file icon into the basket ----
    var drag = null;
    function center(el) { var r = el.getBoundingClientRect(); return { x: r.left + r.width / 2, y: r.top + r.height / 2 }; }

    ball.addEventListener('pointerdown', function (e) {
        e.preventDefault();
        ball.setPointerCapture(e.pointerId);
        var c = center(ball);
        drag = { id: e.pointerId, ox: e.clientX - c.x, oy: e.clientY - c.y, bx: c.x, by: c.y };
    });
    ball.addEventListener('pointermove', function (e) {
        if (!drag || e.pointerId !== drag.id) return;
        ball.style.transform = 'translate(' + (e.clientX - drag.ox - drag.bx) + 'px,' + (e.clientY - drag.oy - drag.by) + 'px) rotate(' + ((e.clientX - drag.bx) / 12) + 'deg) scale(1.08)';
    });
    function release(e) {
        if (!drag || e.pointerId !== drag.id) return;
        var from = center(ball), home = { x: drag.bx, y: drag.by };
        var target = center(hoop); target.y -= 28; // just above the net
        var near = Math.hypot(from.x - target.x, from.y - target.y) < 230;
        drag = null;
        if (near) shoot(from, target, home); else goHome(from, home);
    }
    ball.addEventListener('pointerup', release);
    ball.addEventListener('pointercancel', release);

    function setPos(p, home, rot, s) { ball.style.transform = 'translate(' + (p.x - home.x) + 'px,' + (p.y - home.y) + 'px) rotate(' + rot + 'deg) scale(' + s + ')'; }
    function run(ms, step, done) {
        var t0 = performance.now();
        (function frame(now) {
            var t = Math.min(1, (now - t0) / ms);
            step(t);
            if (t < 1) requestAnimationFrame(frame); else if (done) done();
        })(t0);
    }
    function shoot(from, to, home) {
        var arc = Math.max(70, Math.abs(from.y - to.y) * .6 + 60);
        run(650, function (t) {
            var x = from.x + (to.x - from.x) * t, y = from.y + (to.y - from.y) * t - 4 * arc * t * (1 - t);
            setPos({ x: x, y: y }, home, t * 540, 1.08 - t * .3);
        }, function () {
            swish();
            run(320, function (t) { setPos({ x: to.x, y: to.y + 70 * t * t }, home, 540, .78 - t * .5); ball.style.opacity = String(1 - t); }, function () {
                ball.classList.add('is-hidden');
                input.click(); // pick the manuscript once the shot has scored
                setTimeout(comeBack, 500);
            });
        });
    }
    function comeBack() {
        ball.style.transition = 'none'; ball.style.transform = ''; ball.style.opacity = '0'; ball.classList.remove('is-hidden');
        run(400, function (t) { ball.style.opacity = String(t); ball.style.transform = 'scale(' + (.6 + .4 * t) + ')'; }, function () { ball.style.transform = ''; ball.style.opacity = ''; ball.style.transition = ''; });
    }
    function goHome(from, home) {
        run(420, function (t) { var k = 1 - Math.pow(1 - t, 3); setPos({ x: from.x + (home.x - from.x) * k, y: from.y + (home.y - from.y) * k - Math.sin(t * Math.PI) * 24 }, home, 0, 1.08 - .08 * t); }, function () { ball.style.transform = ''; });
    }
    // keyboard users: Enter/Space on the icon opens the picker directly
    ball.addEventListener('keydown', function (e) { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); input.click(); } });
})();
