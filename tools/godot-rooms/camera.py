"""How a room's scrolling in the old code becomes camera zones and the triggers that open and close them.

The old code gives each screen of a room one of three states: closed to the camera; open; or open with the camera
aligned to the screen's top. A room's version carries the state of every screen, and names its layout when that is
an explicit table. A screen something can change is also a camera zone, the handle that is told how to let the view
in.

The old code changes a screen's state from blocks Samus touches, and as she comes in by certain doors. Each of
those becomes triggers, one for each zone told: a region where the blocks were, or one that fires as the room is
entered at that door's entry point. A change may name a screen below the room's own rows; that screen is a zone
too, for the game keeps such a screen's state.
"""
from __future__ import annotations

import re

import legacy
from enemies import Placed, quoted

SCREEN = 256
VIEW_HEIGHT = 224
BLOCK = 16
ZONE = 'res://engine/world/CameraZone.cs'
TRIGGER = 'res://engine/world/Trigger.cs'

# The way each run of extension blocks goes on from a trigger's block.
RUNS = {'Rightwards': (1, 0), 'Leftwards': (-1, 0), 'Downwards': (0, 1), 'Upwards': (0, -1)}


class Camera:
    """The camera zones and scroll triggers of one version of a room."""

    def __init__(self, layouts, changes, setups, room: legacy.Room, state: legacy.State, entries: list[str],
                 entry_points: list[tuple[str, str]]) -> None:
        """`entry_points` pairs each entry point of the room with the old door setup applied on coming in by it."""
        self.placed: list[Placed] = []
        self.links: list[tuple[Placed, Placed]] = []
        width, height = room.screens
        layout = layouts[state.scroll]
        if isinstance(layout, str):
            # An implicit layout: every screen open, the bottom row perhaps kept from its bottom rows.
            initial = {(x, y): (layout if y == height - 1 else 'Open') for x in range(width) for y in range(height)}
        else:
            if len(layout) != width * height:
                raise ValueError(f'The scroll layout {state.scroll} has {len(layout)} screens; the room {room.name} has {width * height}.')
            initial = {(x, y): layout[y * width + x] for x in range(width) for y in range(height)}

        # Every change this version can make: (where it is made from, the screens it sets).
        regions: list[tuple[tuple[int, int, int, int], list]] = []
        for entry in entries:
            placement = legacy.object_placement(entry)
            if placement is not None and placement.setup_type == 'ScrollTriggerSetup':
                change = re.search(r'RoomScrollChange\.(\w+)', placement.setup).group(1)
                regions.append(((placement.block[0], placement.block[1], 1, 1), changes[change]))
            elif entry.startswith('..') and 'ScrollTriggerPlacements.Reaching' in entry:
                arguments = legacy.call_arguments(entry)
                block = legacy.numbers(arguments[0])
                change = changes[legacy.last_name(arguments[1])]
                # The trigger's own block, joined to its first run where that is straight on from it; then each
                # further run, which may turn a corner, as a region of its own.
                at = (block[0], block[1])
                first = True
                for run in arguments[2:]:
                    way = RUNS[re.search(r'ScrollExtensionDefinitions\.(\w+)', run).group(1)]
                    count = legacy.numbers(run)[-1]
                    start = at if first else (at[0] + way[0], at[1] + way[1])
                    end = (at[0] + way[0] * count, at[1] + way[1] * count)
                    regions.append(((min(start[0], end[0]), min(start[1], end[1]),
                                     abs(end[0] - start[0]) + 1, abs(end[1] - start[1]) + 1), change))
                    at, first = end, False
                if first:
                    regions.append(((at[0], at[1], 1, 1), change))
        arrivals = [(point, setups[setup]) for point, setup in entry_points if setup in setups]
        changes_made = [change for _, change in regions + arrivals]

        # A zone is made for each screen something can tell: one closed to begin with, which something must open,
        # and one a change names, within the room's columns.
        def first_open(screen) -> str:
            states = [initial.get(screen, 'Closed')] + [new for change in changes_made for where, new in change if where == screen]
            return next((state for state in states if state != 'Closed'), 'Open')

        named = {screen for screen in initial if initial[screen] == 'Closed'}
        named |= {where for change in changes_made for where, _ in change if 0 <= where[0] < width}
        zones: dict[tuple[int, int], Placed] = {}
        for screen in sorted(named, key=lambda at: (at[1], at[0])):
            x, y = screen
            zones[screen] = Placed(ZONE, f'Screen{x}x{y}', {'OpensTo': str(STATES.index(first_open(screen)))},
                                   at=(x * SCREEN, y * SCREEN), enemy=False)
        self.placed.extend(zones.values())

        def tell(change, make) -> None:
            for screen, new in change:
                zone = zones.get(screen)
                if zone is None:
                    # A change may name a screen outside the room's columns, which the old code refuses too.
                    continue
                trigger = make(WORDS[new])
                self.placed.append(trigger)
                self.links.append((trigger, zone))
        for (left, top, across, down), change in regions:
            tell(change, lambda what: Placed(TRIGGER, 'Scroll', {
                'Size': f'Vector2i({across * BLOCK}, {down * BLOCK})', 'What': quoted(what), 'WhileTouched': 'true'},
                at=(left * BLOCK, top * BLOCK), enemy=False))
        for point, change in arrivals:
            tell(change, lambda what: Placed(TRIGGER, 'Arriving', {'AtEntry': quoted(point), 'What': quoted(what)}, at=(0, 0), enemy=False))


# The three states, in the order of the engine's ScreenScroll, and the word a zone is told for each.
STATES = ['Closed', 'Open', 'OpenAlignedToTop']
WORDS = {'Closed': 'close', 'Open': 'open_fully', 'OpenAlignedToTop': 'open_to_top'}


def screens_of(layouts, room: legacy.Room, state: legacy.State) -> tuple[list[int], str]:
    """Every screen's state for a room's version, row by row, and the layout's name when it is an explicit table."""
    width, height = room.screens
    layout = layouts[state.scroll]
    if isinstance(layout, str):
        return [STATES.index(layout if y == height - 1 else 'Open') for y in range(height) for x in range(width)], ''
    if len(layout) != width * height:
        raise ValueError(f'The scroll layout {state.scroll} has {len(layout)} screens; the room {room.name} has {width * height}.')
    return [STATES.index(name) for name in layout], state.scroll
