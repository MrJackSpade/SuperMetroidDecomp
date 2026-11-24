"""Checks the Godot project's scenes and resources as text, for what a build does not catch.

    python tools/godot-rooms/check_scenes.py

A scene that names a script or a scene that is not there, sets a property its script does not have, names two
children of one parent alike, or gives a property of a node the value of a resource it never declares, loads wrongly
or not at all, and nothing but opening it says so. This reads every .tscn and .tres under godot/ and reports:

- a `res://` path that names no file;
- two nodes of one scene with the same name under the same parent;
- an `ExtResource("id")` or `SubResource("id")` the file does not declare;
- a property set on a node whose script (a C# class of the project) has no exported property of that name, nor has
  any class it builds on, nor the Godot node it is.
"""
from __future__ import annotations

import os
import re
import sys

REPO = os.path.normpath(os.path.join(os.path.dirname(__file__), '..', '..'))
GODOT = os.path.join(REPO, 'godot')

# Properties every node of these Godot kinds has, which a scene may set without the script declaring them.
NODE_PROPERTIES = {
    'position', 'rotation', 'scale', 'visible', 'z_index', 'z_as_relative', 'modulate', 'self_modulate', 'script',
    'texture', 'centered', 'offset', 'flip_h', 'flip_v', 'region_enabled', 'region_rect', 'texture_filter',
    'layer', 'follow_viewport_enabled', 'color', 'polygon', 'anchors_preset', 'anchor_right', 'anchor_bottom',
    'offset_left', 'offset_top', 'offset_right', 'offset_bottom', 'size', 'mouse_filter', 'grow_horizontal',
    'grow_vertical', 'layout_mode', 'text', 'horizontal_alignment', 'vertical_alignment', 'process_mode',
    'show_behind_parent', 'top_level', 'y_sort_enabled', 'clip_contents', 'custom_minimum_size', 'metadata/_edit_group_',
    'unique_name_in_owner', 'editor_description', 'light_mask', 'visibility_layer', 'material', 'use_parent_material',
    'anchor_left', 'anchor_top', 'pivot_offset', 'theme', 'stretch', 'stretch_mode', 'expand_mode', 'zoom', 'limit_left',
    'limit_top', 'limit_right', 'limit_bottom', 'anchor_mode', 'enabled', 'skew', 'transform',
}


def files(extensions: tuple[str, ...]) -> list[str]:
    found: list[str] = []
    for folder, _, names in os.walk(GODOT):
        if {'.godot', 'assets', 'bin', 'obj'} & set(os.path.relpath(folder, GODOT).split(os.sep)):
            continue
        found += [os.path.join(folder, name) for name in names if name.endswith(extensions)]
    return sorted(found)


