(function () {
    var token = typeof localStorage !== 'undefined' && localStorage.getItem('token');
    function authHeaders() {
        return {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token
        };
    }

    document.querySelectorAll('.cart-qty-plus, .cart-qty-minus').forEach(function (btn) {
        btn.addEventListener('click', function () {
            if (this.disabled) return;
            var row = this.closest('tr');
            var itemId = row.getAttribute('data-item-id');
            var maxStock = parseInt(row.getAttribute('data-max-stock'), 10);
            var qtyEl = row.querySelector('.cart-qty-value');
            var currentQty = parseInt(qtyEl.textContent, 10) || 0;
            var isPlus = this.classList.contains('cart-qty-plus');
            var newQty = isPlus ? currentQty + 1 : Math.max(1, currentQty - 1);
            if (newQty === currentQty) return;
            if (isPlus && !isNaN(maxStock) && newQty > maxStock) {
                alert('Insufficient stock. Available: ' + maxStock);
                return;
            }

            if (!token) {
                window.location.href = '/Home/Login';
                return;
            }

            fetch('/api/Cart/items/' + itemId, {
                method: 'PUT',
                headers: authHeaders(),
                body: JSON.stringify({ quantity: newQty })
            })
                .then(function (res) {
                    if (!res.ok) {
                        return res.json().then(function (body) {
                            var msg = (body && body.message) ? body.message : 'Failed to update quantity.';
                            alert(msg);
                            window.location.reload();
                        }).catch(function () {
                            window.location.reload();
                        });
                    }
                    window.location.reload();
                })
                .catch(function (err) {
                    console.error(err);
                    alert('Failed to update quantity.');
                    window.location.reload();
                });
        });
    });

    document.querySelectorAll('.cart-remove-item').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var row = this.closest('tr');
            var itemId = row.getAttribute('data-item-id');
            if (!token) {
                window.location.href = '/Home/Login';
                return;
            }
            fetch('/api/Cart/items/' + itemId, {
                method: 'DELETE',
                headers: authHeaders()
            })
                .then(function (res) {
                    if (!res.ok)
                        return res.json().then(function (body) {
                            var msg = (body && body.message) ? body.message : 'Failed to remove item.';
                            alert(msg);
                            window.location.reload();
                        }).catch(function () { window.location.reload(); });
                    window.location.reload();
                })
                .catch(function (err) {
                    console.error(err);
                    window.location.reload();
                });
        });
    });
})();
