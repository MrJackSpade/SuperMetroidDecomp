"""Writes the Godot project's room scenes from the old code's room definitions.

    python tools/godot-rooms/generate_rooms.py

Each room of the game becomes one scene under godot/game/rooms/<area>/. What a room is made of is read from the C#
port's definition files as text (see legacy.py) and from one file of the installed assets, which says where each
door's blocks lie. Nothing of the old game is run.

A room's scene holds:
- its place on the map and its versions (the old code's "states"), each with the condition it is chosen on, its
  layout and its music;
- its terrain, which loads the layout of the version in force;
- an exit for each door, leading to the exit it arrives at in the room beyond;
- a marker for each place play can begin at in it: where a save resumes, where an elevator sets down, where Samus
  is set down in Ceres and where her gunship lands;
- its enemies, each with the settings its placement gives it (see enemies.py). One placed in some versions of the
  room and not in others is present only when one of those versions is the one in force.

It also lists every room in the game's definition, and names the room and marker a new game begins at.
"""
from __future__ import annotations

import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(__file__))
import enemies  # noqa: E402
import legacy  # noqa: E402

GODOT = os.path.normpath(os.path.join(os.path.dirname(__file__), '..', '..', 'godot'))
INSTALLED = os.path.join(os.environ.get('LOCALAPPDATA', ''), 'SuperMetroid', 'game')
BLOCK = 16
SCREEN = 256

# How far inside a room, from the door's blocks, someone who arrives through the door is put, and how far above the
# door's sill their middle is.
ARRIVAL_INSET = 40
ARRIVAL_ABOVE_SILL = 24

# The boss each area's "area boss", "mini boss" and "torizo" is, by the name its defeat is recorded under.
AREA_BOSSES = {
    ('Crateria', 'AreaTorizo'): 'bomb_torizo',
    ('Brinstar', 'AreaBoss'): 'kraid', ('Brinstar', 'AreaMiniBoss'): 'spore_spawn',
    ('Norfair', 'AreaBoss'): 'ridley', ('Norfair', 'AreaMiniBoss'): 'crocomire', ('Norfair', 'AreaTorizo'): 'golden_torizo',
    ('WreckedShip', 'AreaBoss'): 'phantoon',
    ('Maridia', 'AreaBoss'): 'draygon', ('Maridia', 'AreaMiniBoss'): 'botwoon',
    ('Tourian', 'AreaBoss'): 'mother_brain',
    ('Ceres', 'AreaBoss'): 'ceres_ridley',
}

# The story events whose names changed on the way over.
EVENT_NAMES = {
    'ZebesTimebombSet': 'escape_started',
    'BabyMetroidEncounterMusicSkipped': 'baby_metroid_music_skipped',
    'MaridiaNoobTubeBroken': 'maridia_tube_broken',
    'CrittersEscaped': 'animals_escaped',
}

OPPOSITE = {'Left': 'Right', 'Right': 'Left', 'Up': 'Down', 'Down': 'Up'}


def snake(name: str) -> str:
    """CamelCase as lower-case words joined by underscores."""
    spaced = re.sub(r'(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])', '_', name)
    return spaced.lower()


def room_scene(room: legacy.Room) -> str:
    return f'res://game/rooms/{snake(room.area)}/{snake(room.name)}.tscn'


