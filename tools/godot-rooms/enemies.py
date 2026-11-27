"""How each kind of enemy the old code places becomes something placed in a Godot room.

The old code places every part of an enemy as an enemy of its own: a Dragon and its wings, a Ki-Hunter and its
wings, each link of Shaktool. In the new project each of those is one scene, so a rule here answers for a whole
enemy from the one placement that leads it and says nothing for its other parts.

A rule is given a placement, where it comes in its room's list, and the whole list. It gives back what to place, or
None for a part that something else answers for.
"""
from __future__ import annotations

import re
from dataclasses import dataclass, field
from fractions import Fraction
from typing import Callable

import legacy

LEFT, RIGHT = '0', '1'

# The sides a crawler's surface can be on, as the engine numbers them.
BELOW, ON_RIGHT, ABOVE, ON_LEFT = '0', '1', '2', '3'


@dataclass
class Placed:
    """One thing to place in a room."""
    scene: str
    name: str
    properties: dict[str, str] = field(default_factory=dict)
    # Properties that name another placement of the same list, by where it comes in the list.
    refers: dict[str, int] = field(default_factory=dict)
    at: tuple[int, int] | None = None
    # Whether it is an enemy, which takes the settings every enemy has.
    enemy: bool = True
    # For something that is no enemy: whether it is drawn in a look of its room's own, as an enemy can be ('Look').
    takes_look: bool = False
    # A condition, beyond being in one of its room's versions, that it is present under: a Condition resource.
    only_when: str | None = None
    # For something that is a script and no scene: the kind of node it is.
    kind: str = 'Node2D'
    # Nodes placed under it: each one's name, script and settings.
    children: list[tuple[str, str, dict[str, str]]] = field(default_factory=list)
    # For a boss: the name its defeat is recorded under.
    boss: str = ''
    # For something that is there only until a boss is beaten: the name that boss's defeat is recorded under.
    until_beaten: str = ''
    # For a Chozo statue that comes to life: which of the two it is.
    chozo: str = ''


Rule = Callable[[legacy.Placement, int, list[legacy.Placement]], 'Placed | None']
RULES: dict[str, Rule] = {}


def snake(name: str) -> str:
    return re.sub(r'(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])', '_', name).lower()


def quoted(text: str) -> str:
    return '"' + text + '"'


def number(expression: str) -> str:
    """One of the old code's fixed-point numbers, as a plain number."""
    expression = expression.strip()
    if expression.endswith('.Zero'):
        return '0'
    values = legacy.numbers(expression[expression.index('('):])
    if 'Fraction' in expression:
        value = Fraction(values[0], values[1])
        return str(value.numerator) if value.denominator == 1 else repr(value.numerator / value.denominator)
    return str(values[0])


def whole(expression: str) -> str:
    """A whole number written inside a wrapper: `RotationAngle.FromSteps(80)` or `new TrigonometryAngle(14)`."""
    return str(legacy.numbers(expression[expression.index('('):])[0])


def facing(expression: str) -> str:
    return RIGHT if legacy.last_name(expression) == 'Right' else LEFT


def truth(value: bool) -> str:
    return 'true' if value else 'false'


def rule(*kinds: str):
    def register(function: Rule) -> Rule:
        for kind in kinds:
            RULES[kind] = function
        return function
    return register


def plain(scene: str, data: str | None = None) -> Rule:
    """A rule for an enemy its placement tells nothing: its scene, and its data file where the scene serves several kinds."""
    def place(p: legacy.Placement, index: int, listed: list[legacy.Placement]) -> Placed:
        return Placed(scene, p.enemy, {'DataFile': quoted(data)} if data else {})
    return place


def part(p: legacy.Placement, index: int, listed: list[legacy.Placement]) -> None:
    """A part of an enemy that another placement answers for."""
    return None


def beside(listed: list[legacy.Placement], index: int, kind: str) -> legacy.Placement:
    """The placement of `kind` that goes with the one at `index`: the nearest in the list at the same place."""
    at = listed[index].at
    for distance in range(1, len(listed)):
        for other in (index + distance, index - distance):
            if 0 <= other < len(listed) and listed[other].at == at and (listed[other].settings_type == kind or listed[other].enemy == kind):
                return listed[other]
    raise ValueError(f'Nothing of kind {kind} is placed with the {listed[index].enemy} at {at}.')


E = 'res://game/enemies/'
B = 'res://game/bosses/'
O = 'res://game/objects/'

for kind, scene in {
    'Alcoon': 'alcoon/alcoon', 'Beetom': 'beetom/beetom', 'Mochtroid': 'metroid/mochtroid', 'Rio': 'rio/rio',
    'Zoa': 'zoa/zoa', 'Polyp': 'polyp/polyp', 'Oum': 'oum/oum', 'MamaTurtle': 'turtle/mama_turtle',
    'Coven': 'covern/covern', 'CorpseTorizo': 'corpse/dead_torizo',
}.items():
    RULES[kind] = plain(E + scene + '.tscn')

for kind, (scene, data) in {
    'Skree': ('diver/diver', 'skree'), 'Metaree': ('diver/diver', 'metaree'),
    'Mellow': ('fly/fly', 'mellow'), 'Mella': ('fly/fly', 'mella'), 'MaridiaFly': ('fly/fly', 'maridia_fly'),
}.items():
    RULES[kind] = plain(E + scene + '.tscn', f'enemies/{data}.json')

RULES['FakeKraid'] = plain(B + 'kraid/fake_kraid.tscn')