def main() -> None:
    # Every class of the project: the properties it exports, and the class it builds on.
    exports: dict[str, set[str]] = {}
    bases: dict[str, str] = {}
    by_script: dict[str, str] = {}
    for path in files(('.cs',)):
        text = open(path, encoding='utf-8-sig').read()
        text = re.sub(r'^\s*///.*$', '', text, flags=re.M)
        for declared in re.finditer(r'\bpartial class (\w+)(?:\s*:\s*([\w.]+))?', text):
            name = declared.group(1)
            # A class written across several files names what it builds on in one of them, and interfaces in others.
            if declared.group(2) and not re.match(r'I[A-Z]', declared.group(2).split('.')[-1]):
                bases.setdefault(name, declared.group(2).split('.')[-1])
            exports.setdefault(name, set())
        owner = re.search(r'\bpartial class (\w+)', text)
        if owner:
            by_script['res://' + os.path.relpath(path, GODOT).replace(os.sep, '/')] = owner.group(1)
            for prop in re.findall(r'\[Export[^\]]*\][^;{=]*?\b(\w+)\s*\{', text):
                exports[owner.group(1)].add(prop)

    def has(kind: str, prop: str) -> bool:
        seen = set()
        while kind and kind not in seen:
            seen.add(kind)
            if prop in exports.get(kind, set()):
                return True
            kind = bases.get(kind, '')
        return False

    problems = 0
    checked = 0
    scene_script: dict[str, str] = {}
    scene_files = files(('.tscn', '.tres'))
    texts = {path: open(path, encoding='utf-8').read() for path in scene_files}
    # The script of each scene's root node, for a scene placed in another.
    for path, text in texts.items():
        resources = dict((ident, target) for target, ident in re.findall(r'\[ext_resource [^\]]*path="([^"]+)"[^\]]*id="([^"]+)"', text))
        root = re.search(r'\[node name="[^"]+"(?: type="[^"]+")?(?: instance=ExtResource\("([^"]+)"\))?\]\n((?:[^\[\n][^\n]*\n)*)', text)
        if not root:
            continue
        script = re.search(r'^script = ExtResource\("([^"]+)"\)', root.group(2), re.M)
        key = 'res://' + os.path.relpath(path, GODOT).replace(os.sep, '/')
        if script and resources.get(script.group(1)) in by_script:
            scene_script[key] = by_script[resources[script.group(1)]]
        elif root.group(1) and resources.get(root.group(1)):
            scene_script[key] = '>' + resources[root.group(1)]
    for key in list(scene_script):
        hops = 0
        while scene_script[key].startswith('>') and hops < 8:
            scene_script[key] = scene_script.get(scene_script[key][1:], '')
            hops += 1

    for path, text in texts.items():
        where = os.path.relpath(path, REPO).replace(os.sep, '/')
        resources = dict((ident, target) for target, ident in re.findall(r'\[ext_resource [^\]]*path="([^"]+)"[^\]]*id="([^"]+)"', text))
        for target in set(resources.values()):
            if target.startswith('res://') and not os.path.exists(os.path.join(GODOT, target[len('res://'):])):
                print(f'{where}: names {target}, which is not there')
                problems += 1
        subs = set(re.findall(r'\[sub_resource [^\]]*id="([^"]+)"', text))
        for ident in set(re.findall(r'ExtResource\("([^"]+)"\)', text)) - set(resources):
            print(f'{where}: uses ExtResource("{ident}"), which it does not declare')
            problems += 1
        for ident in set(re.findall(r'SubResource\("([^"]+)"\)', text)) - subs:
            print(f'{where}: uses SubResource("{ident}"), which it does not declare')
            problems += 1

        names: dict[tuple[str, str], int] = {}
        for node in re.finditer(r'\[node name="([^"]+)"(?: type="([^"]+)")?(?: parent="([^"]*)")?(?: instance=ExtResource\("([^"]+)"\))?[^\]]*\]\n((?:[^\[\n][^\n]*\n)*)', text):
            name, kind, parent, instance, body = node.groups()
            names[(parent or '', name)] = names.get((parent or '', name), 0) + 1
            script = re.search(r'^script = ExtResource\("([^"]+)"\)', body, re.M)
            owner = ''
            if script and resources.get(script.group(1)) in by_script:
                owner = by_script[resources[script.group(1)]]
            elif instance and resources.get(instance):
                owner = scene_script.get(resources[instance], '')
            if not owner:
                continue
            for prop in re.findall(r'^([A-Za-z_][\w/]*) = ', body, re.M):
                if prop in NODE_PROPERTIES or not prop[0].isupper():
                    continue
                checked += 1
                if not has(owner, prop):
                    print(f'{where}: the node "{name}" sets {prop}, which {owner} does not export')
                    problems += 1
        for (parent, name), count in names.items():
            if count > 1:
                print(f'{where}: {count} nodes named "{name}" under "{parent or "the root"}"')
                problems += 1
    print(f'{problems} problems in {len(scene_files)} scenes and resources; {checked} properties set on nodes were checked')
    sys.exit(1 if problems else 0)


if __name__ == '__main__':
    main()
