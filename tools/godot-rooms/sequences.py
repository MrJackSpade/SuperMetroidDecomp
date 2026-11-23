"""What the old code's per-room routines become: room sequences, the things they move, and backdrops.

Each state of a room in the old code may name a routine run every update ("main") and one run as the room is set up
("setup"), and a background with how it scrolls. Here each becomes what the new project has for it: a room sequence
with the things it is given, an overlay, a watch that records a fact, or a backdrop.
"""
from __future__ import annotations

import re

import legacy
from enemies import Placed, quoted, snake

R = 'res://game/rooms/'
VISUAL = 'res://engine/actors/components/ActorVisual.cs'
OVERLAY = 'res://engine/world/OverlayEffect.cs'
BACKDROP = 'res://engine/world/Backdrop.cs'
TURNING_FLOOR = 'res://engine/world/TurningFloor.cs'
TRIGGER = 'res://engine/world/Trigger.cs'
SCREEN = 256

# The routines that are the escape as a room shows it, with how each room's differs.
ESCAPES = {
    'ShakeScreenWithRandomExplosions': {},
    'RandomEscapeExplosions': {},
    'ScrollingSkyLandWithEscapeExplosions': {'Outdoors': 'true'},
    'ShakeScreenLightHorizontalAndMediumDiagonal': {'ShakeStrength': '1'},
    'ShakeScreenMediumHorizontalAndStrongDiagonal': {'ShakeStrength': '2'},
}

# The terrain patches a room's escape version has in place, by the old setup routine that made the change.
ESCAPE_PATCHES = {'AutoDestroyWallDuringEscape': 'escape_wall_destroyed', 'TurnWallIntoShotBlocksDuringEscape': 'wall_shot_blocks'}

# Where Samus is put as she comes into Maridia's tube from above and from below; how far past an end of it a ride
# rests, which is past that end's exit; and where a ride holds her from its corner.
TUBE_ARRIVES_ABOVE = (128, 64)
TUBE_ARRIVES_BELOW = (128, 2496)
TUBE_PAST_END = -8
TUBE_HOLDS = (16, 22)

# The guardians of the Tourian statue, each with a visual and a grey one over it.
GUARDIANS = ['Kraid', 'Phantoon', 'Draygon', 'Ridley']


def sequence(script: str, name: str, properties: dict[str, str], **more) -> Placed:
    return Placed(R + script, name, properties, at=None, enemy=False, kind='Node', **more)


def share(scroll: str) -> str:
    """How much of the room's scrolling a background follows along one axis: all of it, a part, or none."""
    if 'ParallaxRate' in scroll:
        top, bottom = legacy.numbers(scroll[scroll.index('ParallaxRate'):])[:2]
        return repr(top / bottom)
    return '0' if 'Fixed' in scroll else '1'


