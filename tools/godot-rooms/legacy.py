"""Reads the old code's room definitions as text.

The C# port holds every room's structure in definition files: which rooms there are, the versions ("states") of
each, what enemies and room objects each version places and where, and where each door leads. Nothing here runs or
builds that code. The files are read as text and their regular `new(...)` lists are taken apart.
"""
from __future__ import annotations

import os
import re
from dataclasses import dataclass, field

CORE = os.path.join(os.path.dirname(__file__), '..', '..', 'csharp', 'src', 'SuperMetroid.Core')
AREAS = ['Crateria', 'Brinstar', 'Norfair', 'WreckedShip', 'Maridia', 'Tourian', 'Ceres']


def read(*parts: str) -> str:
    with open(os.path.join(CORE, *parts), encoding='utf-8-sig') as source:
        return source.read()


def without_comments(text: str) -> str:
    """The text with its `//` comments taken out, so that nothing in one is read as code."""
    return re.sub(r'//[^\n]*', '', text)


def matching(text: str, start: int) -> int:
    """The index just past the bracket that closes the one at `start`, which is a `(`, `[` or `{`."""
    pairs = {'(': ')', '[': ']', '{': '}'}
    close = pairs[text[start]]
    depth, at, quote = 0, start, ''
    while at < len(text):
        c = text[at]
        if quote:
            if c == '\\':
                at += 1
            elif c == quote:
                quote = ''
        elif c in '"\'':
            quote = c
        elif c == text[start]:
            depth += 1
        elif c == close:
            depth -= 1
            if depth == 0:
                return at + 1
        at += 1
    raise ValueError('a bracket is never closed, from: ' + text[start:start + 80])


def split_arguments(text: str) -> list[str]:
    """The arguments of a list, split at its top-level commas; `text` is what lies between the brackets."""
    parts, depth, quote, last = [], 0, '', 0
    for at, c in enumerate(text):
        if quote:
            if c == quote and text[at - 1] != '\\':
                quote = ''
        elif c in '"\'':
            quote = c
        elif c in '([{<':
            depth += 1
        elif c in ')]}>':
            depth -= 1
        elif c == ',' and depth == 0:
            parts.append(text[last:at].strip())
            last = at + 1
    tail = text[last:].strip()
    if tail:
        parts.append(tail)
    return parts


def call_arguments(expression: str) -> list[str]:
    """The arguments of an expression's outermost call: `new Foo(a, b)` or `Foo.Bar(a, b)` gives [a, b]."""
    start = expression.index('(')
    return split_arguments(expression[start + 1:matching(expression, start) - 1])


def last_name(expression: str) -> str:
    """The last name of a dotted reference: `Rooms.RoomDefinitions.Parlor` gives `Parlor`."""
    return expression.strip().rsplit('.', 1)[-1]


def numbers(expression: str) -> list[int]:
    return [int(number) for number in re.findall(r'-?\d+', expression)]


@dataclass
class Room:
    name: str
    area: str
    map_cell: tuple[int, int]
    screens: tuple[int, int]
    properties: dict[str, str] = field(default_factory=dict)


@dataclass
class Placement:
    """One enemy an enemy population places."""
    enemy: str
    at: tuple[int, int] | None
    arguments: list[str]
    settings_type: str
    settings: dict[str, str]


@dataclass
class ObjectPlacement:
    """One room object an object population places: its block, and what is set up there."""
    block: tuple[int, int]
    setup_type: str
    setup: str


@dataclass
class State:
    """One version of a room, as the old code holds it."""
    name: str
    level: str
    graphics_set: str
    music: str
    fx: str
    enemies: str
    background_scroll: str
    scroll: str
    main: str
    objects: str
    background: str
    setup: str
    properties: dict[str, str] = field(default_factory=dict)


@dataclass
class Door:
    name: str
    to_room: str
    direction: str
    closing: str
    object_block: tuple[int, int] | None
    to_screen: tuple[int, int]
    speed: str
    setup: str
    exchanges_maps: bool = False


def properties_of(text: str, name_pattern: str) -> list[tuple[str, str]]:
    """Every static property or field the pattern names, with the expression it is given, up to its semicolon."""
    found = []
    for match in re.finditer(name_pattern, text):
        start = match.end()
        at, depth, quote = start, 0, ''
        while at < len(text):
            c = text[at]
            if quote:
                if c == quote and text[at - 1] != '\\':
                    quote = ''
            elif c in '"\'':
                quote = c
            elif c in '([{':
                depth += 1
            elif c in ')]}':
                depth -= 1
            elif c == ';' and depth == 0:
                break
            at += 1
        found.append((match.group(1), text[start:at].strip()))
    return found


