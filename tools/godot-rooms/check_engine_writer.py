"""Checks the asset extractor's Godot data writers against the Godot project, as text.

    python tools/godot-rooms/check_engine_writer.py

Nothing here runs the extractor. For each data file a writer starts, this checks that the Godot type it names reads
the values the writer gives it by name, and lists the data files the Godot project names that no writer writes yet.
"""
from __future__ import annotations

import glob
import os
import re

REPO = os.path.normpath(os.path.join(os.path.dirname(__file__), '..', '..'))
ENGINE = os.path.join(REPO, 'csharp', 'src', 'SuperMetroid.AssetExtraction', 'Engine')


def main() -> None:
    generated = open(os.path.join(ENGINE, 'EngineDefaults.Generated.cs'), encoding='utf-8').read()
    bases = dict(re.findall(r'^\s+\["(\w+)"\] = "(\w+)",$', generated, re.M))
    own = {name: set(re.findall(r'\("(\w+)", ', listed)) for name, listed in re.findall(r'^\s+\["(\w+)"\] = \[(.*)\],$', generated, re.M)}

    def values(kind: str) -> set[str]:
        found: set[str] = set()
        while kind:
            found |= own.get(kind, set())
            kind = bases.get(kind, '')
        return found

    written: set[str] = set()
    problems = 0
    for path in sorted(glob.glob(os.path.join(ENGINE, '*.cs'))):
        text = open(path, encoding='utf-8').read()
        for start in re.finditer(r'Enemy\(\$?"([^"]+)", "(\w+)"', text):
            file, kind = start.group(1), start.group(2)
            end = text.find('.Write();', start.end())
            chain = text[start.end():end]
            written.add(file)
            if kind not in own and kind not in bases:
                print(f'{os.path.basename(path)}: {file} names the Godot type {kind}, which reads no values')
                problems += 1
            for name in re.findall(r'\.Value\("(\w+)"', chain):
                if name not in values(kind):
                    print(f'{os.path.basename(path)}: {file} is given {name}, which {kind} does not read')
                    problems += 1

    manifest = open(os.path.join(REPO, 'godot', 'engine', 'content', 'asset-manifest.md'), encoding='utf-8').read()
    wanted = re.findall(r'^\| `((?:enemies|bosses|objects|samus|cinematics)/[^`]+\.json)` \|', manifest, re.M)
    # A writer that writes several kinds names its file with a part to be filled in.
    patterns = [re.compile('^' + re.sub(r'\\{[^}]*\\}', '[a-z_]+', re.escape(file)) + '$') for file in written]
    missing = [file for file in wanted if not any(pattern.match(file) for pattern in patterns)]
    # A writer of several kinds is counted for every file its name could be, so this is the most that are written.
    print(f'{problems} problems; at most {len(wanted) - len(missing)} of {len(wanted)} data files the project names have a writer')
    print('not written yet: ' + ', '.join(missing))


if __name__ == '__main__':
    main()
