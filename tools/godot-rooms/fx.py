"""How a room's effect in the old code becomes its liquid and its room effects.

The old code gives each version of a room one "effect": perhaps a liquid with a level, perhaps rain or fog or spores,
with the colour effects it starts. One of those colour effects is the heat that burns Samus, so a room that starts
it has heat. A room entered by certain doors shows a different effect than by its others; what is placed for such an
effect is present only when the room was entered at that door's entry point.
"""
from __future__ import annotations

from enemies import Placed, number, quoted, snake

import legacy

LIQUID = 'res://engine/world/Liquid.cs'
OVERLAY = 'res://engine/world/OverlayEffect.cs'
TRIGGER = 'res://engine/world/Trigger.cs'
DARKNESS = 'res://game/effects/FirefleaDarkness.cs'
SCREEN = 256

# What heat takes from Samus each update, in hundredths of a point of energy.
HEAT_DAMAGE = 25

# How far each size of tide carries a liquid's surface, and how fast it turns above and below where it lies.
TIDES = {'Small': (8, 0.75, 1.125), 'Large': (32, 0.5, 0.875)}

# The overlays the old code's kinds of effect are, by the name each is known by.
OVERLAYS = {'Rain': 'rain', 'Fog': 'fog', 'Spores': 'spores'}

# The waters of Maridia, which differ in colour.
WATERS = {'GreenMaridiaWater': 'green', 'IndigoMaridiaWater': 'indigo', 'PurpleMaridiaWater': 'purple', 'TealMaridiaWater': 'teal'}


def effect_things(effect: dict, room: legacy.Room) -> tuple[list[Placed], list[tuple[Placed, Placed]]]:
    """What one of the old code's effects places, and which of those tells which."""
    placed: list[Placed] = []
    links: list[tuple[Placed, Placed]] = []
    kind = effect['type']
    if kind in ('Water', 'Lava', 'Acid') and effect['start'] is not None:
        start, target = effect['start'], effect['target']
        name = kind.lower()
        if kind == 'Water' and effect['tint'] in WATERS:
            name += '_' + WATERS[effect['tint']]
        properties = {'TypeFile': quoted(f'liquids/{name}.json')}
        rises = target is not None and target < start
        falls = target is not None and target > start
        properties['LowSurface'] = str(target if falls else start)
        properties['HighSurface'] = str(target if rises else start)
        if falls:
            properties['StartsRaised'] = 'true'
        if rises or falls:
            properties['MoveSpeed'] = number(effect['speed']).lstrip('-')
            properties['MoveDelay'] = str(effect['delay'])
        if effect['tide'] in TIDES:
            reach, above, below = TIDES[effect['tide']]
            properties.update({'TideReach': str(reach), 'TideTurnAbove': repr(above), 'TideTurnBelow': repr(below)})
        liquid = Placed(LIQUID, 'Liquid', properties, at=(0, 0), enemy=False)
        placed.append(liquid)
        if rises or falls:
            # It sets off by itself as the room is entered.
            tide = Placed(TRIGGER, 'LiquidMoves', {
                'Size': f'Vector2i({room.screens[0] * SCREEN}, {room.screens[1] * SCREEN})',
                'What': quoted('rise' if rises else 'fall')}, at=(0, 0), enemy=False)
            placed.append(tide)
            links.append((tide, liquid))
    if 'SamusInHeat' in effect['colours']:
        placed.append(Placed(OVERLAY, 'Heat', {'EffectName': quoted('heat'), 'DamageHundredths': str(HEAT_DAMAGE)}, at=(0, 0), enemy=False))
    if kind in OVERLAYS:
        placed.append(Placed(OVERLAY, kind, {
            'EffectName': quoted(OVERLAYS[kind]), 'Picture': quoted(f'effects/{OVERLAYS[kind]}.png')}, at=(0, 0), enemy=False))
    if kind == 'Fireflea':
        placed.append(Placed(DARKNESS, 'Darkness', {'EffectName': quoted('darkness')}, at=(0, 0), enemy=False))
    return placed, links


def things(selection: dict, room: legacy.Room, ctx, entry_point) -> tuple[list[Placed], list[tuple[Placed, Placed]]]:
    """
    What a version's effect places. `entry_point` gives the entry point of the room that one of the old code's doors
    arrives at.
    """
    placed: list[Placed] = []
    links: list[tuple[Placed, Placed]] = []
    by_door = [(entry_point(door), effect) for door, effect in selection['doors']]
    for point, effect in by_door:
        mine, told = effect_things(effect, room)
        for thing in mine:
            thing.only_when = ctx.entered_at(point)
        placed.extend(mine)
        links.extend(told)
    if selection['default'] is not None:
        mine, told = effect_things(selection['default'], room)
        if by_door:
            for thing in mine:
                thing.only_when = ctx.none_of([ctx.entered_at(point) for point, _ in by_door])
        placed.extend(mine)
        links.extend(told)
    return placed, links
