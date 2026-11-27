"""Writes godot/game/tests/movies/CartridgeRooms.Generated.cs: each room's address on the cartridge, with the scene the
Godot game plays it from.

The addresses are the old table's (csharp/src/SuperMetroid.Tooling/AssetExtraction/Rooms/RoomHeaderSourceDefinitions.cs);
the scenes are named as the room generator names them (tools/godot-rooms/generate_rooms.py, room_scene). The table is the
movie tests' alone: the oracle's side of a comparison, which the game never reads.

Run from anywhere: python tools/movie-trace/cartridge_rooms.py
"""
import os
import re
import sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(__file__), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'tools', 'godot-rooms'))
import legacy  # noqa: E402

TABLE = os.path.join(ROOT, 'csharp', 'src', 'SuperMetroid.Tooling', 'AssetExtraction', 'Rooms', 'RoomHeaderSourceDefinitions.cs')
OUT = os.path.join(ROOT, 'godot', 'game', 'tests', 'movies', 'CartridgeRooms.Generated.cs')


def snake(name: str) -> str:
    return re.sub(r'(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])', '_', name).lower()


def main() -> int:
    rooms = legacy.rooms()
    listed = re.findall(r'\[0x([0-9a-fA-F]{4})\] = RoomDefinitions\.(\w+),', open(TABLE, encoding='utf-8-sig').read())
    if not listed:
        raise SystemExit('the old table of room addresses lists nothing this can read')
    lines = [
        '// Written by tools/movie-trace/cartridge_rooms.py from the old table of room addresses. Do not edit: run the tool again.',
        '#nullable enable',
        'namespace Game.Tests.Movies;',
        '',
        '/// <summary>Each room\'s address on the cartridge, with the scene the game plays it from: the oracle\'s side of a movie\'s check.</summary>',
        'public static class CartridgeRooms',
        '{',
        '    /// <summary>The scene of the room the cartridge keeps at <paramref name="address"/>.</summary>',
        '    /// <exception cref="System.Collections.Generic.KeyNotFoundException">The cartridge keeps no room there.</exception>',
        '    public static string SceneOf(int address) => Scenes.TryGetValue(address, out string? scene)',
        '        ? scene',
        '        : throw new System.Collections.Generic.KeyNotFoundException($"The cartridge keeps no room at ${address:X4}.");',
        '',
        '    private static readonly System.Collections.Generic.Dictionary<int, string> Scenes = new()',
        '    {',
    ]
    for address, name in sorted(listed):
        if name not in rooms:
            raise SystemExit(f'the room {name} at ${address} is not among the old room definitions')
        scene = f'res://game/rooms/{snake(rooms[name].area)}/{snake(rooms[name].name)}.tscn'
        if not os.path.exists(os.path.join(ROOT, 'godot', scene[len('res://'):])):
            raise SystemExit(f'the room {name} at ${address} has no scene at {scene}')
        lines.append(f'        [0x{address.upper()}] = "{scene}",')
    lines += ['    };', '}']
    with open(OUT, 'w', encoding='utf-8', newline='\n') as out:
        out.write('\n'.join(lines) + '\n')
    print(f'{len(listed)} rooms written')
    return 0


if __name__ == '__main__':
    sys.exit(main())
