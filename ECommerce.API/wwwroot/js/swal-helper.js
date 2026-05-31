(function (global) {
    function okText() {
        var meta = document.querySelector('meta[name="swal-ok"]');
        return (meta && meta.getAttribute('content')) || 'OK';
    }

    function fire(message, icon) {
        if (global.Swal && typeof global.Swal.fire === 'function') {
            return global.Swal.fire({
                icon: icon || 'info',
                title: message,
                confirmButtonText: okText()
            });
        }
        global.alert(message);
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
