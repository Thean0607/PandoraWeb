import zipfile
import xml.etree.ElementTree as ET
import re
from pathlib import Path

docx_path = Path(r"c:\Users\thean\Desktop\Đồ án cơ sở\Đồ án cơ sở\Biểu mẫu Tiểu luận - Đồ án.docx")
NS = {"w": "http://schemas.openxmlformats.org/wordprocessingml/2006/main"}
W = "{http://schemas.openxmlformats.org/wordprocessingml/2006/main}"

def get_paragraph_text(p):
    texts = []
    for t in p.findall(".//w:t", NS):
        if t.text: texts.append(t.text)
        if t.tail: texts.append(t.tail)
    return "".join(texts).strip()

def get_style(p):
    pPr = p.find("w:pPr", NS)
    if pPr is not None:
        pStyle = pPr.find("w:pStyle", NS)
        if pStyle is not None:
            return pStyle.get(W + "val")
    return None

with zipfile.ZipFile(docx_path, "r") as z:
    root = ET.fromstring(z.read("word/document.xml"))
body = root.find("w:body", NS)
paragraphs = body.findall("w:p", NS)
lines = [(i, get_paragraph_text(p), get_style(p)) for i, p in enumerate(paragraphs, 1)]

# Print region around key sections
for start, end, label in [(170, 265, "TOC and body"), (1, 41, "Instructions"), (115, 125, "Loi cam on / loi mo dau"), (240, 267, "Chuong 1 and refs")]:
    print(f"\n--- {label} [{start}-{end}] ---")
    for idx, text, style in lines:
        if start <= idx <= end:
            s = f" style={style}" if style else ""
            print(f"{idx:4d}| {text!r}{s}")

# Manual outline: all non-empty that look like structure
print("\n--- STRUCTURAL LINES (filtered) ---")
keywords = re.compile(
    r"MỤC LỤC|Lời mở đầu|LỜI MỞ|CHƯƠNG|Chương|KẾT LUẬN|PHỤ LỤC|Tài liệu|TÓM TẮT|Danh mục|LỜI CẢM|TỜ NHIỆM|^\d+\.\d|HƯỚNG DẪN|TRÌNH BÀY|^\d+\.\s",
    re.I
)
for idx, text, style in lines:
    if text and keywords.search(text):
        print(f"{idx:4d}| {text}")

# Loi mo dau block
print("\n--- BETWEEN 'Lời mở đầu' and 'CHƯƠNG 1' ---")
capturing = False
for idx, text, style in lines:
    if re.search(r"^Lời mở đầu", text, re.I):
        capturing = True
        print(f"{idx:4d}| {text}")
        continue
    if capturing:
        if re.search(r"^CHƯƠNG\s*1|^Chương\s*1", text, re.I) and "..." not in text:
            print(f"{idx:4d}| {text}")
            break
        if text or idx < 250:
            print(f"{idx:4d}| {text!r}")

# Chuong 1 block until chuong 2
print("\n--- CHƯƠNG 1 section until CHƯƠNG 2 ---")
capturing = False
for idx, text, style in lines:
    if re.search(r"^CHƯƠNG\s*1\s*\(|^CHƯƠNG\s*1$", text, re.I):
        capturing = True
    if capturing:
        print(f"{idx:4d}| {text!r}")
        if re.search(r"^CHƯƠNG\s*2|^Chương\s*2", text, re.I) and idx > 247:
            break
