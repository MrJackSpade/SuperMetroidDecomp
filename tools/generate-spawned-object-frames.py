"""Generates csharp/src/SuperMetroid.Core/Enemies/Artwork/SpawnedObjectFrameDefinitions.cs (#1392).

Enemy projectiles and room sprite objects draw from sheets composed per graphics set and tile base. A spawned object
shows its spawner's characters (at the spawner's tile base) or the room's (at base 0), so a sheet needs the frames its
spawners can show. This generator reads them from the source as tools/generate-enemy-definition-frames.py reads enemy
frames: for each enemy behaviour, the projectile frames, projectile animation families, projectile kinds and room
sprite-object kinds its code reaches; and for the code outside every behaviour that spawns objects, the same, as the
objects any room can show. Run from the repository root; --check exits nonzero when the committed file differs.
"""
import glob
import os
import re
import sys

CORE = os.path.join('csharp', 'src', 'SuperMetroid.Core')
OUTPUT = os.path.join(CORE, 'Enemies', 'Artwork', 'SpawnedObjectFrameDefinitions.cs')
FRAME_ENUM = os.path.join(CORE, 'Enemies', 'Projectiles', 'EnemyProjectileFrameId.cs')
TYPED_STARTS = os.path.join(CORE, 'Enemies', 'Projectiles', 'EnemyProjectileTypedStarts.cs')
# Tables that name every kind or frame, this generator's own output among them; reading them would make every kind
# every spawner's.
TABLES = {os.path.join(CORE, 'Enemies', 'Projectiles', name) for name in
          ('EnemyProjectileTypedStarts.cs', 'EnemyProjectileDefinitionCatalog.cs', 'EnemyProjectileFrames.cs')} | {
    OUTPUT, os.path.join(CORE, 'Enemies', 'Artwork', 'EnemyProjectileSpritemapCatalog.cs')}
# Types every family names that carry no frames of their own; following them would merge unrelated families.
SHARED = {'EnemyFrameId', 'EnemyDefinitionId', 'SpriteFrameBuffer', 'SpriteSource', 'RoomEnemySlot', 'SamusState',
          'EnemyPresentation', 'RoomEnemySystem', 'EnemyFrameTable', 'EnemyProjectileFrameId', 'RoomEnemyProjectileKind',
          'RoomSpriteObjectKind', 'EnemyProjectileEngine', 'RoomEnemyProjectileSlot', 'RoomSpriteObjectPool'}
FAMILY_SUFFIXES = ('FrameDefinitions', 'VisualDefinitions', 'Animations', 'Frames')
# Spawn calls and animation starts mark the code outside the behaviours that spawns objects for any room.
SPAWNS = re.compile(r'InitializeEnemyProjectileFromDefinition\(|SpawnRoomSpriteObject\(|StartAnimation\(new |'
                    r'EnemyProjectileFrameId\.\w+')