# Parts another placement answers for.
for kind in ('KihunterGreenWings', 'KihunterRedWings', 'KihunterYellowWings', 'KzanBottom', 'EvirProjectile',
             'CrocomireTongue', 'KraidArm', 'KraidFoot', 'PhantoonEye', 'PhantoonMouth', 'PhantoonTentacles',
             'DraygonArms', 'DraygonEye', 'DraygonTail', 'MotherBrainHead', 'ShipBottomEntrance'):
    RULES[kind] = part


def kraid_number(file: str, pattern: str) -> int:
    """One of the old Kraid's numbers, read from its own definitions."""
    found = re.search(pattern, legacy.read('Bosses', 'Kraid', file))
    if found is None:
        raise ValueError(f'The old Kraid file {file} has nothing like {pattern}.')
    return int(found.group(1))


def kraid_lint(part: str) -> Rule:
    """One of the spikes of Kraid's belly: its height on Kraid, how far left it comes out, and when it first does."""
    arena = 'KraidArenaDefinitions.cs'
    emergence = re.search(rf'\[KraidLintPart\.{part}\] = KraidLintEmergence\.(\w+)', legacy.read('Bosses', 'Kraid', arena)).group(1)
    properties = {
        'Height': str(kraid_number(arena, rf'\[KraidLintPart\.{part}\] = (-?\d+),')),
        'EmergeOffset': str(kraid_number(arena, rf'\[KraidLintEmergence\.{emergence}\] = (-?\d+),')),
        'InitialDelay': str(kraid_number('KraidLintInitializationDefinitions.cs', rf'\[KraidLintPart\.{part}\] = (\d+),')),
        # A touch of Kraid's arm sets off the lowest.
        'FiresWhenArmTouched': truth(part == 'Bottom'),
    }

    def place(p: legacy.Placement, index: int, listed: list[legacy.Placement]) -> Placed:
        return Placed(B + 'kraid/kraid_lint.tscn', p.enemy, dict(properties), until_beaten='kraid')
    return place


def kraid_nail(order: int, data: str | None) -> Rule:
    """One of Kraid's two nails: each waits one launch delay longer than the one before it."""
    delay = kraid_number('KraidArenaDefinitions.cs', r'NailLaunchDelayUpdates = (\d+);')
    properties = {'FirstLaunchDelay': str(delay * order)}
    if data:
        properties['DataFile'] = quoted(data)

    def place(p: legacy.Placement, index: int, listed: list[legacy.Placement]) -> Placed:
        return Placed(B + 'kraid/kraid_nail.tscn', p.enemy, dict(properties), until_beaten='kraid')
    return place


def crawl_start(surface: str, block_side: str) -> dict[str, str]:
    """
    How a crawler starts: the surface it is on, and the way it presses across and down, which the old code takes
    from "the side of its block it holds" whatever surface it is on.
    """
    side = {'OnFloor': BELOW, 'OnCeiling': ABOVE, 'OnLeftWall': ON_LEFT, 'OnRightWall': ON_RIGHT}[legacy.last_name(surface)]
    held = legacy.last_name(block_side)
    return {'StartsOn': side, 'PressesLeft': truth(held == 'OnLeftWall'), 'PressesUp': truth(held == 'OnCeiling')}


@rule('Zoomer', 'ArmoredZoomer', 'OrangeZoomer', 'Zeela', 'Sova', 'Sciser', 'Zero', 'Viola')
def crawler(p, index, listed):
    s = p.settings
    properties = {'DataFile': quoted(f'enemies/{snake(p.enemy)}.json')}
    # A Viola starts on the surface of the side it holds.
    properties.update(crawl_start(s.get('Orientation', s['BlockSide']), s['BlockSide']))
    properties['CrawlSpeed'] = '0' if s['CrawlSpeed'] == 'null' else number(s['CrawlSpeed'])
    return Placed(E + 'crawler/crawler.tscn', p.enemy, properties)


@rule('Yard')
def yard(p, index, listed):
    side, clockwise = {
        'OnLeftWallMovingUp': (ON_LEFT, False), 'OnLeftWallMovingDown': (ON_LEFT, True),
        'OnRightWallMovingUp': (ON_RIGHT, True), 'OnRightWallMovingDown': (ON_RIGHT, False),
        'OnCeilingMovingLeft': (ABOVE, True), 'OnCeilingMovingRight': (ABOVE, False),
        'OnFloorMovingLeft': (BELOW, False), 'OnFloorMovingRight': (BELOW, True),
    }[legacy.last_name(p.settings['Direction'])]
    return Placed(E + 'yard/yard.tscn', 'Yard', {
        'StartsOn': side, 'Clockwise': truth(clockwise), 'CrawlSpeed': number(p.settings['CrawlSpeed'])})


@rule('Atomic')
def atomic(p, index, listed):
    return Placed(E + 'atomic/atomic.tscn', 'Atomic', {
        'Spin': quoted(snake(legacy.last_name(p.settings['Spin']))), 'Speed': number(p.settings['Speed'])})


@rule('BabyTurtle')
def baby_turtle(p, index, listed):
    mother = next(at for at, other in enumerate(listed) if other.enemy == 'MamaTurtle')
    return Placed(E + 'turtle/baby_turtle.tscn', 'BabyTurtle', {'StartsFacing': facing(p.settings['Heading'])}, {'Mother': mother})


@rule('Boulder')
def boulder(p, index, listed):
    s = p.settings
    drop = legacy.numbers(s['Start'])
    return Placed(E + 'boulder/boulder.tscn', 'Boulder', {
        'TriggerHeight': s['TriggerHeight'], 'TriggerReach': s['TriggerReach'],
        'DropHeight': str(drop[0]) if 'FallsFrom' in s['Start'] else '0',
        'Rolls': facing(s['Roll']), 'BreaksOnLanding': truth(legacy.last_name(s['FirstImpact']) == 'Breaks')})


