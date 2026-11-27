"""Writes godot/game/tests/movies/TraceLayouts.Generated.cs: the layouts the committed movie traces are kept in.

A layout is the runs of the cartridge's memory a trace keeps for every record, in ascending order of address. They are
defined in csharp/test-support/MovieReplay/ReplayTraceMemoryCatalog.cs, out of constants; this resolves those
definitions as they are written there, and checks each against the size its traces' manifests give, so the Godot
movie tests read the traces by the same layouts without sharing code with the Classic game.

Run from anywhere: python tools/movie-trace/trace_layouts.py
"""
import json
import os
import re
import sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(__file__), '..', '..'))
SOURCES = os.path.join(ROOT, 'csharp', 'test-support', 'MovieReplay')
TRACES = os.path.join(ROOT, 'csharp', 'test-fixtures', 'replay-traces')
OUT = os.path.join(ROOT, 'godot', 'game', 'tests', 'movies', 'TraceLayouts.Generated.cs')

# Constants the catalog takes from outside its folder, with where each is defined.
OUTSIDE = {
    'EnemyPlaceOrder.Capacity': 32,                             # csharp/src/SuperMetroid.Core/Enemies/EnemyPlaceOrder.cs
    'EnemySlotOffsets.TableSize': 32 * 64,                      # csharp/test-support/VerificationImporters/EnemySlotOffsets.cs
    'HudStateConstants.MutableTileCount': 32 * 3,               # csharp/test-support/VerificationAccess.Core/Hud/HudStateConstants.cs
    'AreaNumberCodec.Count': 7,                                 # csharp/src/SuperMetroid.AssetExtraction/Progression/AreaNumberCodec.cs
    'ProgressionTableCodec.ExploredPlaneByteCount': 128 * 2,    # csharp/src/SuperMetroid.AssetExtraction/Progression/ProgressionTableCodec.cs
}

# Which layout each property of the catalog defines, and a trace kept in it.
LAYOUTS = {'MovieDesync': ('movie-desync', 'low-13-percent'), 'RidleyShowcase': ('ridley-showcase', 'ridley')}


def constants() -> dict[str, str]:
    """Every `const int` of the folder, by `Class.Name`, as its expression's text."""
    found: dict[str, str] = {}
    for name in sorted(os.listdir(SOURCES)):
        if not name.endswith('.cs'):
            continue
        text = open(os.path.join(SOURCES, name), encoding='utf-8-sig').read()
        text = re.sub(r'//[^\n]*', '', text)
        owner = re.search(r'\b(?:class|struct)\s+(\w+)', text)
        if owner is None:
            continue
        for declaration in re.findall(r'\bconst\s+int\s+([^;]+);', text):
            for part in declaration.split(','):
                if '=' not in part:
                    continue
                key, expression = part.split('=', 1)
                found[f'{owner.group(1)}.{key.strip()}'] = expression.strip()
    return found


class Resolver:
    def __init__(self) -> None:
        self.text = constants()
        self.value: dict[str, int] = dict(OUTSIDE)

    def name(self, owner: str, name: str) -> int:
        short = '.'.join(name.split('.')[-2:]) if '.' in name else f'{owner}.{name}'
        if short in self.value:
            return self.value[short]
        if short not in self.text:
            raise SystemExit(f'the constant {name} (read as {short}) is defined nowhere this looks')
        self.value[short] = self.evaluate(short.split('.')[0], self.text[short])
        return self.value[short]

    def evaluate(self, owner: str, expression: str) -> int:
        expression = expression.replace('sizeof(ushort)', '2')
        resolved = re.sub(r'\b[A-Za-z_][\w.]*\b', lambda match: str(self.name(owner, match.group(0))), expression)
        if not re.fullmatch(r'[\dxXa-fA-F\s+\-*/()]+', resolved):
            raise SystemExit(f'cannot evaluate "{expression}" (resolved to "{resolved}")')
        return int(eval(resolved.replace('/', '//')))  # noqa: S307 - digits and arithmetic only, checked above


def split_top(text: str) -> list[str]:
    parts, depth, start = [], 0, 0
    for index, char in enumerate(text):
        if char in '([':
            depth += 1
        elif char in ')]':
            depth -= 1
        elif char == ',' and depth == 0:
            parts.append(text[start:index])
            start = index + 1
    parts.append(text[start:])
    return [part.strip() for part in parts if part.strip()]


