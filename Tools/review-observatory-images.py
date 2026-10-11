"""Index actual Unity RenderTexture exports; no desktop/window/screen APIs."""
import argparse
import hashlib
import html
import json
from pathlib import Path
from PIL import Image, ImageDraw

parser = argparse.ArgumentParser()
parser.add_argument("--phase", choices=("before", "after"), default="after")
args = parser.parse_args()
repo = Path(__file__).resolve().parent.parent
evidence = repo / "Logs/ObservatoryVisual"
images = evidence / args.phase
manifest = json.loads((images / "render-manifest.json").read_text(encoding="utf-8-sig"))
files = [images / name for name in manifest["files"]]
records = []
for path in files:
    with Image.open(path) as im:
        im.load()
        assert im.width >= 960 and im.height >= 720, path
        assert max(hi - lo for lo, hi in im.convert("RGB").getextrema()) > 32, path
        records.append(dict(file=path.name, width=im.width, height=im.height,
                            sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
(evidence / f"{args.phase}-image-integrity.json").write_text(
    json.dumps(dict(count=len(records), checks="Decode, dimensions, nonuniform pixels and SHA256; NOT an automated readability verdict", images=records), indent=2), encoding="utf-8")

groups = {
    "unlocks": [p for p in files if p.name.startswith("unlocks-")],
    "resources": [p for p in files if p.name.startswith("resources-")],
    "tabs": [p for p in files if p.name.startswith("tabs-")],
    "readable": [p for p in files if p.name.startswith("readable-")],
    "night": [p for p in files if p.name.startswith("late-night-1920") and not p.stem.endswith(("-middle", "-bottom"))],
    "dawn": [p for p in files if p.name.startswith("late-dawn-1920") and not p.stem.endswith(("-middle", "-bottom"))],
    "large": [p for p in files if p.name.startswith("large-") and p.stem.endswith("-bottom")],
    "middle": [p for p in files if p.name.startswith("late-night-") and p.stem.endswith("-middle")],
    "bottom": [p for p in files if p.name.startswith("late-night-") and p.stem.endswith("-bottom")],
    "early": [p for p in files if p.name.startswith("early-night-1280-")],
    "qhd": [p for p in files if p.name.startswith("qhd-")],
    "controls": [p for p in files if p.name.startswith("control-")],
    "wide": [p for p in files if p.name.startswith("wide-")],
}
sheets = evidence / f"sheets-{args.phase}"
sheets.mkdir(exist_ok=True)
for group, members in groups.items():
    members.sort()
    for start in range(0, len(members), 4):
        sheet = Image.new("RGB", (1920, 1140), "#303030")
        draw = ImageDraw.Draw(sheet)
        for index, path in enumerate(members[start:start + 4]):
            with Image.open(path) as im:
                im.thumbnail((960, 540))
                x, y = index % 2 * 960, index // 2 * 570
                sheet.paste(im, (x, y + 28))
                draw.text((x + 8, y + 6), path.stem, fill="white")
        sheet.save(sheets / f"{group}-{start // 4 + 1}.jpg", quality=93)

gallery = repo / "Docs/UI-Minimalist-Futuristic/VISUAL-GALLERY.html"
items = []
for path in files:
    url = "../../Logs/ObservatoryVisual/" + args.phase + "/" + path.name
    items.append(f'<figure data-name="{html.escape(path.stem)}"><a href="{url}"><img loading="lazy" src="{url}" alt="{html.escape(path.stem)}"></a><figcaption>{html.escape(path.stem)}</figcaption></figure>')
gallery.write_text('''<!doctype html><html lang="en"><meta charset="utf-8"><title>Observatory visual review</title>
<style>body{background:#0b1118;color:#edf4fa;font:16px system-ui;margin:24px}header{max-width:1000px}input{padding:12px;width:360px;max-width:90%;font:inherit}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(500px,1fr));gap:20px}figure{margin:0;background:#121c27;padding:10px}img{width:100%;height:auto}figcaption{padding:10px;overflow-wrap:anywhere}a{color:#71dfd4}[hidden]{display:none}small{color:#a5b7c8}</style>
<header><h1>Observatory 2.4.3 — actual Unity UI renders</h1><p>Isolated early/late fixtures rendered by Unity URP directly to private GPU textures. No desktop capture or visible game window. These images do not measure player FPS or physical input usability.</p><p><small>Filter: readable, night, dawn, large (200%), qhd (125%), modal, control, wide, early. Click any image for full resolution.</small></p><input aria-label="Filter views" placeholder="Filter views" oninput="document.querySelectorAll('figure').forEach(f=>f.hidden=!f.dataset.name.includes(this.value.toLowerCase()))"></header><main>'''
    + "\n".join(items) + "</main></html>", encoding="utf-8")
print(json.dumps(dict(images=len(records), gallery=str(gallery))))
