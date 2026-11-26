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

# Maridia's tube: the ride that starts as Samus comes in by each of its room's exits, with how far it moves her on
# its first update and how much that gains each update, in pixels; negative is upward.
TUBE_RIDES = (('RideUp', 'ToOasis', -1.0, -0.125), ('RideDown', 'ToPlasmaSpark', 1.0, 0.125))

# The old code's backgrounds that are no picture: none, the layer a boss's body is drawn on, and that layer cleared.
NOT_PICTURES = {'null', 'BossBody', 'Cleared', 'KraidAlive'}

# The guardians of the Tourian statue, each with a visual and a grey one over it, in the order they are drawn:
# Kraid and Draygon are part of the backdrop's picture and lie under the rest, and Ridley's statue is drawn over
# Phantoon's, as the cartridge draws them.
GUARDIANS = ['Kraid', 'Draygon', 'Phantoon', 'Ridley']


def sequence(script: str, name: str, properties: dict[str, str], **more) -> Placed:
    return Placed(R + script, name, properties, at=None, enemy=False, kind='Node', **more)


def share(scroll: str) -> str:
    """How much of the room's scrolling a background follows along one axis: all of it, a part, or none."""
    if 'ParallaxRate' in scroll:
        top, bottom = legacy.numbers(scroll[scroll.index('ParallaxRate'):])[:2]
        return repr(top / bottom)
    return '0' if 'Fixed' in scroll else '1'


# Where on a turning floor the old game lays the pictures it shows there: the elevator's pad, the capsule Ridley
# carries, and his wings.
FLOOR_PLATFORM_AT = (112, 96)
FLOOR_CAPSULE_AT = (32, 80)
FLOOR_WINGS_AT = (0, 0)


def floor_picture(name: str) -> str:
    """One of the pictures shown on a turning floor, which is kept once for each set of graphics it is coloured by."""
    return f'backdrops/{{graphics}}/ceres_{name}.png'


def strings(values: list[str]) -> str:
    """A list of text as a scene writes one."""
    return 'Array[String]([' + ', '.join(quoted(value) for value in values) + '])'


