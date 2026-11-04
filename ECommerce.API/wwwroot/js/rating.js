/* Star ratings: tap a star to rate a book (one rating per visitor, changeable). Every widget of the same book is updated. */
(function () {
    'use strict';
    var ICON = { full: 'bi-star-fill', half: 'bi-star-half', empty: 'bi-star' };

    function paint(w, value) {
        var shown = Math.round(value * 2) / 2;
        w.querySelectorAll('.rate__star').forEach(function (b) {
            var i = +b.getAttribute('data-stars'), ic = b.querySelector('i');
            ic.className = 'bi ' + (shown >= i ? ICON.full : shown >= i - 0.5 ? ICON.half : ICON.empty);
        });
    }
    function settle(w) {
        var avg = parseFloat(w.getAttribute('data-avg')) || 0, count = +w.getAttribute('data-count') || 0;
        paint(w, avg);
        w.classList.toggle('rate--mine', (+w.getAttribute('data-mine') || 0) > 0);
        w.querySelector('.rate__text').textContent = count ? avg.toFixed(1) + ' (' + count + ')' : w.getAttribute('data-rate-label');
    }

    document.addEventListener('mouseover', function (e) {
        var b = e.target.closest && e.target.closest('.rate__star'); if (!b) return;
        var w = b.closest('.rate'); paint(w, +b.getAttribute('data-stars'));
    });
    document.addEventListener('mouseout', function (e) {
        var b = e.target.closest && e.target.closest('.rate__star'); if (!b) return;
        settle(b.closest('.rate'));
    });

    document.addEventListener('click', function (e) {
        var b = e.target.closest && e.target.closest('.rate__star'); if (!b) return;
        e.preventDefault();
        var w = b.closest('.rate'), id = +w.getAttribute('data-product-id'), stars = +b.getAttribute('data-stars');
        if (w.getAttribute('data-busy')) return;
        w.setAttribute('data-busy', '1');
        fetch('/api/Ratings', { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ productId: id, stars: stars }) })
            .then(function (r) { if (!r.ok) throw new Error(r.status); return r.json(); })
            .then(function (d) {
                document.querySelectorAll('.rate[data-product-id="' + id + '"]').forEach(function (x) {
                    x.setAttribute('data-avg', d.average); x.setAttribute('data-count', d.count); x.setAttribute('data-mine', d.mine);
                    settle(x);
                });
                var t = w.querySelector('.rate__text'), old = t.textContent;
                t.textContent = w.getAttribute('data-thanks');
                setTimeout(function () { settle(w); }, 1600);
            })
            .catch(function () { settle(w); })
            .then(function () { w.removeAttribute('data-busy'); });
    });
})();
