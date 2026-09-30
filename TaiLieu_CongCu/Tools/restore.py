import os

source_dir = r'c:\Users\thean\Desktop\Đồ án cơ sở\Pandora'
views_dir = r'c:\Users\thean\Desktop\Đồ án cơ sở\PandoraWeb\Views'

files_to_restore = {
    'index.html': r'Home\Index.cshtml',
    'category.html': r'Product\Category.cshtml',
    'product-detail.html': r'Product\ProductDetail.cshtml',
    'about.html': r'Home\About.cshtml',
    'contact.html': r'Home\Contact.cshtml',
    'faq.html': r'Home\Faq.cshtml',
    'stores.html': r'Home\Stores.cshtml',
    'cart.html': r'Order\Cart.cshtml',
    'checkout.html': r'Order\Checkout.cshtml',
    'order-success.html': r'Order\OrderSuccess.cshtml',
    'orders.html': r'Order\Orders.cshtml',
    'wishlist.html': r'Order\Wishlist.cshtml',
    'login.html': r'Account\Login.cshtml',
    'signup.html': r'Account\Signup.cshtml',
    'change-password.html': r'Account\ChangePassword.cshtml',
    'profile.html': r'Account\Profile.cshtml',
    'address.html': r'Account\Address.cshtml'
}

for html_file, cshtml_rel_path in files_to_restore.items():
    html_path = os.path.join(source_dir, html_file)
    cshtml_path = os.path.join(views_dir, cshtml_rel_path)
    
    if not os.path.exists(html_path):
        continue
        
    with open(html_path, 'r', encoding='utf-8') as f:
        content = f.read()
        
    # Extract body content for layout
    if html_file not in ['login.html', 'signup.html']:
        nav_end = content.find('</nav>')
        if nav_end != -1:
            nav_end += 6
        footer_start = content.find('<footer')
        if nav_end != -1 and footer_start != -1:
            content = content[nav_end:footer_start].strip()
    
    # Add ViewBag based on original html
    view_name = os.path.basename(cshtml_path).replace('.cshtml', '')
    if html_file in ['login.html', 'signup.html']:
        header = f'@{{\n    Layout = null;\n    ViewBag.Title = "{view_name}";\n}}\n\n'
    else:
        header = f'@{{\n    Layout = "~/Views/Shared/_Layout.cshtml";\n    ViewBag.Title = "{view_name}";\n}}\n\n'
        
    final_content = header + content
    
    # Write with UTF-8 BOM
    with open(cshtml_path, 'w', encoding='utf-8-sig') as f:
        f.write(final_content)
        
print('Restored all files with UTF-8-SIG (BOM)')
