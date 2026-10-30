document.addEventListener('DOMContentLoaded', function () {
    // Assuming your buttons have both 'btn-add-cart' and 'button' classes,
    // or you can just target '.btn-add-cart'
    document.querySelectorAll('.btn-add-cart').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.preventDefault();

            // 1. Button Animation Logic
            if (btn.classList.contains('button') && !btn.classList.contains('loading')) {
                btn.classList.add('loading');
                setTimeout(() => btn.classList.remove('loading'), 3700);
            } else if (btn.classList.contains('shop-cart-round') || btn.classList.contains('pd__cta')) {
                btn.classList.add('is-added');
                setTimeout(() => btn.classList.remove('is-added'), 1600);
            }

            // 2. Add to Cart Logic
            var productId = parseInt(this.getAttribute('data-product-id'), 10);
            var productName = this.getAttribute('data-product-name');
            var qtySel = this.getAttribute('data-qty-input');
            var qtyEl = qtySel ? document.querySelector(qtySel) : null;
            var cardQty = this.closest('.shop-card-actions') && this.closest('.shop-card-actions').querySelector('.shop-qty__val');
            var qty = qtyEl ? Math.max(1, parseInt(qtyEl.value, 10) || 1) : (cardQty ? Math.max(1, parseInt(cardQty.textContent, 10) || 1) : 1);
            var addedBtn = this;
            var userId = (typeof localStorage !== 'undefined' && localStorage.getItem('userId')) ||
                (document.querySelector('meta[name="user-id"]') && document.querySelector('meta[name="user-id"]').getAttribute('content')) || 'guest';

            var token = (typeof localStorage !== 'undefined' && localStorage.getItem('token')) || '';
            // no token: the server keeps a guest cart in a cookie

            fetch('/api/Cart', {
                method: 'POST',
                credentials: 'same-origin',
                headers: token
                    ? { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token }
                    : { 'Content-Type': 'application/json' },
                body: JSON.stringify({ productId: productId, quantity: qty })
            })
                .then(function (res) {
                    if (res.ok && cardQty) { cardQty.textContent = '1'; syncQty(cardQty.closest('.shop-qty')); } // back to 1 for the next add
                    if (res.ok && window.updateCartBadge) {
                        // refresh the header badge with the new total
                        fetch('/api/Cart', { credentials: 'same-origin', headers: token ? { 'Authorization': 'Bearer ' + token } : {} })
                            .then(function (r) { return r.json(); })
                            .then(function (items) { window.updateCartBadge(items.reduce(function (s, i) { return s + (i.quantity || 0); }, 0)); })
                            .catch(function () { });
                    }
                    if (!res.ok) {
                        return res.json().then(function (body) {
                            var msg = (body && (body.message || (body.Errors && body.Errors.Message)))
                                ? (body.message || body.Errors.Message)
                                : 'Failed to add to cart.';
                            showError(msg);
                        }).catch(function () { showError('Failed to add to cart.'); });
                    }
                })
                .catch(function (err) {
                    console.error(err);
                    showError('Failed to add to cart.');
                });
        });
    });
});

// + / - on the product cards (delegated: works for the looped copies in the home slider too)
function syncQty(box) {
    if (!box) return;
    var v = parseInt(box.querySelector('.shop-qty__val').textContent, 10) || 1, max = parseInt(box.getAttribute('data-max'), 10) || 1;
    box.querySelector('[data-qty-step="-1"]').disabled = v <= 1;
    box.querySelector('[data-qty-step="1"]').disabled = v >= max;
}
document.addEventListener('click', function (e) {
    var b = e.target.closest && e.target.closest('[data-qty-step]');
    if (!b) return;
    e.preventDefault();
    var box = b.closest('.shop-qty'), out = box.querySelector('.shop-qty__val');
    var max = parseInt(box.getAttribute('data-max'), 10) || 1;
    var next = Math.min(max, Math.max(1, (parseInt(out.textContent, 10) || 1) + parseInt(b.getAttribute('data-qty-step'), 10)));
    out.textContent = String(next);
    syncQty(box);
});
