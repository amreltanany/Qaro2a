document.addEventListener('DOMContentLoaded', function () {
    // Assuming your buttons have both 'btn-add-cart' and 'button' classes,
    // or you can just target '.btn-add-cart'
    document.querySelectorAll('.btn-add-cart').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.preventDefault();

            // 1. Button Animation Logic
            if (!btn.classList.contains('loading')) {
                btn.classList.add('loading');
                setTimeout(() => btn.classList.remove('loading'), 3700);
            }

            // 2. Add to Cart Logic
            var productId = parseInt(this.getAttribute('data-product-id'), 10);
            var productName = this.getAttribute('data-product-name');
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
                body: JSON.stringify({ productId: productId, quantity: 1 })
            })
                .then(function (res) {
                    if (!res.ok) {
                        return res.json().then(function (body) {
                            var msg = (body && (body.message || (body.Errors && body.Errors.Message)))
                                ? (body.message || body.Errors.Message)
                                : 'Failed to add to cart.';
                            alert(msg);
                        }).catch(function () { alert('Failed to add to cart.'); });
                    }
                })
                .catch(function (err) {
                    console.error(err);
                    alert('Failed to add to cart.');
                });
        });
    });
});