(function (global) {
    function okText() {
        var meta = document.querySelector('meta[name="swal-ok"]');
        return (meta && meta.getAttribute('content')) || 'OK';
    }

    var isAr = function () { return (document.documentElement.lang || '').toLowerCase().indexOf('ar') === 0; };

    // [pattern, Arabic text, English text]. $1 is the first capture group.
    var RULES = [
        [/failed to fetch|networkerror|load failed|network request failed/i, 'تعذر الاتصال بالخادم. تأكد من اتصالك بالإنترنت وحاول مرة أخرى.', 'Could not reach the server. Check your connection and try again.'],
        [/invalid email or password/i, 'البريد الإلكتروني أو كلمة المرور غير صحيحة.', 'Invalid email or password.'],
        [/insufficient stock\.? available: ?(\d+)/i, 'الكمية المطلوبة غير متوفرة. المتاح: $1', 'Insufficient stock. Available: $1'],
        [/product .*not found/i, 'هذا الكتاب غير متوفر حاليًا.', 'This book is no longer available.'],
        [/failed to add to cart/i, 'تعذرت إضافة الكتاب إلى السلة. حاول مرة أخرى.', 'Could not add the book to your cart. Please try again.'],
        [/failed to update quantity/i, 'تعذر تحديث الكمية. حاول مرة أخرى.', 'Could not update the quantity. Please try again.'],
        [/failed to (remove|delete)/i, 'تعذر الحذف. حاول مرة أخرى.', 'Could not remove it. Please try again.'],
        [/failed to add to wishlist/i, 'تعذرت الإضافة إلى المفضلة. حاول مرة أخرى.', 'Could not add to your wishlist. Please try again.'],
        [/failed to remove from wishlist/i, 'تعذرت الإزالة من المفضلة. حاول مرة أخرى.', 'Could not remove from your wishlist. Please try again.'],
        [/already registered/i, 'هذا البريد الإلكتروني مسجّل بالفعل.', 'This email is already registered.'],
        [/agree to (the )?terms/i, 'يجب الموافقة على شروط الخدمة.', 'You must agree to the Terms of Service.'],
        [/password must be at least (\d+)/i, 'كلمة المرور يجب ألا تقل عن $1 أحرف وتحتوي على حرف كبير وصغير ورقم.', 'Password must be at least $1 characters with upper and lower case letters and a number.'],
        [/email is required/i, 'البريد الإلكتروني مطلوب.', 'Email is required.'],
        [/password is required/i, 'كلمة المرور مطلوبة.', 'Password is required.'],
        [/(valid )?email address|invalid email/i, 'اكتب بريدًا إلكترونيًا صحيحًا.', 'Enter a valid email address.'],
        [/field quantity|quantity must/i, 'الكمية غير صحيحة.', 'The quantity is not valid.'],
        [/one or more validation errors/i, 'راجع البيانات المدخلة وحاول مرة أخرى.', 'Please check the details you entered and try again.'],
        [/unauthorized|session expired|not authenticated/i, 'انتهت الجلسة. سجّل الدخول وحاول مرة أخرى.', 'Your session has ended. Please log in and try again.'],
        [/unexpected error|something went wrong|internal server error/i, 'حدث خطأ غير متوقع. حاول مرة أخرى بعد قليل.', 'An unexpected error occurred. Please try again shortly.']
    ];

    // Turn a raw server/network message into friendly text in the page language.
    global.qaroLocalize = function (message) {
        var raw = String(message == null ? '' : message);
        for (var i = 0; i < RULES.length; i++) {
            var m = raw.match(RULES[i][0]);
            if (m) return (isAr() ? RULES[i][1] : RULES[i][2]).replace('$1', m[1] || '');
        }
        // an English technical sentence shown to an Arabic visitor: use the generic Arabic line
        if (isAr() && /^[\x00-\x7F]*[A-Za-z]{3}[\x00-\x7F]*$/.test(raw) && raw.length > 3) return 'حدث خطأ. حاول مرة أخرى.';
        return raw;
    };

    function fire(message, icon) {
        var text = icon === 'error' ? global.qaroLocalize(message) : message;
        if (global.Swal && typeof global.Swal.fire === 'function') {
            return global.Swal.fire({
                icon: icon || 'info',
                title: text,
                confirmButtonText: okText()
            });
        }
        global.alert(text);
        return Promise.resolve();
    }

    global.showAlert = function (message, icon) {
        return fire(message, icon);
    };

    global.showSuccess = function (message) {
        return fire(message, 'success');
    };

    global.showError = function (message) {
        return fire(message, 'error');
    };
})(window);