@rule('Boyon')
def boyon(p, index, listed):
    s = p.settings
    return Placed(E + 'boyon/boyon.tscn', 'Boyon', {
        'SpeedMultiplier': s['SpeedMultiplier'], 'JumpHeight': s['JumpHeight'], 'Reach': s['TriggerDistance']})


@rule('Bull')
def bull(p, index, listed):
    intervals = legacy.numbers(p.settings['Intervals'])
    return Placed(E + 'bull/bull.tscn', 'Bull', {
        'AccelerationInterval': str(intervals[0]), 'DecelerationInterval': str(intervals[1]),
        'MaximumSpeed': number(p.settings['MaximumSpeed'])})


@rule('Cacatac')
def cacatac(p, index, listed):
    s = p.settings
    return Placed(E + 'cacatac/cacatac.tscn', 'Cacatac', {
        'StartsFacing': facing(s['Direction']), 'StartsStopped': truth(legacy.last_name(s['Start']) == 'Stopped'),
        'OnCeiling': truth(legacy.last_name(s['Mounting']) == 'Ceiling'),
        'TravelDistance': s['TravelDistance'], 'Speed': number(s['Speed'])})


@rule('Choot')
def choot(p, index, listed):
    s = p.settings
    return Placed(E + 'choot/choot.tscn', 'Choot', {
        'FallingPath': quoted(snake(legacy.last_name(s['FallingPath']))),
        'PatternLoops': s['PatternLoops'], 'JumpDelay': s['JumpDelay']})


@rule('CorpseZoomer', 'CorpseRipper', 'CorpseSkree')
def corpse(p, index, listed):
    # Each way a husk can lie has a data file of its own.
    return Placed(E + 'corpse/corpse.tscn', p.enemy, {
        'DataFile': quoted(f'enemies/corpse_{snake(legacy.last_name(p.settings["Corpse"]))}.json')})


@rule('Dessgeega', 'DessgeegaLarge', 'Sidehopper', 'SidehopperLarge', 'SidehopperTourian')
def hopper(p, index, listed):
    return Placed(E + 'hopper/hopper.tscn', p.enemy, {
        'DataFile': quoted(f'enemies/{snake(p.enemy)}.json'),
        'OnCeiling': truth(legacy.last_name(p.settings['Surface']) == 'Ceiling')})


@rule('Dragon')
def dragon(p, index, listed):
    if legacy.last_name(p.settings['Part']) != 'Body':
        return None
    # The cartridge carries a hit on its wing, the part listed right after its body, onto whatever is listed after
    # that: the Dragon is told which of the room's enemies that is, where the list goes on.
    after_wing = index + 2
    return Placed(E + 'dragon/dragon.tscn', 'Dragon', refers={'ListedAfterWing': after_wing} if after_wing < len(listed) else {})


@rule('Evir')
def evir(p, index, listed):
    s = p.settings
    if legacy.last_name(s['Part']) != 'Body':
        return None
    # Its first bob lasts a number the cartridge reads from another of the room's enemies: half the turning time
    # of the second listed, for one bobbing at half a pixel, and one update or none from the third listed, for one
    # at three quarters. An enemy listed after it has not been set up yet, and gives nothing.
    speed = number(s['Speed'])
    lender = {'0.5': 1, '0.75': 2}[speed]
    if lender > index:
        first = 0
    elif speed == '0.5':
        first = int(listed[lender].settings['DirectionChangePeriod']) // 2
    else:
        if listed[lender].enemy != 'EvirProjectile':
            raise ValueError('An Evir reads its first bob from something that is not a spine ball.')
        first = 1
    return Placed(E + 'evir/evir.tscn', 'Evir', {
        'BobSpeed': speed, 'BobUpdates': s['DirectionChangePeriod'], 'FirstBobUpdates': str(first)})


@rule('FaceBlock')
def face_block(p, index, listed):
    return Placed(E + 'face_block/face_block.tscn', 'FaceBlock', {
        'TriggerDistance': p.settings['TriggerDistance'], 'StartsFacing': facing(p.settings['Facing'])})


@rule('Fireflea')
def fireflea(p, index, listed):
    flight = p.settings['Flight']
    properties = {'Radius': p.settings['Radius'], 'Darkens': 'NodePath("../Darkness")'}
    if 'Circling' in flight:
        turn = legacy.numbers(flight[flight.index('AngularSpeed'):])
        value = Fraction(turn[0], turn[1]) if 'Fraction' in flight else Fraction(turn[0])
        properties.update({'Circles': 'true', 'TurnPerUpdate': repr(float(value)) if value.denominator != 1 else str(value.numerator)})
    else:
        arguments = legacy.call_arguments(flight)
        properties.update({'Circles': 'false', 'Speed': number(arguments[0]),
                           'StartsUpward': truth(legacy.last_name(arguments[1]) == 'Up')})
    return Placed(E + 'fireflea/fireflea.tscn', 'Fireflea', properties)


@rule('Fune')
def fune(p, index, listed):
    s = p.settings
    return Placed(E + 'fune/fune.tscn', 'Fune', {
        'StartsFacing': facing(s['Facing']), 'CooldownUpdates': s['CooldownTime'], 'LaunchSpeed': number(s['LaunchSpeed'])})


@rule('Namihe')
def namihe(p, index, listed):
    s = p.settings
    return Placed(E + 'fune/fune.tscn', 'Namihe', {
        'DataFile': quoted('enemies/namihe.json'), 'StartsFacing': facing(s['Facing']),
        'CooldownUpdates': '0', 'LevelDistance': s['YProximity'], 'LaunchSpeed': number(s['LaunchSpeed'])})


