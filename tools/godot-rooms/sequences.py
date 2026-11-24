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
    'ShakeScreenWithRandomExplosions': {'Outdoors': 'true', 'TurnsHarder': 'false'},
    'RandomEscapeExplosions': {'TurnsHarder': 'false'},
    'ScrollingSkyLandWithEscapeExplosions': {'Outdoors': 'true', 'TurnsHarder': 'false'},
    'ShakeScreenLightHorizontalAndMediumDiagonal': {},
    'ShakeScreenMediumHorizontalAndStrongDiagonal': {},
}

# How hard each room of the escape shakes, by the old setup routine of its escape version. The old game starts a
# shake in some rooms and lets it carry on into the next: the old Tourian shaft keeps the last Tourian room's, and
# the rooms past the one the animals leave by keep that room's.
ESCAPE_SHAKES = {
    'SetZebesTimebombEventAndLightHorizontalShaking': 1, 'SetLightHorizontalRoomShaking': 1,
    'SetMediumHorizontalRoomShaking': 2, 'MediumHorizontalShakingWithRisingAcid': 2, 'AutoDestroyWallDuringEscape': 2,
    'OpenRescuedAnimalsPassageAndQuake': 3, 'TurnWallIntoShotBlocksDuringEscape': 3, 'None': 3, 'LandSkyWithLandingSiteQuake': 3,
}

# What the escape's blasts are shown and heard by: the old game's small blast, puff of smoke, smoke and two clouds of
# dust, and the sounds of a small blast and of a big one.
ESCAPE_BLASTS = {
    'SmallExplosionEffect': 'effects/small_explosion.json', 'DustPuffEffect': 'effects/smoke_puff.json',
    'SmokeEffect': 'effects/smoke.json', 'ShortDustCloudEffect': 'effects/short_big_dust_cloud.json',
    'BigDustCloudEffect': 'effects/big_dust_cloud.json',
    'SmallExplosionSound': 'enemy_small_explosion', 'SmokeSound': 'enemy_big_explosion',
}

# The terrain patches a room's escape version has in place, by the old setup routine that made the change.
ESCAPE_PATCHES = {'AutoDestroyWallDuringEscape': 'escape_wall_destroyed', 'TurnWallIntoShotBlocksDuringEscape': 'wall_shot_blocks'}

# Where Samus is put as she comes into Maridia's tube from above and from below; how far past an end of it a ride
# rests, which is past that end's exit; and where a ride holds her from its corner.
TUBE_ARRIVES_ABOVE = (128, 64)
TUBE_ARRIVES_BELOW = (128, 2496)
TUBE_PAST_END = -8
TUBE_HOLDS = (16, 22)

# The old code's backgrounds that are no picture: none, the layer a boss's body is drawn on, and that layer cleared.
NOT_PICTURES = {'null', 'BossBody', 'Cleared', 'KraidAlive'}

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


def floor_picture(name: str) -> str:
    """One of the pictures shown on a turning floor, which is kept once for each set of graphics it is coloured by."""
    return f'backdrops/{{graphics}}/ceres_{name}.png'


