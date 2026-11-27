"""How a room's effect in the old code becomes its liquid and its room effects.

The old code gives each version of a room one "effect": perhaps a liquid with a level, perhaps rain or fog or spores,
with the colour effects it starts. One of those colour effects is the heat that burns Samus, so a room that starts
it has heat. Each of the others that shows on the room's own pictures (rock that glows, lights, lightning) is a colour
effect placed in the room, in the order the old effect starts them. A room entered by certain doors shows a different effect than by its others; what is placed for such an
effect is present only when the room was entered at that door's entry point.
"""
from __future__ import annotations

import re

from enemies import Placed, number, quoted, snake

import legacy

LIQUID = 'res://engine/world/Liquid.cs'
OVERLAY = 'res://engine/world/OverlayEffect.cs'
TRIGGER = 'res://engine/world/Trigger.cs'
DARKNESS = 'res://game/effects/FirefleaDarkness.cs'
COLOURS = 'res://engine/world/ColourEffect.cs'
SCREEN = 256

# What heat takes from Samus each update, in hundredths of a point of energy.
HEAT_DAMAGE = 25

# How far each size of tide carries a liquid's surface, and how fast it turns above and below where it lies.
TIDES = {'Small': (8, 0.75, 1.125), 'Large': (32, 0.5, 0.875)}

# The overlays the old code's kinds of effect are, by the name each is known by.
OVERLAYS = {'Rain': 'rain', 'Fog': 'fog', 'Spores': 'spores'}

# The liquid each of the old code's kinds of effect fills a room with. The water of the statues' hall is water.
LIQUIDS = {'Water': 'water', 'TourianEntranceStatue': 'water', 'Lava': 'lava', 'Acid': 'acid'}

# The liquids whose effect moves, and stirs the game's random number, though it has no level in the room.
STIRRING = ('Lava', 'Acid')

# What follows a liquid's name in the name of its type, by how the old code colours it: liquids of one kind that
# differ in colour are types of their own.
COLOURED = {
    ('water', 'WaterAndAcid'): '', ('water', 'GreenMaridiaWater'): '_green', ('water', 'IndigoMaridiaWater'): '_indigo',
    ('water', 'PurpleMaridiaWater'): '_purple', ('water', 'TealMaridiaWater'): '_teal',
    ('lava', 'Lava'): '',
    ('acid', 'None'): '', ('acid', 'Lava'): '_lava', ('acid', 'WaterAndAcid'): '_water',
}


def room_colour_effects() -> dict[str, dict]:
    """
    Each of the old code's colour effects that shows on a room's own pictures, by name: how it starts and what it
    watches, with the height it watches Samus against where it watches that.
    """
    shown_on_rooms = set(re.findall(r'ColourEffectColours\.(\w+)', legacy.without_comments(
        legacy.read('Fx', 'ColourEffects', 'ColourEffectRoomColourDefinitions.cs'))))
    text = legacy.without_comments(legacy.read('Fx', 'ColourEffects', 'ColourEffectAnimations.cs'))
    heights = dict(re.findall(r'const int (\w+) = (\d+);', legacy.without_comments(
        legacy.read('Fx', 'ColourEffects', 'CrateriaLightningDefinitions.cs'))))
    effects = {}
    listed = re.findall(
        r'\[A\.(\w+)\]\s*=\s*new\(ColourEffectStart\.(\w+),\s*ColourEffectColours\.(\w+),\s*'
        r'(?:new\s+)?ColourEffectCondition\.(\w+)(?:\(CrateriaLightningDefinitions\.(\w+)\))?', text)
    for name, start, colours, watches, height in listed:
        if colours in shown_on_rooms:
            effects[name] = {'start': start, 'watches': watches, 'height': int(heights[height]) if height else None}
    if not effects:
        raise ValueError('No colour effect of the old code shows on a room\'s own pictures.')
    return effects


ROOM_COLOUR_EFFECTS = room_colour_effects()


def colour_effect(name: str, ctx) -> Placed:
    """One of the old code's colour effects on a room's own pictures, as the colour effect placed in the room."""
    old = ROOM_COLOUR_EFFECTS[name]
    properties = {'EffectName': quoted(snake(name)), 'StepsFile': quoted('colours/{graphics}/' + snake(name) + '.json')}
    only_when = None
    mini_boss = lambda: ctx.fact(ctx.boss('AreaMiniBoss') + '_defeated')  # noqa: E731
    if old['start'] == 'SkippedOnceAreaMiniBossIsDead':
        only_when = ctx.none_of([mini_boss()])
    elif old['start'] != 'Plain':
        raise ValueError(f'The colour effect {name} starts in a way this does not know: {old["start"]}.')
    if old['watches'] == 'EndsWhenAreaMiniBossDies':
        properties['EndsWhen'] = mini_boss()
    elif old['watches'] == 'RestartsWhileSamusIsHigh':
        properties['RestartsWhile'] = ctx.player_above(old['height'])
    elif old['watches'] != 'WatchesNothing':
        raise ValueError(f'The colour effect {name} watches something this does not know: {old["watches"]}.')
    return Placed(COLOURS, name, properties, at=(0, 0), enemy=False, only_when=only_when)