class Sequences:
    """The sequences and backdrops of one version of a room."""

    def __init__(self, room: legacy.Room, state: legacy.State, chosen_on: str, ctx, named: dict[str, Placed], landing: tuple[str, str],
                 things: list[Placed]) -> None:
        """
        `chosen_on` is the condition the version is chosen on, empty for the one with none; `named` holds what the
        version has placed so far, by name, the first of each name; `landing` is the scene and entry point Ceres's
        elevator leads to; `things` is everything the version has placed so far.
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

        if room.area == 'Brinstar':
            # The plants that seize Samus are blocks of the layout: one watcher finds and plays whichever the room has.
            self.placed.append(sequence('brinstar/SamusEaters.cs', 'Plants', {
                'StartsWhen': ctx.always(), 'DataFile': quoted('objects/samus_eater.json')}))
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
            self.ceres(state, 'BossDead' in chosen_on, ctx, named, landing, things)
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
            # The tube takes whoever comes in at one end out at the other: a ride for each end, started by coming
            # in there.
            for name, entry, speed, gain in TUBE_RIDES:
                self.placed.append(sequence('maridia/TubeRide.cs', name, {
                    'StartsAtEntry': quoted(entry), 'StartSpeed': repr(speed), 'Gain': repr(gain)}))

    def ceres(self, state: legacy.State, ridley_gone: bool, ctx, named: dict[str, Placed], landing: tuple[str, str],
              things: list[Placed]) -> None:
        shaking = sequence('ceres/CeresShaking.cs', 'Shaking', {'StartsWhen': ctx.always()})
        self.placed.append(shaking)
        doors = [thing for thing in things if thing.name in ('CeresDoor', 'RidleyRoomDoor', 'ElevatorDoor')]
        if doors:
            # A door takes its other colours as its room is set up once Ridley has left his: the old door chose
            # between its two looks as it was made, by whether he had yet escaped.
            colours = sequence('ceres/CeresDoorColours.cs', 'DoorColours', {
                'StartsWhen': ctx.fact('ceres_ridley_escaped'), 'ColoursFile': quoted('objects/ceres_doors_escaping.json')})
            self.placed.append(colours)
            self.links.append((colours, 'Doors', doors))
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
            # The floor he flies off over is put away until he does: the room rests on its own picture.
            # Nothing shows beyond its edges: the cartridge's floor for his flight does not come round again, as
            # the shaft's does.
            floor = Placed(TURNING_FLOOR, 'Floor', {'Picture': quoted('backdrops/{graphics}/ceres_ridley_floor.png'), 'Repeats': 'false', 'visible': 'false'}, at=(0, 0), enemy=False,
                           children=[('Capsule', VISUAL, {'position': f'Vector2{FLOOR_CAPSULE_AT}'}),
                                     ('Wings', VISUAL, {'position': f'Vector2{FLOOR_WINGS_AT}'})])
            getaway = sequence('ceres/CeresGetaway.cs', 'Getaway', {
                # The capsule tumbles and his wings beat through the old game's own pictures, in its order.
                'CapsulePictures': strings([floor_picture(name) for name in ('baby_vertical', 'baby_round', 'baby_horizontal', 'baby_round')]),
                'WingPictures': strings([floor_picture(name) for name in ('wings_raised', 'wings_lowered')]),
                'HeadingPicture': quoted('text/emergency.png'), 'Message': quoted('ceres_self_destruct_warning'),
                'Subtitle': quoted('ceres_self_destruct_subtitle'), 'LetterSound': quoted('room_typewriter:low')})
            self.placed += [floor, getaway]
            self.links.append((getaway, 'Floor', floor))
            self.links.append((getaway, 'ToldWhenCountdownBegins', [shaking]))
            if 'Ridley' in named:
                self.links.append((named['Ridley'], 'ToldWhenGone', [getaway]))
        if state.main == 'CeresElevatorShaft':
            # The cartridge shows a room's picture from the row below the view's, and draws what stands in it where
            # its place says: the shaft, which is all the room's picture, lies a row above the room's own rows, so
            # the black that hides the landing pad until the platform lands covers its top row.
            floor = Placed(TURNING_FLOOR, 'Floor', {'Picture': quoted('backdrops/{graphics}/ceres_elevator_shaft.png'), 'LiesAt': 'Vector2i(0, -1)'}, at=(0, 0), enemy=False,
                           children=[('Platform', VISUAL, {'position': f'Vector2{FLOOR_PLATFORM_AT}'})])
            self.placed.append(floor)
            if 'ElevatorDoor' in named:
                self.links.append((named['ElevatorDoor'], 'Floor', floor))
            if ridley_gone:
                shaft = sequence('ceres/CeresShaft.cs', 'Shaft', {
                    'StartsWhen': ctx.always(), 'LandingRoom': quoted(landing[0]), 'LandingEntry': quoted(landing[1])})
                self.placed.append(shaft)
                self.links.append((shaft, 'Floor', floor))
                # The steam tilts with Samus; the room's doors do not.
                steam = [thing for thing in things if thing.name == 'Steam']
                if steam:
                    self.links.append((shaft, 'Steam', steam))
            else:
                self.placed.append(sequence('ceres/CeresArrival.cs', 'Arrival', {
                    'StartsAtEntry': quoted('AtCeresElevator'),
                    'PadTimeline': quoted('objects/ceres_arrival_pad.json'),
                    'PlatformTimeline': quoted('objects/ceres_arrival_platform.json')},
                                            children=[('Pad', VISUAL, {}), ('Platform', VISUAL, {})]))

    def statue_hall(self, ctx, named: dict[str, Placed]) -> None:
        # The statue is a plain thing the hall's sequence moves and shows: its base, a visual for each guardian, and a
        # grey one over each. It stands at the corner of the room's backdrop, which is the picture the statue is part
        # of and sinks with it; the hall's data says where each picture lies from there. The base is drawn over the
        # guardians, as the cartridge draws it.
        statue = Placed('', 'Statue', {}, at=(0, 0), enemy=False,
                        children=[(guardian + grey, VISUAL, {}) for guardian in GUARDIANS for grey in ('', 'Grey')] + [('Base', VISUAL, {})])
        hall = sequence('crateria/TourianStatueHall.cs', 'StatueHall', {
            'DataFile': quoted('rooms/tourian_statue_hall.json'), 'StartsWhen': ctx.always(), 'ClearPatch': quoted('floor_clear')})
        self.placed += [statue, hall]
        self.links.append((hall, 'Statue', statue))
        self.links.append((hall, 'Backdrop', next(placed for placed in self.placed if placed.name == 'Backdrop')))
        if 'Screen0x1' in named:
            self.links.append((hall, 'LowerView', named['Screen0x1']))
