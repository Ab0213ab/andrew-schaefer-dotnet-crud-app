const { createApp } = Vue;

const toastApp = {
    data() {
        return {
            toasts: []
        };
    },
    methods: {
        showToast(message, type = "success") {
            const id = Date.now();
            this.toasts.push({ id, message, type });

            // Auto-remove after 4 seconds
            setTimeout(() => this.removeToast(id), 4000);
        },
        removeToast(id) {
            this.toasts = this.toasts.filter(t => t.id !== id);
        }
    }
};

const vm = createApp(toastApp).mount('#toastApp');

// Global reference
window.showVueToast = function (message, type = "success") {
    if (vm && typeof vm.showToast === "function") {
        vm.showToast(message, type);
    } else {
        console.error("Toast app not mounted yet");
    }
};