def layout(resolver: Resolver, catalog: str, property_name: str) -> list[tuple[int, int]]:
    begin = catalog.index(f'ReplayTraceLayout {property_name} ')
    opened = catalog.index('[', begin)
    depth, index = 0, opened
    while True:
        depth += {'[': 1, ']': -1}.get(catalog[index], 0)
        if depth == 0:
            break
        index += 1
    owner = 'ReplayTraceMemoryCatalog'
    ranges: list[tuple[int, int]] = []
    for entry in split_top(catalog[opened + 1:index]):
        entry = ' '.join(entry.split())
        if match := re.fullmatch(r'WorkRamRange\.Words\((.+)\)', entry):
            arguments = split_top(match.group(1))
            count = resolver.evaluate(owner, arguments[1]) if len(arguments) > 1 else 1
            ranges.append((resolver.evaluate(owner, arguments[0]), count * 2))
        elif match := re.fullmatch(r'WorkRamRange\.Between\((.+)\)', entry):
            start, end = (resolver.evaluate(owner, argument) for argument in split_top(match.group(1)))
            ranges.append((start, end - start))
        elif match := re.fullmatch(r'new\((.+)\)', entry):
            start, length = (resolver.evaluate(owner, argument) for argument in split_top(match.group(1)))
            ranges.append((start, length))
        elif match := re.fullmatch(r'\.\. Enumerable\.Range\(0, (.+?)\)\.Select\(slot => new WorkRamRange\((.+) \+ slot \* (.+?), (.+)\)\)', entry):
            count, base, stride, length = (resolver.evaluate(owner, argument) for argument in match.groups())
            ranges.extend((base + slot * stride, length) for slot in range(count))
        else:
            raise SystemExit(f'the layout {property_name} has an entry this cannot read: {entry}')
    ranges.sort()
    for (start, length), (following, _) in zip(ranges, ranges[1:]):
        if start + length > following:
            raise SystemExit(f'the layout {property_name} keeps ${start:04X}+{length} and ${following:04X}, which overlap')
    return ranges


def main() -> int:
    resolver = Resolver()
    catalog = re.sub(r'//[^\n]*', '', open(os.path.join(SOURCES, 'ReplayTraceMemoryCatalog.cs'), encoding='utf-8-sig').read())
    lines = [
        '// Written by tools/movie-trace/trace_layouts.py from csharp/test-support/MovieReplay/ReplayTraceMemoryCatalog.cs.',
        '// Do not edit: run the tool again.',
        'namespace Game.Tests.Movies;',
        '',
        '/// <summary>The layouts the committed movie traces are kept in: the runs of the cartridge\'s memory each record holds, in ascending order of address.</summary>',
        'public static class TraceLayouts',
        '{',
        '    /// <summary>The runs of the layout named <paramref name="layout"/>: where each starts and how long it is.</summary>',
        '    public static (int Start, int Length)[] Of(string layout) => layout switch',
        '    {',
    ]
    bodies = []
    for property_name, (name, trace) in LAYOUTS.items():
        ranges = layout(resolver, catalog, property_name)
        total = sum(length for _, length in ranges)
        manifest = json.load(open(os.path.join(TRACES, trace, 'compact-manifest.json'), encoding='utf-8'))
        if manifest['layout'] != name or manifest['payloadBytes'] != total:
            raise SystemExit(f'{name} comes to {total} bytes; the trace {trace} is kept in {manifest["layout"]} at {manifest["payloadBytes"]}')
        lines.append(f'        "{name}" => {property_name},')
        bodies.append(f'    /// <summary>The layout "{name}": {len(ranges)} runs, {total} bytes a record.</summary>')
        bodies.append(f'    private static readonly (int Start, int Length)[] {property_name} =')
        bodies.append('    [')
        for at in range(0, len(ranges), 6):
            bodies.append('        ' + ' '.join(f'(0x{start:04X}, {length}),' for start, length in ranges[at:at + 6]))
        bodies.append('    ];')
        bodies.append('')
        print(f'{name}: {len(ranges)} runs, {total} bytes a record')
    lines.append('        _ => throw new System.IO.InvalidDataException($"No movie trace layout is named \\"{layout}\\"."),')
    lines.append('    };')
    lines.append('')
    lines.extend(bodies[:-1])
    lines.append('}')
    with open(OUT, 'w', encoding='utf-8', newline='\n') as out:
        out.write('\n'.join(lines) + '\n')
    return 0


if __name__ == '__main__':
    sys.exit(main())
