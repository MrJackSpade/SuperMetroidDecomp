"""How each room object the old code places becomes something placed in a Godot room.

The old code places its room objects on blocks. Each becomes a scene placed where its block's top-left corner is; the
scene's data says where its parts lie from there. An item is the exception: an item stands on its middle.

A door's cap is not always in the old code's lists. A blue cap is part of the old level's blocks, and the lists name
only the caps that are something else: coloured, grey, or guarded by an eye. Here every cap is a barrier, so a room
gets a blue cap for each door that closes behind Samus, unless its list puts another cap on the same block.
"""
from __future__ import annotations

import re

import legacy
from enemies import LEFT, RIGHT, Placed, quoted, snake, truth

BLOCK = 16
# Every place an item lies, by the name it is remembered taken under: the generator lists them for the game.
ITEM_LOCATIONS: set[str] = set()

O = 'res://game/objects/'
B = 'res://game/bosses/'
E = 'res://game/enemies/'

# What an open barrier does, for a switch: it is shut again at once.
COMES_BACK_AT_ONCE = '3'

# How a barrier that was opened and remembered is set up afterwards: as a plain one any hit opens.
REMEMBERED_PLAIN = '1'

# Each station the old code's access rules name: its scene, and how far to the left and right of its block its
# arms are, in blocks. Samus uses it by running against an arm.
STATIONS = {'Map': ('map_station', -2, 1), 'Energy': ('energy_station', -1, 1), 'Missile': ('missile_station', -1, 1)}

# What the old code places that is not a thing here: storage it kept for a room, and platforms that are part of the
# room's blocks in the new layouts.
NOT_THINGS = {'EscapeRoomHeldPlaceSetup', 'WreckedShipAtticHeldPlaceSetup', 'ElevatorPlatformSetup', 'MotherBrainEscapeRoomGateSetup'}


def corner(block: tuple[int, int]) -> tuple[int, int]:
    return (block[0] * BLOCK, block[1] * BLOCK)


def named(setup: str, family: str) -> str:
    """The name a setup gives after `family.`: `LockedDoorDefinitions.ParlorRedDoorFacingLeft` gives the last part."""
    return re.search(family + r'\.(\w+)', setup).group(1)


def remembered(setup: str, stays_open: bool = False) -> dict[str, str]:
    """What a lock that stays open says of its barrier: the name it is remembered by, and that it is plain from then on.
    The old code's lists say which locks stay open, unless `stays_open` says this one does whatever they say."""
    if not stays_open and 'DoorOpeningPersistence.Persistent' not in setup:
        return {}
    return {'RememberedAs': quoted(snake(named(setup, 'LockedDoorDefinitions'))), 'Remembered': REMEMBERED_PLAIN}


def cap(block: tuple[int, int], colour: str, way: str, properties: dict[str, str] | None = None) -> Placed:
    return Placed(O + 'door_cap/door_cap.tscn', colour.capitalize() + 'Cap',
                  {'DataFile': quoted(f'objects/door_cap_{colour}_{way.lower()}.json'), **(properties or {})},
                  at=corner(block), enemy=False)


