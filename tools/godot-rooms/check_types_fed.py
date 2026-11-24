"""Checks, as text, that each data file's writers name what the file's Godot type plays and asks for.

    python tools/godot-rooms/check_types_fed.py

For every data file the manifest gives to a type, this reads the type's source (and that of the types it builds on,
within the game) for:

- the timelines it plays by a name written out (`Play("idle")`, `Timelines.Get("open")`), unless it first asks
  whether it has one of that name;
- the lists and groups it asks its data for outright in `ReadValues` (`data.List("emitters")`), unless it first asks
  whether the data has one;
- the shots it fires by name.

Each must be named somewhere in a writer that names the file: as a string, or as a string with parts filled in that
the name fits. A name built in code on both sides is not seen, and a writer shared by many files is not held to which
of them it gives what: this lists what no writer of the file names at all. Nothing here runs the extractor.
"""
from __future__ import annotations

import glob
import os
import re
import sys

REPO = os.path.normpath(os.path.join(os.path.dirname(__file__), '..', '..'))
GODOT = os.path.join(REPO, 'godot')
ENGINE = os.path.join(REPO, 'csharp', 'src', 'SuperMetroid.AssetExtraction', 'Engine')
MANIFEST = os.path.join(GODOT, 'engine', 'content', 'asset-manifest.md')


def main() -> None:
    sources: dict[str, str] = {}
    bases: dict[str, str] = {}
    for folder, _, names in os.walk(os.path.join(GODOT, 'game')):
        if {'tests'} & set(os.path.relpath(folder, GODOT).split(os.sep)):
            continue
        for name in names:
            if not name.endswith('.cs'):
                continue
            text = open(os.path.join(folder, name), encoding='utf-8-sig').read()
            code = re.sub(r'^\s*///.*$', '', text, flags=re.M)
            for declared in re.finditer(r'\bpartial class (\w+)(?:\s*:\s*([\w.]+))?', code):
                kind = declared.group(1)
                if declared.group(2) and not re.match(r'I[A-Z]', declared.group(2).split('.')[-1]):
                    bases.setdefault(kind, declared.group(2).split('.')[-1])
            owner = re.search(r'\bpartial class (\w+)', code)
            if owner:
                sources[owner.group(1)] = sources.get(owner.group(1), '') + code

    def source_of(kind: str) -> str:
        text, seen = '', set()
        while kind in sources and kind not in seen:
            seen.add(kind)
            text += sources[kind]
            kind = bases.get(kind, '')
        return text

    writers = {path: open(path, encoding='utf-8-sig').read() for path in sorted(glob.glob(os.path.join(ENGINE, '*.cs')))}
    strings: dict[str, tuple[set[str], list[re.Pattern[str]]]] = {}
    for path, text in writers.items():
        plain, patterns = set(), []
        for dollar, literal in re.findall(r'(\$?)"((?:[^"\\\n]|\\.)*)"', text):
            if dollar and '{' in literal:
                patterns.append(re.compile('^' + re.sub(r'\\\{[^}]*\\\}', '.+', re.escape(literal)) + '$'))
            else:
                plain.add(literal)
        strings[path] = (plain, patterns)

    manifest = open(MANIFEST, encoding='utf-8').read()
    given = manifest[manifest.index('## Data files given to things'):manifest.index('## Room layouts')]
    problems = 0
    checked = 0
    for file, kind in re.findall(r'^\| `([^`]+\.json)` \| `(\w+)` \|', given, re.M):
        code = source_of(kind)
        if not code:
            continue
        # The writers that name the file, outright or with parts filled in.
        mine = [path for path, (plain, patterns) in strings.items() if file in plain or any(pattern.match(file) for pattern in patterns)]
        if not mine:
            continue
        plain = set().union(*(strings[path][0] for path in mine))
        patterns = [pattern for path in mine for pattern in strings[path][1]]

        def named(name: str) -> bool:
            return name in plain or any(pattern.match(name) for pattern in patterns)

        guarded = set(re.findall(r'Timelines\.Has\("(\w+)"\)', code)) | set(re.findall(r'data\.Has\("(\w+)"\)', code))
        played = set(re.findall(r'\bPlay(?:Part|Facing|Timeline|Look)?\(\s*"(\w+)"', code)) | set(re.findall(r'Timelines\.Get\("(\w+)"\)', code))
        asked = set(re.findall(r'(?<![\w.])data\.(?:List|Get)\("(\w+)"\)', code)) - {'colliders', 'timelines', 'shots', 'reactions', 'drops', 'death'}
        fired = set(re.findall(r'\b(?:Fire|FireAtPlayer|Shot)\("(\w+)"', code))
        for what, names in (('plays the timeline', played - guarded), ('asks its data for', asked - guarded), ('fires the shot', fired)):
            for name in sorted(names):
                checked += 1
                if not named(name):
                    print(f'{file} ({kind}) {what} "{name}", which no writer of the file names')
                    problems += 1
    print(f'{problems} problems; {checked} names checked')
    sys.exit(1 if problems else 0)


if __name__ == '__main__':
    main()