class Scene:
    """A scene's text, gathered a piece at a time."""

    def __init__(self) -> None:
        self.external: dict[tuple[str, str], str] = {}
        self.sub: list[str] = []
        self.nodes: list[str] = []
        self.names: set[str] = set()
        self.promised: set[str] = set()
        self.resources: dict[tuple, str] = {}
        self._sub_count = 0

    def name(self, base: str) -> str:
        """
        A name no node of the scene has or has been promised: the base, or the base with the first free number after
        it. The name is promised to whoever asked.
        """
        name, number = base, 1
        while name in self.names or name in self.promised:
            number += 1
            name = f'{base}{number}'
        self.promised.add(name)
        return name

    def script(self, path: str) -> str:
        return self._external('Script', path)

    def packed(self, path: str) -> str:
        return self._external('PackedScene', path)

    def _external(self, kind: str, path: str) -> str:
        key = (kind, path)
        if key not in self.external:
            self.external[key] = str(len(self.external) + 1)
        return f'ExtResource("{self.external[key]}")'

    def resource(self, script: str, properties: dict[str, str]) -> str:
        # A resource holds values only, so two that would be alike are one.
        alike = (script, tuple(properties.items()))
        if alike in self.resources:
            return self.resources[alike]
        self._sub_count += 1
        ident = f'r{self._sub_count}'
        lines = [f'[sub_resource type="Resource" id="{ident}"]', f'script = {self.script(script)}']
        lines += [f'{key} = {value}' for key, value in properties.items()]
        self.sub.append('\n'.join(lines))
        self.resources[alike] = f'SubResource("{ident}")'
        return self.resources[alike]

    def node(self, name: str, parent: str | None, properties: dict[str, str], script: str | None = None,
             instance: str | None = None, kind: str = 'Node2D') -> None:
        # The root's name is its own; everything else here is placed straight under it and must differ.
        if parent is not None:
            if name in self.names:
                raise ValueError(f'Two nodes of one scene are named {name}.')
            self.names.add(name)
        head = f'[node name="{name}"'
        if instance is None:
            head += f' type="{kind}"'
        if parent is not None:
            head += f' parent="{parent}"'
        if instance is not None:
            head += f' instance={self.packed(instance)}'
        lines = [head + ']']
        if script is not None:
            lines.append(f'script = {self.script(script)}')
        lines += [f'{key} = {value}' for key, value in properties.items()]
        self.nodes.append('\n'.join(lines))

    def text(self) -> str:
        steps = len(self.external) + len(self.sub) + 1
        parts = [f'[gd_scene load_steps={steps} format=3]']
        if self.external:
            parts.append('\n'.join(
                f'[ext_resource type="{kind}" path="{path}" id="{ident}"]' for (kind, path), ident in self.external.items()))
        parts += self.sub
        parts += self.nodes
        return '\n\n'.join(parts) + '\n'


def quoted(text: str) -> str:
    return '"' + text.replace('\\', '\\\\').replace('"', '\\"') + '"'


def condition(scene: Scene, room: legacy.Room, expression: str) -> str:
    """The Condition resource for one of the old code's conditions on a room's state."""
    script = 'res://engine/world/Condition.cs'
    fact, owns, carries, all_of = 0, 2, 7, 4

    def asks(kind: int, about: str) -> str:
        return scene.resource(script, {'Kind': str(kind), 'About': quoted(about)})

    name = legacy.last_name(expression.split('(')[0])
    if name == 'BossDead':
        which = legacy.last_name(legacy.call_arguments(expression)[0])
        return asks(fact, AREA_BOSSES[(room.area, which)] + '_defeated')
    if name == 'Event':
        event = legacy.last_name(legacy.call_arguments(expression)[0])
        return asks(fact, EVENT_NAMES.get(event, snake(event)))
    if name == 'PowerBombs':
        return asks(carries, 'power_bombs')
    if name == 'MorphBallAndMissiles':
        parts = [asks(owns, 'morph_ball'), asks(carries, 'missiles')]
        return scene.resource(script, {'Kind': str(all_of), 'Parts': '[' + ', '.join(parts) + ']'})
    raise ValueError(f'The room {room.name} is chosen on a condition this does not know: {expression}')


# The theme of each set of music, which a state may ask for without naming it.
AREA_THEMES = {
    'EmptyCrateria': 'CrateriaArrival', 'LowerCrateria': 'LowerCrateria', 'UpperCrateria': 'SamusTheme',
    'GreenBrinstar': 'GreenBrinstar', 'RedBrinstar': 'RedBrinstar', 'UpperNorfair': 'UpperNorfair',
    'LowerNorfair': 'LowerNorfair', 'Maridia': 'EastMaridia', 'Tourian': 'Tourian', 'MotherBrain': 'MotherBrainBattle',
    'RidleyAndDraygonBattles': 'RidleyAndDraygonBattle', 'KraidAndPhantoonBattles': 'KraidAndPhantoonBattle',
    'MiniBossFight': 'MiniBossBattle', 'Ceres': 'CeresFlight', 'WreckedShip': 'WreckedShipPoweredDown',
    'BabyMetroidEncounter': 'KraidAndPhantoonBattle', 'SamusThemeUpperCrateria': 'SamusTheme',
}