# The four that follow a Gamet's leader, in the order a room lists them: how far each spreads below where the five
# stop rising, and how long after that it flies.
GAMET_FOLLOWERS = [(-16, 96), (-32, 88), (16, 112), (32, 120)]


@rule('Gamet')
def gamet(p, index, listed):
    if p.settings_type == 'NorfairPipeBugLeaderSpawnSettings':
        return Placed(E + 'pipe_bug/gamet.tscn', 'Gamet', {
            'TriggerWidth': p.settings['TriggerWidth'], 'Speed': number(p.settings['Speed']),
            'SpreadDown': '0', 'DepartureWait': '104'})
    leader = max(at for at in range(index) if listed[at].settings_type == 'NorfairPipeBugLeaderSpawnSettings')
    spread, wait = GAMET_FOLLOWERS[index - leader - 1]
    return Placed(E + 'pipe_bug/gamet.tscn', 'Gamet', {
        'Speed': number(p.settings['Speed']), 'SpreadDown': str(spread), 'DepartureWait': str(wait)}, {'Leader': leader})


@rule('Geega')
def geega(p, index, listed):
    return Placed(E + 'pipe_bug/geega.tscn', 'Geega', {
        'StartsFacing': facing(p.settings['Facing']), 'Speed': number(p.settings['Speed'])})


@rule('Zeb', 'Zebbo')
def pipe_bug(p, index, listed):
    return Placed(E + 'pipe_bug/pipe_bug.tscn', p.enemy, {'DataFile': quoted(f'enemies/{snake(p.enemy)}.json')})


@rule('Geruta', 'Holtz')
def swooper(p, index, listed):
    if legacy.last_name(p.settings['Role']) != 'Leader':
        # Its flames, listed right after it: a thing of the room in their own right, which takes its turn among the
        # enemies after it and only within their window, by where the flames themselves stand. Their pictures are
        # their swooper's.
        return Placed(E + 'swooper/swooper_flames.tscn', p.enemy + 'Flames', only_in_view(p), enemy=False, refers={'Leader': index - 1})
    return Placed(E + 'swooper/swooper.tscn', p.enemy, {'DataFile': quoted(f'enemies/{snake(p.enemy)}.json')})


@rule('Squeept')
def squeept(p, index, listed):
    if legacy.last_name(p.settings['Role']) != 'Leader':
        return None
    return Placed(E + 'squeept/squeept.tscn', 'Squeept')


@rule('Ripper', 'JetRipper')
def ripper(p, index, listed):
    return Placed(E + 'ripper/ripper.tscn', p.enemy, {
        'DataFile': quoted(f'enemies/{snake(p.enemy)}.json'),
        'Speed': number(p.settings['Speed']), 'StartsFacing': facing(p.settings['Direction'])})


@rule('GrappleRipper')
def grapple_ripper(p, index, listed):
    s = p.settings
    return Placed(E + 'ripper/ripper.tscn', 'GrappleRipper', {
        'DataFile': quoted('enemies/grapple_ripper.json'), 'Speed': number(s['Speed']),
        'StartsFacing': facing(s['Direction']), 'PatrolLeft': s['MinimumX'], 'PatrolRight': s['MaximumX']})


@rule('Hibashi')
def hibashi(p, index, listed):
    if p.settings_type != 'HibashiFlameSpawnSettings':
        return None
    return Placed(E + 'hibashi/hibashi.tscn', 'Hibashi', {'RestUpdates': p.settings['InactiveTime']})


@rule('Kago')
def kago(p, index, listed):
    return Placed(E + 'kago/kago.tscn', 'Kago', {'HitCount': p.settings['HitCount']})


SHUTTER_TRIGGERS = {'Timer': '0', 'HorizontalProximity': '1', 'Immediate': '2', 'OneShotReaction': '3', 'RepeatableReaction': '4'}


def shutter_rest(expression: str) -> str:
    return '-1' if 'Permanent' in expression else whole(expression)


@rule('Kamer', 'ShutterShootable', 'ShutterDestroyable')
def shutter(p, index, listed):
    s = p.settings
    trigger = re.search(r'ShutterTrigger\.(\w+)', s['Trigger']).group(1)
    amount = legacy.numbers(s['Trigger'][s['Trigger'].index('('):]) if '(' in s['Trigger'] else [0]
    return Placed(E + 'shutter/shutter.tscn', p.enemy, {
        'DataFile': quoted(f'enemies/{snake(p.enemy)}.json'),
        'Trigger': SHUTTER_TRIGGERS[trigger], 'TriggerAmount': str(amount[0]),
        'MovesUpFirst': truth(legacy.last_name(s['PrimaryDirection']) == 'Up'),
        'Speed': number(s['Speed']), 'Travel': s['TravelDistance'],
        'RestAfterUp': shutter_rest(s['RestAfterMovingUp']), 'RestAfterDown': shutter_rest(s['RestAfterMovingDown'])})


@rule('ShutterGrowing')
def growing_shutter(p, index, listed):
    s = p.settings
    wait = legacy.numbers(s['Wait'][s['Wait'].index('('):])[0]
    near = 'SamusNear' in s['Wait']
    return Placed(E + 'shutter/growing_shutter.tscn', 'GrowingShutter', {
        'WaitUpdates': '0' if near else str(wait), 'WaitsForSamusWithin': str(wait) if near else '0',
        'GrowsUp': truth(legacy.last_name(s['Growth']) == 'Up'), 'GrowthSpeed': number(s['GrowthSpeed'])})


