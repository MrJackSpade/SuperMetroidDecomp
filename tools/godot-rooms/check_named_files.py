"""Lists the asset files the Godot project names that no writer of the extractor writes, as far as text can tell.

    python tools/godot-rooms/check_named_files.py

The manifest (godot/engine/content/asset-manifest.md) lists every asset file the project's scenes and scripts name.
A writer names the files it writes in its source: outright, or with parts filled in. This matches each file of the
manifest's "Other files named" against every path the writers' source holds, a filled-in part matching anything, and
lists the files nothing matches. Nothing here runs the extractor, so a file may be matched by a path that is written
only for other fillings, and a file whose whole name is built in code is listed though it is written: it is a list
to read, not a verdict.
"""
from __future__ import annotations

import glob
import os
import re

REPO = os.path.normpath(os.path.join(os.path.dirname(__file__), '..', '..'))
ENGINE = os.path.join(REPO, 'csharp', 'src', 'SuperMetroid.AssetExtraction', 'Engine')
MANIFEST = os.path.join(REPO, 'godot', 'engine', 'content', 'asset-manifest.md')


def main() -> None:
    manifest = open(MANIFEST, encoding='utf-8').read()
    other = manifest[manifest.index('## Other files named'):]
    wanted = re.findall(r'^\| `([^`]+)` \|', other, re.M)

    patterns: list[re.Pattern[str]] = []
    folders: set[str] = set()
    for path in sorted(glob.glob(os.path.join(ENGINE, '*.cs'))):
        text = open(path, encoding='utf-8-sig').read()
        for literal in re.findall(r'\$?"((?:[^"\\]|\\.)*)"', text):
            # Only a path that names its own top folder says where its file goes: one built wholly from parts would
            # match every file there is.
            if not re.match(r'[a-z_]+/', literal):
                continue
            # A part filled in by code matches anything; so does the placeholder for a room's set of graphics.
            escaped = re.escape(literal)
            escaped = re.sub(r'\\\{[^}]*\\\}', '[^/]+', escaped)
            patterns.append(re.compile('^' + escaped + '$'))
            # A writer that names a folder and builds the file's name in code: the folder is known to be written to.
            if re.fullmatch(r'[a-z_]+(/[a-z_{}]+)*', literal):
                folders.add(literal.split('{')[0].rstrip('/'))

    missing, by_folder_only = [], []
    for file in wanted:
        if any(pattern.match(file) for pattern in patterns):
            continue
        if any(file.startswith(folder + '/') for folder in folders):
            by_folder_only.append(file)
        else:
            missing.append(file)
    print(f'{len(wanted)} files named; {len(missing)} that no writer names a path or a folder for; '
          f'{len(by_folder_only)} whose folder a writer names but whose own name none does')
    for file in missing:
        print('  no writer: ' + file)
    for file in by_folder_only:
        print('  folder only: ' + file)


if __name__ == '__main__':
    main()
