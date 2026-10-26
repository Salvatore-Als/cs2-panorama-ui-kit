"""Small Tk window: pick a picture, choose which kit screamer it becomes, create the image and its .vtex.

The sound is not here: the kit plays ScreamerOptions.Sound (sounds/jumpscare.vsnd).
"""

from __future__ import annotations

import tkinter as tk
from pathlib import Path
from tkinter import filedialog, messagebox, ttk

from PIL import Image, ImageTk

from . import picture, vtex
from .paths import ADDON_ROOT, IMAGES_DIR, NAMES, display

PREVIEW_SIZE: tuple[int, int] = (480, 270)
SOURCE_TYPES = [("Pictures", "*.png *.jpg *.jpeg *.webp *.bmp *.gif *.tga"), ("All files", "*.*")]


def suggest_name(path: Path) -> str:
    """Kit screamer guessed from a file name ('Momo (2).PNG' -> 'momo'), else the first free one."""
    stem = path.stem.lower()
    for name in NAMES:
        if name in stem:
            return name
    for name in NAMES:
        if not (IMAGES_DIR / f"{name}.png").is_file():
            return name
    return NAMES[0]


class ScreamerApp:
    def __init__(self, root: tk.Tk) -> None:
        self._root = root
        self._source_path: Path | None = None
        self._source: Image.Image | None = None
        self._preview_photo: ImageTk.PhotoImage | None = None

        self._name = tk.StringVar(value=NAMES[0])
        self._width = tk.StringVar(value=str(picture.DEFAULT_SIZE[0]))
        self._height = tk.StringVar(value=str(picture.DEFAULT_SIZE[1]))
        self._source_label = tk.StringVar(value="No picture selected")
        self._info = tk.StringVar()
        self._status = tk.StringVar(value=f"Addon {ADDON_ROOT}: {display(IMAGES_DIR)}")

        root.title("Panorama UI Kit - screamers")
        root.minsize(820, 420)
        self._build()
        self._refresh_list()

        for variable in (self._width, self._height):
            variable.trace_add("write", lambda *_: self._refresh_preview())

    # ---- Layout ----

    def _build(self) -> None:
        main = ttk.Frame(self._root, padding=12)
        main.grid(row=0, column=0, sticky="nsew")
        self._root.columnconfigure(0, weight=1)
        self._root.rowconfigure(0, weight=1)
        main.columnconfigure(1, weight=1)
        main.rowconfigure(0, weight=1)

        existing = ttk.LabelFrame(main, text="Screamers", padding=8)
        existing.grid(row=0, column=0, sticky="nsw", padx=(0, 12))
        existing.rowconfigure(0, weight=1)
        self._list = tk.Listbox(existing, width=24, exportselection=False)
        self._list.grid(row=0, column=0, columnspan=2, sticky="nsew")
        self._list.bind("<<ListboxSelect>>", lambda _: self._show_existing())
        ttk.Button(existing, text="Delete", command=self._delete).grid(row=1, column=0, sticky="ew", pady=(8, 0))
        ttk.Button(existing, text="Refresh", command=self._refresh_list).grid(row=1, column=1, sticky="ew", pady=(8, 0))

        create = ttk.LabelFrame(main, text="New picture", padding=8)
        create.grid(row=0, column=1, sticky="nsew")
        create.columnconfigure(1, weight=1)

        ttk.Button(create, text="Choose picture...", command=self._choose).grid(row=0, column=0, sticky="w")
        ttk.Label(create, textvariable=self._source_label).grid(row=0, column=1, columnspan=3, sticky="w", padx=8)

        ttk.Label(create, text="Screamer").grid(row=1, column=0, sticky="w", pady=(8, 0))
        ttk.Combobox(create, textvariable=self._name, values=NAMES, state="readonly").grid(
            row=1, column=1, columnspan=3, sticky="ew", padx=8, pady=(8, 0))

        ttk.Label(create, text="Size").grid(row=2, column=0, sticky="w", pady=(8, 0))
        size = ttk.Frame(create)
        size.grid(row=2, column=1, columnspan=3, sticky="w", padx=8, pady=(8, 0))
        ttk.Entry(size, textvariable=self._width, width=6).grid(row=0, column=0)
        ttk.Label(size, text=" x ").grid(row=0, column=1)
        ttk.Entry(size, textvariable=self._height, width=6).grid(row=0, column=2)
        ttk.Label(size, text="  (fitted to the screen, black around)").grid(row=0, column=3)


        self._preview = tk.Label(create, background="#202020", width=PREVIEW_SIZE[0], height=PREVIEW_SIZE[1])
        self._preview.grid(row=4, column=0, columnspan=4, pady=(12, 0))
        ttk.Label(create, textvariable=self._info).grid(row=5, column=0, columnspan=4, sticky="w", pady=(4, 0))

        ttk.Button(create, text="Create", command=self._create).grid(row=6, column=3, sticky="e", pady=(8, 0))

        ttk.Label(main, textvariable=self._status, foreground="#666666").grid(row=1, column=0, columnspan=2, sticky="w", pady=(8, 0))

    # ---- Helpers ----

    def _canvas_size(self) -> tuple[int, int] | None:
        try:
            width = int(self._width.get())
            height = int(self._height.get())
        except ValueError:
            return None
        if width < 16 or height < 16 or width > 8192 or height > 8192:
            return None
        return width, height

    def _show(self, image: Image.Image | None) -> None:
        if image is None:
            self._preview_photo = None
            self._preview.configure(image="")
            return
        thumbnail = image.copy()
        thumbnail.thumbnail(PREVIEW_SIZE, Image.Resampling.LANCZOS)
        self._preview_photo = ImageTk.PhotoImage(thumbnail)
        self._preview.configure(image=self._preview_photo)

    # ---- Actions ----

    def _refresh_list(self) -> None:
        self._list.delete(0, tk.END)
        for name in NAMES:
            self._list.insert(tk.END, name)

    def _selected(self) -> str | None:
        selection = self._list.curselection()
        if not selection:
            return None
        return self._list.get(selection[0])

    def _show_existing(self) -> None:
        name = self._selected()
        if name is None:
            return
        self._name.set(name)
        path = IMAGES_DIR / f"{name}.png"
        if not path.is_file():
            self._show(None)
            self._info.set(f"'{name}': no picture yet ({display(path)})")
            return
        with Image.open(path) as image:
            image.load()
            self._show(image)
            self._info.set(f"'{name}': {image.width} x {image.height}")

    def _choose(self) -> None:
        chosen = filedialog.askopenfilename(title="Screamer picture", filetypes=SOURCE_TYPES)
        if not chosen:
            return
        path = Path(chosen)
        try:
            with Image.open(path) as image:
                image.load()
                self._source = image.copy()
        except OSError as error:
            messagebox.showerror("Screamer", f"Cannot open {path.name}: {error}")
            return
        self._source_path = path
        self._source_label.set(f"{path.name} ({self._source.width} x {self._source.height})")
        self._name.set(suggest_name(path))
        self._list.selection_clear(0, tk.END)
        self._refresh_preview()

    def _refresh_preview(self) -> None:
        if self._source is None:
            return
        size = self._canvas_size()
        if size is None:
            self._info.set("Size must be two numbers between 16 and 8192")
            return
        placement = picture.place(self._source.size, size)
        self._show(picture.compose(self._source, size))
        if placement.width == size[0] and placement.height == size[1]:
            self._info.set("Fills the whole screen")
        else:
            self._info.set(f"Fitted to {placement.width} x {placement.height}, black around")

    def _create(self) -> None:
        if self._source_path is None:
            messagebox.showwarning("Screamer", "Choose a picture first.")
            return
        size = self._canvas_size()
        if size is None:
            messagebox.showwarning("Screamer", "Size must be two numbers between 16 and 8192.")
            return
        name = self._name.get().strip()
        if name not in NAMES:
            messagebox.showwarning("Screamer", "Choose one of the kit screamers.")
            return

        target = IMAGES_DIR / f"{name}.png"
        if target.exists() and not messagebox.askyesno("Screamer", f"'{name}' already exists. Replace it?"):
            return

        try:
            picture.build(self._source_path, target, size)
            vtex.build(name)
        except (OSError, ValueError) as error:
            messagebox.showerror("Screamer", f"Creation failed: {error}")
            return

        self._refresh_list()
        self._status.set(f"Created '{name}'. Rebuild the addon, then ScreamerOptions.Name = \"{name}\".")

    def _delete(self) -> None:
        name = self._selected()
        if name is None:
            return
        if not messagebox.askyesno("Screamer", f"Delete '{name}' (picture and .vtex)?"):
            return
        try:
            (IMAGES_DIR / f"{name}.png").unlink(missing_ok=True)
            vtex.remove(name)
        except (OSError, ValueError) as error:
            messagebox.showerror("Screamer", f"Delete failed: {error}")
            return
        self._show(None)
        self._info.set("")
        self._refresh_list()
        self._status.set(f"Deleted '{name}'.")


def run() -> None:
    root = tk.Tk()
    ScreamerApp(root)
    root.mainloop()
