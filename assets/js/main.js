document.addEventListener('DOMContentLoaded', function() {
    const formatCurrency = function(value) {
        return value.toLocaleString('vi-VN') + ' ₫';
    };

    const parseCurrency = function(text) {
        return Number(String(text).replace(/[^\d]/g, '')) || 0;
    };

    const getQuantityValue = function(input) {
        return Math.max(1, parseInt(input.value, 10) || 1);
    };

    const setQuantityValue = function(input, value) {
        input.value = Math.max(1, parseInt(value, 10) || 1);
    };

    // 1. Sticky Navbar
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        const defaultPadding = navbar.classList.contains('shadow-sm') ? '10px 0' : '15px 0';

        window.addEventListener('scroll', function() {
            if (window.scrollY > 50) {
                navbar.classList.add('shadow-sm');
                navbar.style.padding = '10px 0';
            } else {
                navbar.classList.toggle('shadow-sm', defaultPadding === '10px 0');
                navbar.style.padding = defaultPadding;
            }
        });
    }

    // 2. Keep active navigation consistent with the current page.
    const currentPage = window.location.pathname.split('/').pop() || 'index.html';
    document.querySelectorAll('.navbar .nav-link').forEach(function(link) {
        const linkPage = link.getAttribute('href');
        link.classList.toggle('active', linkPage === currentPage);
    });

    // 3. Quantity Input Logic (for Cart and Product Detail)
    const quantityInputs = document.querySelectorAll('.quantity-input');
    quantityInputs.forEach(function(wrapper) {
        const input = wrapper.querySelector('input');
        const btnMinus = wrapper.querySelector('.btn-minus');
        const btnPlus = wrapper.querySelector('.btn-plus');

        if (!input || !btnMinus || !btnPlus) {
            return;
        }

        input.setAttribute('inputmode', 'numeric');
        setQuantityValue(input, input.value);

        btnMinus.addEventListener('click', function() {
            setQuantityValue(input, getQuantityValue(input) - 1);
            input.dispatchEvent(new Event('change', { bubbles: true }));
        });

        btnPlus.addEventListener('click', function() {
            setQuantityValue(input, getQuantityValue(input) + 1);
            input.dispatchEvent(new Event('change', { bubbles: true }));
        });

        input.addEventListener('change', function() {
            setQuantityValue(input, input.value);
            updateCartSummary();
        });
    });

    // 4. Lightweight static cart behaviour for demo consistency.
    const updateCartSummary = function() {
        const cartRows = document.querySelectorAll('table tbody tr');
        if (!cartRows.length) {
            return;
        }

        let subtotal = 0;
        cartRows.forEach(function(row) {
            const priceEl = row.querySelector('.text-primary-gold.fw-bold');
            const quantityEl = row.querySelector('.quantity-input input');
            const lineTotalEl = row.querySelector('td:nth-child(3)');

            if (!priceEl || !quantityEl || !lineTotalEl) {
                return;
            }

            const lineTotal = parseCurrency(priceEl.textContent) * getQuantityValue(quantityEl);
            subtotal += lineTotal;
            lineTotalEl.textContent = formatCurrency(lineTotal);
        });

        const summaryValues = document.querySelectorAll('.col-lg-4 .d-flex.justify-content-between span.fw-bold, .col-lg-4 strong.text-primary-gold');
        summaryValues.forEach(function(el) {
            el.textContent = formatCurrency(subtotal);
        });

        const badge = document.querySelector('.fa-shopping-bag + .badge');
        if (badge) {
            const totalItems = Array.from(document.querySelectorAll('.quantity-input input')).reduce(function(total, input) {
                return total + getQuantityValue(input);
            }, 0);
            badge.textContent = totalItems;
        }
    };

    document.querySelectorAll('.table .btn-link.text-danger').forEach(function(button) {
        button.setAttribute('type', 'button');
        button.addEventListener('click', function() {
            const row = button.closest('tr');
            if (row) {
                row.remove();
                updateCartSummary();
            }
        });
    });
    updateCartSummary();

    // 5. Form validation with Bootstrap
    document.querySelectorAll('form').forEach(function(form) {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });

    const checkoutButton = document.querySelector('a[href="order-success.html"].btn-gold');
    const checkoutForm = document.querySelector('.checkout-form');
    if (checkoutButton && checkoutForm) {
        checkoutButton.addEventListener('click', function(event) {
            if (!checkoutForm.checkValidity()) {
                event.preventDefault();
                checkoutForm.classList.add('was-validated');
                checkoutForm.reportValidity();
            }
        });
    }

    document.querySelectorAll('a[href="#"]').forEach(function(link) {
        link.addEventListener('click', function(event) {
            if (!link.hasAttribute('title') || link.getAttribute('title') !== 'Thêm vào giỏ hàng') {
                event.preventDefault();
            }
        });
    });

    // 6. Initialize Bootstrap Tooltips (if any)
    if (window.bootstrap) {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function(tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // Handle Add to Cart via AJAX
    document.querySelectorAll('.btn-add-cart').forEach(function(btn) {
        btn.addEventListener('click', function(event) {
            event.preventDefault();
            const pid = btn.getAttribute('data-pid');
            if (!pid) return;

            // Fetch add to cart
            fetch('/Order/AddToCart', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams({ productId: pid, quantity: 1 })
            })
            .then(res => {
                if (!res.ok) throw new Error('Network response was not ok');
                return res.json();
            })
            .then(data => {
                console.log('AddToCart response:', data);
                if(data.success) {
                    const toastEl = document.getElementById('cartToast');
                    if (toastEl) {
                        toastEl.querySelector('.toast-body').innerHTML = '<i class="fas fa-check-circle me-2"></i> Đã thêm sản phẩm vào giỏ hàng!';
                        const toast = new bootstrap.Toast(toastEl, { delay: 3000 });
                        toast.show();
                    }
                    const badge = document.getElementById('cart-badge');
                    if (badge) badge.textContent = data.totalItems;
                } else {
                    alert('Lỗi: ' + data.message);
                }
            })
            .catch(err => {
                console.error('AddToCart error:', err);
                alert('Đã xảy ra lỗi khi thêm vào giỏ hàng.');
            });
        });
    });

    // Handle Remove Cart
    document.querySelectorAll('.btn-remove-cart').forEach(function(btn) {
        btn.addEventListener('click', function() {
            const pid = this.getAttribute('data-pid');
            const vid = this.getAttribute('data-vid');
            fetch('/Order/RemoveFromCart', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams({ productId: pid, variantId: vid })
            }).then(res => res.json()).then(data => {
                if (data.success) window.location.reload();
            });
        });
    });

    // Handle Update Cart
    document.querySelectorAll('.btn-update-cart').forEach(function(btn) {
        btn.addEventListener('click', function() {
            const pid = this.getAttribute('data-pid');
            const vid = this.getAttribute('data-vid');
            const action = this.getAttribute('data-action');
            const input = this.parentElement.querySelector('.cart-qty-input');
            let qty = parseInt(input.value);
            
            if (action === 'minus') qty = Math.max(1, qty - 1);
            else qty += 1;
            
            fetch('/Order/UpdateQuantity', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams({ productId: pid, variantId: vid, quantity: qty })
            }).then(res => res.json()).then(data => {
                if (data.success) window.location.reload();
            });
        });
    });    // Handle Add/Remove from Wishlist
    document.querySelectorAll('.btn-wishlist').forEach(function(btn) {
        btn.addEventListener('click', function(event) {
            event.preventDefault();
            const pid = btn.getAttribute('data-pid');
            // Toggle icon visual
            const icon = btn.querySelector('i');
            const isAdded = icon.classList.contains('fas');
            if (isAdded) {
                icon.classList.remove('fas');
                icon.classList.add('far');
                icon.style.color = '';
            } else {
                icon.classList.remove('far');
                icon.classList.add('fas');
                icon.style.color = '#d4af37';
            }

            fetch(isAdded ? '/Order/RemoveFromWishlist' : '/Order/AddToWishlist', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams({ productId: pid })
            })
            .then(res => {
                if (!res.ok) throw new Error('Network response was not ok');
                return res.json();
            })
            .then(data => {
                console.log('Wishlist response:', data);
                if (data.success) {
                    const toastEl = document.getElementById('cartToast');
                    if (toastEl) {
                        toastEl.querySelector('.toast-body').innerHTML = isAdded 
                            ? '<i class="fas fa-info-circle me-2"></i> Đã gỡ khỏi yêu thích!' 
                            : '<i class="fas fa-heart me-2"></i> Đã thêm vào yêu thích!';
                        const toast = new bootstrap.Toast(toastEl, { delay: 3000 });
                        toast.show();
                    }
                } else {
                    alert('Lỗi: ' + data.message);
                }
            })
            .catch(err => {
                console.error('Wishlist error:', err);
                alert('Đã xảy ra lỗi khi cập nhật yêu thích.');
            });
        });
    });

    // Handle Remove from Wishlist (in Wishlist page)
    document.querySelectorAll('.btn-remove-wishlist').forEach(function(btn) {
        btn.addEventListener('click', function() {
            const pid = this.getAttribute('data-pid');
            fetch('/Order/RemoveFromWishlist', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams({ productId: pid })
            })
            .then(res => res.json())
            .then(data => {
                if (data.success) window.location.reload();
            });
        });
    });
});