def effect_things(effect: dict, room: legacy.Room, ctx, told_speed: float | None = None) -> tuple[list[Placed], list[tuple[Placed, Placed]]]:
    """
    What one of the old code's effects places, and which of those tells which. `told_speed` is the speed a liquid
    with a level to go to and no speed of its own is given by whatever of the room tells it to go.
    """
    placed: list[Placed] = []
    links: list[tuple[Placed, Placed]] = []
    kind = effect['type']
    if kind in LIQUIDS and effect['start'] is not None:
        start, target = effect['start'], effect['target']
        coloured = (LIQUIDS[kind], effect['tint'])
        if coloured not in COLOURED:
            raise ValueError(f'No liquid is known for the effect {kind} coloured {effect["tint"]}.')
        name = LIQUIDS[kind] + COLOURED[coloured]
        properties = {'TypeFile': quoted(f'liquids/{name}.json')}
        if effect['drifts']:
            properties['Drifts'] = 'true'

        # Water held back from her until something of the room lets it in: the glass tube does, once it is broken.
        if effect['held_back']:
            properties['HeldBack'] = 'true'

        # A liquid given a level to go to and no speed stays where it is, unless something of the room gives it a
        # speed as it tells it to go: then it waits where it is to be told.
        moves = target is not None and number(effect['speed']).lstrip('-') not in ('0', '0.0')
        # One that waits to be told to fall waits raised, as one that waits to be told to rise waits low.
        waits = target is not None and not moves and told_speed is not None
        rises = (moves or waits) and target < start
        falls = (moves or waits) and target > start
        properties['LowSurface'] = str(target if falls else start)
        properties['HighSurface'] = str(target if rises else start)
        if falls:
            properties['StartsRaised'] = 'true'
        if rises or falls:
            properties['MoveSpeed'] = repr(told_speed) if waits else number(effect['speed']).lstrip('-')
            properties['MoveDelay'] = str(effect['delay'])
        if effect['tide'] in TIDES:
            reach, above, below = TIDES[effect['tide']]
            properties.update({'TideReach': str(reach), 'TideTurnAbove': repr(above), 'TideTurnBelow': repr(below)})
        liquid = Placed(LIQUID, 'Liquid', properties, at=(0, 0), enemy=False)
        placed.append(liquid)
        if (rises or falls) and not waits:
            # It sets off by itself as the room is entered.
            tide = Placed(TRIGGER, 'LiquidMoves', {
                'Size': f'Vector2i({room.screens[0] * SCREEN}, {room.screens[1] * SCREEN})',
                'What': quoted('rise' if rises else 'fall')}, at=(0, 0), enemy=False)
            placed.append(tide)
            links.append((tide, liquid))
    elif kind in STIRRING and effect['start'] is None:
        # Lava's or acid's effect with no level: the liquid is the room's, lying below it all through. Nothing is
        # ever under it and it is seen nowhere, but it moves, and the game's random number is stirred by its moves
        # as by any lava's: the old effect stirs the number whether or not it has a level (RoomFxOverlayState).
        coloured = (LIQUIDS[kind], effect['tint'])
        if coloured not in COLOURED:
            raise ValueError(f'No liquid is known for the effect {kind} coloured {effect["tint"]}.')
        placed.append(Placed(LIQUID, 'Liquid', {
            'TypeFile': quoted(f'liquids/{LIQUIDS[kind] + COLOURED[coloured]}.json'), 'LiesBelowRoom': 'true'}, at=(0, 0), enemy=False))
    if 'SamusInHeat' in effect['colours']:
        placed.append(Placed(OVERLAY, 'Heat', {'EffectName': quoted('heat'), 'DamageHundredths': str(HEAT_DAMAGE)}, at=(0, 0), enemy=False))
    if kind in OVERLAYS:
        placed.append(Placed(OVERLAY, kind, {
            'EffectName': quoted(OVERLAYS[kind]), 'LookFile': quoted(f'effects/{OVERLAYS[kind]}.json')}, at=(0, 0), enemy=False))
    if kind == 'Fireflea':
        placed.append(Placed(DARKNESS, 'Darkness', {'EffectName': quoted('darkness')}, at=(0, 0), enemy=False))
    # Its colour effects on the room's own pictures, in the order it starts them.
    placed.extend(colour_effect(name, ctx) for name in effect['colours'] if name in ROOM_COLOUR_EFFECTS)
    return placed, links


def things(selection: dict, room: legacy.Room, ctx, entry_point, told_speed: float | None = None) -> tuple[list[Placed], list[tuple[Placed, Placed]]]:
    """
    What a version's effect places. `entry_point` gives the entry point of the room that one of the old code's doors
    arrives at; `told_speed` is as for `effect_things`.
    """
    placed: list[Placed] = []
    links: list[tuple[Placed, Placed]] = []
    by_door = [(entry_point(door), effect) for door, effect in selection['doors']]
    def also(thing: Placed, entered: str) -> str:
        """A thing's own condition, if it has one, together with the way the room was entered."""
        return entered if thing.only_when is None else ctx.all_of([thing.only_when, entered])

    for point, effect in by_door:
        mine, told = effect_things(effect, room, ctx, told_speed)
        for thing in mine:
            thing.only_when = also(thing, ctx.entered_at(point))
        placed.extend(mine)
        links.extend(told)
    if selection['default'] is not None:
        mine, told = effect_things(selection['default'], room, ctx, told_speed)
        if by_door:
            for thing in mine:
                thing.only_when = also(thing, ctx.none_of([ctx.entered_at(point) for point, _ in by_door]))
        placed.extend(mine)
        links.extend(told)
    return placed, links
