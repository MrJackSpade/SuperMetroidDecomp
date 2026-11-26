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
- its camera zones, and the triggers that open and close them (see camera.py);
- its liquid and its room effects: heat, rain, fog, spores, the Fireflea rooms' dark (see fx.py);
- its room sequences with what they are given, and its backdrop (see sequences.py);
- its enemies, each with the settings its placement gives it (see enemies.py), and its room objects: door caps,
  items, stations, gates and the rest (see objects.py). One placed in some versions of the room and not in others is
  present only when one of those versions is the one in force.

It also lists every room in the game's definition, and names the room and marker a new game begins at.
"""
from __future__ import annotations

import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(__file__))
import camera  # noqa: E402
import enemies  # noqa: E402
import fx  # noqa: E402
import legacy  # noqa: E402
import objects  # noqa: E402
import sequences  # noqa: E402

GODOT = os.path.normpath(os.path.join(os.path.dirname(__file__), '..', '..', 'godot'))
INSTALLED = os.path.join(os.environ.get('LOCALAPPDATA', ''), 'SuperMetroid', 'game')
BLOCK = 16
SCREEN = 256
# The ways through a door, in the order of the game's Door.Way.
DOOR_WAYS = ['Right', 'Left', 'Down', 'Up']


def samus_step(door: legacy.Door) -> float:
    """How far a door moves Samus on each update of its scroll: its own speed, or the default of its axis."""
    speed = door.speed.strip()
    if speed.endswith('AxisDefault'):
        # Twenty-five thirty-seconds of a pixel across, a pixel and a half up or down.
        return 25 / 32 if door.direction in ('Left', 'Right') else 1.5
    if speed.endswith('None'):
        return 0.0
    parts = [int(part) for part in re.search(r'PixelsPerStep\(([^)]*)\)', speed).group(1).split(',')]
    whole, numerator, denominator = (parts + [0, 1])[:3] if len(parts) == 1 else parts
    # The old speed is kept to 256ths of a pixel.
    return whole + (numerator * 256 // denominator) / 256
# How far above Samus the view's top makes for, by the name of a room's camera lead: while she moves any way but up,
# and while she moves up.
CAMERA_LEADS = {name: (int(falling), int(rising)) for name, falling, rising in re.findall(
    r'RoomCameraLead (\w+) \{ get; \} = new\((\d+), (\d+)\);', legacy.read('Rooms', 'RoomCameraLeads.cs'))}
# How far from the view's left edge Samus is kept while she faces right, and while she faces left: the middle of the
# view, less and more the old lead.
_LEAD = int(re.search(r'NormalTrackingLead = (\d+);', legacy.read('Rooms', 'Scrolling', 'HorizontalCameraTargetDefinitions.cs')).group(1))
PLAYER_IN_VIEW = (SCREEN // 2 - _LEAD, SCREEN // 2 + _LEAD)

# How far inside a room, from the door's blocks, someone who arrives through the door is put, and how far above the
# door's sill their middle is.
ARRIVAL_INSET = 40
ARRIVAL_ABOVE_SILL = 24

# How fast the Speed Booster's room's lava rises once the item's message has closed, in pixels an update: the old
# SpeedBoosterPickupDefinitions.LavaRiseVelocity, an eighth of a pixel.
SPEED_BOOSTER_LAVA_RISE = 0.125

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
        # The root's name is its own; everything placed straight under it must differ, and so must what is placed
        # under any one of those.
        if parent == '.':
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

    def listed(self, script: str, items: list[str]) -> str:
        """A list of resources of one script's kind, written as the editor writes it."""
        return f'Array[{self.script(script)}]([' + ', '.join(items) + '])'

    def text(self) -> str:
        steps = len(self.external) + len(self.sub) + 1
        parts = [f'[gd_scene load_steps={steps} format=3]']
        if self.external:
            parts.append('\n'.join(
                f'[ext_resource type="{kind}" path="{path}" id="{ident}"]' for (kind, path), ident in self.external.items()))
        parts += self.sub
        parts += self.nodes
        return '\n\n'.join(parts) + '\n'


