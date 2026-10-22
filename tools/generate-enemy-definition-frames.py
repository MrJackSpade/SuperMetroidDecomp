"""Generates csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyDefinitionFrameDefinitions.cs (#1392).

Each enemy behaviour declares the definitions it serves, directly or through a parts table. The frames a definition can
show are the EnemyFrameId members its behaviour's source reaches: the behaviour file, and transitively every Core type it
names that lives in its own folder tree or is a frame, animation or frames table. The pre-coloured enemy sheets hold
exactly those frames. Run from the repository root; --check exits nonzero when the committed file differs.
"""
import glob
import os
import re
import sys

CORE = os.path.join('csharp', 'src', 'SuperMetroid.Core')
OUTPUT = os.path.join(CORE, 'Enemies', 'Artwork', 'EnemyDefinitionFrameDefinitions.cs')
FRAME_ENUM = os.path.join(CORE, 'Enemies', 'Artwork', 'EnemyFrameId.cs')
# Types every family names that carry no frames of their own; following them would merge unrelated families.
SHARED = {'EnemyFrameId', 'EnemyDefinitionId', 'SpriteFrameBuffer', 'SpriteSource', 'RoomEnemySlot', 'SamusState',
          'EnemyPresentation', 'RoomEnemySystem', 'EnemyFrameTable'}
# The common empty frame draws nothing and is compiled, not installed.
EXCLUDED_FRAMES = {'Empty', 'EmptyExtended'}
FAMILY_SUFFIXES = ('FrameDefinitions', 'Animations', 'Frames')


def read(path):
    with open(path, encoding='utf-8-sig') as handle:
        return handle.read()


def main():
    files = glob.glob(os.path.join(CORE, '**', '*.cs'), recursive=True)
    texts = {path: read(path) for path in files}
    types = {}
    for path, text in texts.items():
        for match in re.finditer(r'\b(?:class|record|struct|enum|interface)\s+(\w+)', text):
            types.setdefault(match.group(1), set()).add(path)
    order = {name: index for index, name in enumerate(re.findall(r'^\s+(\w+)(?: = \d+)?,\s*$', read(FRAME_ENUM), re.M))}

    def reach(start):
        seen_paths, seen_types, frames = set(), set(), set()
        stack = [start]
        while stack:
            path = stack.pop()
            if path in seen_paths:
                continue
            seen_paths.add(path)
            text = texts[path]
            frames.update(name for name in re.findall(r'\bEnemyFrameId\.(\w+)', text) if name not in EXCLUDED_FRAMES)
            for name in set(re.findall(r'\b([A-Z]\w+)\b', text)):
                if name in SHARED or name in seen_types or name not in types:
                    continue
                for candidate in types[name]:
                    if os.path.dirname(candidate).startswith(os.path.dirname(start)) or name.endswith(FAMILY_SUFFIXES):
                        seen_types.add(name)
                        stack.append(candidate)
        unknown = frames - order.keys()
        if unknown:
            raise SystemExit(f'{start} names frames missing from EnemyFrameId: {sorted(unknown)}')
        return sorted(frames, key=order.__getitem__)

    def definitions(path, text):
        direct = re.search(r'IReadOnlyList<EnemyDefinitionId> Definitions \{ get; \} =\s*\[([^\]]*)\]', text)
        if direct is None:
            return None
        names = re.findall(r'EnemyDefinitionId\.(\w+)', direct.group(1))
        parts = re.search(r'\.\.\s*(\w+)\.Parts\.Keys', direct.group(1))
        if parts:
            table = [p for p in types.get(parts.group(1), ())]
            if len(table) != 1:
                raise SystemExit(f'{path}: parts table {parts.group(1)} is not one Core type')
            body = texts[table[0]]
            start = body.index(' Parts { get; }')
            names += re.findall(r'\[EnemyDefinitionId\.(\w+)\]', body[start:body.index('};', start)])
        if not names:
            raise SystemExit(f'{path}: declares no definitions this generator can read')
        return names

    families = []
    owner = {}
    for path in sorted(files):
        text = texts[path]
        names = definitions(path, text)
        if names is None:
            continue
        family = os.path.splitext(os.path.basename(path))[0]
        family = re.sub(r'(Behavior|Factory)$', '', family)
        frames = reach(path)
        for name in names:
            if name in owner:
                raise SystemExit(f'{name} is served by both {owner[name]} and {family}')
            owner[name] = family
        families.append((family, os.path.relpath(path, CORE).replace(os.sep, '/'), names, frames))

    lines = [
        'namespace SuperMetroid.Core.Enemies.Artwork;',
        '',
        '/// <summary>',
        '/// The frames each enemy definition can show (#1392): the frames its behaviour\'s code names, which its pre-coloured',
        '/// sheets hold. Generated from the enemy behaviours by tools/generate-enemy-definition-frames.py; regenerate rather',
        '/// than edit.',
        '/// </summary>',
        'internal static class EnemyDefinitionFrameDefinitions',
        '{',
    ]
    for family, path, _, frames in families:
        lines.append(f'    /// <summary>The frames <c>{path}</c> shows.</summary>')
        if not frames:
            lines.append(f'    private static readonly EnemyFrameId[] {family} = [];')
        else:
            lines.append(f'    private static readonly EnemyFrameId[] {family} =')
            lines.append('    [')
            lines.extend(f'        EnemyFrameId.{frame},' for frame in frames)
            lines.append('    ];')
        lines.append('')
    lines.append('    /// <summary>The frames each enemy definition can show.</summary>')
    lines.append('    internal static IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<EnemyFrameId>> Frames { get; } =')
    lines.append('        new Dictionary<EnemyDefinitionId, IReadOnlyList<EnemyFrameId>>')
    lines.append('        {')
    for family, _, names, _ in families:
        lines.extend(f'            [EnemyDefinitionId.{name}] = {family},' for name in names)
    lines.append('        };')
    lines.append('}')
    content = '\n'.join(lines) + '\n'

    if '--check' in sys.argv:
        current = read(OUTPUT).replace('\r\n', '\n') if os.path.exists(OUTPUT) else ''
        if current != content:
            raise SystemExit(f'{OUTPUT} is stale; run tools/generate-enemy-definition-frames.py')
        print('enemy definition frames current')
        return
    with open(OUTPUT, 'w', encoding='utf-8', newline='\n') as handle:
        handle.write(content)
    print(f'wrote {OUTPUT}: {len(families)} families, {len(owner)} definitions, '
          f'{sum(len(frames) for _, _, _, frames in families)} frames')


main()
