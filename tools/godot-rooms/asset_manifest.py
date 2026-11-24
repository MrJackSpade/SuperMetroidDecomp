"""Lists every asset file the Godot project names, and what reads each.

    python tools/godot-rooms/asset_manifest.py

The Godot project commits no assets: the asset extractor writes them. This writes down what it must write, as
godot/engine/content/asset-manifest.md, by reading the project's scenes and scripts as text:

- each data file a scene gives a thing, with the type that reads it and the values that type takes from it by name;
- each room layout, with the terrain patches the project asks it for by name;
- every other file a scene or a script names: pictures, timelines, shots, effects.

What lies inside each kind of file is described beside it, in asset-formats.md, and exactly in the remarks of the
type that reads it.
"""
from __future__ import annotations

import os
import re

GODOT = os.path.normpath(os.path.join(os.path.dirname(__file__), '..', '..', 'godot'))
ASSET = re.compile(r'"([a-z0-9_]+(?:/[a-z0-9_{}]+)+\.(?:json|png|ogg|wav))"')


def files(extension: str) -> list[str]:
    found = []
    for folder, _, names in os.walk(GODOT):
        if '.godot' in folder or os.sep + 'tests' in folder or os.sep + 'assets' in folder:
            continue
        found += [os.path.join(folder, name) for name in names if name.endswith(extension)]
    return sorted(found)


def read(path: str) -> str:
    with open(path, encoding='utf-8-sig') as source:
        return source.read()


class Types:
    """The project's types that take values from data files: each one's base, and the values it reads by name."""

    def __init__(self) -> None:
        self.base: dict[str, str] = {}
        self.values: dict[str, list[str]] = {}
        self.of_script: dict[str, str] = {}
        for path in files('.cs'):
            text = read(path)
            declared = re.search(r'\bclass (\w+)(?:<[^>]*>)?\s*(?:\([^)]*\))?\s*:\s*(\w+)', text) or re.search(r'\bclass (\w+)', text)
            if not declared:
                continue
            name = declared.group(1)
            self.base[name] = declared.group(2) if declared.lastindex and declared.lastindex > 1 else ''
            self.values.setdefault(name, [])
            self.values[name] += re.findall(r'DataValue\("(\w+)"', text)
            self.of_script['res://' + os.path.relpath(path, GODOT).replace(os.sep, '/')] = name

    def all_values(self, name: str) -> list[str]:
        values: list[str] = []
        while name:
            values += self.values.get(name, [])
            name = self.base.get(name, '')
        return sorted(set(values))


def scene_nodes(path: str, types: Types, roots: dict[str, tuple[str, str]]):
    """Each node of a scene that has a script or is an instance: (type name, its properties)."""
    text = read(path)
    scripts = dict(re.findall(r'\[ext_resource type="Script" path="([^"]+)" id="([^"]+)"\]', text))
    scripts = {ident: script for script, ident in scripts.items()}
    scenes = dict(re.findall(r'\[ext_resource type="PackedScene" path="([^"]+)" id="([^"]+)"\]', text))
    scenes = {ident: scene for scene, ident in scenes.items()}
    for block in text.split('\n\n'):
        head = re.match(r'\[node name="[^"]+"(.*?)\]', block)
        if not head:
            continue
        properties = dict(re.findall(r'^(\w+) = (.*)$', block, re.M))
        script = re.search(r'ExtResource\("([^"]+)"\)', properties.get('script', ''))
        instance = re.search(r'instance=ExtResource\("([^"]+)"\)', head.group(1))
        if script:
            yield types.of_script.get(scripts[script.group(1)], ''), properties, None
        elif instance:
            yield '', properties, scenes[instance.group(1)]