class Sequences:
    """The sequences and backdrops of one version of a room."""

    def __init__(self, room: legacy.Room, state: legacy.State, chosen_on: str, ctx, named: dict[str, Placed], landing: tuple[str, str]) -> None:
        """
        `chosen_on` is the condition the version is chosen on, empty for the one with none; `named` holds what the
        version has placed so far, by name; `landing` is the scene and entry point Ceres's elevator leads to.
        """
        self.placed: list[Placed] = []
        self.links: list[tuple[Placed, str, object]] = []
        whole = f'Vector2i({room.screens[0] * SCREEN}, {room.screens[1] * SCREEN})'

        # The background, where the room has a picture of its own for one; a room's layout draws the others.
        if state.background != 'null' and 'TurningFloor' not in state.properties:
            modes = legacy.call_arguments(state.background_scroll)
            self.placed.append(Placed(BACKDROP, 'Backdrop', {
                'Picture': quoted(f'backdrops/{snake(state.background)}.png'),
                'ScrollShare': f'Vector2({share(modes[0])}, {share(modes[1])})'}, at=(0, 0), enemy=False))

        if state.main in ESCAPES:
            self.placed.append(sequence('EscapeRoom.cs', 'Escape', {'StartsWhen': ctx.always(), **ESCAPES[state.main]}))
        if state.setup == 'OpenRescuedAnimalsPassageAndQuake':
            # The way the animals left by stands open, and the room shakes, once they have been set free.
            relay = Placed('res://engine/world/Relay.cs', 'AnimalsPassage', {'Rules': 'rules:open:animals_passage_open'},
                           at=None, enemy=False, kind='Node')
            opens = Placed(TRIGGER, 'AnimalsGone', {'Size': whole, 'What': quoted('open'), 'OnlyWhen': ctx.fact('animals_escaped')},
                           at=(0, 0), enemy=False)
            self.placed += [relay, opens]
            self.links.append((opens, 'Target', relay))
        if state.setup == 'SetCollectedMap':
            self.placed.append(sequence('RoomWatch.cs', 'MapKnown', {
                'StartsWhen': ctx.always(), 'Until': ctx.always(), 'Records': f'PackedStringArray("map revealed: {snake(room.area)}")'}))

        if room.area == 'Ceres':
            self.ceres(state, 'BossDead' in chosen_on, ctx, named, landing)
        if state.main == 'ScrollingSkyLand' and 'Gunship' in named:
            landing_sequence = sequence('crateria/GunshipLanding.cs', 'Landing', {'StartsAtEntry': quoted('AtZebesArrival')})
            departure = sequence('crateria/ZebesDeparture.cs', 'Departure', {})
            self.placed += [landing_sequence, departure]
            self.links.append((landing_sequence, 'Gunship', named['Gunship']))
            self.links.append((named['Gunship'], 'ToldWhenAway', [departure]))
        if state.main == 'ScrollingSkyLandWithEscapeExplosions' and 'Gunship' in named:
            departure = sequence('crateria/ZebesDeparture.cs', 'Departure', {})
            self.placed.append(departure)
            self.links.append((named['Gunship'], 'ToldWhenAway', [departure]))
        if state.main == 'CrocomireRoomShaking' and 'Crocomire' in named:
            shaking = sequence('norfair/CrocomireRoomShaking.cs', 'WallRattles', {'StartsWhen': ctx.always()},
                               only_when=named['Crocomire'].only_when)
            self.placed.append(shaking)
            self.links.append((shaking, 'Crocomire', named['Crocomire']))
        if state.setup == 'RunStatueUnlockingAnimations':
            self.statue_hall(ctx, named)
        if state.main == 'MaridiaElevatube':
            # The tube takes whoever comes in at one end out at the other: each ride rests past the far end and
            # brings an arrival back to it, which carries them out by that end's exit.
            height = room.screens[1] * SCREEN
            for name, rest, arrives in (('RideUp', TUBE_PAST_END, TUBE_ARRIVES_BELOW[1]), ('RideDown', height + TUBE_PAST_END + 18, TUBE_ARRIVES_ABOVE[1])):
                top = rest - TUBE_HOLDS[1]
                self.placed.append(Placed('res://game/objects/elevator/tube_ride.tscn', name, {
                    'Path': f'Array[Vector2i]([Vector2i(0, 0), Vector2i(0, {arrives - rest})])'},
                    at=(TUBE_ARRIVES_BELOW[0] - TUBE_HOLDS[0], top), enemy=False))

    def ceres(self, state: legacy.State, ridley_gone: bool, ctx, named: dict[str, Placed], landing: tuple[str, str]) -> None:
        shaking = sequence('ceres/CeresShaking.cs', 'Shaking', {'StartsWhen': ctx.always()})
        self.placed.append(shaking)
        if 'Haze' in state.setup:
            self.placed.append(Placed(OVERLAY, 'Haze', {
                'EffectName': quoted('ceres_haze'), 'Picture': quoted('effects/ceres_haze.png')}, at=(0, 0), enemy=False))
        if state.main == 'SpawnCeresPreElevatorHallFallingDebris':
            self.placed.append(sequence('ceres/CeresDebris.cs', 'Debris', {
                'StartsWhen': ctx.fact('ceres_self_destruct_started'),
                'LightPiece': quoted('effects/ceres_debris_light.json'), 'DarkPiece': quoted('effects/ceres_debris_dark.json')}))
        if state.main == 'CeresRidleyGetaway':
            floor = Placed(TURNING_FLOOR, 'Floor', {'Picture': quoted('backdrops/ceres_ridley_floor.png')}, at=(0, 0), enemy=False)
            getaway = sequence('ceres/CeresGetaway.cs', 'Getaway', {
                'HeadingPicture': quoted('text/emergency.png'), 'Message': quoted('ceres_self_destruct_warning'),
                'Subtitle': quoted('ceres_self_destruct_subtitle'), 'LetterSound': quoted('typing')})
            self.placed += [floor, getaway]
            self.links.append((getaway, 'Floor', floor))
            self.links.append((getaway, 'ToldWhenCountdownBegins', [shaking]))
            if 'Ridley' in named:
                self.links.append((named['Ridley'], 'ToldWhenGone', [getaway]))
        if state.main == 'CeresElevatorShaft':
            floor = Placed(TURNING_FLOOR, 'Floor', {'Picture': quoted('backdrops/ceres_elevator_shaft.png')}, at=(0, 0), enemy=False,
                           children=[('Platform', VISUAL, {})])
            self.placed.append(floor)
            if 'ElevatorDoor' in named:
                self.links.append((named['ElevatorDoor'], 'Floor', floor))
            if ridley_gone:
                shaft = sequence('ceres/CeresShaft.cs', 'Shaft', {
                    'StartsWhen': ctx.always(), 'LandingRoom': quoted(landing[0]), 'LandingEntry': quoted(landing[1])})
                self.placed.append(shaft)
                self.links.append((shaft, 'Floor', floor))
            else:
                self.placed.append(sequence('ceres/CeresArrival.cs', 'Arrival', {'StartsAtEntry': quoted('AtCeresElevator')},
                                            children=[('Pad', VISUAL, {}), ('Platform', VISUAL, {})]))

    def statue_hall(self, ctx, named: dict[str, Placed]) -> None:
        # The statue is a plain thing the hall's sequence moves and shows: a visual for each guardian, and a grey one over each.
        statue = Placed('', 'Statue', {}, at=(120, 184), enemy=False,
                        children=[(guardian + grey, VISUAL, {}) for guardian in GUARDIANS for grey in ('', 'Grey')])
        hall = sequence('crateria/TourianStatueHall.cs', 'StatueHall', {
            'DataFile': quoted('rooms/tourian_statue_hall.json'), 'StartsWhen': ctx.always(),
            'CrumblePatch': quoted('floor_crumbling'), 'ClearPatch': quoted('floor_clear')})
        self.placed += [statue, hall]
        self.links.append((hall, 'Statue', statue))
        if 'Screen0x1' in named:
            self.links.append((hall, 'LowerView', named['Screen0x1']))
