"""Writes godot/game/tests/movies/CartridgeProgress.Generated.cs: what each of the cartridge's bits of progress is in
the Godot game, and where each of its numbered places to begin play lies.

The cartridge keeps progress as bits: an item collected and a door opened by the number room data gives it, a story
event by its number, a boss beaten by its area and kind; and it keeps where a game was saved as an area and a number.
The Godot game keeps progress as named facts and a saved game's place as a room and an entry point. The numbers are
the old tables' (csharp/src/SuperMetroid.AssetExtraction/Progression/); the names are made as the room generator makes
them (tools/godot-rooms/), and each is checked to be a name the game's rooms or code use. The table is the movie
tests' alone: the oracle's side, which the game never reads.

Run from anywhere: python tools/movie-trace/cartridge_progress.py
"""
import os
import re
import sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(__file__), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'tools', 'godot-rooms'))
import generate_rooms  # noqa: E402
import legacy  # noqa: E402

PROGRESSION = os.path.join(ROOT, 'csharp', 'src', 'SuperMetroid.AssetExtraction', 'Progression')
GAME = os.path.join(ROOT, 'godot', 'game')
OUT = os.path.join(GAME, 'tests', 'movies', 'CartridgeProgress.Generated.cs')

# The cartridge's areas in the order of their numbers (AreaNumberCodec.cs), as the old code names them.
AREAS = ['Crateria', 'Brinstar', 'Norfair', 'WreckedShip', 'Maridia', 'Tourian', 'Ceres']
# The cartridge's bits for the bosses of an area (ProgressionTableCodec.cs, DefeatedBosses.cs).
BOSS_BITS = {1: 'AreaBoss', 2: 'AreaMiniBoss', 4: 'AreaTorizo'}
# The story events the game records under a name the room generator's own table of renames does not give.
EVENT_FACTS = {'LowerNorfairChozoLoweredAcid': generate_rooms.CHOZO_ACID_LOWERED, 'OutranSpeedBoosterLavaquake': 'outran_lava_quake'}


def snake(name: str) -> str:
    return generate_rooms.snake(name)


def read(name: str) -> str:
    return re.sub(r'//[^\n]*', '', open(os.path.join(PROGRESSION, name), encoding='utf-8-sig').read())


def used_names() -> str:
    """Everything the game's scenes and code say, to check a made name against."""
    parts = []
    for folder, _, files in os.walk(GAME):
        if os.sep + 'tests' in folder:
            continue
        for name in files:
            if name.endswith(('.tscn', '.cs', '.tres')):
                parts.append(open(os.path.join(folder, name), encoding='utf-8-sig').read())
    return '\n'.join(parts)


