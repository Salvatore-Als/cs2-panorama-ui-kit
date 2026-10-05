#!/usr/bin/env bash
#
# Builds the kit and lays the result out like the server, in dist/:
#
#   dist/plugins/PanoramaUiKit/        the plugin and the DLLs it ships with
#   dist/plugins/PanoramaUiKit.Demo/   the demo consumer (skipped with --no-demo)
#   dist/shared/PanoramaUiKit.Shared/  the shared API, loaded once for every plugin
#
# Copy dist/* onto addons/counterstrikesharp/. The Shared DLL never goes next to a plugin: a second
# copy is another type identity and the capability would resolve to null.
#
#   ./build.sh                  Debug build
#   ./build.sh --release        Release build
#   ./build.sh --out DIR        write to DIR instead of dist/
#   ./build.sh --no-demo        leave the demo plugin out
#   ./build.sh --skip-checks    skip the layout contract check
#   ./build.sh --previews       also regenerate previews/*.preview.html from the layouts
#
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG="Debug"
OUT="$ROOT/dist"
DEMO=1
CHECKS=1
PREVIEWS=0
FRAMEWORK="net10.0"

usage() {
    sed -n '2,/^set -euo/p' "$0" | sed '$d' | sed 's/^# \{0,1\}//'
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --release) CONFIG="Release" ;;
        --out)
            if [[ $# -lt 2 ]]; then
                echo "build.sh: --out needs a directory" >&2
                exit 2
            fi
            OUT="$(cd "$(dirname "$2")" && pwd)/$(basename "$2")"
            shift
            ;;
        --no-demo) DEMO=0 ;;
        --skip-checks) CHECKS=0 ;;
        --previews) PREVIEWS=1 ;;
        -h|--help) usage; exit 0 ;;
        *)
            echo "build.sh: unknown option '$1'" >&2
            usage >&2
            exit 2
            ;;
    esac
    shift
done

step() {
    printf '\n==> %s\n' "$1"
}

# Copies the DLLs a project ships into dist/<kind>/<project>/, after emptying it so a file the
# project stopped shipping does not linger.
package() {
    local kind="$1" project="$2"
    local from="$ROOT/artifacts/bin/$project/$CONFIG/$FRAMEWORK"
    local to="$OUT/$kind/$project"

    if [[ ! -d "$from" ]]; then
        echo "build.sh: no build output for $project in $from" >&2
        exit 1
    fi

    rm -rf "$to"
    mkdir -p "$to"
    find "$from" -maxdepth 1 -type f \( -name '*.dll' -o -name '*.pdb' -o -name '*.deps.json' \) -exec cp {} "$to/" \;
    echo "    $kind/$project  ($(find "$to" -type f | wc -l | tr -d ' ') files)"
}

cd "$ROOT"

if [[ $CHECKS -eq 1 ]]; then
    step "Layout contracts"
    (cd tools && python3 -m uikit check-all)
fi

step "dotnet build ($CONFIG)"
dotnet build PanoramaUiKit.slnx -c "$CONFIG" --nologo -v q -clp:NoSummary

step "Packaging into ${OUT#"$ROOT"/}"
package shared PanoramaUiKit.Shared
package plugins PanoramaUiKit
if [[ $DEMO -eq 1 ]]; then
    package plugins PanoramaUiKit.Demo
else
    rm -rf "$OUT/plugins/PanoramaUiKit.Demo"
fi

if [[ $PREVIEWS -eq 1 ]]; then
    step "Previews"
    for layout in panorama/layout/custom_game/uikit_*.xml; do
        python3 .claude/skills/cs2-panorama-hud/scripts/preview.py "$layout" 3 | sed 's|^|    |'
    done
fi

step "Done"
(cd "$OUT" && find . -type f -name '*.dll' | sort | sed 's|^\./|    |')
