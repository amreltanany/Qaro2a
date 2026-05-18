document.addEventListener('DOMContentLoaded', function () {
    var token = (typeof localStorage !== 'undefined' && localStorage.getItem('token')) || '';
    var userId = (typeof localStorage !== 'undefined' && localStorage.getItem('userId')) ||
        (document.querySelector('meta[name="user-id"]') && document.querySelector('meta[name="user-id"]').getAttribute('content')) || 'guest';

    function isLoggedIn() {
        return token && userId && userId !== 'guest';
    }

    function authHeaders() {
        return {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token
        };
    }

    function setButtonState(btn, isActive, itemId) {
        btn.classList.toggle('active', isActive);
        if (itemId) {
            btn.setAttribute('data-wishlist-item-id', itemId);
        } else {
            btn.removeAttribute('data-wishlist-item-id');
        }

        var icon = btn.querySelector('i');
        if (!icon) return;
        icon.classList.toggle('bi-heart-fill', isActive);
        icon.classList.toggle('bi-heart', !isActive);
    }

    function bindToggle(btn) {
        btn.addEventListener('click', function (e) {
            e.preventDefault();

            if (!isLoggedIn()) {
                window.location.href = '/Home/Login';
                return;
            }

            var productId = parseInt(btn.getAttribute('data-product-id'), 10);
            var wishlistItemId = btn.getAttribute('data-wishlist-item-id');
            var isActive = btn.classList.contains('active');

            if (isActive && wishlistItemId) {
                fetch('/api/Wishlist/items/' + wishlistItemId, {
                    method: 'DELETE',
                    headers: authHeaders()
                })
                    .then(function (res) {
                        if (!res.ok) {
                            return res.json().then(function (body) {
                                var msg = (body && body.message) ? body.message : 'Failed to remove from wishlist.';
                                alert(msg);
                            }).catch(function () { alert('Failed to remove from wishlist.'); });
                        }
                        setButtonState(btn, false);
                    })
                    .catch(function (err) {
                        console.error(err);
                        alert('Failed to remove from wishlist.');
                    });
                return;
            }

            fetch('/api/Wishlist', {
                method: 'POST',
                headers: authHeaders(),
                body: JSON.stringify({ productId: productId })
            })
                .then(function (res) {
                    if (!res.ok) {
                        return res.json().then(function (body) {
                            var msg = (body && body.message) ? body.message : 'Failed to add to wishlist.';
                            alert(msg);
                        }).catch(function () { alert('Failed to add to wishlist.'); });
                    }

                    return res.json().then(function (data) {
                        setButtonState(btn, true, data && data.id ? data.id : null);
                    });
                })
                .catch(function (err) {
                    console.error(err);
                    alert('Failed to add to wishlist.');
                });
        });
    }

    var buttons = document.querySelectorAll('.btn-add-wishlist');
    buttons.forEach(bindToggle);

    if (!isLoggedIn()) return;

    // Preload wishlist so hearts are accurate on Shop page.
    fetch('/api/Wishlist', {
        method: 'GET',
        headers: authHeaders()
    })
        .then(function (res) {
            if (!res.ok) return [];
            return res.json();
        })
        .then(function (items) {
            if (!Array.isArray(items)) return;
            var byProduct = {};
            items.forEach(function (item) {
                byProduct[item.productId] = item.id;
            });

            buttons.forEach(function (btn) {
                var productId = parseInt(btn.getAttribute('data-product-id'), 10);
                if (byProduct[productId]) {
                    setButtonState(btn, true, byProduct[productId]);
                }
            });
        })
        .catch(function (err) {
            console.error(err);
        });
});
