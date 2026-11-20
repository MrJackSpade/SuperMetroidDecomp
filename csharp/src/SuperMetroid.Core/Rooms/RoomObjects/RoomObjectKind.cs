namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The kind of a room object, what it is: the thing a room population places, a door, item or block reaction leaves, or a scripted
/// scene spawns. The values are arbitrary and nothing reads them (#1414): a family recognises its objects by member.
/// A place in the room's update order that holds no object holds no kind (#2558).
/// </summary>
/// <remarks>
/// The exposed, Chozo-orb and concealed shot-block items name one object for each <see cref="InWorldCollectibleKind"/>
/// some room places in that presentation, and doors one for each colour and facing some room places (#2354): a kind no
/// room population places has no member. A boss's change to its room (a wall, ceiling, bridge, platform, mural
/// row or tube) has no identity here (#1608), and neither has a gate trigger's shot, the Wrecked Ship treadmill or the
/// Maridia elevatube, nor does any object a Chozo statue, Tourian, the Wrecked Ship, Shaktool's room, the critters'
/// escape or an enemy's break of a block spawns carry one (#1622): its spawner places it by what it does, and its typed
/// room object is how the room finds it. The same holds for what a block's reaction leaves (a plant's bite, quicksand
/// and sandfalls, a Speed Booster or crumble block breaking), for a blue door cap and the cap closing behind Samus, and
/// for the escape gate closing at a door with no resident cap (#2126): their reactions and doors name them by
/// <see cref="Rooms.LevelData.SpecialBlockReaction"/>, <see cref="ContactCrumbleBlock"/> and the door's own cap object.
/// </remarks>
public enum RoomObjectKind
{
    /// <summary>A block extending the scroll trigger to its left along a horizontal chain.</summary>
    RightwardsScrollExtension,

    /// <summary>A block extending the scroll trigger to its right along a horizontal chain.</summary>
    LeftwardsScrollExtension,

    /// <summary>A block extending the scroll trigger above it along a vertical chain.</summary>
    DownwardsScrollExtension,

    /// <summary>A block extending the scroll trigger below it along a vertical chain.</summary>
    UpwardsScrollExtension,

    /// <summary>A map station.</summary>
    MapStation,

    /// <summary>An energy refill station.</summary>
    EnergyStation,

    /// <summary>A missile refill station.</summary>
    MissileStation,

    /// <summary>A scroll trigger: the block that changes the room's scroll cells when Samus touches it.</summary>
    ScrollTrigger,

    /// <summary>An elevator platform.</summary>
    ElevatorPlatform,

    /// <summary>A save station.</summary>
    SaveStation,

    /// <summary>The lava quake of the Speed Booster room's escape, once the Speed Booster is taken.</summary>
    SpeedBoosterEscape,

    /// <summary>Bomb Torizo's right-facing grey door, which opens once Bomb Torizo is defeated.</summary>
    BombTorizoGreyDoor,

    /// <summary>
    /// Holds one of the Wrecked Ship attic's room-object places for as long as the room is loaded and does nothing else
    /// (<see cref="Compatibility.RoomObjectPlaces"/>).
    /// </summary>
    WreckedShipAtticHeldPlace,

    /// <summary>A five-block gate that slides down out of the way when its trigger is shot or Samus stands below it.</summary>
    DownwardGate,

    /// <summary>The coloured shot block beside a downward gate that triggers it.</summary>
    DownwardGateShotBlock,

    /// <summary>A grey door cap facing left, opened by its room's condition.</summary>
    GreyDoorFacingLeft,

    /// <summary>A grey door cap facing right, opened by its room's condition.</summary>
    GreyDoorFacingRight,

    /// <summary>A grey door cap facing up, opened by its room's condition.</summary>
    GreyDoorFacingUp,

    /// <summary>A grey door cap facing down, opened by its room's condition.</summary>
    GreyDoorFacingDown,

    /// <summary>A yellow door cap facing left, opened by a Power Bomb.</summary>
    YellowDoorFacingLeft,

    /// <summary>A yellow door cap facing right, opened by a Power Bomb.</summary>
    YellowDoorFacingRight,

    /// <summary>A yellow door cap facing up, opened by a Power Bomb.</summary>
    YellowDoorFacingUp,

    /// <summary>A yellow door cap facing down, opened by a Power Bomb.</summary>
    YellowDoorFacingDown,

    /// <summary>A green door cap facing left, opened by a Super Missile.</summary>
    GreenDoorFacingLeft,

    /// <summary>A green door cap facing right, opened by a Super Missile.</summary>
    GreenDoorFacingRight,

    /// <summary>A green door cap facing up, opened by a Super Missile.</summary>
    GreenDoorFacingUp,

    /// <summary>A green door cap facing down, opened by a Super Missile.</summary>
    GreenDoorFacingDown,

    /// <summary>A red door cap facing left, opened by missiles.</summary>
    RedDoorFacingLeft,

    /// <summary>A red door cap facing right, opened by missiles.</summary>
    RedDoorFacingRight,