def strings(values: list[str]) -> str:
    """A list of text as a scene writes one."""
    return 'Array[String]([' + ', '.join(quoted(value) for value in values) + '])'


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
        if state.background not in NOT_PICTURES and 'TurningFloor' not in state.properties:
            modes = legacy.call_arguments(state.background_scroll)
            self.placed.append(Placed(BACKDROP, 'Backdrop', {
                # A background is coloured by the room's set of graphics, so its picture is kept once for each set.
                'Picture': quoted(f'backdrops/{{graphics}}/{snake(state.background)}.png'),
                'ScrollShare': f'Vector2({share(modes[0])}, {share(modes[1])})'}, at=(0, 0), enemy=False))

        elif state.background == 'null':
            # What the level draws behind itself, which is kept as one picture for each set of graphics it is drawn in.
            modes = legacy.call_arguments(state.background_scroll)
            self.placed.append(Placed(BACKDROP, 'Backdrop', {
                'Picture': quoted(f'backdrops/{{graphics}}/level_{snake(state.level)}.png'),
                'ScrollShare': f'Vector2({share(modes[0])}, {share(modes[1])})'}, at=(0, 0), enemy=False))

        if state.main in ESCAPES:
            self.placed.append(sequence('EscapeRoom.cs', 'Escape', {
                'StartsWhen': ctx.always(), **ESCAPES[state.main], 'ShakeStrength': str(ESCAPE_SHAKES[state.setup]),
                **{name: quoted(value) for name, value in ESCAPE_BLASTS.items()}}))
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
        if state.main == 'DachoraShaftCamera':
            # Once the screen over the top of the shaft is open to the view, the screen beside it is closed.
            shaft = sequence('brinstar/DachoraShaftCamera.cs', 'ShaftCamera', {'StartsWhen': ctx.always()})
            self.placed.append(shaft)
            self.links.append((shaft, 'ShaftTop', named['Screen4x1']))
            self.links.append((shaft, 'Beside', named['Screen3x1']))
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
            # The haze is one colour while Ridley is on the station and another once he is gone.
            look = 'effects/ceres_haze_escape.json' if ridley_gone else 'effects/ceres_haze.json'
            self.placed.append(Placed(OVERLAY, 'Haze', {
                'EffectName': quoted('ceres_haze'), 'LookFile': quoted(look)}, at=(0, 0), enemy=False))
        if state.main == 'SpawnCeresPreElevatorHallFallingDebris':
            self.placed.append(sequence('ceres/CeresDebris.cs', 'Debris', {
                'StartsWhen': ctx.fact('ceres_self_destruct_started'),
                'LightPiece': quoted('effects/ceres_debris_light.json'), 'DarkPiece': quoted('effects/ceres_debris_dark.json')}))
        if state.main == 'CeresRidleyGetaway':
            floor = Placed(TURNING_FLOOR, 'Floor', {'Picture': quoted('backdrops/{graphics}/ceres_ridley_floor.png')}, at=(0, 0), enemy=False)
            getaway = sequence('ceres/CeresGetaway.cs', 'Getaway', {
                # The capsule tumbles and his wings beat through the old game's own pictures, in its order.
                'CapsulePictures': strings([floor_picture(name) for name in ('baby_vertical', 'baby_round', 'baby_horizontal', 'baby_round')]),
                'WingPictures': strings([floor_picture(name) for name in ('wings_raised', 'wings_lowered')]),
                'HeadingPicture': quoted('text/emergency.png'), 'Message': quoted('ceres_self_destruct_warning'),
                'Subtitle': quoted('ceres_self_destruct_subtitle'), 'LetterSound': quoted('typing')})
            self.placed += [floor, getaway]
            self.links.append((getaway, 'Floor', floor))
            self.links.append((getaway, 'ToldWhenCountdownBegins', [shaking]))
            if 'Ridley' in named:
                self.links.append((named['Ridley'], 'ToldWhenGone', [getaway]))
        if state.main == 'CeresElevatorShaft':
            floor = Placed(TURNING_FLOOR, 'Floor', {'Picture': quoted('backdrops/{graphics}/ceres_elevator_shaft.png')}, at=(0, 0), enemy=False,
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
                self.placed.append(sequence('ceres/CeresArrival.cs', 'Arrival', {
                    'StartsAtEntry': quoted('AtCeresElevator'),
                    'PadPictures': strings([floor_picture('elevator_light'), floor_picture('elevator_dark')]),
                    'PlatformPicture': quoted(floor_picture('elevator_dark'))},
                                            children=[('Pad', VISUAL, {}), ('Platform', VISUAL, {})]))

    def statue_hall(self, ctx, named: dict[str, Placed]) -> None:
        # The statue is a plain thing the hall's sequence moves and shows: its base, a visual for each guardian, and a
        # grey one over each. It stands at the corner of the room's backdrop, which is the picture the statue is part
        # of and sinks with it; the hall's data says where each picture lies from there.
        statue = Placed('', 'Statue', {}, at=(0, 0), enemy=False,
                        children=[('Base', VISUAL, {})] + [(guardian + grey, VISUAL, {}) for guardian in GUARDIANS for grey in ('', 'Grey')])
        hall = sequence('crateria/TourianStatueHall.cs', 'StatueHall', {
            'DataFile': quoted('rooms/tourian_statue_hall.json'), 'StartsWhen': ctx.always(), 'ClearPatch': quoted('floor_clear')})
        self.placed += [statue, hall]
        self.links.append((hall, 'Statue', statue))
        self.links.append((hall, 'Backdrop', next(placed for placed in self.placed if placed.name == 'Backdrop')))
        if 'Screen0x1' in named:
            self.links.append((hall, 'LowerView', named['Screen0x1']))
