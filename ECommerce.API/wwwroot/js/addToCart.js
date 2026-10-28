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
            var qty = qtyEl ? Math.max(1, parseInt(qtyEl.value, 10) || 1) : 1;
            var userId = (typeof localStorage !== 'undefined' && localStorage.getItem('userId')) ||
                (document.querySelector('meta[name="user-id"]') && document.querySelector('meta[name="user-id"]').getAttribute('content')) || 'guest';

            var token = (typeof localStorage !== 'undefined' && localStorage.getItem('token')) || '';
            if (!token || !userId || userId === 'guest') {
                window.location.href = '/Home/Login';
                return;
            }

            fetch('/api/Cart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + token
                },
                body: JSON.stringify({ productId: productId, quantity: qty })
            })
                .then(function (res) {
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