    /// <summary>
    /// The gate of the first room of the Tourian escape. Samus coming in from Mother Brain's room makes the door closing
    /// close it behind her.
    /// </summary>
    MotherBrainEscapeRoomGate,

    /// <summary>The glass of Mother Brain's tank, which missiles break.</summary>
    MotherBrainGlass,

    /// <summary>The hand of Bomb Torizo's statue, which wakes Bomb Torizo when Samus takes its item.</summary>
    BombTorizoHand,

    /// <summary>Maridia's glass tube, which a Power Bomb shatters.</summary>
    NoobTube,

    /// <summary>
    /// Watches a Tourian Metroid room's kills and marks the room cleared once its authored count of Metroids is dead.
    /// </summary>
    MetroidRoomClearance,

    /// <summary>
    /// Holds one of an escape room's room-object places for as long as the room is loaded and does nothing else
    /// (<see cref="Compatibility.RoomObjectPlaces"/>).
    /// </summary>
    EscapeRoomHeldPlace,

    /// <summary>The eye of a right-facing eye door.</summary>
    EyeDoorEyeFacingRight,

    /// <summary>The door of a right-facing eye door.</summary>
    EyeDoorFacingRight,

    /// <summary>The bottom of a right-facing eye door.</summary>
    EyeDoorBottomFacingRight,

    /// <summary>The eye of a left-facing eye door.</summary>
    EyeDoorEyeFacingLeft,

    /// <summary>The door of a left-facing eye door.</summary>
    EyeDoorFacingLeft,

    /// <summary>The bottom of a left-facing eye door.</summary>
    EyeDoorBottomFacingLeft,

    /// <summary>A right-facing shielded cannon of Draygon's room.</summary>
    DraygonCannonFacingRight,

    /// <summary>
    /// A right-facing cannon of Draygon's room that starts destroyed, so the upper-left firing position never fires.
    /// </summary>
    DraygonCannonFacingRightDestroyed,

    /// <summary>A left-facing shielded cannon of Draygon's room.</summary>
    DraygonCannonFacingLeft,

    /// <summary>An Energy Tank lying in the open.</summary>
    ExposedEnergyTank,

    /// <summary>A Missile Tank lying in the open.</summary>
    ExposedMissileTank,

    /// <summary>A Super Missile Tank lying in the open.</summary>
    ExposedSuperMissileTank,

    /// <summary>A Power Bomb Tank lying in the open.</summary>
    ExposedPowerBombTank,

    /// <summary>The Morph Ball lying in the open.</summary>
    ExposedMorphBall,

    /// <summary>A Missile Tank held in a Chozo statue's orb.</summary>
    ChozoMissileTank,

    /// <summary>A Super Missile Tank held in a Chozo statue's orb.</summary>
    ChozoSuperMissileTank,

    /// <summary>A Power Bomb Tank held in a Chozo statue's orb.</summary>
    ChozoPowerBombTank,

    /// <summary>The Bombs held in a Chozo statue's orb.</summary>
    ChozoBombs,

    /// <summary>The Charge Beam held in a Chozo statue's orb.</summary>
    ChozoChargeBeam,

    /// <summary>The Ice Beam held in a Chozo statue's orb.</summary>
    ChozoIceBeam,

    /// <summary>The Hi-Jump Boots held in a Chozo statue's orb.</summary>
    ChozoHiJumpBoots,

    /// <summary>The Speed Booster held in a Chozo statue's orb.</summary>
    ChozoSpeedBooster,

    /// <summary>The Wave Beam held in a Chozo statue's orb.</summary>
    ChozoWaveBeam,

    /// <summary>The Spazer held in a Chozo statue's orb.</summary>
    ChozoSpazerBeam,

    /// <summary>The Spring Ball held in a Chozo statue's orb.</summary>
    ChozoSpringBall,

    /// <summary>The Varia Suit held in a Chozo statue's orb.</summary>
    ChozoVariaSuit,

    /// <summary>The Gravity Suit held in a Chozo statue's orb.</summary>
    ChozoGravitySuit,

    /// <summary>The X-Ray Scope held in a Chozo statue's orb.</summary>
    ChozoXrayScope,

    /// <summary>The Plasma Beam held in a Chozo statue's orb.</summary>
    ChozoPlasmaBeam,

    /// <summary>The Grapple Beam held in a Chozo statue's orb.</summary>
    ChozoGrappleBeam,

    /// <summary>The Space Jump held in a Chozo statue's orb.</summary>
    ChozoSpaceJump,

    /// <summary>The Screw Attack held in a Chozo statue's orb.</summary>
    ChozoScrewAttack,

    /// <summary>A Reserve Tank held in a Chozo statue's orb.</summary>
    ChozoReserveTank,

    /// <summary>An Energy Tank hidden in a shot block.</summary>
    ShotBlockEnergyTank,

    /// <summary>A Missile Tank hidden in a shot block.</summary>
    ShotBlockMissileTank,

    /// <summary>A Super Missile Tank hidden in a shot block.</summary>
    ShotBlockSuperMissileTank,
}