@rule('KihunterGreen', 'KihunterRed', 'KihunterYellow')
def ki_hunter(p, index, listed):
    wings = beside(listed, index, p.enemy + 'Wings')
    return Placed(E + 'ki_hunter/ki_hunter.tscn', p.enemy, {
        'DataFile': quoted(f'enemies/ki_hunter_{p.enemy[len("Kihunter"):].lower()}.json'),
        'Wingless': truth(legacy.last_name(p.settings['Wings']) == 'Wingless'),
        'WingsLostAtHealth': wings.settings['DetachHealth']})


@rule('KzanTop')
def kzan(p, index, listed):
    s = p.settings
    return Placed(E + 'kzan/kzan.tscn', 'Kzan', {
        'FallSpeed': number(s['FallSpeed']), 'FallWait': s['FallWait'], 'FallDistance': s['FallDistance']})


@rule('Magdollite')
def magdollite(p, index, listed):
    if p.settings_type != 'MagdolliteHeadSpawnSettings':
        return None
    # Its pillar is listed straight after its head, and its overlay after that. The cartridge carries a hit on a
    # part onto the two enemies listed after it: the creature is told which of the room's enemies are listed first
    # and second after its overlay, where the list goes on.
    after = {name: place for name, place in (('ListedAfter', index + 3), ('ListedSecondAfter', index + 4)) if place < len(listed)}
    return Placed(E + 'magdollite/magdollite.tscn', 'Magdollite', {
        'Speed': number(p.settings['Speed']), 'Range': p.settings['Range'], 'ListedAt': str(index + 1)}, refers=after)


@rule('Metroid')
def metroid(p, index, listed):
    return Placed(E + 'metroid/metroid.tscn', 'Metroid', {'IceDamageToFreeze': p.settings['IceDamageToFreeze']})


@rule('Multiviola')
def multiviola(p, index, listed):
    return Placed(E + 'multiviola/multiviola.tscn', 'Multiviola', {
        'Heading': whole(p.settings['Heading']), 'Speed': p.settings['Speed']})


@rule('NuclearWaffle')
def nuclear_waffle(p, index, listed):
    s = p.settings
    steps = legacy.numbers(s['AngularSpeed'][s['AngularSpeed'].index('('):])
    turn = Fraction(steps[0], steps[1]) if 'Fraction' in s['AngularSpeed'] else Fraction(steps[0])
    return Placed(E + 'nuclear_waffle/nuclear_waffle.tscn', 'NuclearWaffle', {
        'TurnSpeed': str(int(turn * 256)), 'OrbitRadius': s['OrbitRadius'],
        'Backward': truth(legacy.last_name(s['Direction']) != 'Forward'), 'WaitUpdates': s['WaitTime']})


@rule('Owtch')
def owtch(p, index, listed):
    s = p.settings
    doing = ['MovingLeft', 'MovingRight', 'Underground', 'Sinking', 'Rising', 'TurningAtLeftEnd'].index(legacy.last_name(s['InitialBehavior']))
    return Placed(E + 'owtch/owtch.tscn', 'Owtch', {
        'StartsBy': str(doing), 'UndergroundWait': s['UndergroundWait'], 'Speed': number(s['Speed']),
        'TravelDistance': s['TravelDistance']})


# How fast a pirate's laser flies, in pixels an update.
LASER_SPEEDS = {'Slow': '2', 'Fast': '4'}


def pirate_data(kind: str, enemy: str, way: str) -> str:
    colour = enemy[len('Pirate'):-len(way)].lower()
    return quoted(f'enemies/{kind}_pirate_{colour}.json')


@rule('PirateGoldWalking', 'PirateGreenWalking', 'PirateGreyWalking', 'PirateMagentaWalking', 'PirateRedWalking', 'PirateSilverWalking')
def walking_pirate(p, index, listed):
    s = p.settings
    return Placed(E + 'space_pirate/walking_pirate.tscn', p.enemy, {
        'DataFile': pirate_data('walking', p.enemy, 'Walking'), 'StartsFacing': facing(s['Facing']),
        'LaserSpeed': LASER_SPEEDS[legacy.last_name(s['LaserSpeed'])],
        'FlinchesFromShots': truth(legacy.last_name(s['ShotWatch']) == 'FlinchesFromShots'), 'PatrolReach': s['PatrolReach']})


@rule('PirateGoldWall', 'PirateGreyWall', 'PirateMagentaWall', 'PirateSilverWall')
def wall_pirate(p, index, listed):
    s = p.settings
    return Placed(E + 'space_pirate/wall_pirate.tscn', p.enemy, {
        'DataFile': pirate_data('wall', p.enemy, 'Wall'), 'Wall': facing(s['Wall']),
        'LaserSpeed': LASER_SPEEDS[legacy.last_name(s['LaserSpeed'])],
        'SlowJump': truth(legacy.last_name(s['JumpSpeed']) == 'Slow'), 'JumpSpan': s['JumpSpan']})


@rule('PirateGoldNinja', 'PirateSilverNinja')
def ninja_pirate(p, index, listed):
    s = p.settings
    return Placed(E + 'space_pirate/ninja_pirate.tscn', p.enemy, {
        'DataFile': pirate_data('ninja', p.enemy, 'Ninja'),
        'StartsAtLeftPost': truth(legacy.last_name(s['StartPost']) == 'Left'), 'PostDistance': s['PostDistance']})


@rule('Powamp')
def powamp(p, index, listed):
    if p.settings_type != 'PowampBodySpawnSettings':
        return None
    balloon = beside(listed, index, 'PowampBalloonSpawnSettings')
    return Placed(E + 'powamp/powamp.tscn', 'Powamp', {
        'GrappleTravelDistance': balloon.settings['GrappleTravelDistance'],
        'Untouchable': truth(legacy.last_name(p.settings['Contact']) == 'Refuses')})


