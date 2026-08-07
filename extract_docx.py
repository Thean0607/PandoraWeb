import zipfile
import xml.etree.ElementTree as ET
import re
from pathlib import Path

docx_path = Path(r"c:\Users\thean\Desktop\Đồ án cơ sở\Đồ án cơ sở\Biểu mẫu Tiểu luận - Đồ án.docx")
out_path = Path(r"C:\Users\thean\Desktop\PandoraWeb\docx_extract_output.txt")

NS = {"w": "http://schemas.openxmlformats.org/wordprocessingml/2006/main"}
W = "{http://schemas.openxmlformats.org/wordprocessingml/2006/main}"

def get_paragraph_text(p):
    texts = []
    for t in p.findall(".//w:t", NS):
        if t.text:
            texts.append(t.text)
        if t.tail:
            texts.append(t.tail)
    return "".join(texts).strip()

def get_style(p):
    pPr = p.find("w:pPr", NS)
    if pPr is not None:
        pStyle = pPr.find("w:pStyle", NS)
        if pStyle is not None:
            return pStyle.get(W + "val")
    return None

def get_outline_level(p):
    pPr = p.find("w:pPr", NS)
    if pPr is not None:
        ol = pPr.find("w:outlineLvl", NS)
        if ol is not None:
            return int(ol.get(W + "val"))
    return None

def is_heading_like(text, style, ol):
    if ol is not None:
        return True
    if style and ("Heading" in style or style.startswith("1") or "TieuDe" in style or "heading" in style.lower()):
        return True
    # Vietnamese thesis patterns
    patterns = [
        r"^LỜI MỞ ĐẦU", r"^Lời mở đầu", r"^CHƯƠNG\s+\d", r"^Chương\s+\d",
        r"^MỤC LỤC", r"^Mục lục", r"^TÓM TẮT", r"^Tóm tắt", r"^KẾT LUẬN",
        r"^Kết luận", r"^TÀI LIỆU", r"^Tài liệu", r"^PHỤ LỤC", r"^Phụ lục",
        r"^\d+\.\d+", r"^\d+\.",
    ]
    for pat in patterns:
        if re.match(pat, text, re.I):
            return True
    return False

def classify_placeholder(text):
    if not text:
        return "empty"
    t = text.strip()
    placeholders = [
        r"^\.{3,}$", r"^\.{2,}\s*$", r"^\[\.\.\.\]", r"^\(\.\.\.\)",
        r"^\.{5,}", r"^_{2,}", r"^\.\.\.$",
    ]
    for pat in placeholders:
        if re.match(pat, t):
            return "placeholder"
    if re.search(r"(\.{4,}|_{3,}|\[\s*\.\.\.\s*\])", t):
        return "mixed_placeholder"
    # instruction markers
    instr = [
        r"^\*", r"^Lưu ý", r"^Chú ý", r"^Ghi chú", r"^Hướng dẫn",
        r"^\(", r"^VD:", r"^Ví dụ:", r"italic", r"cần", r"không được",
        r"sinh viên", r"viết", r"bắt buộc", r"tối thiểu", r"tối đa",
    ]
    for pat in instr:
        if re.search(pat, t, re.I):
            return "possible_instruction"
    if len(t) < 3 and t in (".", "..", "..."):
        return "placeholder"
    return "content"

if not docx_path.exists():
    raise SystemExit(f"File not found: {docx_path}")

with zipfile.ZipFile(docx_path, "r") as z:
    xml = z.read("word/document.xml")
    try:
        styles_xml = z.read("word/styles.xml")
    except KeyError:
        styles_xml = None

root = ET.fromstring(xml)
body = root.find("w:body", NS)
paragraphs = body.findall("w:p", NS)

lines = []
for i, p in enumerate(paragraphs, 1):
    text = get_paragraph_text(p)
    style = get_style(p)
    ol = get_outline_level(p)
    lines.append({"idx": i, "text": text, "style": style, "ol": ol})

# Build outline from headings
outline = []
for L in lines:
    text = L["text"]
    if not text:
        continue
    if is_heading_like(text, L["style"], L["ol"]) or L["style"] in ("Heading1", "Heading2", "Heading3", "1", "2", "3"):
        outline.append((L["idx"], text, L["style"], L["ol"]))

