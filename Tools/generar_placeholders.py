"""Genera los placeholders de Zombie Slug con las dimensiones del arte definitivo.
Escala: 32 px = 1 m (PPU 32). Colores planos y saturados para que nadie los
confunda con arte final (regla del placeholder, U2-L5)."""
from PIL import Image, ImageDraw, ImageOps
import os, shutil

A = "Assets/_ZombieSlug/Art"
def guardar(img, ruta):
    os.makedirs(os.path.dirname(ruta), exist_ok=True); img.save(ruta)

def figura(w_canvas, h_canvas, w, h, cuerpo, cabeza, piernas, extra=None, fase=0, pose="normal"):
    """Figura humanoide simple anclada abajo al centro."""
    im = Image.new("RGBA", (w_canvas, h_canvas), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    cx = w_canvas // 2; base = h_canvas - 1
    if pose == "muerte":
        d.rectangle([cx - h // 2, base - w // 2, cx + h // 2, base], fill=cuerpo, outline=(20, 20, 20))
        d.rectangle([cx + h // 2 - h // 6, base - w // 2, cx + h // 2, base], fill=cabeza)
        return im
    cab = h // 5; pier = int(h * 0.38); tor = h - cab - pier
    bob = 1 if (pose == "idle" and fase == 1) else 0
    # piernas (alternan en caminar/correr)
    off = [0, 3, 0, -3][fase % 4] if pose in ("correr", "caminar") else 0
    pw = max(3, w // 3)
    d.rectangle([cx - pw - 1 + off, base - pier, cx - 1 + off, base], fill=piernas)
    d.rectangle([cx + 1 - off, base - pier, cx + pw + 1 - off, base], fill=piernas)
    y0 = base - pier - tor + bob
    d.rectangle([cx - w // 2, y0, cx + w // 2, base - pier], fill=cuerpo, outline=(20, 20, 20))
    d.rectangle([cx - cab // 2 - 2, y0 - cab, cx + cab // 2 + 2, y0], fill=cabeza, outline=(20, 20, 20))
    d.rectangle([cx + cab // 4, y0 - cab + 3, cx + cab // 4 + 2, y0 - cab + 5], fill=(10, 10, 10))  # ojo mirando a la derecha
    if extra: extra(d, cx, y0, w, tor, base, pier, pose)
    return im

# ---------------- Ken (JUG-01): figura 24 x 56 px = 0,75 x 1,75 m ----------------
def ken_extra(d, cx, y0, w, tor, base, pier, pose):
    d.rectangle([cx - w // 2 - 6, y0 + 3, cx - w // 2, y0 + tor - 4], fill=(214, 40, 40))   # morral rojo
    d.rectangle([cx - 3, y0 + 2, cx + 3, y0 + tor], fill=(205, 186, 140))                     # chaleco beige
    arm_y = y0 + 8
    if pose == "disparar":
        d.rectangle([cx, arm_y, cx + w // 2 + 12, arm_y + 4], fill=(150, 110, 80)); d.rectangle([cx + w // 2 + 8, arm_y - 1, cx + w // 2 + 14, arm_y + 2], fill=(40, 40, 40))
    else:
        d.rectangle([cx + w // 2 - 2, arm_y, cx + w // 2 + 2, arm_y + 14], fill=(150, 110, 80))
KEN = dict(cuerpo=(58, 110, 200), cabeza=(176, 128, 92), piernas=(46, 70, 120))
ken = {"idle_01": ("idle", 0), "idle_02": ("idle", 1), "correr_01": ("correr", 0), "correr_02": ("correr", 1),
       "correr_03": ("correr", 2), "correr_04": ("correr", 3), "saltar_01": ("saltar", 0), "disparar_01": ("disparar", 0),
       "dano_01": ("dano", 0), "muerte_01": ("muerte", 0)}
for n, (pose, f) in ken.items():
    im = figura(64, 64, 24, 56, extra=ken_extra, fase=f, pose=pose, **KEN)
    if pose == "dano": im = Image.blend(im, Image.new("RGBA", im.size, (255, 255, 255, 0)), 0.0); im = ImageOps.colorize(im.convert("L"), (90, 0, 0), (255, 220, 220)).convert("RGBA"); im.putalpha(figura(64, 64, 24, 56, extra=ken_extra, pose="normal", **KEN).getchannel("A"))
    guardar(im, f"{A}/Sprites/Jugador/spr_jugador_ken_{n}.png")

# ---------------- Enemigos ----------------
def enemigos(nombre, canvas, w, h, col, frames):
    for n, (pose, f) in frames.items():
        im = figura(canvas, canvas, w, h, fase=f, pose=pose, **col)
        guardar(im, f"{A}/Sprites/Enemigos/spr_enemigo_{nombre}_{n}.png")
CAM = dict(cuerpo=(235, 235, 225), cabeza=(232, 220, 120), piernas=(60, 90, 160))
COR = dict(cuerpo=(240, 140, 40), cabeza=(200, 210, 110), piernas=(80, 60, 50))
TAN = dict(cuerpo=(130, 70, 170), cabeza=(170, 190, 110), piernas=(60, 40, 80))
JEF = dict(cuerpo=(140, 20, 30), cabeza=(150, 170, 90), piernas=(50, 20, 20))
enemigos("caminante", 64, 26, 56, CAM, {"caminar_01": ("caminar", 0), "caminar_02": ("caminar", 2), "atacar_01": ("disparar", 0)})
enemigos("corredor", 64, 24, 52, COR, {"correr_01": ("correr", 0), "correr_02": ("correr", 2), "saltar_01": ("saltar", 0)})
enemigos("tanque", 96, 56, 80, TAN, {"caminar_01": ("caminar", 0), "caminar_02": ("caminar", 2)})
enemigos("jefe", 128, 96, 128, JEF, {"caminar_01": ("caminar", 0), "caminar_02": ("caminar", 2)})

# ---------------- Proyectiles ----------------
im = Image.new("RGBA", (8, 4), (255, 214, 60, 255)); guardar(im, f"{A}/Sprites/Proyectiles/spr_proyectil_bala.png")

# ---------------- Escenario ----------------
t = Image.new("RGBA", (32, 32), (70, 72, 78, 255)); d = ImageDraw.Draw(t)
d.rectangle([0, 0, 31, 2], fill=(236, 214, 170)); d.line([(0, 31), (31, 31)], fill=(40, 40, 44))
guardar(t, f"{A}/Tiles/tile_comun_suelo_01.png")
t = Image.new("RGBA", (32, 32), (0, 0, 0, 0)); d = ImageDraw.Draw(t)
d.rectangle([0, 0, 31, 9], fill=(110, 104, 96, 255), outline=(236, 214, 170, 255)); d.line([(0, 9), (31, 9)], fill=(60, 56, 52, 255))
guardar(t, f"{A}/Tiles/tile_comun_plataforma_01.png")
# Tablero (máquina expendedora de Don Kuro): 48 x 64 px = 1,5 x 2 m
t = Image.new("RGBA", (48, 64), (0, 0, 0, 0)); d = ImageDraw.Draw(t)
d.rectangle([2, 4, 45, 63], fill=(200, 50, 60, 255), outline=(30, 20, 20, 255))
d.rectangle([7, 10, 40, 34], fill=(150, 210, 230, 255)); [d.rectangle([9 + 8 * i, 13, 14 + 8 * i, 30], fill=c) for i, c in enumerate([(240, 200, 60), (60, 180, 90), (230, 230, 230), (90, 90, 200)])]
d.rectangle([7, 40, 40, 46], fill=(30, 30, 30, 255)); d.rectangle([30, 50, 40, 58], fill=(40, 40, 40, 255))
guardar(t, f"{A}/Sprites/Escenario/spr_escenario_tablero_01.png")
t = Image.new("RGBA", (8, 8), (60, 255, 120, 255)); guardar(t, f"{A}/Sprites/Escenario/spr_escenario_tablero_luz.png")
# UI
t = Image.new("RGBA", (16, 16), (0, 0, 0, 0)); d = ImageDraw.Draw(t); d.ellipse([1, 1, 14, 14], fill=(250, 200, 50, 255), outline=(150, 100, 20, 255))
guardar(t, f"{A}/Sprites/UI/spr_ui_moneda_01.png")
t = Image.new("RGBA", (4, 4), (255, 255, 255, 255)); guardar(t, f"{A}/Sprites/UI/spr_ui_blanco.png")

# ---------------- Fondos (3 capas por arena, 640 x 360 = 20 x 11,25 m) ----------------
src = "/home/claude/zs/fondo"
capas = ["lejana", "media", "cercana"]
tintes = {1: (1.0, 1.0, 1.0), 2: (0.75, 0.70, 1.15), 3: (1.20, 0.85, 0.65)}
for arena, (r, g, b) in tintes.items():
    for c in capas:
        im = Image.open(f"{src}/bg_esc1_capa_{c}.png").convert("RGBA")
        if arena != 1:
            R, G, B, Al = im.split()
            R = R.point(lambda v: min(255, int(v * r))); G = G.point(lambda v: min(255, int(v * g))); B = B.point(lambda v: min(255, int(v * b)))
            im = Image.merge("RGBA", (R, G, B, Al))
        guardar(im, f"{A}/Fondos/Arena{arena}/bg_arena{arena}_capa_{c}.png")
print("ok")