@rule('Puyo')
def puyo(p, index, listed):
    return Placed(E + 'puyo/puyo.tscn', 'Puyo', {
        'HopCooldown': p.settings['HopCooldown'], 'BigHopDistance': p.settings['HopDistance']})


SBUG_RUNS = {'MoveForward': '0', 'ZigZag': '1', 'MoveForwardThenWait': '2'}


@rule('Sbug')
def sbug(p, index, listed):
    s = p.settings
    return Placed(E + 'sbug/sbug.tscn', 'Sbug', {
        'Speed': s['Speed'], 'Heading': whole(s['InitialAngle']), 'ActivationDistance': s['ActivationRadius'],
        'Runs': SBUG_RUNS[legacy.last_name(s['Behavior'])]})


@rule('Shaktool')
def shaktool(p, index, listed):
    if legacy.last_name(p.settings['Segment']) != 'Head':
        return None
    return Placed(E + 'shaktool/shaktool.tscn', 'Shaktool', {
        'StartsClockwise': truth(legacy.last_name(p.settings['Motion']) == 'CurlingClockwise')})


@rule('Skultera')
def skultera(p, index, listed):
    s = p.settings
    return Placed(E + 'skultera/skultera.tscn', 'Skultera', {
        'Speed': number(s['Speed']), 'StartsFacing': facing(s['StartDirection']), 'Radius': s['Radius'],
        'AngleStep': whole(s['AngleStep'])})


SPARK_KINDS = {'AlwaysActive': '0', 'Intermittent': '1', 'Emitter': '2'}


@rule('Spark')
def spark(p, index, listed):
    return Placed(E + 'spark/spark.tscn', 'Spark', {
        'Does': SPARK_KINDS[legacy.last_name(p.settings['Start'])], 'SpellUpdates': p.settings['BaseActionTime']})


@rule('Tripper', 'SuspensorPlatform')
def tripper(p, index, listed):
    s = p.settings
    return Placed(E + 'tripper/tripper.tscn', p.enemy, {
        'DataFile': quoted(f'enemies/{snake(p.enemy)}.json'), 'StartsFacing': facing(s['Movement']),
        'Speed': number(s['Speed']), 'FastestMark': s['VerticalAccelerationUpdates']})


@rule('Waver')
def waver(p, index, listed):
    return Placed(E + 'waver/waver.tscn', 'Waver', {'StartsFacing': facing(p.settings['Facing'])})


@rule('WorkRobot', 'UnpoweredWorkRobot')
def work_robot(p, index, listed):
    powered = p.enemy == 'WorkRobot'
    properties = {'Powered': truth(powered)}
    # The robot without power is an enemy of its own, with a data file of its own, and stands in the pose it is
    # placed in; a powered one has no use for a pose.
    if not powered:
        properties['Pose'] = quoted(snake(legacy.last_name(p.settings['Pose'])))
        properties['DataFile'] = quoted('enemies/work_robot_unpowered.json')
    return Placed(E + 'work_robot/work_robot.tscn', p.enemy, properties)


@rule('YappingMaw')
def yapping_maw(p, index, listed):
    return Placed(E + 'yapping_maw/yapping_maw.tscn', 'YappingMaw', {
        'ActivationDistance': p.settings['ActivationDistance'],
        'StrikesDown': truth(legacy.last_name(p.settings['Attack']) == 'Down')})


def only_in_view(p) -> dict[str, str]:
    """What an object placed for an old enemy says of its turns: one the old game does not play out of view takes its turn only in view."""
    return {} if 'EnemyProcessing.OffScreen' in ' '.join(p.arguments[2:6]) else {'TurnsOnlyInView': 'true'}


@rule('SteamVent')
def steam(p, index, listed):
    # A vent of the turning shaft blows left or right like any other; the room shows it turned.
    way = legacy.last_name(p.settings['Variant']).replace('RotatingElevator', '').lower()
    return Placed(O + 'ceres_steam/ceres_steam.tscn', 'Steam',
                  {'DataFile': quoted(f'objects/ceres_steam_{way}.json'), **only_in_view(p)}, enemy=False)


# The elevator's scene; the room's generator tells each one the blocks it rests on.
ELEVATOR = O + 'elevator/elevator.tscn'


@rule('Elevator')
def elevator(p, index, listed):
    # It stands where the old elevator does, and says its way and the height at which a ride arrives.
    down = legacy.last_name(p.settings['Direction']) == 'Down'
    return Placed(ELEVATOR, 'Elevator', {'GoesDown': truth(down), 'ArrivesAtY': str(int(p.settings['ArrivalY']))},
                  at=(p.at[0], p.at[1]), enemy=False)


@rule('CeresDoor')
def ceres_door(p, index, listed):
    way = legacy.last_name(p.settings['Facing']).lower()
    return Placed(O + 'ceres_door/ceres_door.tscn', 'CeresDoor', {'DataFile': quoted(f'objects/ceres_door_{way}.json')}, enemy=False)


@rule('Etecoon')
def etecoon(p, index, listed):
    route = ['RunLeftToTeachingStart', 'RunRightToTeachingStart', 'TeachAtOnce'].index(legacy.last_name(p.settings['Route']))
    return Placed(O + 'etecoon/etecoon.tscn', 'Etecoon', {'Route': str(route)}, enemy=False)


# Where each trapped Etecoon starts, the way it faces and how fast it runs, by what it does: the old code keeps
# these with the Etecoon, not with the room.
ESCAPE_ETECOONS = {
    'LeftWalker': ((128, 200), LEFT, '2'), 'RightWalker': ((160, 200), RIGHT, '2.5'), 'WaitForEscapeEvent': ((232, 200), RIGHT, '2'),
}


