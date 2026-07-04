// Initial Mock Data
const MOCK_DATA = {
    products: [
        { id: 1, name: 'Nhẫn Đính Kim Cương Tinh Tế', category: 'Nhẫn', price: 12500000, stock: 15, status: 'active' },
        { id: 2, name: 'Dây Chuyền Vàng Mặt Tròn', category: 'Dây Chuyền', price: 8200000, stock: 8, status: 'active' },
        { id: 3, name: 'Lắc Tay Bạc Đính Đá', category: 'Lắc Tay', price: 3150000, stock: 0, status: 'inactive' },
        { id: 4, name: 'Hoa Tai Vàng Hồng Ngọc Trai', category: 'Hoa Tai', price: 5600000, stock: 20, status: 'active' }
    ],
    customers: [
        { id: 1, name: 'Nguyễn Văn A', email: 'nguyenvana@email.com', phone: '0901234567', totalOrders: 5, status: 'active' },
        { id: 2, name: 'Trần Thị B', email: 'tranthib@email.com', phone: '0912345678', totalOrders: 2, status: 'active' },
        { id: 3, name: 'Lê Văn C', email: 'levanc@email.com', phone: '0987654321', totalOrders: 0, status: 'inactive' }
    ],
    employees: [
        { id: 1, name: 'Phạm Admin', email: 'admin@pandora.vn', roleId: 1, status: 'active' },
        { id: 2, name: 'Hoàng Nhân Viên', email: 'staff1@pandora.vn', roleId: 2, status: 'active' },
        { id: 3, name: 'Ngô Kho', email: 'warehouse@pandora.vn', roleId: 3, status: 'active' }
    ],
    roles: [
        { id: 1, name: 'Quản trị viên (Admin)', permissions: ['all'] },
        { id: 2, name: 'Chăm sóc khách hàng', permissions: ['read_product', 'manage_customer'] },
        { id: 3, name: 'Quản lý kho', permissions: ['manage_product'] }
    ]
};

// Initialize LocalStorage Data
function initAdminData() {
    if (!localStorage.getItem('admin_products')) {
        localStorage.setItem('admin_products', JSON.stringify(MOCK_DATA.products));
    }
    if (!localStorage.getItem('admin_customers')) {
        localStorage.setItem('admin_customers', JSON.stringify(MOCK_DATA.customers));
    }
    if (!localStorage.getItem('admin_employees')) {
        localStorage.setItem('admin_employees', JSON.stringify(MOCK_DATA.employees));
    }
    if (!localStorage.getItem('admin_roles')) {
        localStorage.setItem('admin_roles', JSON.stringify(MOCK_DATA.roles));
    }
}

// Format Currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

// DOM Loaded
document.addEventListener('DOMContentLoaded', () => {
    initAdminData();

    // Toggle Sidebar
    const toggleBtn = document.getElementById('toggleSidebar');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('mainContent');

    if (toggleBtn) {
        toggleBtn.addEventListener('click', () => {
            sidebar.classList.toggle('collapsed');
            mainContent.classList.toggle('expanded');
        });
    }
});

// Generic LocalStorage Utils
function getStorage(key) {
    return JSON.parse(localStorage.getItem(key) || '[]');
}

function setStorage(key, data) {
    localStorage.setItem(key, JSON.stringify(data));
}

function generateId(dataArray) {
    if (dataArray.length === 0) return 1;
    return Math.max(...dataArray.map(item => item.id)) + 1;
}