def initializer_of(expression: str) -> dict[str, str]:
    """The `{ A = x, B = y }` that follows a constructor call, as a dictionary; empty when there is none."""
    close = matching(expression, expression.index('(')) if '(' in expression else 0
    rest = expression[close:].strip()
    if not rest.startswith('{'):
        return {}
    inner = rest[1:matching(rest, 0) - 1]
    return {part.split('=', 1)[0].strip(): part.split('=', 1)[1].strip() for part in split_arguments(inner) if '=' in part}


def rooms() -> dict[str, Room]:
    text = without_comments(read('Rooms', 'RoomDefinitions.cs'))
    # What each helper a room may be wrapped in gives it: `room with { A = x, B = y }`.
    helpers = {}
    for name, expression in properties_of(text, r'private static RoomDefinition (\w+)\(RoomDefinition room\) =>'):
        given = expression[expression.index('{'):] if expression.startswith('room with') else ''
        if not given:
            raise ValueError(f'The room helper {name} is not a `room with {{ ... }}`, which this can read.')
        helpers[name] = {part.split('=', 1)[0].strip(): part.split('=', 1)[1].strip()
                         for part in split_arguments(given[1:matching(given, 0) - 1]) if '=' in part}
    result = {}
    for name, expression in properties_of(text, r'public static RoomDefinition (\w+) \{ get; \} ='):
        inner = expression
        properties = {}
        # A room wrapped in a helper (`WetWithShallowerFall(new(...))`) is the `new(...)` inside it, with what the
        # helper gives it over what it gives itself.
        if not inner.startswith('new('):
            helper = inner[:inner.index('(')].strip()
            if helper not in helpers:
                raise ValueError(f'The room {name} is wrapped in {helper}, which is no helper this has read.')
            inner = inner[inner.index('new('):]
            properties = helpers[helper]
        arguments = call_arguments(inner)
        cell, size = numbers(arguments[2]), numbers(arguments[3])
        properties = initializer_of(inner) | properties
        result[name] = Room(arguments[0].strip('"'), last_name(arguments[1]), (cell[0], cell[1]), (size[0], size[1]), properties)
    return result


def states() -> dict[str, State]:
    result = {}
    for area in AREAS:
        text = without_comments(read('World', 'RoomStates', area + 'RoomStateDefinitions.cs'))
        for name, expression in properties_of(text, r'public static RoomStateSettings (\w+) \{ get; \} ='):
            a = call_arguments(expression)
            result[name] = State(
                name=a[0].strip('"'), level=last_name(a[1]), graphics_set=last_name(a[2]), music=a[3],
                fx=last_name(a[4]), enemies=last_name(a[5]), background_scroll=a[6], scroll=last_name(a[7]),
                main=last_name(a[9]), objects=last_name(a[10]), background=last_name(a[11]), setup=last_name(a[12]),
                properties=initializer_of(expression))
    return result


def state_choices() -> dict[str, tuple[str, list[tuple[str, str]]]]:
    """For each room: its default state, and its other states with the condition each is chosen on, first match first."""
    text = without_comments(read('World', 'RoomStates', 'RoomStateSelectionDefinitions.cs'))
    result = {}
    for match in re.finditer(r'\[RoomDefinitions\.(\w+)\] = new\(', text):
        open_at = match.end() - 1
        arguments = split_arguments(text[open_at + 1:matching(text, open_at) - 1])
        conditional = []
        if len(arguments) > 1:
            listed = arguments[1].strip()
            for entry in split_arguments(listed[1:matching(listed, 0) - 1]):
                parts = call_arguments(entry)
                conditional.append((parts[0], last_name(parts[1])))
        result[match.group(1)] = (last_name(arguments[0]), conditional)
    return result


