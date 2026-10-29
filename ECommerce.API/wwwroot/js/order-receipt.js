/* Order receipt: a printer prints the receipt, "Download image" tears it off and saves it as a PNG. */
(function () {
    var H2C = 'https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js';
    var h2cPromise;
    function loadH2C() {
        if (window.html2canvas) return Promise.resolve(window.html2canvas);
        return h2cPromise || (h2cPromise = new Promise(function (res, rej) {
            var s = document.createElement('script'); s.src = H2C; s.onload = function () { res(window.html2canvas); }; s.onerror = function () { h2cPromise = null; rej(new Error('html2canvas')); };
            document.head.appendChild(s);
        }));
    }
    function el(tag, cls, text) { var e = document.createElement(tag); if (cls) e.className = cls; if (text != null) e.textContent = text; return e; }
    function row(a, b) { var r = el('div', 'rcpt-row'); r.appendChild(el('span', null, a)); var v = el('span', null, b); v.dir = 'auto'; r.appendChild(v); return r; }

    function barcode(seed) {
        var ns = 'http://www.w3.org/2000/svg', svg = document.createElementNS(ns, 'svg');
        svg.setAttribute('viewBox', '0 0 220 36'); svg.setAttribute('width', '220'); svg.setAttribute('height', '36');
        var x = 0, n = Number(seed) || 7;
        while (x < 218) { n = (Math.imul(n, 1103515245) + 12345) & 0x7fffffff; var w = 1 + (n % 3); if ((n >> 4) % 2) { var r = document.createElementNS(ns, 'rect'); r.setAttribute('x', x); r.setAttribute('y', 0); r.setAttribute('width', w); r.setAttribute('height', 36); r.setAttribute('fill', '#2b2118'); svg.appendChild(r); } x += w + 1; }
        return svg;
    }
    function zigzag() {
        var ns = 'http://www.w3.org/2000/svg', svg = document.createElementNS(ns, 'svg');
        svg.setAttribute('viewBox', '0 0 300 10'); svg.setAttribute('width', '300'); svg.setAttribute('height', '10'); svg.setAttribute('class', 'rcpt-zig');
        var d = 'M0 0'; for (var i = 0; i < 30; i++) d += ' L' + (i * 10 + 5) + ' 10 L' + (i * 10 + 10) + ' 0'; d += ' Z';
        var p = document.createElementNS(ns, 'path'); p.setAttribute('d', d); p.setAttribute('fill', '#fffdf6'); svg.appendChild(p); return svg;
    }

    function buildPaper(card, T) {
        var d = card.dataset, paper = el('div', 'rcpt-paper'); paper.dir = T.dir;
        paper.appendChild(el('div', 'rcpt-brand', 'قروءه'));
        paper.appendChild(el('div', 'rcpt-sub', 'Qaro2a'));
        paper.appendChild(el('div', 'rcpt-dash'));
        paper.appendChild(el('div', 'rcpt-title', T.official));
        paper.appendChild(el('div', 'rcpt-dash'));
        paper.appendChild(row(T.order, '#' + d.orderId));
        paper.appendChild(row(T.date, d.orderDate));
        paper.appendChild(row(T.status, d.orderStatus));
        if (d.orderPhone) paper.appendChild(row(T.phone, d.orderPhone));
        if (d.orderAddress) { var a = el('div', 'rcpt-addr', T.ship + ': ' + d.orderAddress); paper.appendChild(a); }
        paper.appendChild(el('div', 'rcpt-dash'));
        var head = row(T.item, T.amount); head.classList.add('rcpt-head'); paper.appendChild(head);
        card.querySelectorAll('.order-items-table tbody tr').forEach(function (tr) {
            var c = tr.children, line = el('div', 'rcpt-item');
            line.appendChild(el('span', 'rcpt-qty', (c[2] ? c[2].textContent.trim() : '') + '×'));
            line.appendChild(el('span', 'rcpt-name', c[0] ? c[0].textContent.trim() : ''));
            var amt = el('span', 'rcpt-amt', c[3] ? c[3].textContent.trim() : ''); amt.dir = 'ltr'; line.appendChild(amt);
            paper.appendChild(line);
        });
        paper.appendChild(el('div', 'rcpt-dash'));
        if (d.orderDelivery) paper.appendChild(row(T.delivery || 'DELIVERY', d.orderDelivery));
        var tot = row(T.total, d.orderTotal); tot.classList.add('rcpt-total'); paper.appendChild(tot);
        paper.appendChild(el('div', 'rcpt-dash'));
        paper.appendChild(el('div', 'rcpt-thanks', T.thanks));
        var bc = el('div', 'rcpt-bar'); bc.appendChild(barcode(d.orderId)); paper.appendChild(bc);
        paper.appendChild(el('div', 'rcpt-code', 'QR-' + String(d.orderId).padStart(6, '0')));
        paper.appendChild(zigzag());
        return paper;
    }

    function open(card, T) {
        var overlay = el('div', 'rcpt-overlay'); overlay.setAttribute('role', 'dialog'); overlay.setAttribute('aria-modal', 'true');
        var stage = el('div', 'rcpt-stage');
        var printer = el('div', 'rcpt-printer'); printer.appendChild(el('div', 'rcpt-slot')); printer.appendChild(el('i', 'rcpt-led'));
        var win = el('div', 'rcpt-window'), paper = buildPaper(card, T); win.appendChild(paper);
        var bar = el('div', 'rcpt-actions');
        var again = el('button', 'rcpt-btn', T.reprint), dl = el('button', 'rcpt-btn rcpt-btn--primary', T.download), close = el('button', 'rcpt-btn', T.close);
        [again, dl, close].forEach(function (b) { b.type = 'button'; bar.appendChild(b); });
        stage.appendChild(printer); stage.appendChild(win); stage.appendChild(bar); overlay.appendChild(stage);
        document.body.appendChild(overlay); document.body.classList.add('rcpt-open');
        var prevFocus = document.activeElement;

        function print() {
            paper.classList.remove('is-torn', 'is-printing'); void paper.offsetWidth;
            printer.classList.add('is-busy'); paper.classList.add('is-printing');
            clearTimeout(print.t); print.t = setTimeout(function () { printer.classList.remove('is-busy'); }, 3200);
        }
        function shut() { document.body.classList.remove('rcpt-open'); overlay.remove(); document.removeEventListener('keydown', onKey); if (prevFocus && prevFocus.focus) prevFocus.focus(); }
        function onKey(e) { if (e.key === 'Escape') shut(); }
        document.addEventListener('keydown', onKey);
        overlay.addEventListener('click', function (e) { if (e.target === overlay) shut(); });
        close.addEventListener('click', shut);
        again.addEventListener('click', print);
        dl.addEventListener('click', function () {
            dl.disabled = true; dl.textContent = T.preparing;
            loadH2C().then(function (h2c) {
                // render a static copy (no animation transforms) so the picture is always the full receipt
                var holder = el('div', 'rcpt-capture'); var copy = buildPaper(card, T); copy.classList.add('is-static'); holder.appendChild(copy); document.body.appendChild(holder);
                return h2c(copy, { backgroundColor: null, scale: 3, useCORS: true }).then(function (canvas) {
                    holder.remove();
                    canvas.toBlob(function (blob) {
                        var a = document.createElement('a'); a.href = URL.createObjectURL(blob); a.download = 'receipt-order-' + card.dataset.orderId + '.png';
                        document.body.appendChild(a); a.click(); a.remove(); setTimeout(function () { URL.revokeObjectURL(a.href); }, 1500);
                    }, 'image/png');
                });
            }).catch(function () { alert(T.failed); }).then(function () { setTimeout(function () { dl.disabled = false; dl.textContent = T.download; }, 900); });
        });
        print(); dl.focus({ preventScroll: true });
    }
    window.QaroReceipt = { open: open };
})();
