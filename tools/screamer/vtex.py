"""Writes the texture source of a screamer picture.

The Workshop Tools do not compile a bare PNG: a .vtex source next to it does. It is named
<name>_png.vtex, the Source 2 name of a texture made from <name>.png, so it compiles to
panorama/images/custom_game/uikit/screamer/<name>_png.vtex_c, which the kit stylesheet points at
(s2r://panorama/images/custom_game/uikit/screamer/<name>_png.vtex). Same DMX format as the .vtex files that
compile in cs2-light-battle-royal.
"""

from __future__ import annotations

from pathlib import Path

from .paths import IMAGES_DIR

# Uncompressed: DXT1 came out with wrong colours in game (pink background, white picture).
# ~8 MB for 1920x1080, loaded once.
OUTPUT_FORMAT = "RGBA8888"

TEMPLATE = """<!-- dmx encoding keyvalues2_noids 1 format vtex 1 -->
"CDmeVtex"
{{
    "m_inputTextureArray" "element_array"
    [
        "CDmeInputTexture"
        {{
            "m_name" "string" "InputTexture0"
            "m_fileName" "string" "{source}"
            "m_colorSpace" "string" "srgb"
            "m_typeString" "string" "2D"
            "m_imageProcessorArray" "element_array"
            [
                "CDmeImageProcessor"
                {{
                    "m_algorithm" "string" "None"
                    "m_stringArg" "string" ""
                    "m_vFloat4Arg" "vector4" "0 0 0 0"
                }}
            ]
        }}
    ]
    "m_outputTypeString" "string" "2D"
    "m_outputFormat" "string" "{output_format}"
    "m_outputClearColor" "vector4" "0 0 0 0"
    "m_nOutputMinDimension" "int" "0"
    "m_nOutputMaxDimension" "int" "0"
    "m_textureOutputChannelArray" "element_array"
    [
        "CDmeTextureOutputChannel"
        {{
            "m_inputTextureArray" "string_array" [ "InputTexture0" ]
            "m_srcChannels" "string" "rgba"
            "m_dstChannels" "string" "rgba"
            "m_mipAlgorithm" "CDmeImageProcessor"
            {{
                "m_algorithm" "string" "Box"
                "m_stringArg" "string" ""
                "m_vFloat4Arg" "vector4" "0 0 0 0"
            }}
            "m_outputColorSpace" "string" "srgb"
        }}
    ]
    "m_vClamp" "vector3" "0 0 0"
    "m_bNoLod" "bool" "1"
}}
"""


def target(name: str) -> Path:
    return IMAGES_DIR / f"{name}_png.vtex"


def source_path(name: str) -> str:
    """The PNG, as the addon content sees it."""
    return f"panorama/images/custom_game/uikit/screamer/{name}.png"


def content(name: str) -> str:
    return TEMPLATE.format(source=source_path(name), output_format=OUTPUT_FORMAT)


def build(name: str) -> Path:
    destination = target(name)
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text(content(name), encoding="utf-8", newline="\n")
    return destination


def remove(name: str) -> None:
    target(name).unlink(missing_ok=True)
