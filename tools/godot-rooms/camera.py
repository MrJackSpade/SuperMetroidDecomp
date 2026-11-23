"""How a room's scrolling in the old code becomes camera zones and the triggers that open and close them.

The old code gives each screen of a room one of three states: closed to the camera; open; or open with the camera
kept from the screen's bottom rows. A screen here is two zones the view can be kept out of: the 224 rows the view
fills, and the 32 rows under them. Closed, both are shut; open but kept from the bottom rows, only the lower one
is; fully open, neither. A zone is only made where something can shut it.

The old code changes a screen's state from blocks Samus touches, and as she comes in by certain doors. Each of
those becomes triggers, one for each zone told to open or close: a region where the blocks were, or one that fires
as the room is entered at that door's entry point.
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

        # A zone is made only for a screen something can shut: shut to begin with, or by a change.
        def ever(screen, states) -> bool:
            return initial[screen] in states or any(
                where == screen and new in states for _, change in regions + arrivals for where, new in change)

        upper: dict[tuple[int, int], Placed] = {}
        lower: dict[tuple[int, int], Placed] = {}
        for screen in initial:
            x, y = screen
            if ever(screen, ('Closed',)):
                upper[screen] = Placed(ZONE, f'Screen{x}x{y}', {
                    'StartsOpen': 'true' if initial[screen] != 'Closed' else 'false'}, at=(x * SCREEN, y * SCREEN), enemy=False)
            if ever(screen, ('Closed', 'OpenAlignedToTop')):
                lower[screen] = Placed(ZONE, f'Screen{x}x{y}Foot', {
                    'Size': f'Vector2i({SCREEN}, {SCREEN - VIEW_HEIGHT})',
                    'StartsOpen': 'true' if initial[screen] == 'Open' else 'false'}, at=(x * SCREEN, y * SCREEN + VIEW_HEIGHT), enemy=False)
        self.placed.extend(list(upper.values()) + list(lower.values()))

        def tell(change, make) -> None:
            for screen, new in change:
                if screen not in initial:
                    # Some doors set a screen the room does not have.
                    continue
                for zones, opens in ((upper, new != 'Closed'), (lower, new == 'Open')):
                    if screen in zones:
                        trigger = make('open' if opens else 'close')
                        self.placed.append(trigger)
                        self.links.append((trigger, zones[screen]))

        for (left, top, across, down), change in regions:
            tell(change, lambda what: Placed(TRIGGER, 'Scroll', {
                'Size': f'Vector2i({across * BLOCK}, {down * BLOCK})', 'What': quoted(what), 'Once': 'false'},
                at=(left * BLOCK, top * BLOCK), enemy=False))
        for point, change in arrivals:
            tell(change, lambda what: Placed(TRIGGER, 'Arriving', {'AtEntry': quoted(point), 'What': quoted(what)}, at=(0, 0), enemy=False))