class Objects:
    """The room objects of one version of a room."""

    def __init__(self, ctx, entries: list[str], doors_in: list[legacy.Door]) -> None:
        self.ctx = ctx
        self.placed: list[Placed] = []
        # Each thing that tells another, with which property lists whom it tells.
        self.tells: list[tuple[Placed, str, list[Placed]]] = []
        # The item whose pickup sets the room's liquid rising, where the room has one.
        self.raises_liquid: Placed | None = None
        self.caps: dict[tuple[int, int], list[Placed]] = {}
        for door in doors_in:
            if door.closing == 'BlueDoorCloses' and door.object_block is not None:
                self.caps[door.object_block] = [cap(door.object_block, 'blue', door.direction)]
        gates: dict[tuple[int, int], Placed] = {}
        placements = [legacy.object_placement(entry) for entry in entries]
        for placement in placements:
            if placement is not None and placement.setup_type == 'DownwardGateSetup':
                gates[placement.block] = Placed(O + 'gate/gate.tscn', 'Gate', {}, at=corner(placement.block), enemy=False)
                self.placed.append(gates[placement.block])
        for placement in placements:
            # A list spread in from elsewhere is the room's scroll triggers, which are placed with its camera zones.
            if placement is None or placement.setup_type in NOT_THINGS or placement.setup_type in ('DownwardGateSetup', 'ScrollTriggerSetup'):
                continue
            rule = getattr(self, snake(placement.setup_type), None)
            if rule is None:
                ctx.unplaced[placement.setup_type] = ctx.unplaced.get(placement.setup_type, 0) + 1
                continue
            rule(placement.block, placement.setup, gates)
        for placed in self.caps.values():
            self.placed.extend(placed)

    def place_cap(self, block, things: list[Placed]) -> None:
        """Places a cap the room's own list names, with whatever stands with it.

        It takes its turn where the list has it among the room's objects, as the old room updates them in the order
        it places them: a cap listed before an item has had its turn when the item is taken. It stands in place of
        the blue cap its door would otherwise close with, which like every such cap comes after the room's own.
        """
        self.caps.pop(block, None)
        self.placed.extend(things)

    def colored_door_setup(self, block, setup, gates) -> None:
        colour = named(setup, 'ColoredDoorColor').lower()
        self.place_cap(block, [cap(block, colour, named(setup, 'ColoredDoorOrientation'), remembered(setup))])

    def grey_door_setup(self, block, setup, gates) -> None:
        ctx = self.ctx
        condition = named(setup, 'GreyDoorCondition')
        extra: list[Placed] = []
        stays_open = False
        if condition in ('AreaBossDefeated', 'AreaMiniBossDefeated', 'AreaTorizoDefeated'):
            opens = ctx.fact(ctx.boss(condition[:-len('Defeated')]) + '_defeated')
        elif condition == 'EnemyDeathQuota':
            opens = ctx.killed(ctx.quota)
            # Killing a room's guard also wakes Zebes.
            extra.append(Placed('res://game/rooms/RoomWatch.cs', 'ZebesWakes', {
                'StartsWhen': ctx.always(), 'Until': opens, 'Records': 'PackedStringArray("zebes_awake")'},
                at=None, enemy=False, kind='Node'))
        elif condition == 'Never':
            opens = ctx.never()
        elif condition == 'TourianStatueFinished':
            opens = ctx.fact('tourian_unlocked')
            # The cartridge keeps this door's bit once it has been shot open, as it does a coloured door's, and it is
            # a blue door on every visit after; the old code's lists shut it again each time the room is set up.
            stays_open = True
        elif condition == 'CrittersEscaped':
            opens = ctx.fact('animals_escaped')
        else:
            raise ValueError(f'A grey door opens on a condition this does not know: {condition}')
        held = {'HeldShutBy': opens, **remembered(setup, stays_open)}
        if 'GreyDoorClosing.AfterBombs' in setup:
            # This one does not shut behind Samus until she has the bombs: it stands open till then, and then shuts
            # and is held shut as any other is.
            held['HeldOpenBy'] = ctx.none_of([ctx.owns('bombs')])
        self.place_cap(block, [cap(block, 'grey', named(setup, 'ColoredDoorOrientation'), held)] + extra)

    def eye_door_part_setup(self, block, setup, gates) -> None:
        # The growth round an eye door is the eye's own; its topmost part lies on the cap's block.
        pass

    def eye_door_eye_setup(self, block, setup, gates) -> None:
        ctx = self.ctx
        way = named(setup, 'EyeDoorOrientation')
        destroyed = snake(named(setup, 'LockedDoorDefinitions'))
        top = (block[0], block[1] - 1)
        # The cap is held shut for as long as the eye lives; with the eye gone it is an ordinary door.
        self.place_cap(top, [
            cap(top, 'eye', way, {'HeldShutBy': ctx.fact(destroyed)}),
            Placed(E + 'eye_door/eye_door_eye.tscn', 'EyeDoorEye', {
                'StartsFacing': LEFT if way == 'Left' else RIGHT, 'DestroyedFact': quoted(destroyed), 'AlwaysActive': 'true'},
                at=corner(block), only_when=ctx.none_of([ctx.fact(destroyed)])),
        ])

    def collectible_setup(self, block, setup, gates) -> None:
        ctx = self.ctx
        kind = named(setup, 'InWorldCollectibleKind')
        name = snake(named(setup, 'ItemLocationDefinitions'))
        ITEM_LOCATIONS.add(name)
        item = Placed(O + 'item/item.tscn', kind, {
            'TypeFile': quoted(f'items/{snake(kind)}.json'), 'RememberedAs': quoted(name)},
            at=(block[0] * BLOCK + BLOCK // 2, block[1] * BLOCK + BLOCK // 2), enemy=False)
        self.placed.append(item)
        # The Speed Booster in its Chozo orb sets its room's lava rising as its message closes
        # (the old CollectibleObjects.Acquire: LavaRisingPlacements).
        if kind == 'SpeedBooster' and 'CollectiblePresentation.ChozoOrb' in setup:
            self.raises_liquid = item
        # One held in a shell has the shell round it for as long as it has not been taken. One hidden in the wall is
        # behind a block of the room's layout.
        if 'CollectiblePresentation.ChozoOrb' in setup:
            self.placed.append(Placed(O + 'item_shell/item_shell.tscn', 'ItemShell', {}, at=corner(block), enemy=False,
                                      only_when=ctx.none_of([ctx.fact(name)])))

    def save_station_setup(self, block, setup, gates) -> None:
        station = named(setup, 'LoadStationDefinitions')
        # Samus stands on its pad, which is two blocks wide, with her middle within half a block of the pad's. The
        # station is that block-wide strip, three blocks up from the pad.
        self.placed.append(Placed(O + 'station/save_station.tscn', 'SaveStation', {'ResumesAt': quoted('At' + station)},
                                  at=(block[0] * BLOCK + BLOCK // 2, (block[1] - 3) * BLOCK), enemy=False))

    def access_station_setup(self, block, setup, gates) -> None:
        scene, left, right = STATIONS[named(setup, 'StationAccessRuleDefinitions')]
        # The station is the row of blocks from its left arm to its right arm: its scene is that wide.
        self.placed.append(Placed(O + f'station/{scene}.tscn', scene.title().replace('_', ''), {},
                                  at=((block[0] + left) * BLOCK, block[1] * BLOCK), enemy=False))

    def downward_gate_shot_block_setup(self, block, setup, gates) -> None:
        colour = named(setup, 'DownwardGateTriggerColor').lower()
        way = named(setup, 'HorizontalDirection').lower()
        # A switch never opens: each hit that would open it tells its gate to open if it is shut and shut if it is open.
        switch = Placed(O + 'gate/gate_switch.tscn', 'GateSwitch', {
            'DataFile': quoted(f'objects/gate_switch_{colour}_{way}.json'),
            'ComesBack': COMES_BACK_AT_ONCE, 'TellsWhat': quoted('toggle')}, at=corner(block), enemy=False)
        self.placed.append(switch)
        self.tells.append((switch, 'TellsWhenOpened', [gates[block]]))

    def bomb_torizo_hand_setup(self, block, setup, gates) -> None:
        self.placed.append(Placed(B + 'torizo/bomb_torizo_hand.tscn', 'BombTorizoHand', {}, at=corner(block), enemy=False))

    def noob_tube_setup(self, block, setup, gates) -> None:
        self.placed.append(Placed(O + 'glass_tube/glass_tube.tscn', 'GlassTube', {}, at=corner(block), enemy=False))

    def draygon_wall_cannon_setup(self, block, setup, gates) -> None:
        which = named(setup, 'DraygonWallCannon')
        self.placed.append(Placed(B + 'draygon/draygon_cannon.tscn', which + 'Cannon', {
            'Which': quoted(which),
            'StartsFacing': LEFT if named(setup, 'DraygonCannonOrientation') == 'Left' else RIGHT,
            'StartsBroken': truth(named(setup, 'DraygonCannonStart') == 'Destroyed'), 'AlwaysActive': 'true'}, at=corner(block)))

    def mother_brain_glass_setup(self, block, setup, gates) -> None:
        # A room where the glass has been broken, or Mother Brain beaten, begins without it: the room's version has
        # the patch that shows the tank so.
        ctx = self.ctx
        self.placed.append(Placed(B + 'mother_brain/mother_brain_glass.tscn', 'MotherBrainGlass', {
            'AlwaysActive': 'true', 'RecordsFact': quoted('mother_brain_glass_destroyed')}, at=corner(block),
            only_when=ctx.none_of([ctx.fact('mother_brain_glass_destroyed'), ctx.fact('mother_brain_defeated')])))

    def speed_booster_escape_setup(self, block, setup, gates) -> None:
        # The lava's chase begins as the hall is entered from the Speed Booster's room, until it has been outrun once.
        ctx = self.ctx
        self.placed.append(Placed('res://game/rooms/norfair/SpeedBoosterLavaquake.cs', 'Lavaquake', {
            'StartsAtEntry': quoted('ToSpeedBooster'), 'RememberedAs': quoted('outran_lava_quake')},
            at=None, enemy=False, kind='Node', only_when=ctx.none_of([ctx.fact('outran_lava_quake')])))

    def metroid_room_clearance_setup(self, block, setup, gates) -> None:
        ctx = self.ctx
        self.placed.append(Placed('res://game/rooms/RoomWatch.cs', 'Cleared', {
            'StartsWhen': ctx.always(), 'Until': ctx.killed(ctx.quota),
            'Records': f'PackedStringArray("{snake(named(setup, "StoryEvent"))}")'}, at=None, enemy=False, kind='Node'))