@rule('EtecoonEscape')
def escape_etecoon(p, index, listed):
    role = legacy.last_name(p.settings['Role'])
    at, way, speed = ESCAPE_ETECOONS[role]
    return Placed(O + 'etecoon/escape_etecoon.tscn', 'EscapeEtecoon', {
        'Waits': truth(role == 'WaitForEscapeEvent'), 'StartsFacing': way, 'RunSpeed': speed}, at=at, enemy=False)


@rule('Dachora')
def dachora(p, index, listed):
    return Placed(O + 'dachora/dachora.tscn', 'Dachora', {'StartsFacing': facing(p.arguments[-1])}, enemy=False)


@rule('DachoraEscape')
def escape_dachora(p, index, listed):
    return Placed(O + 'dachora/escape_dachora.tscn', 'EscapeDachora', enemy=False)


# Each boss the old code places: its scene, the name its defeat is recorded under, and what its placement sets.
# The terrain patches named here are the room's layout's to hold.
BOSSES = {
    'Kraid': ('kraid/kraid.tscn', 'kraid', {
        'FightMusic': '"kraid_and_phantoon_battle"', 'VictoryMusic': '"elevator"'}),
    'Crocomire': ('crocomire/crocomire.tscn', 'crocomire', {
        'FightMusic': '"kraid_and_phantoon_battle"', 'TensionMusic': '"boss_room_tension"',
        'BridgeCrumblePatches': 'Array[String](["bridge_crumbles_1", "bridge_crumbles_2"])',
        'BridgeCollapsePatch': '"bridge_collapsed"', 'SpikeWallPatch': '"spike_wall_open"'}),
    'PhantoonBody': ('phantoon/phantoon.tscn', 'phantoon', {
        'FightMusic': '"kraid_and_phantoon_battle"', 'VictoryMusic': '"elevator"'}),
    'DraygonBody': ('draygon/draygon.tscn', 'draygon', {'VictoryMusic': '"elevator"'}),
    'Ridley': ('ridley/ridley.tscn', 'ridley', {'FightMusic': '"ridley_and_draygon_battle"'}),
    'SporeSpawn': ('spore_spawn/spore_spawn.tscn', 'spore_spawn', {}),
    'Botwoon': ('botwoon/botwoon.tscn', 'botwoon', {'VictoryMusic': '"elevator"'}),
    'BombTorizo': ('torizo/torizo.tscn', 'bomb_torizo', {
        'AwakeningMusic': '"torizo_awakening"', 'FightMusic': '"ridley_and_draygon_battle"', 'VictoryMusic': '"elevator"'}),
    'GoldenTorizo': ('torizo/torizo.tscn', 'golden_torizo', {
        'DataFile': '"bosses/golden_torizo.json"', 'Golden': 'true',
        'AwakeningMusic': '"torizo_awakening"', 'FightMusic': '"ridley_and_draygon_battle"', 'VictoryMusic': '"elevator"'}),
    'MotherBrainBody': ('mother_brain/mother_brain.tscn', 'mother_brain', {'FightMusic': '"mother_brain_battle"'}),
}

# The terrain patches a boss's room keeps once the boss is beaten: what its death did to the room.
BEATEN_PATCHES = {
    'crocomire': ['bridge_collapsed', 'spike_wall_open'], 'spore_spawn': ['ceiling_open'], 'botwoon': ['wall_down'],
    'kraid': ['ceiling_clear', 'spikes_clear'], 'mother_brain': ['glass_cleared'],
}

for part_name in ('Top', 'Middle', 'Bottom'):
    RULES['KraidLint' + part_name] = kraid_lint(part_name)
RULES['KraidFirstNail'] = kraid_nail(1, None)
RULES['KraidSecondNail'] = kraid_nail(2, 'bosses/kraid_second_nail.json')


# Where a boss stands that the old code moves as it sets it up, whatever its room's list says: the Golden Torizo
# starts high above the floor it drops to.
BOSS_STARTS = {'GoldenTorizo': (424, 144)}


def boss(kind: str) -> Rule:
    scene, name, properties = BOSSES[kind]

    def place(p: legacy.Placement, index: int, listed: list[legacy.Placement]) -> Placed:
        return Placed(B + scene, kind.removesuffix('Body'), dict(properties), boss=name, at=BOSS_STARTS.get(kind))
    return place


for kind in BOSSES:
    RULES[kind] = boss(kind)

# The crack is the glass tube's own; the pictures of the Tourian statue belong to its room's sequence.
for kind in ('NoobTubeCrack', 'TourianStatue'):
    RULES[kind] = part


@rule('CeresElevatorRoomWall')
def ceres_elevator_room_wall(p, index, listed):
    # Once Ridley has gone the elevator room's left door is an unseen solid wall: the door shut for good, opening
    # for nobody and shown nowhere. Samus comes in through it, and it stops her once she is clear of it.
    return Placed(O + 'ceres_door/ceres_door.tscn', 'ElevatorRoomWall', {
        'DataFile': quoted('objects/ceres_door_left.json'), 'OpensWhenNear': '0', 'visible': 'false'}, enemy=False)


@rule('CeresElevatorDoor')
def ceres_elevator_door(p, index, listed):
    # It lights the elevator's pad on the floor, by turns light and dark.
    pad = ', '.join(quoted(f'backdrops/{{graphics}}/ceres_elevator_{look}.png') for look in ('light', 'dark'))
    return Placed(O + 'ceres_elevator_door/ceres_elevator_door.tscn', 'ElevatorDoor', {'PlatformPictures': f'Array[String]([{pad}])'}, enemy=False)


@rule('CeresRidleyRoomDoor')
def ceres_ridley_room_door(p, index, listed):
    # It shows itself shutting as Samus comes in, and is held shut until Ridley has gone.
    return Placed(O + 'ceres_door/ceres_door.tscn', 'RidleyRoomDoor', {
        'DataFile': quoted('objects/ceres_ridley_room_door.json'), 'HeldShutBy': 'fact:ceres_ridley_defeated'}, enemy=False)


