/* Add to cart.
   Product page / other buttons: add the chosen quantity to the cart.
   Product cards (price | - n + | cart): the number on the card IS the quantity of that book in the visitor's cart.
     - "Add to cart" puts n copies in the cart (it does not stack on top of what is already there);
     - once the book is in the cart, + / - change the cart straight away (so lowering the number lowers the cart);
     - the number is kept after adding, and never goes above the stock. */
(function () {
    var TRUCK = '<svg class="shop-oos-truck" viewBox="0 0 120 70" aria-hidden="true" focusable="false"><g fill="currentColor"><rect x="2" y="22" width="34" height="5" rx="2.5"/><rect x="9" y="33" width="30" height="5" rx="2.5"/><rect x="14" y="44" width="26" height="5" rx="2.5"/><path d="M30 10h52l-2 40H26z"/><path d="M86 22h16l10 14v16h-26z"/><rect x="20" y="52" width="96" height="8" rx="2"/></g><path d="M92 27h8l6 8H92z" fill="#fff"/><g fill="currentColor" stroke="#fff" stroke-width="3"><circle cx="42" cy="58" r="9"/><circle cx="96" cy="58" r="9"/></g><g fill="#fff"><circle cx="42" cy="58" r="3.4"/><circle cx="96" cy="58" r="3.4"/></g></svg>';
    var lines = {};            // productId -> { id, quantity } for this visitor's cart
    var timers = {};           // productId -> debounce timer for the live + / - sync
    var failMsg = 'Failed to add to cart.';

    function token() { try { return (typeof localStorage !== 'undefined' && localStorage.getItem('token')) || ''; } catch (e) { return ''; } }
    function headers(json) {
        var h = {}; if (json) h['Content-Type'] = 'application/json';
        var t = token(); if (t) h['Authorization'] = 'Bearer ' + t; // no token: the server keeps a guest cart in a cookie
        return h;
    }
    function message(body) {
        return (body && (body.message || (body.Errors && body.Errors.Message))) || failMsg;
    }
    function badge(items) {
        if (!window.updateCartBadge) return;
        window.updateCartBadge(items.reduce(function (s, i) { return s + (i.quantity || 0); }, 0));
    }
    function loadCart() {
        return fetch('/api/Cart', { credentials: 'same-origin', headers: headers(false) })
            .then(function (r) { return r.ok ? r.json() : []; })
            .then(function (items) {
                lines = {};
                items.forEach(function (i) { lines[i.productId] = { id: i.id, quantity: i.quantity }; });
                return items;
            })
            .catch(function () { return []; });
    }
    var ready = null;
    function afterCart(items) { badge(items); paintCards(); } // header badge + every card of that book (the slider has copies)

    // ---------- card helpers ----------
    function val(box) { return Math.max(1, parseInt(box.querySelector('.shop-qty__val').textContent, 10) || 1); }
    function cap(box) { return Math.max(1, parseInt(box.getAttribute('data-max'), 10) || 1); }
    function sync(box) {
        if (!box) return;
        var row = box.closest('.shop-card-actions'), minus = box.querySelector('[data-qty-step="-1"]'), v = val(box);
        minus.removeAttribute('title'); minus.removeAttribute('data-qt'); // no tooltip bubble on the stepper (the aria-label is kept for screen readers)
        if (!minus.hasAttribute('data-minus-label')) minus.setAttribute('data-minus-label', minus.getAttribute('aria-label') || '');
        if (row && lines[pidOf(row)] && v <= 1) {
            // the book is in the cart and the number is 1: the minus becomes a red x that takes it out of the cart
            minus.disabled = false; minus.classList.add('is-remove'); minus.innerHTML = '&times;';
            minus.setAttribute('aria-label', box.getAttribute('data-remove-label') || 'Remove from cart');
        } else {
            minus.classList.remove('is-remove'); minus.innerHTML = '&minus;'; 
            minus.setAttribute('aria-label', minus.getAttribute('data-minus-label')); minus.disabled = v <= 1;
        }
        box.querySelector('[data-qty-step="1"]').disabled = v >= cap(box);
    }
    window.syncQty = sync;
    function setVal(box, v) { box.querySelector('.shop-qty__val').textContent = String(Math.min(cap(box), Math.max(1, v))); sync(box); }
    function pidOf(row) { var b = row.querySelector('.btn-add-cart'); return b ? parseInt(b.getAttribute('data-product-id'), 10) : NaN; }

    // The page says how many copies are in stock (that is what is NOT in anybody's cart); what this visitor can
    // hold in their cart is that plus what they already hold.
    function paintCards() {
        document.querySelectorAll('.shop-card-actions').forEach(function (row) {
            var box = row.querySelector('.shop-qty'); if (!box || isNaN(pidOf(row))) return;
            var inCart = lines[pidOf(row)] ? lines[pidOf(row)].quantity : 0;
            // fixed once, from the page as it was served: copies nobody holds + the copies this visitor holds = what the visitor can have
            if (!box.hasAttribute('data-cap')) box.setAttribute('data-cap', String((parseInt(box.getAttribute('data-max'), 10) || 0) + inCart));
            box.setAttribute('data-max', box.getAttribute('data-cap'));
            setVal(box, inCart ? inCart : val(box));
        });
    }

    // nothing left: swap the stepper and the cart button for the same 'Out of stock' pill the page shows on load
    function outOfStock(row) {
        var box = row.querySelector('.shop-qty'), btn = row.querySelector('.btn-add-cart');
        var label = row.getAttribute('data-out-label') || 'Out of stock';
        if (box) box.remove(); if (btn) btn.remove();
        row.classList.add('shop-card-actions--out-of-stock');
        var truck = document.createElement('span'); truck.className = 'shop-oos-truck-wrap'; truck.title = label; truck.innerHTML = TRUCK; row.appendChild(truck);
        var pill = document.createElement('button'); pill.type = 'button'; pill.className = 'shop-out-of-stock-btn'; pill.disabled = true;
        pill.innerHTML = '<span></span>'; pill.firstChild.textContent = label; row.appendChild(pill);
    }
    // the server answered "Insufficient stock. Available: A" (A = what nobody holds): fix the card to the truth
    function stockSaid(row, a) {
        var box = row.querySelector('.shop-qty'); if (!box) return;
        var held = lines[pidOf(row)] ? lines[pidOf(row)].quantity : 0;
        if (held + a <= 0) { outOfStock(row); return; }
        box.setAttribute('data-max', String(held + a));
        setVal(box, val(box));
    }
    function failed(row, res) {
        return res.json().catch(function () { return null; }).then(function (body) {
            var msg = message(body), m = /Available:\s*(\d+)/i.exec(msg || '');
            if (m && row) { var a = parseInt(m[1], 10); stockSaid(row, a); if (a > 0 || (lines[pidOf(row)] || {}).quantity) showError(msg); } // silent when the card just turns "Out of stock"
            else showError(msg);
        });
    }

    // set the cart line of this book to the number shown on its card
    function putQty(row) {
        var pid = pidOf(row), line = lines[pid], box = row.querySelector('.shop-qty'); if (!line || !box) return Promise.resolve();
        var qty = val(box); if (qty === line.quantity) return Promise.resolve();
        return fetch('/api/Cart/items/' + line.id, { method: 'PUT', credentials: 'same-origin', headers: headers(true), body: JSON.stringify({ quantity: qty }) })
            .then(function (res) {
                if (res.ok) { line.quantity = qty; return loadCart().then(afterCart); }
                return failed(row, res);
            })
            .catch(function () { showError(failMsg); });
    }

    // take the book out of the cart (the red x that replaces the minus when the number is 1)
    function removeLine(row) {
        var pid = pidOf(row), line = lines[pid]; if (!line) return;
        clearTimeout(timers[pid]);
        fetch('/api/Cart/items/' + line.id, { method: 'DELETE', credentials: 'same-origin', headers: headers(false) })
            .then(function (res) { if (res.ok || res.status === 204) { delete lines[pid]; return loadCart().then(afterCart); } return failed(row, res); })
            .catch(function () { showError(failMsg); });
    }

    // ---------- the add button ----------
    function animate(btn) {
        if (btn.classList.contains('button') && !btn.classList.contains('loading')) {
            btn.classList.add('loading');
            setTimeout(function () { btn.classList.remove('loading'); }, 3700);
        } else if (btn.classList.contains('shop-cart-round') || btn.classList.contains('pd__cta')) {
            btn.classList.add('is-added');
            setTimeout(function () { btn.classList.remove('is-added'); }, 1600);
        }
    }
    function legacyAdd(btn) {
        var qtySel = btn.getAttribute('data-qty-input'), qtyEl = qtySel ? document.querySelector(qtySel) : null;
        var qty = qtyEl ? Math.max(1, parseInt(qtyEl.value, 10) || 1) : 1;
        fetch('/api/Cart', { method: 'POST', credentials: 'same-origin', headers: headers(true), body: JSON.stringify({ productId: parseInt(btn.getAttribute('data-product-id'), 10), quantity: qty }) })
            .then(function (res) { if (res.ok) return loadCart().then(afterCart); return failed(null, res); })
            .catch(function (err) { console.error(err); showError(failMsg); });
    }
    function cardAdd(btn, row) {
        var box = row.querySelector('.shop-qty'), pid = pidOf(row), qty = val(box);
        (ready || Promise.resolve()).then(function () {
            if (lines[pid]) return putQty(row); // already in the cart: make the cart match the card
            return fetch('/api/Cart', { method: 'POST', credentials: 'same-origin', headers: headers(true), body: JSON.stringify({ productId: pid, quantity: qty }) })
                .then(function (res) {
                    if (!res.ok) return failed(row, res);
                    return res.json().then(function (item) { lines[pid] = { id: item.id, quantity: qty }; }).catch(function () { }).then(function () { return loadCart(); }).then(afterCart);
                })
                .catch(function (err) { console.error(err); showError(failMsg); });
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        ready = loadCart().then(function (items) { paintCards(); return items; });
        document.querySelectorAll('.btn-add-cart').forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                animate(btn);
                var row = btn.closest('.shop-card-actions');
                if (row && row.querySelector('.shop-qty')) cardAdd(btn, row); else legacyAdd(btn);
            });
        });
    });
    // coming back with the back button: the cart may have changed meanwhile
    window.addEventListener('pageshow', function (e) { if (e.persisted) { ready = loadCart().then(function (i) { paintCards(); return i; }); } });

    // + / - on the cards (delegated: works for the looped copies in the home slider too)
    document.addEventListener('click', function (e) {
        var b = e.target.closest && e.target.closest('[data-qty-step]');
        if (!b) return;
        e.preventDefault();
        var box = b.closest('.shop-qty'), row = box.closest('.shop-card-actions');
        if (b.classList.contains('is-remove')) { removeLine(row); return; }
        setVal(box, val(box) + parseInt(b.getAttribute('data-qty-step'), 10));
        var pid = pidOf(row);
        if (lines[pid]) { clearTimeout(timers[pid]); timers[pid] = setTimeout(function () { putQty(row); }, 350); } // in the cart already: the cart follows the number
    });
})();
