"""Writes down, for the asset extractor, every value each type of the Godot project reads from its data file by name.

    python tools/godot-rooms/engine_defaults.py

A type of the Godot project that names a data file reads each of its values from it, and a file that lacks one is
refused. So that every file the extractor writes is whole, this lists each type's values with the default the type's
own code gives it, as csharp/src/SuperMetroid.AssetExtraction/Engine/EngineDefaults.Generated.cs. The extractor starts
a file from its type's list and then writes over whatever the old game's definitions say otherwise.
"""
from __future__ import annotations

import os
import re

REPO = os.path.normpath(os.path.join(os.path.dirname(__file__), '..', '..'))
GODOT = os.path.join(REPO, 'godot')
TARGET = os.path.join(REPO, 'csharp', 'src', 'SuperMetroid.AssetExtraction', 'Engine', 'EngineDefaults.Generated.cs')

VALUE = re.compile(
    r'\[[^\]\n]*?DataValue\("(\w+)"([^\]\n]*)\]\s*(?:\[[^\]\n]*\]\s*)*public (?:new )?([\w<>\[\]?., ]+?) (\w+)\s*\{[^}]*\}(?:\s*=\s*([^;]+);)?')


def snake(name: str) -> str:
    return re.sub(r'(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])', '_', name).lower()


def sources() -> list[str]:
    found = []
    for folder, _, names in os.walk(GODOT):
        if '.godot' in folder or os.sep + 'tests' in folder:
            continue
        found += [os.path.join(folder, name) for name in names if name.endswith('.cs')]
    return sorted(found)


def main() -> None:
    texts = {path: open(path, encoding='utf-8-sig').read() for path in sources()}
    constants: dict[str, str] = {}
    enums: dict[str, list[str]] = {}
    for text in texts.values():
        for owner in re.finditer(r'\bclass (\w+)[^{]*\{', text):
            for name, value in re.findall(r'public const string (\w+) = ("[^"]*");', text[owner.end():]):
                constants.setdefault(f'{owner.group(1)}.{name}', value)
        for name, members in re.findall(r'\benum (\w+)\s*\{([^}]*)\}', text):
            members = re.sub(r'///[^\n]*|//[^\n]*', '', members)
            enums[name] = [member.split('=')[0].strip() for member in members.split(',') if member.strip()]

    def json_of(kind: str, initial: str | None, where: str) -> str:
        kind = kind.strip().rstrip('?')
        last = kind.split('.')[-1]
        if initial is None:
            if kind == 'int':
                return '0'
            if kind in ('double', 'float'):
                return '0.0'
            if kind == 'bool':
                return 'false'
            if kind == 'string':
                return '""'
            if kind.endswith('[]'):
                return '[]'
            if last in enums:
                return '"' + snake(enums[last][0]) + '"'
            raise ValueError(f'{where}: a value of type {kind} has no default this can write.')
        initial = initial.strip()
        if kind in ('int', 'double', 'float') and re.fullmatch(r'[-0-9.+*/ ()f]+', initial):
            value = eval(initial.replace('f', ''), {'__builtins__': {}})  # noqa: S307 - digits and arithmetic only
            # A whole number is written whole and any other number with its fraction, so that the default itself says
            # which of the two the type reads.
            return str(int(value)) if kind == 'int' else repr(float(value))
        if initial in ('true', 'false'):
            return initial
        if initial.startswith('"'):
            return initial
        if initial == '[]':
            return '[]'
        if initial in constants:
            return constants[initial]
        if last in enums and initial.split('.')[-1] in enums[last]:
            return '"' + snake(initial.split('.')[-1]) + '"'
        raise ValueError(f'{where}: the default {initial} of a {kind} is not something this can write.')

    bases: dict[str, str] = {}
    values: dict[str, list[tuple[str, str]]] = {}
    for path, text in texts.items():
        declared = re.search(r'\bpartial class (\w+)(?:\s*:\s*([\w.]+))?', text)
        if not declared:
            continue
        name = declared.group(1)
        if declared.group(2) and not re.match(r'I[A-Z]', declared.group(2).split('.')[-1]):
            bases[name] = declared.group(2).split('.')[-1]
        for key, options, kind, prop, initial in VALUE.findall(text):
            if 'ReadByOwner' in options:
                continue
            values.setdefault(name, []).append((key, json_of(kind, initial or None, f'{name}.{prop}')))

    lines = [
        '// Written by tools/godot-rooms/engine_defaults.py from the Godot project; do not edit it by hand.',
        'using System.Collections.Frozen;', '',
        'namespace SuperMetroid.AssetExtraction.Engine;', '',
        '/// <summary>',
        '/// Every value each type of the Godot project reads from its data file by name, with the default the type gives',
        '/// it, as JSON. A data file is begun from its type\'s list, so that it lacks nothing its type asks for.',
        '/// </summary>',
        'internal static partial class EngineDefaults',
        '{',
        '    /// <summary>The type each type builds on, where that is a type of the Godot project.</summary>',
        '    private static readonly FrozenDictionary<string, string> Bases = new Dictionary<string, string>',
        '    {',
    ]
    lines += [f'        ["{name}"] = "{base}",' for name, base in sorted(bases.items()) if name in values or base in values or base in bases]
    lines += ['    }.ToFrozenDictionary();', '',
              '    /// <summary>The values each type reads by name, each with its default as JSON.</summary>',
              '    private static readonly FrozenDictionary<string, (string Name, string Json)[]> Own = new Dictionary<string, (string Name, string Json)[]>',
              '    {']
    for name in sorted(values):
        listed = ', '.join('("' + key + '", ' + '"' + json.replace('\\', '\\\\').replace('"', '\\"') + '")' for key, json in values[name])
        lines.append(f'        ["{name}"] = [{listed}],')
    lines += ['    }.ToFrozenDictionary();', '}']
    with open(TARGET, 'w', encoding='utf-8', newline='\n') as output:
        output.write('\n'.join(lines) + '\n')
    print(f'{sum(len(listed) for listed in values.values())} values of {len(values)} types written')


if __name__ == '__main__':
    main()