@rule('RidleyCeres')
def ceres_ridley(p, index, listed):
    return Placed(B + 'ridley/ridley.tscn', 'Ridley', {
        'DataFile': quoted('bosses/ceres_ridley.json'), 'OnCeres': 'true', 'FightMusic': quoted('ridley_and_draygon_battle')})


# How far above where the old code places it the gunship's top half rests, in pixels.
GUNSHIP_TOP_RAISE = 25


@rule('ShipTop')
def gunship(p, index, listed):
    # The gunship stands where its top half rests.
    return Placed(O + 'gunship/gunship.tscn', 'Gunship', {
        'SaveQuestion': quoted('gunship_save_confirmation'), 'SavedMessage': quoted('save_completed'), **only_in_view(p)},
        at=(p.at[0], p.at[1] - GUNSHIP_TOP_RAISE), enemy=False)


@rule('MorphBallEye')
def morph_ball_eye(p, index, listed):
    # The old code places the eye and its socket as two things at one place: here they are one.
    if p.settings_type != 'MorphBallEyeMountSpawnSettings':
        return None
    eye = beside(listed, index, 'MorphBallEyeBodySpawnSettings')
    side = {'Left': '0', 'Right': '1', 'Above': '2', 'Below': '3'}[legacy.last_name(p.settings['Side'])]
    return Placed(O + 'morph_ball_eye/morph_ball_eye.tscn', 'MorphBallEye', {
        'Faces': facing(eye.settings['Facing']), 'MountedOn': side}, enemy=False, takes_look=True)


# The places Rinkas form at in Mother Brain's room, in the order they are looked through.
RINKA_PLACES = {
    'FarRightUpper': (999, 38), 'FarRightLower': (999, 166), 'RightUpper': (823, 54), 'RightLower': (823, 166),
    'CentreUpper': (631, 28), 'CentreLower': (631, 182), 'LeftUpper': (439, 54), 'LeftLower': (439, 166),
    'FarLeftUpper': (247, 28), 'FarLeftLower': (247, 182), 'Leftmost': (128, 168),
}


@rule('Rinka')
def rinka(p, index, listed):
    # A Rinka comes from a source, which makes another when it is gone.
    if p.settings_type == 'RinkaSpawnSettings.AtOwnPlacement':
        return Placed(E + 'rinka/rinka_source.tscn', 'RinkaSource', {}, enemy=False)
    own = list(RINKA_PLACES).index(legacy.last_name(p.settings['0']))
    spots = ', '.join(f'Vector2i({x}, {y})' for x, y in RINKA_PLACES.values())
    return Placed(E + 'rinka/rinka_spawner.tscn', 'RinkaSource', {
        'EnemyScene': 'scene:' + E + 'rinka/rinka.tscn', 'IntervalUpdates': '1',
        'Places': f'Array[Vector2i]([{spots}])', 'OwnPlace': str(own)}, enemy=False)


@rule('Zebetite')
def zebetite(p, index, listed):
    # Its data says where each of the barriers stands.
    return Placed(E + 'zebetite/zebetite.tscn', 'Zebetite', {'AlwaysActive': 'true'}, at=(0, 0))


@rule('BabyMetroid')
def baby_metroid(p, index, listed):
    victim = next(at for at, other in enumerate(listed)
                  if other.enemy == 'CorpseSidehopper' and legacy.last_name(other.settings['Layout']) == 'BabyMetroidVictim')
    return Placed(E + 'baby_metroid/baby_metroid.tscn', 'BabyMetroid', {
        'AttackMusic': quoted('kraid_and_phantoon_battle'), 'AfterwardMusic': quoted('baby_metroid_theme')}, {'Victim': victim})


@rule('CorpseSidehopper')
def dead_sidehopper(p, index, listed):
    return Placed(E + 'corpse/dead_sidehopper.tscn', 'Sidehopper', {
        'AlreadyDead': truth(legacy.last_name(p.settings['Layout']) == 'AlreadyDead')})


@rule('Chozo')
def chozo_statue(p, index, listed):
    site = legacy.last_name(p.settings['Site'])
    return Placed(E + 'chozo_statue/chozo_statue.tscn', 'ChozoStatue', {
        'DataFile': quoted(f'enemies/chozo_statue_{snake(site)}.json')}, chozo=site)


def places(listed: list[legacy.Placement], rules: dict[str, Rule], unplaced: dict[str, int] | None = None) -> list[tuple[int, Placed]]:
    """
    What a list of the old code's placements places, each with where its placement comes in the list. A kind no rule
    places is an error, unless `unplaced` is given to count such kinds in.
    """
    placed = []
    for index, p in enumerate(listed):
        if p.enemy not in rules:
            if unplaced is None:
                raise KeyError(f'No rule places the old code\'s {p.enemy}.')
            unplaced[p.enemy] = unplaced.get(p.enemy, 0) + 1
            continue
        result = rules[p.enemy](p, index, listed)
        if result is None:
            continue
        if result.at is None and result.kind == 'Node2D':
            if p.at is None:
                raise ValueError(f'The {p.enemy} is placed nowhere, and its rule gives it no place.')
            result.at = p.at
        if result.enemy:
            flags = ' '.join(p.arguments[2:6])
            if 'EnemyProcessing.OffScreen' in flags:
                result.properties['AlwaysActive'] = 'true'
            if 'RespawnsIfKilled' in flags:
                result.properties['RespawnsWhenKilled'] = 'true'
        placed.append((index, result))
    return placed