# Extract sections under Loi mo dau and Chuong 1
def find_section_subtitles(start_pattern, next_major_patterns):
    subs = []
    in_section = False
    for L in lines:
        text = L["text"]
        if not in_section:
            if re.search(start_pattern, text, re.I):
                in_section = True
                subs.append(("SECTION_START", L["idx"], text))
            continue
        if text and any(re.match(p, text, re.I) for p in next_major_patterns):
            break
        if text and re.match(r"^\d+\.\d+\.?\s+", text):
            subs.append(("subsection", L["idx"], text))
        elif text and re.match(r"^\d+\.\s+[A-Za-zÀ-ỹ]", text) and "Chương" not in text:
            subs.append(("subsection_num", L["idx"], text))
    return subs

loi_mo_dau = find_section_subtitles(
    r"Lời mở đầu|LỜI MỞ ĐẦU",
    [r"^CHƯƠNG\s+1", r"^Chương\s+1", r"^CHƯƠNG\s+I", r"^MỤC LỤC", r"^TÓM TẮT"],
)
chuong1 = find_section_subtitles(
    r"^CHƯƠNG\s+1|^Chương\s+1|^CHƯƠNG\s+I\b",
    [r"^CHƯƠNG\s+2|^Chương\s+2|^CHƯƠNG\s+II"],
)

instructions = []
for L in lines:
    text = L["text"]
    if not text:
        continue
    cat = classify_placeholder(text)
    if cat in ("possible_instruction", "mixed_placeholder"):
        instructions.append((L["idx"], cat, text))
    # also catch explicit notes in parentheses at start
    if re.match(r"^\*", text) or re.match(r"^\(", text) and len(text) > 20:
        if (L["idx"], cat, text) not in instructions:
            instructions.append((L["idx"], "instruction", text))

with open(out_path, "w", encoding="utf-8") as f:
    f.write(f"SOURCE: {docx_path}\n\n")
    f.write("=" * 80 + "\n")
    f.write("1. COMPLETE OUTLINE / HEADINGS (in order)\n")
    f.write("=" * 80 + "\n")
    if outline:
        for idx, text, style, ol in outline:
            f.write(f"  [{idx:4d}] {text}")
            if style or ol is not None:
                f.write(f"  (style={style}, outlineLvl={ol})")
            f.write("\n")
    else:
        f.write("  (No explicit outline styles — listing candidate headings by text pattern)\n")
        for L in lines:
            t = L["text"]
            if t and is_heading_like(t, L["style"], L["ol"]):
                f.write(f"  [{L['idx']:4d}] {t}\n")

    f.write("\n" + "=" * 80 + "\n")
    f.write("2. PLACEHOLDERS vs FILLED (non-empty paragraphs)\n")
    f.write("=" * 80 + "\n")
    stats = {}
    for L in lines:
        t = L["text"]
        cat = classify_placeholder(t)
        stats[cat] = stats.get(cat, 0) + 1
        if t or cat == "empty":
            if cat != "content" or t:
                marker = cat.upper()
                if t:
                    f.write(f"  [{L['idx']:4d}] [{marker}] {t[:200]}{'...' if len(t)>200 else ''}\n")
                elif cat == "empty":
                    pass  # skip most empties
    f.write(f"\nStats: {stats}\n")

    f.write("\n" + "=" * 80 + "\n")
    f.write("3. SUBSECTIONS UNDER 'Lời mở đầu'\n")
    f.write("=" * 80 + "\n")
    for kind, idx, text in loi_mo_dau:
        f.write(f"  [{idx:4d}] ({kind}) {text}\n")

    f.write("\n" + "=" * 80 + "\n")
    f.write("3b. SUBSECTIONS UNDER 'Chương 1'\n")
    f.write("=" * 80 + "\n")
    for kind, idx, text in chuong1:
        f.write(f"  [{idx:4d}] ({kind}) {text}\n")

    f.write("\n" + "=" * 80 + "\n")
    f.write("4. INSTRUCTIONS IN TEMPLATE\n")
    f.write("=" * 80 + "\n")
    for idx, cat, text in instructions:
        f.write(f"  [{idx:4d}] {text}\n")

    f.write("\n" + "=" * 80 + "\n")
    f.write("FULL EXTRACTED TEXT (line numbers = paragraph index)\n")
    f.write("=" * 80 + "\n")
    for L in lines:
        t = L["text"]
        display = t if t else ""
        extra = []
        if L["style"]:
            extra.append(f"style={L['style']}")
        if L["ol"] is not None:
            extra.append(f"outlineLvl={L['ol']}")
        suffix = f"  /* {'; '.join(extra)} */" if extra else ""
        f.write(f"{L['idx']:4d}| {display}{suffix}\n")

print(f"Wrote {out_path}")
print(f"Paragraphs: {len(lines)}")