# How long something frozen stays frozen in the rooms of an area where that is not as long as a hit says: the heat
# of Norfair thaws it sooner (the old code's NorfairFreezeUpdates).
FROZEN_UPDATES = {'Norfair': 300}


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
        return scene.resource(script, {'Kind': str(all_of), 'Parts': scene.listed(script, parts)})
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
    What a version says about music: a name, silence, or that whatever is playing goes on; the set of music it
    names, if it names one; and whether it asks for "the theme" of whatever set is loaded. The name written for a
    theme is that of the set it names or, where it names none, of the set the rooms about it use: `nearby_set`.
    """
    named = re.search(r'MusicDataSet\.(\w+)', state.music)
    of_set = {'MusicSet': quoted(snake(named.group(1)))} if named else {}
    track = re.search(r'(?<![A-Za-z])MusicTrack\.(\w+)', state.music)
    if track:
        return {'Music': quoted(snake(track.group(1))), **of_set}
    if 'PlaysAreaTheme' in state.music:
        return {'Music': quoted(snake(AREA_THEMES[named.group(1) if named else nearby_set])), **of_set, 'MusicIsTheme': 'true'}
    if 'Silenced' in state.music:
        return {'Music': quoted(''), **of_set}
    if 'Inherited' in state.music:
        return {'KeepsMusic': 'true', **of_set}
    raise ValueError(f'The state {state.name} asks for music this does not know: {state.music}')


def layout_of(room: legacy.Room, state: legacy.State) -> str:
    """
    The layout of a state: its level as drawn with its set of graphics. One level drawn with two sets, a ship with
    its power off and on, is two layouts.
    """
    return f'rooms/{snake(room.area)}/{snake(state.level)}_in_{snake(state.graphics_set)}.json'


CONDITION = 'res://engine/world/Condition.cs'


class Context:
    """What a rule that places a room's objects may ask about the room, and the conditions it may build."""

    def __init__(self, generator: 'Generator', scene: Scene, room: legacy.Room, quota: int) -> None:
        self.scene, self.room, self.quota = scene, room, quota
        self.unplaced = generator.unplaced

    def _about(self, kind: int, about: str) -> str:
        return self.scene.resource(CONDITION, {'Kind': str(kind), 'About': quoted(about)})

    def _of(self, kind: int, parts: list[str]) -> str:
        return self.scene.resource(CONDITION, {'Kind': str(kind), 'Parts': self.scene.listed(CONDITION, parts)})

    def fact(self, name: str) -> str:
        return self._about(0, name)

    def owns(self, equipment: str) -> str:
        return self._about(2, equipment)

    def killed(self, enemies_killed: int) -> str:
        return self._about(8, str(enemies_killed))

    def entered_at(self, entry_point: str) -> str:
        return self._about(9, entry_point)

    def player_above(self, height: int) -> str:
        return self._about(10, str(height))

    def all_of(self, parts: list[str]) -> str:
        return self._of(4, parts)

    def any_of(self, parts: list[str]) -> str:
        return self._of(5, parts)

    def none_of(self, parts: list[str]) -> str:
        return self._of(6, parts)

    def always(self) -> str:
        return self.all_of([])

    def never(self) -> str:
        return self.any_of([])

    def boss(self, which: str) -> str:
        """The boss this room's area calls its "area boss", "area mini boss" or "area torizo"."""
        return AREA_BOSSES[(self.room.area, which)]