def main() -> int:
    used = used_names()
    unused: list[str] = []

    def fact(name: str) -> str:
        if f'"{name}"' not in used:
            unused.append(name)
        return name

    numbers = read('RoomPlacementNumbers.Generated.cs')
    doors = {int(number): snake(name) for number, name in re.findall(r'new\((\d+), LockedDoorDefinitions\.(\w+)\)', numbers)}
    # A door the game remembers opened has a barrier remembered by its name. One it does not (a grey door the game
    # holds shut by what is true in the room each time) has no fact: its bit cannot be carried into the game.
    not_kept = sorted(number for number, name in doors.items() if f'"{name}"' not in used)
    items = {int(number): fact(snake(name)) for number, name in re.findall(r'new\((\d+), ItemLocationDefinitions\.(\w+)\)', numbers)}
    if not doors or not items:
        raise SystemExit('the old table of numbered doors and items lists nothing this can read')

    listed = re.search(r'Numbered =\s*\[(.*?)\];', read('StoryEventNumbers.cs'), re.S)
    if listed is None:
        raise SystemExit('the old table of numbered events cannot be read')
    events = {}
    for number, entry in enumerate(part.strip() for part in listed.group(1).split(',') if part.strip()):
        if entry != 'null':
            name = entry.split('.')[-1]
            events[number] = fact(EVENT_FACTS.get(name) or generate_rooms.EVENT_NAMES.get(name, snake(name)))

    bosses = {}
    for (area, kind), boss in generate_rooms.AREA_BOSSES.items():
        bit = next(bit for bit, named in BOSS_BITS.items() if named == kind)
        bosses[(AREAS.index(area), bit)] = fact(boss + '_defeated')

    rooms, all_doors = legacy.rooms(), legacy.doors()
    station_numbers = dict(re.findall(r'\[LoadStationDefinitions\.(\w+)\] = (\d+)', read('LoadStationNumberCodec.cs')))
    stations = {}
    for station, (door, _, _) in legacy.load_stations().items():
        room = rooms[all_doors[door].to_room]
        scene = generate_rooms.room_scene(room)
        # A game saved at the gunship is begun again from the gunship itself, as the game's own saves there say.
        entry = station if station == 'Gunship' else 'At' + station
        if f'[node name="{entry}"' not in open(os.path.join(ROOT, 'godot', scene[len('res://'):]), encoding='utf-8').read():
            raise SystemExit(f'the room {scene} has no entry point {entry}')
        key = (AREAS.index(room.area), int(station_numbers[station]))
        if key in stations:
            raise SystemExit(f'two places to begin play share the area and number {key}')
        stations[key] = (scene, entry)

    lines = [
        '// Written by tools/movie-trace/cartridge_progress.py from the old tables of numbered progress. Do not edit: run the tool again.',
        '#nullable enable',
        'using System.Collections.Generic;',
        '',
        'namespace Game.Tests.Movies;',
        '',
        '/// <summary>',
        '/// What each of the cartridge\'s bits of progress is in the game, by the fact the game records for it, and where each',
        '/// of the cartridge\'s numbered places to begin play lies: the oracle\'s side of a movie\'s check.',
        '/// </summary>',
        'public static class CartridgeProgress',
        '{',
        '    /// <summary>The fact of each door opened, by the number room data gives the door.</summary>',
        '    public static IReadOnlyDictionary<int, string> Doors { get; } = new Dictionary<int, string>',
        '    {',
        *[f'        [{number}] = "{name}",' for number, name in sorted(doors.items())],
        '    };',
        '',
        '    /// <summary>',
        '    /// The doors the game keeps no fact for: grey doors it holds shut by what is true of the room each time it is',
        '    /// entered, where the cartridge also keeps a bit once one has opened. Such a bit cannot be carried into the game.',
        '    /// </summary>',
        '    public static IReadOnlySet<int> DoorsNotKept { get; } = new HashSet<int> { ' + ', '.join(str(number) for number in not_kept) + ' };',
        '',
        '    /// <summary>The fact of each item collected, by the number room data gives the place it lies.</summary>',
        '    public static IReadOnlyDictionary<int, string> Items { get; } = new Dictionary<int, string>',
        '    {',
        *[f'        [{number}] = "{name}",' for number, name in sorted(items.items())],
        '    };',
        '',
        '    /// <summary>The fact of each story event, by the event\'s number.</summary>',
        '    public static IReadOnlyDictionary<int, string> Events { get; } = new Dictionary<int, string>',
        '    {',
        *[f'        [{number}] = "{name}",' for number, name in sorted(events.items())],
        '    };',
        '',
        '    /// <summary>The fact of each boss beaten, by the number of its area and its bit of the area\'s byte.</summary>',
        '    public static IReadOnlyDictionary<(int Area, int Bit), string> Bosses { get; } = new Dictionary<(int Area, int Bit), string>',
        '    {',
        *[f'        [({area}, {bit})] = "{name}",' for (area, bit), name in sorted(bosses.items())],
        '    };',
        '',
        '    /// <summary>Where play begins from each of the cartridge\'s numbered places, by the number of its area and its own: the room\'s scene and the entry point in it.</summary>',
        '    public static IReadOnlyDictionary<(int Area, int Number), (string Room, string EntryPoint)> Stations { get; } =',
        '        new Dictionary<(int Area, int Number), (string Room, string EntryPoint)>',
        '    {',
        *[f'        [({area}, {number})] = ("{scene}", "{entry}"),' for (area, number), (scene, entry) in sorted(stations.items())],
        '    };',
        '}',
    ]
    with open(OUT, 'w', encoding='utf-8', newline='\n') as out:
        out.write('\n'.join(lines) + '\n')
    print(f'{len(doors)} doors ({len(not_kept)} the game keeps no fact for), {len(items)} items, {len(events)} events, {len(bosses)} bosses, {len(stations)} places to begin written')
    if unused:
        print(f'{len(unused)} names the game\'s scenes and code never say: ' + ' '.join(unused))
        return 1
    return 0


if __name__ == '__main__':
    sys.exit(main())
