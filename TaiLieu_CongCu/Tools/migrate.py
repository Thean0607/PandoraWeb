import os
import glob
import re

source_dir = r"c:\Users\thean\Desktop\Đồ án cơ sở\Pandora"
dest_dir = r"c:\Users\thean\Desktop\Đồ án cơ sở\PandoraWeb\Views\Home"
controller_path = r"c:\Users\thean\Desktop\Đồ án cơ sở\PandoraWeb\Controllers\HomeController.cs"

if not os.path.exists(dest_dir):
    os.makedirs(dest_dir)

files = glob.glob(os.path.join(source_dir, "*.html"))
excludes = ["admin", "index.html", "404.html"]

actions = []

for file_path in files:
    filename = os.path.basename(file_path)
    if any(filename.startswith(ex) or filename == ex for ex in excludes):
        continue

    with open(file_path, "r", encoding="utf-8") as f:
        content = f.read()

    # Find first </nav>
    nav_end = content.find("</nav>")
    if nav_end == -1:
        print(f"Skipping {filename}: no </nav>")
        continue
    nav_end += 6

    # Find first <footer
    footer_start = content.find("<footer")
    if footer_start == -1:
        print(f"Skipping {filename}: no <footer")
        continue

    body_content = content[nav_end:footer_start].strip()

    # Create view name
    name_parts = filename.replace(".html", "").split("-")
    view_name = "".join([p.capitalize() for p in name_parts])
    if view_name == "Productdetail": view_name = "ProductDetail"

    actions.append(view_name)

    cshtml_path = os.path.join(dest_dir, f"{view_name}.cshtml")
    
    out_content = f'@{{\n    Layout = "~/Views/Shared/_Layout.cshtml";\n    ViewBag.Title = "{view_name}";\n}}\n\n' + body_content

    with open(cshtml_path, "w", encoding="utf-8") as f:
        f.write(out_content)
    
    print(f"Generated {view_name}.cshtml")

# Now update HomeController.cs
with open(controller_path, "r", encoding="utf-8") as f:
    controller_content = f.read()

# Add actions before the last closing brace of the class
actions_code = ""
for action in actions:
    # check if action already exists
    if f"public ActionResult {action}()" not in controller_content:
        actions_code += f"""
        public ActionResult {action}()
        {{
            ViewBag.ActiveMenu = "{action}";
            ViewBag.Title = "{action}";
            return View();
        }}
"""

if actions_code:
    # insert before the last two closing braces (assuming namespace and class braces)
    last_brace = controller_content.rfind("}")
    second_last_brace = controller_content.rfind("}", 0, last_brace)
    
    new_controller_content = controller_content[:second_last_brace] + actions_code + controller_content[second_last_brace:]
    
    with open(controller_path, "w", encoding="utf-8") as f:
        f.write(new_controller_content)
    
    print("Updated HomeController.cs")

print("Done!")