def _placements_in(listed: str, shared: dict[str, list[Placement]]) -> list[Placement]:
    """The placements a `[ ... ]` list holds, with any shared list it spreads in (`.. Name`) put in its place."""
    placements = []
    for entry in split_arguments(listed[1:matching(listed, 0) - 1]):
        if entry.startswith('..'):
            spread = entry[2:].strip()
            if spread.startswith('Repeated<'):
                # One placement, placed a number of times over.
                repeated = call_arguments(spread)
                placements.extend(_placements_in('[' + repeated[0] + ']', shared) * int(repeated[1]))
            elif spread.startswith('DachoraTrail.Placed'):
                # The Dachora and the echoes that trail it: the echoes are the Dachora's own in the new project.
                at = numbers(call_arguments(spread)[0])
                placements.append(Placement('Dachora', (at[0], at[1]), call_arguments(spread), 'DachoraTrail', {}))
            else:
                placements.extend(shared[last_name(spread)])
            continue
        a = call_arguments(entry)
        # A place off the room's edge is written inside a wrapper whose own name holds a number; the place is the last two.
        at = numbers(a[1])[-2:] if a[1].startswith('EnemyPlacing.At') else []
        settings = a[6] if len(a) > 6 else 'EnemySpawnSettings.None'
        settings_type = re.match(r'new (\w+)', settings).group(1) if settings.startswith('new ') else settings
        named = {}
        if settings.startswith('new ') and '(' in settings:
            for argument in call_arguments(settings):
                if ':' in argument:
                    key, value = argument.split(':', 1)
                    named[key.strip()] = value.strip()
                else:
                    named[str(len(named))] = argument
            named.update(initializer_of(settings))
        placements.append(Placement(last_name(a[0]), (at[0], at[1]) if at else None, a, settings_type, named))
    return placements


def boss_kinds() -> set[str]:
    """The enemy kinds whose definition names a boss: a room that places one counts as that boss's room."""
    text = without_comments(read('Enemies', 'RoomEnemyDefinitionCatalog.cs'))
    kinds = set()
    for match in re.finditer(r'new RoomEnemyDefinitionEntry\(EnemyDefinitionId\.(\w+), new\(', text):
        open_at = match.end() - 1
        arguments = split_arguments(text[open_at + 1:matching(text, open_at) - 1])
        if arguments[5].strip() != 'null':
            kinds.add(match.group(1))
    if not kinds:
        raise ValueError('The old enemy definitions name no boss.')
    return kinds


def enemy_populations() -> dict[str, list[Placement]]:
    result = {}
    for area in AREAS:
        text = without_comments(read('World', 'RoomStates', area + 'RoomEnemyPopulationDefinitions.cs'))
        shared: dict[str, list[Placement]] = {}
        for name, expression in properties_of(text, r'private static readonly RoomEnemyPlacement\[\] (\w+) ='):
            shared[name] = _placements_in(expression, shared)
        # A population that places nothing.
        for name, _ in properties_of(text, r'internal static EmptyRoomEnemyPopulationDefinition (\w+) \{ get; \} ='):
            result[name] = []
        pattern = r'internal static PlacedRoomEnemyPopulationDefinition (\w+) \{ get; \} ='
        for name, expression in properties_of(text, pattern):
            arguments = call_arguments(expression)
            listed = arguments[1].strip()
            result[name] = shared[last_name(listed)] if not listed.startswith('[') else _placements_in(listed, shared)
    return result


def population_names() -> dict[str, str]:
    """Each enemy population's own name, the one the installation lists the looks of its enemies under, by the name a room state gives it."""
    result = {}
    for area in AREAS:
        text = without_comments(read('World', 'RoomStates', area + 'RoomEnemyPopulationDefinitions.cs'))
        pattern = r'internal static (?:Empty|Placed)RoomEnemyPopulationDefinition (\w+) \{ get; \} ='
        for name, expression in properties_of(text, pattern):
            result[name] = call_arguments(expression)[0].strip('"')
    return result


def death_quotas() -> dict[str, int]:
    """For each enemy population, how many kills clear its room; none for a population that places nothing."""
    result = {}
    for area in AREAS:
        text = without_comments(read('World', 'RoomStates', area + 'RoomEnemyPopulationDefinitions.cs'))
        for name, _ in properties_of(text, r'internal static EmptyRoomEnemyPopulationDefinition (\w+) \{ get; \} ='):
            result[name] = 0
        for name, expression in properties_of(text, r'internal static PlacedRoomEnemyPopulationDefinition (\w+) \{ get; \} ='):
            result[name] = int(call_arguments(expression)[2])
    return result