def main() -> None:
    types = Types()
    scene_paths = files('.tscn')
    # Each scene's root: its type, and the data file it names.
    roots: dict[str, tuple[str, str]] = {}
    for path in scene_paths:
        for kind, properties, _ in scene_nodes(path, types, roots):
            roots['res://' + os.path.relpath(path, GODOT).replace(os.sep, '/')] = (kind, properties.get('DataFile', '').strip('"'))
            break

    data: dict[str, set[str]] = {}
    layouts: dict[str, set[str]] = {}
    others: dict[str, set[str]] = {}
    for path in scene_paths:
        where = os.path.relpath(path, GODOT).replace(os.sep, '/')
        text = read(path)
        for kind, properties, instanced in scene_nodes(path, types, roots):
            if instanced:
                kind, file = roots.get(instanced, ('', ''))
                file = properties.get('DataFile', '"' + file + '"').strip('"')
            else:
                file = properties.get('DataFile', '').strip('"')
            if file:
                data.setdefault(file, set()).add(kind)
        for layout in re.findall(r'LayoutFile = "([^"]+)"', text):
            layouts.setdefault(layout, set())
        if '/rooms/' in where:
            patches = set()
            for listed in re.findall(r'^Patches = PackedStringArray\((.*)\)$', text, re.M):
                patches |= set(re.findall(r'"([^"]+)"', listed))
            patches |= set(re.findall(r'^\w*Patch = "([^"]+)"$', text, re.M))
            patches |= set(re.findall(r'^AppliesPatch = "([^"]+)"$', text, re.M))
            for listed in re.findall(r'^\w*Patches = Array\[String\]\(\[(.*)\]\)$', text, re.M):
                patches |= set(re.findall(r'"([^"]+)"', listed))
            for layout in set(re.findall(r'LayoutFile = "([^"]+)"', text)):
                layouts[layout] |= patches
        for file in ASSET.findall(text):
            if file not in data and file not in layouts:
                others.setdefault(file, set()).add(where)
    for path in files('.cs'):
        where = os.path.relpath(path, GODOT).replace(os.sep, '/')
        code = re.sub(r'^\s*///.*$', '', read(path), flags=re.M)
        for file in ASSET.findall(code):
            if file not in data and file not in layouts:
                others.setdefault(file, set()).add(where)

    lines = ['# What the project asks of the assets', '',
             'Written by `tools/godot-rooms/asset_manifest.py`; do not edit it by hand. It lists every asset file the',
             "project's scenes and scripts name. What lies inside each kind of file is in",
             '[asset-formats.md](asset-formats.md).', '',
             '## Data files given to things', '',
             'Each is read by the type shown, which takes the listed values from it by name, besides whatever the',
             "type's own remarks say it reads (timelines, colliders, shots, lists of its own).", '',
             '| File | Read by | Values by name |', '| --- | --- | --- |']
    for file in sorted(data):
        kinds = sorted(kind for kind in data[file] if kind)
        values = sorted({value for kind in kinds for value in types.all_values(kind)})
        lines.append(f'| `{file}` | {", ".join(f"`{kind}`" for kind in kinds)} | {", ".join(values)} |')
    lines += ['', '## Room layouts', '',
              'Each is a terrain layout. Those with patches listed must hold patches of those names.', '',
              '| File | Patches |', '| --- | --- |']
    for file in sorted(layouts):
        lines.append(f'| `{file}` | {", ".join(sorted(layouts[file]))} |')
    lines += ['', '## Other files named', '', '| File | Named in |', '| --- | --- |']
    for file in sorted(others):
        named = sorted(others[file])
        shown = ', '.join(f'`{where}`' for where in named[:3]) + (f' and {len(named) - 3} more' if len(named) > 3 else '')
        lines.append(f'| `{file}` | {shown} |')
    target = os.path.join(GODOT, 'engine', 'content', 'asset-manifest.md')
    with open(target, 'w', encoding='utf-8', newline='\n') as output:
        output.write('\n'.join(lines) + '\n')
    print(f'{len(data)} data files, {len(layouts)} layouts and {len(others)} other files named')


if __name__ == '__main__':
    main()