def music_of(state: legacy.State, nearby_set: str) -> dict[str, str]:
    """
    What a version says about music: a name, silence, or that whatever is playing goes on. One that asks for "the
    theme" without saying of what means the theme of the music set already loaded, which is the set the rooms about
    it use: `nearby_set`.
    """
    track = re.search(r'(?<![A-Za-z])MusicTrack\.(\w+)', state.music)
    if track:
        return {'Music': quoted(snake(track.group(1)))}
    if 'PlaysAreaTheme' in state.music:
        named = re.search(r'MusicDataSet\.(\w+)', state.music)
        return {'Music': quoted(snake(AREA_THEMES[named.group(1) if named else nearby_set]))}
    if 'Silenced' in state.music:
        return {'Music': quoted('')}
    if 'Inherited' in state.music:
        return {'KeepsMusic': 'true'}
    raise ValueError(f'The state {state.name} asks for music this does not know: {state.music}')


def layout_of(room: legacy.Room, state: legacy.State) -> str:
    return f'rooms/{snake(room.area)}/{snake(state.level)}.json'


class Generator:
    def __init__(self) -> None:
        self.rooms = legacy.rooms()
        self.states = legacy.states()
        self.choices = legacy.state_choices()
        self.doors = legacy.doors()
        self.populations = legacy.enemy_populations()
        # The kinds of enemy no rule places yet, with how many placements of each were passed over.
        self.unplaced: dict[str, int] = {}
        with open(os.path.join(INSTALLED, 'room-level-streams', 'room-door-blocks.json'), encoding='utf-8') as source:
            self.door_blocks = json.load(source)['rooms']
        self.by_name = {room.name: key for key, room in self.rooms.items()}
        # Each room's doors: the name of each, with the rectangle its blocks cover, in pixels.
        self.room_doors: dict[str, dict[str, tuple[int, int, int, int]]] = {}
        for key, room in self.rooms.items():
            levels = self.door_blocks.get(room.name, {})
            default_level = self.states[self.choices[key][0]].level
            blocks = levels.get(default_level) or (next(iter(levels.values())) if levels else {})
            doors = {}
            for door, cells in blocks.items():
                # The blocks an elevator stands in are marked as a door that leads nowhere; the elevator is an object.
                if door == 'ElevatorPseudoDoor':
                    continue
                xs, ys = [cell['x'] for cell in cells], [cell['y'] for cell in cells]
                doors[door] = (min(xs) * BLOCK, min(ys) * BLOCK, (max(xs) - min(xs) + 1) * BLOCK, (max(ys) - min(ys) + 1) * BLOCK)
            self.room_doors[key] = doors

    def nearby_music_set(self, key: str) -> str:
        """The set of music the nearest room that names one uses, going outward from `key` through its doors."""
        seen, queue = {key}, [key]
        while queue:
            at = queue.pop(0)
            named = re.search(r'MusicDataSet\.(\w+)', self.states[self.choices[at][0]].music)
            if named and at != key:
                return named.group(1)
            for door in self.room_doors[at]:
                beyond = self.doors[door].to_room
                if beyond not in seen:
                    seen.add(beyond)
                    queue.append(beyond)
        raise ValueError(f'No room within reach of {key} names a set of music.')

    def exit_name(self, room: legacy.Room, door: str) -> str:
        return door[len(room.name):] if door.startswith(room.name) and len(door) > len(room.name) else door

    def arrival_in(self, key: str, door: legacy.Door) -> str | None:
        """The exit of the room `key` that someone coming through `door` arrives at, if it has one."""
        left, top = door.to_screen[0] * SCREEN, door.to_screen[1] * SCREEN
        best, best_distance = None, None
        for name, (x, y, _, _) in self.room_doors[key].items():
            other = self.doors.get(name)
            if other is None or other.direction != OPPOSITE.get(door.direction):
                continue
            distance = abs(x - left) + abs(y - top)
            inside = left <= x < left + SCREEN and top <= y < top + SCREEN
            if inside:
                return self.exit_name(self.rooms[key], name)
            if best_distance is None or distance < best_distance:
                best, best_distance = name, distance
        # A door with none facing it in the screen it leads to arrives at the nearest one that faces it, if any.
        return self.exit_name(self.rooms[key], best) if best else None

    def present_when(self, scene: Scene, room: legacy.Room, key: str, versions: set[int]) -> str | None:
        """
        The condition under which one of the room's versions in `versions` is the one in force, as a Condition
        resource; None when that is every version. Versions are numbered in the order they are tried, the one with no
        condition last.
        """
        script = 'res://engine/world/Condition.cs'
        any_of, none_of, all_of = 5, 6, 4
        expressions = [expression for expression, _ in self.choices[key][1]]
        if versions == set(range(len(expressions) + 1)):
            return None

        def gathered(kind: int, parts: list[str]) -> str:
            return scene.resource(script, {'Kind': str(kind), 'Parts': '[' + ', '.join(parts) + ']'})

        ways = []
        for version in sorted(versions):
            earlier = [condition(scene, room, expression) for expression in expressions[:version]]
            parts = [gathered(none_of, earlier)] if earlier else []
            if version < len(expressions):
                parts.insert(0, condition(scene, room, expressions[version]))
            ways.append(parts[0] if len(parts) == 1 else gathered(all_of, parts))
        return ways[0] if len(ways) == 1 else gathered(any_of, ways)

    def place_enemies(self, key: str, scene: Scene, room: legacy.Room) -> None:
        """Places the enemies of every version of the room, each once, present in the versions that list it."""
        default, conditional = self.choices[key]
        versions = [name for _, name in conditional] + [default]
        placed: dict[tuple, tuple[enemies.Placed, set[int], dict[str, tuple]]] = {}
        # The default version's enemies come first, in the order it lists them; the order enemies are listed in is the
        # order they act in.
        for version in reversed(range(len(versions))):
            listed = self.populations[self.states[versions[version]].enemies]
            results = dict(enemies.places(listed, enemies.RULES, self.unplaced))
            # Two alike in one version are two things; two alike in two versions are one thing in both.
            counts: dict[tuple, int] = {}
            plain: dict[int, tuple] = {}
            for index, result in results.items():
                alike = (result.scene, result.name, result.at, tuple(sorted(result.properties.items())))
                counts[alike] = counts.get(alike, 0) + 1
                plain[index] = alike + (counts[alike],)
            for index, result in results.items():
                refers = {name: plain[target] for name, target in result.refers.items()}
                signature = plain[index] + tuple(sorted(refers.items()))
                if signature not in placed:
                    placed[signature] = (result, set(), refers)
                placed[signature][1].add(version)

        names = {signature[:5]: scene.name(result.name) for signature, (result, _, _) in placed.items()}
        for signature, (result, present_in, refers) in placed.items():
            properties = {'position': f'Vector2({result.at[0]}, {result.at[1]})', **result.properties}
            for name, target in refers.items():
                properties[name] = f'NodePath("../{names[target]}")'
            present = self.present_when(scene, room, key, present_in)
            if present is not None:
                properties['PresentWhen'] = present
            scene.node(names[signature[:5]], '.', properties, instance=result.scene)

    def write_room(self, key: str) -> str:
        room = self.rooms[key]
        scene = Scene()
        default, conditional = self.choices[key]

        versions = []
        for expression, state_name in conditional:
            state = self.states[state_name]
            versions.append(scene.resource('res://engine/world/RoomVersion.cs', {
                'When': condition(scene, room, expression),
                'LayoutFile': quoted(layout_of(room, state)),
                **music_of(state, self.nearby_music_set(key)),
            }))
        base = self.states[default]
        versions.append(scene.resource('res://engine/world/RoomVersion.cs', {
            'LayoutFile': quoted(layout_of(room, base)),
            **music_of(base, self.nearby_music_set(key)),
        }))

        scene.node(room.name, None, {
            'Area': quoted(snake(room.area)),
            'MapCell': f'Vector2i({room.map_cell[0]}, {room.map_cell[1]})',
            'Versions': '[' + ', '.join(versions) + ']',
        }, script='res://engine/world/Room.cs')
        scene.node('Terrain', '.', {'LayoutFile': quoted(layout_of(room, base))}, script='res://engine/world/Terrain.cs')

        entries: list[tuple[str, int, int]] = []
        for name, (x, y, width, height) in self.room_doors[key].items():
            door = self.doors.get(name)
            if door is None:
                raise ValueError(f'The installed door blocks name a door the definitions do not have: {name}')
            target_key = door.to_room
            target = self.rooms[target_key]
            arrives_at = self.arrival_in(target_key, door)
            if arrives_at is None:
                # Nothing faces this door where it leads: it arrives at a marker of its own there.
                arrives_at = 'From' + name
                self.markers.setdefault(target_key, []).append((arrives_at, door))
            across = door.direction in ('Left', 'Right')
            # Someone arriving at this exit comes the other way through it, and is put just inside the room.
            if across:
                offset_x = ARRIVAL_INSET if door.direction == 'Left' else width - ARRIVAL_INSET
                offset_y = height - ARRIVAL_ABOVE_SILL
            else:
                offset_x = width // 2
                offset_y = ARRIVAL_INSET if door.direction == 'Up' else height - ARRIVAL_INSET
            scene.node(self.exit_name(room, name), '.', {
                'position': f'Vector2({x}, {y})',
                'Size': f'Vector2i({width}, {height})',
                'LeadsToRoom': quoted(room_scene(target)),
                'LeadsToExit': quoted(arrives_at),
                'Style': '0' if across else '1',
                'ArrivalOffset': f'Vector2i({offset_x}, {offset_y})',
            }, script='res://engine/world/RoomExit.cs')
            entries.append((name, x, y))

        self.place_enemies(key, scene, room)
        self.scenes[key] = scene
        return room_scene(room)

    def run(self) -> None:
        self.scenes: dict[str, Scene] = {}
        self.markers: dict[str, list[tuple[str, legacy.Door]]] = {}
        paths = [self.write_room(key) for key in self.rooms]
        for key, markers in self.markers.items():
            for name, door in markers:
                x = door.to_screen[0] * SCREEN + SCREEN // 2
                y = door.to_screen[1] * SCREEN + SCREEN // 2
                self.scenes[key].node(name, '.', {'position': f'Vector2({x}, {y})'}, kind='Marker2D')
        for station, (door, (x, y)) in legacy.load_stations().items():
            # The Ceres elevator sets her down from the top of its shaft.
            if station == 'CeresElevator':
                y = 0
            self.scenes[self.doors[door].to_room].node('At' + station, '.', {'position': f'Vector2({x}, {y})'}, kind='Marker2D')
        for key, scene in self.scenes.items():
            path = os.path.join(GODOT, room_scene(self.rooms[key])[len('res://'):])
            os.makedirs(os.path.dirname(path), exist_ok=True)
            with open(path, 'w', encoding='utf-8', newline='\n') as output:
                output.write(scene.text())
        exits = sum(len(doors) for doors in self.room_doors.values())
        markers = sum(len(markers) for markers in self.markers.values())
        print(f'{len(paths)} rooms written, with {exits} exits and {markers} arrival markers')
        if self.unplaced:
            print('not placed yet: ' + ', '.join(f'{kind} x{count}' for kind, count in sorted(self.unplaced.items())))
        self.write_definition(sorted(paths))

    def write_definition(self, paths: list[str]) -> None:
        """Lists the rooms in the game's definition, and names where a new game begins."""
        path = os.path.join(GODOT, 'game', 'game_definition.tres')
        with open(path, encoding='utf-8') as source:
            lines = [line for line in source.read().split('\n')
                     if not line.startswith(('Rooms = ', 'FirstRoom = ', 'FirstEntryPoint = '))]
        while lines and not lines[-1]:
            lines.pop()
        lines.append('FirstRoom = ' + quoted(room_scene(self.rooms['CeresElevator'])))
        lines.append('FirstEntryPoint = "AtCeresElevator"')
        lines.append('Rooms = PackedStringArray(' + ', '.join(quoted(scene) for scene in paths) + ')')
        with open(path, 'w', encoding='utf-8', newline='\n') as output:
            output.write('\n'.join(lines) + '\n')


if __name__ == '__main__':
    Generator().run()