def object_populations() -> dict[str, list[str]]:
    """Each object population's entries, as text: a placement `new(new(x, y), new SomeSetup(...))` or a spread list."""
    text = without_comments(read('Rooms', 'RoomObjects', 'RoomObjectPopulationDefinitions.cs'))
    result = {}
    for name, expression in properties_of(text, r'public static RoomObjectPopulation (\w+) \{ get; \} ='):
        if '[' not in expression:
            result[name] = [expression]
            continue
        open_at = expression.index('[')
        result[name] = split_arguments(expression[open_at + 1:matching(expression, open_at) - 1])
    return result


def object_placement(entry: str) -> ObjectPlacement | None:
    """An entry of an object population as a placement; None for an entry that is a spread list."""
    if not entry.startswith('new('):
        return None
    arguments = call_arguments(entry)
    block = numbers(arguments[0])
    setup = arguments[1]
    kind = re.match(r'new (\w+)', setup)
    return ObjectPlacement((block[0], block[1]), kind.group(1) if kind else last_name(setup), setup)


def doors() -> dict[str, Door]:
    result = {}
    folder = os.path.join(CORE, 'Rooms', 'Doors')
    for file in sorted(os.listdir(folder)):
        if not file.endswith('DoorDefinitions.cs'):
            continue
        text = without_comments(read('Rooms', 'Doors', file))
        for name, expression in properties_of(text, r'public static DoorDefinition (\w+) \{ get; \} ='):
            if not expression.startswith('new('):
                continue
            a = call_arguments(expression)
            orientation = call_arguments(a[2]) if '(' in a[2] else [a[2], '']
            block = numbers(a[3])
            screen = numbers(a[4])
            result[name] = Door(
                a[0].strip('"'), last_name(a[1]), last_name(orientation[0]), last_name(orientation[1]) if len(orientation) > 1 else '',
                (block[0], block[1]) if len(block) >= 2 else None, (screen[0], screen[1]), last_name(a[5]), last_name(a[6]),
                initializer_of(expression).get('ExchangesExploredMaps') == 'true')
    return result


def _screen_states(text: str) -> list[tuple[tuple[int, int], str]]:
    """Every `new(new(x, y), RoomScrollState.S)` in a piece of text: a screen and the state it is given."""
    found = re.findall(r'new\(new\((\d+), (\d+)\), RoomScrollState\.(\w+)\)', text)
    return [((int(x), int(y)), state) for x, y, state in found]


def scroll_layouts() -> dict[str, object]:
    """
    Each scroll layout a room state can name: the state of every screen, row by row; or, for a layout that fits
    any room, the one state its bottom row has.
    """
    text = without_comments(read('Rooms', 'Scrolling', 'RoomScrollDefinitions.cs'))
    short = {'Closed': 'Closed', 'Top': 'OpenAlignedToTop', 'Open': 'Open'}
    shared: dict[str, list[str]] = {}

    def states_of(expression: str) -> list[str]:
        expression = expression.strip()
        if not expression.startswith('['):
            return shared[last_name(expression)]
        states = []
        for item in split_arguments(expression[1:matching(expression, 0) - 1]):
            if item.startswith('..'):
                spread = item[2:].strip()
                if spread.startswith('Repeated('):
                    what, times = call_arguments(spread)
                    states.extend([short[what.strip()]] * int(times))
                else:
                    states.extend(shared[last_name(spread)])
            else:
                states.append(short[item])
        return states

    for name, expression in properties_of(text, r'private static readonly RoomScrollState\[\] (\w+) ='):
        shared[name] = states_of(expression)
    result: dict[str, object] = {}
    for name, expression in properties_of(text, r'public static RoomScrollDefinition (\w+) \{ get; \} ='):
        result[name] = last_name(call_arguments(expression)[1])
    for name, expression in properties_of(text, r'public static RoomScrollDefinition\.ExplicitLayout (\w+) \{ get; \} ='):
        result[name] = states_of(call_arguments(expression)[1])
    return result


def scroll_changes() -> dict[str, list[tuple[tuple[int, int], str]]]:
    """Each change a scroll trigger can make: the screens it sets and the state it gives each."""
    text = without_comments(read('Rooms', 'RoomObjects', 'RoomObjectScrollChangeDefinitions.Generated.cs'))
    result = {}
    for match in re.finditer(r'\[RoomScrollChange\.(\w+)\] = \[', text):
        open_at = match.end() - 1
        result[match.group(1)] = _screen_states(text[open_at:matching(text, open_at)])
    return result