class Generator:
    def __init__(self) -> None:
        self.rooms = legacy.rooms()
        self.states = legacy.states()
        self.choices = legacy.state_choices()
        self.doors = legacy.doors()
        self.populations = legacy.enemy_populations()
        self.quotas = legacy.death_quotas()
        self.scroll_layouts = legacy.scroll_layouts()
        self.scroll_changes = legacy.scroll_changes()
        self.door_setups = legacy.door_setups()
        self.room_fx = legacy.room_fx()
        self.object_lists = legacy.object_populations()
        # The kinds of enemy no rule places yet, with how many placements of each were passed over.
        self.unplaced: dict[str, int] = {}
        with open(os.path.join(INSTALLED, 'room-level-streams', 'room-door-blocks.json'), encoding='utf-8') as source:
            self.door_blocks = json.load(source)['rooms']
        self.by_name = {room.name: key for key, room in self.rooms.items()}
        # Each room's doors: the name of each, with the rectangle its blocks cover, in pixels.
        self.room_doors: dict[str, dict[str, tuple[int, int, int, int]]] = {}
        # The blocks an elevator rests on in each room that has one: their rectangle, in pixels.
        self.elevator_rests: dict[str, tuple[int, int, int, int]] = {}
        for key, room in self.rooms.items():
            levels = self.door_blocks.get(room.name, {})
            default_level = self.states[self.choices[key][0]].level
            blocks = levels.get(default_level) or (next(iter(levels.values())) if levels else {})
            doors = {}
            for door, cells in blocks.items():
                xs, ys = [cell['x'] for cell in cells], [cell['y'] for cell in cells]
                # The blocks an elevator rests on are marked as a door that leads nowhere; the elevator is an object,
                # which is told where they are.
                if door == 'ElevatorPseudoDoor':
                    if len(cells) != (max(xs) - min(xs) + 1) * (max(ys) - min(ys) + 1):
                        raise ValueError(f'The blocks an elevator rests on in {room.name} are not one rectangle')
                    self.elevator_rests[key] = (min(xs) * BLOCK, min(ys) * BLOCK, (max(xs) - min(xs) + 1) * BLOCK, (max(ys) - min(ys) + 1) * BLOCK)
                    continue
                doors[door] = (min(xs) * BLOCK, min(ys) * BLOCK, (max(xs) - min(xs) + 1) * BLOCK, (max(ys) - min(ys) + 1) * BLOCK)
            self.room_doors[key] = doors
        # Mother Brain's way out is a door her death makes: her room's level has no blocks for it until then. It lies
        # in the wall until the patch of her death opens it.
        death = legacy.read('Bosses', 'MotherBrain', 'MotherBrainDeathDefinitions.cs')
        door_x, door_y = (int(part) for part in re.search(r'EscapeDoorBlock = new\((\d+), (\d+)\)', death).groups())
        door_setup = re.search(r'EscapeDoorSetup = new\(\s*\[(.*?)\]\);', legacy.read('Bosses', 'MotherBrain', 'MotherBrainRoomMutationDefinitions.cs'), re.S)
        door_blocks = len(re.findall(r'new\(new\(0, \d+\)', door_setup.group(1)))
        escape_door = re.search(r'EscapeDoor = TourianDoorDefinitions\.(\w+);', death).group(1)
        self.room_doors[self.by_name['MotherBrain']][escape_door] = (door_x * BLOCK, door_y * BLOCK, BLOCK, door_blocks * BLOCK)

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
            return scene.resource(script, {'Kind': str(kind), 'Parts': scene.listed(script, parts)})

        ways = []
        for version in sorted(versions):
            earlier = [condition(scene, room, expression) for expression in expressions[:version]]
            parts = [gathered(none_of, earlier)] if earlier else []
            if version < len(expressions):
                parts.insert(0, condition(scene, room, expressions[version]))
            ways.append(parts[0] if len(parts) == 1 else gathered(all_of, parts))
        return ways[0] if len(ways) == 1 else gathered(any_of, ways)

    def boss_extras(self, ctx: Context, room: legacy.Room, boss: enemies.Placed, links: dict[int, dict[str, object]]) -> list[enemies.Placed]:
        """
        What a boss's room has besides the boss: the encounter that records its defeat, and for Spore Spawn the husk
        it leaves. The boss itself is there only until it is beaten.
        """
        defeated = ctx.fact(boss.boss + '_defeated')
        boss.only_when = ctx.none_of([defeated])
        encounter = enemies.Placed('res://engine/world/objects/boss_encounter/BossEncounter.cs', 'Encounter', {
            'Size': f'Vector2i({room.screens[0] * SCREEN}, {room.screens[1] * SCREEN})',
            'RememberedAs': quoted(boss.boss + '_defeated')}, at=(0, 0), enemy=False)
        links[id(encounter)] = {'Boss': boss}
        extras = [encounter]
        if boss.boss == 'spore_spawn':
            extras.append(enemies.Placed(boss.scene, 'SporeSpawnHusk', {'Husk': 'true'}, at=boss.at, only_when=defeated))
            # Until it dies the view is kept from rising above its chamber: the old code's lowest place for the view's top.
            lowest = int(re.search(r'LowestCameraY = (\d+);', legacy.read('Bosses', 'SporeSpawn', 'SporeSpawnArenaDefinitions.cs')).group(1))
            above = enemies.Placed('res://engine/world/ViewFloor.cs', 'AboveSporeSpawn', {}, at=(0, lowest), enemy=False,
                                   only_when=ctx.none_of([defeated]))
            links[id(boss)]['ArenaView'] = above
            extras.append(above)
        if boss.boss == 'crocomire':
            # A room where it has been beaten shows its skeleton where the old code lays it.
            at = re.search(r'CorpsePosition = new\((\d+), (\d+)\);', legacy.read('Bosses', 'Crocomire', 'CrocomireArenaDefinitions.cs'))
            extras.append(enemies.Placed(boss.scene, 'CrocomireSkeleton', {'Corpse': 'true'},
                                         at=(int(at.group(1)), int(at.group(2))), only_when=defeated))
        if boss.boss == 'kraid':
            # The old Kraid sets its room's screens as it is set up, and again as it has grown. Each screen it changes
            # is a zone, told its first state as the room is set up and opened by Kraid as it grows.
            cameras = legacy.read('Bosses', 'Kraid', 'KraidCameraDefinitions.cs')

            def screens(name: str) -> dict[tuple[int, int], str]:
                listed = re.search(name + r' =\s*\[(.*?)\];', cameras, re.S).group(1)
                return {(int(x), int(y)): state for x, y, state in re.findall(r'new\(new\((\d+), (\d+)\), RoomScrollState\.(\w+)\)', listed)}
            before, after = screens('InitialScrolls'), screens('GrownScrolls')
            told = []
            for (x, y), state in before.items():
                if after[(x, y)] == 'Closed':
                    raise ValueError('Kraid leaves a screen closed once grown, which a zone told to open cannot say.')
                told.append(enemies.Placed(camera.ZONE, f'KraidScreen{x}x{y}', {
                    'ToldAtSetup': enemies.quoted(camera.WORDS[state]), 'OpensTo': str(camera.STATES.index(after[(x, y)]))},
                    at=(x * SCREEN, y * SCREEN), enemy=False, only_when=ctx.none_of([defeated])))
            links[id(boss)]['ToldWhenGrown'] = told
            extras.extend(told)
        return extras

    def place_things(self, key: str, scene: Scene, room: legacy.Room) -> None:
        """
        Places the enemies and room objects of every version of the room, each once, present in the versions that
        list it.
        """
        default, conditional = self.choices[key]
        versions = [name for _, name in conditional] + [default]
        doors_in = [door for door in self.doors.values() if door.to_room == key]
        merged: dict[tuple, tuple[enemies.Placed, set[int], dict[str, object]]] = {}
        # The default version's things come first, in the order it lists them; the order enemies are listed in is the
        # order they act in.
        for version in reversed(range(len(versions))):
            state = self.states[versions[version]]
            things: list[enemies.Placed] = []
            # What each thing names: for each such property, the thing or things it names.
            links: dict[int, dict[str, object]] = {}
            by_index = dict(enemies.places(self.populations[state.enemies], enemies.RULES, self.unplaced))
            ctx = Context(self, scene, room, self.quotas[state.enemies])
            for placed in by_index.values():
                # What the old game steps as an enemy and the Godot game places as a room object still takes its
                # turn among the enemies, before Samus moves.
                if not placed.enemy and placed.scene.endswith('.tscn'):
                    placed.properties.setdefault('TurnsWithEnemies', 'true')
                things.append(placed)
                links[id(placed)] = {name: by_index[target] for name, target in placed.refers.items()}
                if placed.until_beaten:
                    placed.only_when = ctx.none_of([ctx.fact(placed.until_beaten + '_defeated')])
                if placed.boss:
                    things.extend(self.boss_extras(ctx, room, placed, links))
                if placed.chozo == 'WreckedShip':
                    placed.properties['WakesWhen'] = ctx.fact('phantoon_defeated')
                elif placed.chozo == 'LowerNorfair':
                    placed.properties.update({'WakesWhen': ctx.owns('space_jump'), 'RecordsFact': quoted('lower_norfair_acid_lowered')})
            placed_objects = objects.Objects(ctx, self.object_lists[state.objects], doors_in)
            things.extend(placed_objects.placed)
            for boss in [thing for thing in things if thing.boss == 'phantoon']:
                # The door of Phantoon's room is wall while the fight lasts, so its cap is put out of sight.
                links.setdefault(id(boss), {})['HiddenInFight'] = [thing for thing in placed_objects.placed if thing.name.endswith('Cap')]
            for teller, name, told in placed_objects.tells:
                links.setdefault(id(teller), {})[name] = told
            # The camera's zones, and what opens and closes them: blocks Samus touches, and doors she comes in by.
            entry_points = [(self.arrival_in(key, door) or 'From' + name, door.setup)
                            for name, door in self.doors.items() if door.to_room == key]
            view = camera.Camera(self.scroll_layouts, self.scroll_changes, self.door_setups, room, state,
                                 self.object_lists[state.objects], entry_points)
            things.extend(view.placed)
            for trigger, zone in view.links:
                links[id(trigger)] = {'Target': zone}
            # Its liquid and its room effects.
            booster = placed_objects.raises_liquid
            shown, told = fx.things(self.room_fx[state.fx], room, ctx,
                                    lambda door: self.arrival_in(key, self.doors[door]) or 'From' + door,
                                    told_speed=SPEED_BOOSTER_LAVA_RISE if booster is not None else None)
            things.extend(shown)
            for teller, target in told:
                links[id(teller)] = {'Target': target}
            if booster is not None:
                # The item tells the room's lava to rise as its pickup ends, and the lava has its speed from that.
                liquids = [thing for thing in shown if thing.name == 'Liquid']
                if len(liquids) != 1:
                    raise ValueError(f'{key}: the Speed Booster sets one liquid rising, and the room has {len(liquids)}.')
                links.setdefault(id(booster), {})['Tells'] = liquids[0]
                booster.properties['TellsWhat'] = quoted('rise')
            # Its sequences, what they move, and its backdrop.
            by_name: dict[str, enemies.Placed] = {}
            for placed in things:
                by_name.setdefault(placed.name, placed)
            for placed in things:
                if placed.boss == 'botwoon':
                    # The old wall, as it begins to fall, opens the screens it kept the view from; a room where
                    # Botwoon has been beaten never shuts them.
                    walls = legacy.read('Bosses', 'Botwoon', 'BotwoonWallMutationDefinitions.cs')
                    opened = re.search(r'OpenedScrollCells \{ get; \} = \[(.*?)\];', walls).group(1)
                    zones = [by_name[f'Screen{x}x{y}'] for x, y in re.findall(r'new\((\d+), (\d+)\)', opened) if f'Screen{x}x{y}' in by_name]
                    for zone in zones:
                        zone.properties['OpensTo'] = str(camera.STATES.index('OpenAlignedToTop'))
                    # A room where Botwoon has been beaten has them open from the start.
                    for x, y in re.findall(r'new\((\d+), (\d+)\)', opened):
                        things.append(enemies.Placed(camera.ZONE, f'BotwoonOpened{x}x{y}', {
                            'ToldAtSetup': enemies.quoted(camera.WORDS['OpenAlignedToTop'])},
                            at=(int(x) * SCREEN, int(y) * SCREEN), enemy=False, only_when=ctx.fact('botwoon_defeated')))
                    links[id(placed)]['ToldWhenWallFalls'] = zones
            for placed in list(things):
                if placed.name == 'BabyMetroid':
                    # The old one shuts two of its room's screens to the view as it wakes, and opens them once it has fed.
                    fight = legacy.read('Bosses', 'MotherBrain', 'BabyMetroidFightDefinitions.cs')
                    shut = re.search(r'ClosingScreens \{ get; \} = \[(.*?)\];', fight).group(1)
                    zones = [enemies.Placed(camera.ZONE, f'BabyMetroidScreen{x}x{y}', {'OpensTo': str(camera.STATES.index('OpenAlignedToTop'))},
                                            at=(int(x) * SCREEN, int(y) * SCREEN), enemy=False)
                             for x, y in re.findall(r'new\((\d+), (\d+)\)', shut)]
                    things.extend(zones)
                    links[id(placed)]['ToldOfRoom'] = zones
                if placed.boss == 'mother_brain':
                    alive = ctx.none_of([ctx.fact('mother_brain_defeated')])
                    # The turrets of her room, which the old Mother Brain sets up with herself.
                    turrets = legacy.read('Bosses', 'MotherBrain', 'MotherBrainTurretDefinitions.cs')
                    ways = re.findall(r'MotherBrainTurretDirection\.(\w+)', re.search(r'Ring = new\((.*?)\);', turrets, re.S).group(1))
                    listed = re.search(r'Turrets \{ get; \} =\s*\[(.*?)\];', turrets, re.S).group(1)
                    way = r'MotherBrainTurretDirection\.(\w+)'
                    found = re.findall(r'new\(new\((\d+), (\d+)\), ' + way + ', ' + way + ', ' + way + r'\)', listed)
                    if len(found) != listed.count('new(new('):
                        raise ValueError('Not every turret of Mother Brain\'s room could be read.')
                    for number, (x, y, starts, first, last) in enumerate(found, 1):
                        things.append(enemies.Placed('res://game/bosses/mother_brain/mother_brain_turret.tscn', f'Turret{number}', {
                            'StartsPointing': str(ways.index(starts)), 'Sweep': f'Vector2i({ways.index(first)}, {ways.index(last)})',
                            'AlwaysActive': 'true'}, at=(int(x), int(y)), only_when=alive))
                    # The view is shut into her end of the room once the way back is walled off.
                    fight = legacy.read('Bosses', 'MotherBrain', 'MotherBrainFightDefinitions.cs')
                    shut_x, shut_y = (int(part) for part in re.search(r'ArenaClosedScreen = new\((\d+), (\d+)\)', fight).groups())
                    arena = enemies.Placed(camera.ZONE, f'MotherBrainScreen{shut_x}x{shut_y}', {},
                                           at=(shut_x * SCREEN, shut_y * SCREEN), enemy=False)
                    # The warning of her death is written out; she waits for it.
                    warning = sequences.sequence('TypedWarning.cs', 'EscapeWarning', {
                        'Message': quoted('zebes_time_bomb_warning'), 'LetterSound': quoted('room_typewriter')})
                    things.extend([arena, warning])
                    links[id(placed)].update({'ToldWhenSealed': [arena], 'ToldWhenEscapeBegins': [warning]})
                    placed.properties['EscapeMusic'] = quoted('escape')
            chosen_on = conditional[version][0] if version < len(conditional) else ''
            played = sequences.Sequences(room, state, chosen_on, ctx, by_name, (room_scene(self.rooms['LandingSite']), 'AtZebesArrival'), things)
            things.extend(played.placed)
            for teller, name, told_things in played.links:
                links.setdefault(id(teller), {})[name] = told_things

            # Two alike in one version are two things; two alike in two versions are one thing in both.
            counts: dict[tuple, int] = {}
            plain: dict[int, tuple] = {}
            for placed in things:
                alike = (placed.scene, placed.name, placed.at, placed.kind, placed.only_when, tuple(sorted(placed.properties.items())))
                counts[alike] = counts.get(alike, 0) + 1
                plain[id(placed)] = alike + (counts[alike],)

            def signature(placed: enemies.Placed) -> tuple:
                named = []
                for name, target in sorted(links.get(id(placed), {}).items()):
                    named.append((name, tuple(signature(one) for one in target) if isinstance(target, list) else signature(target)))
                return plain[id(placed)] + (tuple(named),)

            for placed in things:
                mine = signature(placed)
                if mine not in merged:
                    named = {name: [signature(one) for one in target] if isinstance(target, list) else signature(target)
                             for name, target in links.get(id(placed), {}).items()}
                    merged[mine] = (placed, set(), named)
                merged[mine][1].add(version)

        # A barrier remembered by one name is one barrier, whatever holds it shut in each version: where the versions
        # differ only in that, it is held shut by each version's condition while that version is in force.
        same_name: dict[tuple, list[tuple]] = {}
        for mine, (placed, _, _) in merged.items():
            if 'RememberedAs' in placed.properties and 'HeldShutBy' in placed.properties:
                same_name.setdefault((placed.scene, placed.at, placed.properties['RememberedAs']), []).append(mine)
        for group in same_name.values():
            if len(group) < 2:
                continue
            ways, everywhere = [], set()
            for mine in group:
                placed, present_in, _ = merged[mine]
                present = self.present_when(scene, room, key, present_in)
                held = placed.properties['HeldShutBy']
                ways.append(held if present is None else scene.resource(CONDITION, {'Kind': '4', 'Parts': scene.listed(CONDITION, [present, held])}))
                everywhere |= present_in
            kept = merged[group[0]]
            kept[0].properties['HeldShutBy'] = scene.resource(CONDITION, {'Kind': '5', 'Parts': scene.listed(CONDITION, ways)})
            kept[1].update(everywhere)
            for mine in group[1:]:
                del merged[mine]

        names = {mine: scene.name(placed.name) for mine, (placed, _, _) in merged.items()}
        for mine, (placed, present_in, named) in merged.items():
            properties = {}
            if placed.at is not None:
                properties['position'] = f'Vector2({placed.at[0]}, {placed.at[1]})'
            for name, value in placed.properties.items():
                if value.startswith('scene:'):
                    value = scene.packed(value[len('scene:'):])
                elif value.startswith('fact:'):
                    value = scene.resource(CONDITION, {'Kind': '0', 'About': quoted(value[len('fact:'):])})
                elif value.startswith('rules:'):
                    # A relay's one rule: the word it hears and the terrain patch it applies, shaking the room.
                    _, hears, patch = value.split(':')
                    rule = 'res://engine/world/RelayRule.cs'
                    value = scene.listed(rule, [scene.resource(rule, {
                        'Hears': quoted(hears), 'AppliesPatch': quoted(patch), 'ShakeStrength': '1', 'ShakeUpdates': '60'})])
                properties[name] = value
            for name, target in named.items():
                if isinstance(target, list):
                    properties[name] = 'Array[NodePath]([' + ', '.join(f'NodePath("../{names[one]}")' for one in target) + '])'
                else:
                    properties[name] = f'NodePath("../{names[target]}")'
            present = self.present_when(scene, room, key, present_in)
            if present is not None and placed.only_when is not None:
                present = scene.resource(CONDITION, {'Kind': '4', 'Parts': scene.listed(CONDITION, [present, placed.only_when])})
            present = present or placed.only_when
            if present is not None:
                properties['PresentWhen'] = present
            if placed.scene == enemies.ELEVATOR:
                if key not in self.elevator_rests:
                    raise ValueError(f'{room.name} has an elevator and no blocks marked for it to rest on')
                properties['RestsOn'] = 'Rect2i(%d, %d, %d, %d)' % self.elevator_rests[key]
            if placed.scene.endswith('.tscn'):
                scene.node(names[mine], '.', properties, instance=placed.scene)
            else:
                # A script alone, or a plain node that something else moves and shows.
                scene.node(names[mine], '.', properties, script=placed.scene or None, kind=placed.kind)
            for child, script, settings in placed.children:
                scene.node(child, names[mine], settings, script=script)

    def screens_of(self, room: legacy.Room, state: legacy.State) -> dict[str, str]:
        """How each screen of a version lets the view in, and its layout's name where that is an explicit table."""
        screens, layout = camera.screens_of(self.scroll_layouts, room, state)
        settings = {'Screens': 'PackedInt32Array(' + ', '.join(str(screen) for screen in screens) + ')'}
        if layout:
            settings['ScrollLayout'] = quoted(layout)
        return settings

    def write_room(self, key: str) -> str:
        room = self.rooms[key]
        scene = Scene()
        default, conditional = self.choices[key]

        versions = []
        base_state = self.states[default]
        for expression, state_name in conditional:
            state = self.states[state_name]
            settings = {
                'When': condition(scene, room, expression),
                'LayoutFile': quoted(layout_of(room, state)),
                **self.screens_of(room, state),
                **music_of(state, self.nearby_music_set(key)),
            }
            patches = []
            if state.setup in sequences.ESCAPE_PATCHES:
                patches.append(sequences.ESCAPE_PATCHES[state.setup])
            if 'MotherBrainGlassDestroyed' in expression:
                # The glass of Mother Brain's tank, once broken, stays broken.
                patches.append('glass_rim_gone')
            if 'BossDead' in expression:
                beaten = AREA_BOSSES[(room.area, legacy.last_name(legacy.call_arguments(expression)[0]))]
                if beaten in enemies.BEATEN_PATCHES and any(p.enemy in enemies.BOSSES and enemies.BOSSES[p.enemy][1] == beaten
                                                            for p in self.populations[base_state.enemies]):
                    patches += enemies.BEATEN_PATCHES[beaten]
            if patches:
                settings['Patches'] = 'PackedStringArray(' + ', '.join(quoted(patch) for patch in patches) + ')'
            versions.append(scene.resource('res://engine/world/RoomVersion.cs', settings))
        base = self.states[default]
        versions.append(scene.resource('res://engine/world/RoomVersion.cs', {
            'LayoutFile': quoted(layout_of(room, base)),
            **self.screens_of(room, base),
            **music_of(base, self.nearby_music_set(key)),
        }))
        falling, rising = CAMERA_LEADS[legacy.last_name(room.properties.get('CameraLead', 'RoomCameraLeads.Standard'))]

        room_settings = {
            'Area': quoted(snake(room.area)),
            'MapCell': f'Vector2i({room.map_cell[0]}, {room.map_cell[1]})',
            'Versions': scene.listed('res://engine/world/RoomVersion.cs', versions),
            'PlayerInView': f'Vector2i({PLAYER_IN_VIEW[0]}, {PLAYER_IN_VIEW[1]})',
            'ViewAboveWhileRising': str(rising),
            'ViewAboveOtherwise': str(falling),
        }
        if room.area in FROZEN_UPDATES:
            room_settings['StatusUpdates'] = 'Dictionary[String, int]({"frozen": %d})' % FROZEN_UPDATES[room.area]
        scene.node(room.name, None, room_settings, script='res://engine/world/Room.cs')
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
                'Travel': str(DOOR_WAYS.index(door.direction)),
                'ToScreen': f'Vector2i({door.to_screen[0]}, {door.to_screen[1]})',
                'SamusStep': repr(samus_step(door)),
                # A room that shows only what stands in it through its doors shows no tiles through the scroll, and
                # neither does the room on the door's other side.
                **({'ShowsTilesThrough': 'false'} if 'ObjectsOnly' in room.properties.get('DoorTransition', '') + target.properties.get('DoorTransition', '') else {}),
            }, script='res://game/rooms/doors/Door.cs')
            entries.append((name, x, y))

        self.place_things(key, scene, room)
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
        for station, (door, (x, y), (view_x, view_y)) in legacy.load_stations().items():
            # The Ceres elevator sets her down from the top of its shaft.
            if station == 'CeresElevator':
                y = 0
            # Play that begins at one of these puts the view where the cartridge's table of them does.
            self.scenes[self.doors[door].to_room].node(
                'At' + station, '.',
                {'position': f'Vector2({x}, {y})', 'PlacesView': 'true', 'ViewAt': f'Vector2i({view_x}, {view_y})'},
                script='res://engine/world/EntryPoint.cs', kind='Marker2D')
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