EXCLUDED_FRAMES = {'Blank'}


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
    order = {name: index for index, name in enumerate(re.findall(r'^\s+(\w+)(?: = \w+)?,\s*$', read(FRAME_ENUM), re.M))}
    family_pattern = re.compile(r'class (\w+Animation)\b[^{;]*?:\s*EnemyProjectileProgramAnimation<', re.S)
    families = {match.group(1): path for path, text in texts.items() for match in family_pattern.finditer(text)}
    starts = dict(re.findall(r'\[RoomEnemyProjectileKind\.(\w+)\] = \(\) => new (\w+)\(', read(TYPED_STARTS)))

    def walk(start, follow):
        """The files reached from start, following the type names follow accepts."""
        seen_paths, seen_types = set(), set()
        stack = [start]
        while stack:
            path = stack.pop()
            if path in seen_paths:
                continue
            seen_paths.add(path)
            for name in set(re.findall(r'\b([A-Z]\w+)\b', texts[path])):
                if name in SHARED or name in seen_types or name not in types:
                    continue
                for candidate in types[name]:
                    if candidate not in TABLES and follow(start, name, candidate):
                        seen_types.add(name)
                        stack.append(candidate)
        return seen_paths

    def own_tree(start, name, candidate):
        # A behaviour reaches the helper behaviours it calls, whose spawns are its own.
        return (os.path.dirname(candidate).startswith(os.path.dirname(start)) or name.endswith(FAMILY_SUFFIXES)
                or name.endswith('Behavior') or name in families)

    def names(paths, pattern):
        return {name for path in paths for name in re.findall(pattern, texts[path])}

    def frame_tables(start, name, candidate):
        # A family presents its frames through the frame tables it names; their own folders hold other families'.
        return name.endswith(('FrameDefinitions', 'VisualDefinitions', 'Definitions', 'Frames')) and not name.endswith('Animations')

    family_frames = {}
    family_paths = set()
    for family, path in families.items():
        paths = {path} | {candidate for name in set(re.findall(r'\b([A-Z]\w+)\b', texts[path])) if name not in SHARED
                          for candidate in types.get(name, ()) if candidate not in TABLES and frame_tables(path, name, candidate)}
        family_paths |= paths
        # A family that only ever shows the blank frame, which draws nothing, holds no frames.
        family_frames[family] = names(paths, r'\bEnemyProjectileFrameId\.(\w+)') - EXCLUDED_FRAMES

    def spawned(paths):
        """The projectile frames and sprite-object kinds the code in paths spawns."""
        frames = names(paths, r'\bEnemyProjectileFrameId\.(\w+)') - EXCLUDED_FRAMES
        reached = names(paths, r'\bnew (\w+Animation)\(') & families.keys()
        reached |= {starts[kind] for kind in names(paths, r'\bRoomEnemyProjectileKind\.(\w+)') if kind in starts}
        for family in reached:
            frames |= family_frames[family]
        unknown = frames - order.keys()
        if unknown:
            raise SystemExit(f'spawned frames missing from EnemyProjectileFrameId: {sorted(unknown)}')
        kinds = names(paths, r'\bRoomSpriteObjectKind\.(\w+)') - {'None'}
        return sorted(frames, key=order.__getitem__), sorted(kinds)

    def definitions(path, text):
        direct = re.search(r'IReadOnlyList<EnemyDefinitionId> Definitions \{ get; \} =\s*\[([^\]]*)\]', text)
        if direct is None:
            return None
        found = re.findall(r'EnemyDefinitionId\.(\w+)', direct.group(1))
        parts = re.search(r'\.\.\s*(\w+)\.Parts\.Keys', direct.group(1))
        if parts:
            table = list(types.get(parts.group(1), ()))
            if len(table) != 1:
                raise SystemExit(f'{path}: parts table {parts.group(1)} is not one Core type')
            body = texts[table[0]]
            start = body.index(' Parts { get; }')
            found += re.findall(r'\[EnemyDefinitionId\.(\w+)\]', body[start:body.index('};', start)])
        if not found:
            raise SystemExit(f'{path}: declares no definitions this generator can read')
        return found

    behaviours = []
    reached_by_behaviours = set()
    for path in sorted(files):
        served = definitions(path, texts[path])
        if served is None:
            continue
        paths = walk(path, own_tree)
        reached_by_behaviours |= paths
        frames, kinds = spawned(paths)
        if frames or kinds:
            family = re.sub(r'(Behavior|Factory)$', '', os.path.splitext(os.path.basename(path))[0])
            behaviours.append((family, os.path.relpath(path, CORE).replace(os.sep, '/'), served, frames, kinds))

    anywhere_paths = {path for path, text in texts.items()
                      if path not in reached_by_behaviours and path not in TABLES and path not in family_paths
                      and SPAWNS.search(text) and os.path.basename(path) != 'EnemyProjectileFrameId.cs'}
    anywhere_frames, anywhere_kinds = spawned(anywhere_paths)

    def array(element, name, values, summary):
        out = [f'    /// <summary>{summary}</summary>']
        if not values:
            return out + [f'    private static readonly {element}[] {name} = [];', '']
        return out + [f'    private static readonly {element}[] {name} =', '    ['] + \
            [f'        {element}.{value},' for value in values] + ['    ];', '']

    lines = [
        'using SuperMetroid.Core.Enemies.Projectiles;',
        'using SuperMetroid.Core.Rooms.SpriteObjects;',
        '',
        'namespace SuperMetroid.Core.Enemies.Artwork;',
        '',
        '/// <summary>',
        '/// The projectile frames and room sprite-object kinds each enemy definition can spawn, and those any room can show',
        '/// (#1392), which the pre-coloured spawned-object sheets hold. Generated from the source by',
        '/// tools/generate-spawned-object-frames.py; regenerate rather than edit.',
        '/// </summary>',
        '[ArchitectureReview("large", "Generated data: the projectile frames and sprite-object kinds each enemy definition spawns; '
        'it holds no logic, and splitting it by family would only scatter one table the sheet composer reads as a whole.")]',
        'internal static class SpawnedObjectFrameDefinitions',
        '{',
    ]
    for family, path, _, frames, kinds in behaviours:
        lines += array('EnemyProjectileFrameId', f'{family}Projectiles', frames, f'The projectile frames <c>{path}</c> spawns.')
        lines += array('RoomSpriteObjectKind', f'{family}SpriteObjects', kinds, f'The room sprite objects <c>{path}</c> spawns.')
    lines += array('EnemyProjectileFrameId', 'AnywhereProjectiles', anywhere_frames,
                   'The projectile frames code outside the enemy behaviours spawns in any room.')
    lines += array('RoomSpriteObjectKind', 'AnywhereSpriteObjects', anywhere_kinds,
                   'The room sprite objects code outside the enemy behaviours spawns in any room.')
    lines += [
        '    /// <summary>What each enemy definition can spawn.</summary>',
        '    internal static IReadOnlyDictionary<EnemyDefinitionId, SpawnedObjects> ByEnemy { get; } =',
        '        new Dictionary<EnemyDefinitionId, SpawnedObjects>',
        '        {',
    ]
    for family, _, served, _, _ in behaviours:
        lines += [f'            [EnemyDefinitionId.{name}] = new({family}Projectiles, {family}SpriteObjects),' for name in served]
    lines += [
        '        };',
        '',
        '    /// <summary>What code outside the enemy behaviours spawns, in any room.</summary>',
        '    internal static SpawnedObjects Anywhere { get; } = new(AnywhereProjectiles, AnywhereSpriteObjects);',
        '}',
    ]
    content = '\n'.join(lines) + '\n'
    if '--check' in sys.argv:
        current = read(OUTPUT).replace('\r\n', '\n') if os.path.exists(OUTPUT) else ''
        if current != content:
            raise SystemExit(f'{OUTPUT} is stale; run tools/generate-spawned-object-frames.py')
        print('spawned object frames current')
        return
    with open(OUTPUT, 'w', encoding='utf-8', newline='\n') as handle:
        handle.write(content)
    print(f'wrote {OUTPUT}: {len(behaviours)} spawning behaviours, {len(anywhere_frames)} anywhere frames, '
          f'{len(anywhere_kinds)} anywhere kinds; anywhere code: '
          + ', '.join(sorted(os.path.relpath(path, CORE).replace(os.sep, '/') for path in anywhere_paths)))


main()