def door_setups() -> dict[str, list[tuple[tuple[int, int], str]]]:
    """Each door setup that changes the scrolling of the room it leads into: the screens it sets."""
    text = without_comments(read('Rooms', 'Doors', 'DoorSetupDefinitions.cs'))

    # A list of changes more than one setup makes is written once and named by each of them.
    shared = {name: _screen_states(expression)
              for name, expression in properties_of(text, r'private static readonly ScreenScrollChange\[\] (\w+) =')}
    result = {}
    for name, expression in properties_of(text, r'public static DoorSetup (\w+) \{ get; \} ='):
        changes = call_arguments(expression)[1].strip()
        if changes.startswith('['):
            states = _screen_states(changes)
        elif changes in shared:
            states = shared[changes]
        else:
            raise ValueError(f'The door setup {name} gives its screen changes as {changes}, which is neither a list nor a shared one.')
        if states:
            result[name] = states
    return result


def _room_effect(expression: str) -> dict | None:
    """One of the old code's room effects as its parts; None for no effect."""
    if expression.strip() == 'null':
        return None
    a = call_arguments(expression)
    tide = re.search(r'LiquidTide\.(\w+)', a[7])
    return {
        'start': None if a[0] == 'null' else int(a[0]), 'target': None if a[1] == 'null' else int(a[1]),
        'speed': a[2], 'delay': int(a[3]), 'type': last_name(a[4]), 'tide': tide.group(1) if tide else 'None',
        'colours': re.findall(r'ColourEffectId\.(\w+)', a[8]), 'tint': last_name(a[10]),
        'drifts': 'SurfaceDrift: true' in a[7],
        # Water Samus's movement ignores until something of the room lets it at her: the water behind the glass tube.
        'held_back': 'IgnoresSurfaceUntilEnabled: true' in a[7],
    }


def room_fx() -> dict[str, dict]:
    """
    Each effect selection a room state can name: the effect shown on coming in by each of certain doors, and the
    effect shown otherwise.
    """
    text = without_comments(read('Fx', 'RoomFxDefinitions.cs'))
    result = {}
    for name, expression in properties_of(text, r'public static RoomFxSelection (\w+) \{ get; \} ='):
        arguments = call_arguments(expression)
        listed = arguments[0].strip()
        doors = []
        for entry in split_arguments(listed[1:matching(listed, 0) - 1]):
            door, effect = call_arguments(entry)
            doors.append((last_name(door), _room_effect(effect)))
        result[name] = {'doors': doors, 'default': _room_effect(arguments[1])}
    return result


def load_stations() -> dict[str, tuple[str, tuple[int, int], tuple[int, int]]]:
    """The places play can begin at: for each, the door that leads to its room, where Samus is put there, and where the view is."""
    text = without_comments(read('Rooms', 'RoomObjects', 'LoadStationPlacementDefinitions.cs'))
    result = {}
    for match in re.finditer(r'\[LoadStationDefinitions\.(\w+)\] = new\(', text):
        open_at = match.end() - 1
        arguments = split_arguments(text[open_at + 1:matching(text, open_at) - 1])
        view, samus = numbers(arguments[1]), numbers(arguments[2])
        result[match.group(1)] = (last_name(arguments[0]), (samus[0], samus[1]), (view[0], view[1]))
    return result


if __name__ == '__main__':
    all_rooms, all_states, choices = rooms(), states(), state_choices()
    populations, objects, all_doors = enemy_populations(), object_populations(), doors()
    print(f'{len(all_rooms)} rooms, {len(all_states)} states, {len(choices)} rooms with a choice of state')
    print(f'{len(populations)} enemy populations placing {sum(len(p) for p in populations.values())} enemies')
    print(f'{len(objects)} object populations with {sum(len(o) for o in objects.values())} entries, {len(all_doors)} doors')
    missing = sorted({state.enemies for state in all_states.values()} - set(populations))
    print('enemy populations named by states and not read:', missing[:20])
    print('object populations named by states and not read:', sorted({s.objects for s in all_states.values()} - set(objects))[:20])
    kinds: dict[str, int] = {}
    for entries in objects.values():
        for entry in entries:
            placed = object_placement(entry)
            kind = placed.setup_type if placed else 'spread: ' + entry.split('(')[0]
            kinds[kind] = kinds.get(kind, 0) + 1
    for kind, count in sorted(kinds.items(), key=lambda item: -item[1]):
        print(f'  {count:5} {kind}')
