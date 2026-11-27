"""What a room's ground raises where Samus lands or runs on it, read from the old code's grounds.

The old code gives every room a ground: its area's (Norfair's and the Wrecked Ship's dusty, Maridia's wet, the
others' bare) unless the room names its own. Wet ground splashes and dusty ground raises dust; bare ground raises
nothing. Two rooms are wet only at times: the Landing Site while it rains, and the West Ocean at and below its
waterline. A room whose ground raises something gets a Ground node saying so.
"""
from __future__ import annotations

import re

from enemies import Placed

import legacy

GROUND = 'res://game/rooms/Ground.cs'
OVERLAY = 'res://engine/world/OverlayEffect.cs'

# The Godot game's kinds of ground, in the order its GroundKind lists them.
KINDS = ['Wet', 'Dusty']


def old_grounds() -> tuple[dict[str, tuple[str, bool]], dict[str, str]]:
    """Each of the old code's grounds by name, as its wetness and whether dry landings on it raise dust; and each area's ground."""
    text = legacy.without_comments(legacy.read('Rooms', 'RoomGroundDefinitions.cs'))
    grounds = {name: (wetness, dust == 'true') for name, wetness, dust in re.findall(
        r'RoomGround (\w+) = new\(RoomGroundWetness\.(\w+), DryLandingsRaiseDust: (true|false)\);', text)}
    areas = dict(re.findall(r'\[AreaId\.(\w+)\] = (\w+),', text))
    if not grounds or set(areas) != set(legacy.AREAS) or not set(areas.values()) <= set(grounds):
        raise ValueError('The old code\'s grounds, or the ground of every area, could not be read.')
    return grounds, areas


GROUNDS, AREA_GROUNDS = old_grounds()

# The height of the West Ocean's waterline, at and below which its ground is wet.
WATERLINE = int(re.search(r'const int WreckedShipEntranceWaterlineY = (\d+);', legacy.without_comments(
    legacy.read('Samus', 'Drawing', 'SamusAtmosphericEffectDefinitions.cs'))).group(1))


def things(room: legacy.Room, shown: list[Placed]) -> tuple[list[Placed], list[tuple[Placed, Placed]]]:
    """
    What a version of the room places for its ground, given what its effect places (`shown`), and which of those the
    ground names: nothing where its ground raises nothing.
    """
    name = legacy.last_name(room.properties['Ground']) if 'Ground' in room.properties else AREA_GROUNDS[room.area]
    if name not in GROUNDS:
        raise ValueError(f'{room.name}: its ground {name} is none the old code defines.')
    wetness, dust = GROUNDS[name]
    if dust and wetness != 'Dry':
        raise ValueError(f'{room.name}: its ground {name} is wet at times and dusty otherwise, which a Ground cannot say.')
    if wetness == 'Dry':
        return ([Placed(GROUND, 'Ground', {'Kind': str(KINDS.index('Dusty'))}, at=None, enemy=False, kind='Node')], []) if dust else ([], [])
    ground = Placed(GROUND, 'Ground', {'Kind': str(KINDS.index('Wet'))}, at=None, enemy=False, kind='Node')
    if wetness == 'Wet':
        return [ground], []
    if wetness == 'WetBelowWaterline':
        ground.properties['FromHeight'] = str(WATERLINE)
        return [ground], []
    if wetness == 'WetWhileRaining':
        # Wet while its rain is in the room and showing; a version without rain has plain ground.
        rains = [thing for thing in shown if thing.scene == OVERLAY and thing.properties.get('EffectName') == '"rain"']
        if len(rains) > 1:
            raise ValueError(f'{room.name}: its ground is wet while it rains, and a version of it has {len(rains)} rains.')
        return ([ground], [(ground, rains[0])]) if rains else ([], [])
    raise ValueError(f'{room.name}: its ground {name} is wet in a way this does not know: {wetness}.')
