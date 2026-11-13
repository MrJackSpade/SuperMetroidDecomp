namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The kind of a room object, what it is: the thing a room population places, a door, item or block reaction leaves, or a scripted
/// scene spawns. The values are arbitrary and nothing reads them (#1414): a family recognises its objects by member, and
/// verification maps the game's identities through its own table. <see cref="None"/> marks a place in
/// the room's update order that holds no identified object.
/// </summary>
/// <remarks>
/// The exposed, Chozo-orb and concealed shot-block items each name one object per <see cref="InWorldCollectibleKind"/>;
/// doors name one object per colour and facing. A boss's change to its room (a wall, ceiling, bridge, platform, mural
/// row or tube) has no identity here (#1608), and neither has a gate trigger's shot, the Wrecked Ship treadmill or the
/// Maridia elevatube, nor does any object a Chozo statue, Tourian, the Wrecked Ship, Shaktool's room, the critters'
/// escape or an enemy's break of a block spawns carry one (#1622): its spawner places it by what it does, and its typed
/// room object is how the room finds it.
/// </remarks>
public enum RoomObjectKind
{
    /// <summary>No identified object: a free place, or a block reaction's short-lived object.</summary>
    None = 0,

    /// <summary>A block extending the scroll trigger to its left along a horizontal chain.</summary>
    RightwardsScrollExtension,

    /// <summary>A block extending the scroll trigger to its right along a horizontal chain.</summary>
    LeftwardsScrollExtension,

    /// <summary>A block extending the scroll trigger above it along a vertical chain.</summary>
    DownwardsScrollExtension,

    /// <summary>A block extending the scroll trigger below it along a vertical chain.</summary>
    UpwardsScrollExtension,

    /// <summary>The Brinstar plant that bites Samus from the floor when she stands inside it.</summary>
    InsideReactionBrinstarFloorPlant,

    /// <summary>The Brinstar plant that bites Samus from the ceiling when she stands inside it.</summary>
    InsideReactionBrinstarCeilingPlant,

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

    /// <summary>Maridia's quicksand surface, when Samus stands inside it.</summary>
    InsideReactionQuicksandSurface,

    /// <summary>Maridia's submerging quicksand, when Samus stands inside it.</summary>
    InsideReactionSubmergingQuicksand,

    /// <summary>Maridia's slow sandfall, when Samus stands inside it.</summary>
    InsideReactionSandFallsSlow,

    /// <summary>Maridia's fast sandfall, when Samus stands inside it.</summary>
    InsideReactionSandFallsFast,

    /// <summary>Maridia's quicksand surface, when Samus touches it.</summary>
    CollisionReactionQuicksandSurface,

    /// <summary>Maridia's submerging quicksand, when Samus touches it.</summary>
    CollisionReactionSubmergingQuicksand,

    /// <summary>Maridia's slow sandfall, when Samus touches it.</summary>
    CollisionReactionSandFallsSlow,

    /// <summary>Maridia's fast sandfall, when Samus touches it.</summary>
    CollisionReactionSandFallsFast,

    /// <summary>A save station.</summary>
    SaveStation,


    /// <summary>The lava quake of the Speed Booster room's escape, once the Speed Booster is taken.</summary>
    SpeedBoosterEscape,

    /// <summary>
    /// The fake wall of the old Tourian escape shaft, spawned by its room's setup during the escape; it explodes once
    /// Samus passes below and to the right of it.
    /// </summary>
    OldTourianEscapeShaftFakeWall,

    /// <summary>Bomb Torizo's right-facing grey door, which opens once Bomb Torizo is defeated.</summary>
    BombTorizoGreyDoor,

    /// <summary>The Wrecked Ship attic's watcher, which does nothing while it waits.</summary>
    WreckedShipAttic,

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

    /// <summary>A red door cap facing up, opened by missiles.</summary>
    RedDoorFacingUp,

    /// <summary>A red door cap facing down, opened by missiles.</summary>
    RedDoorFacingDown,

    /// <summary>A blue door cap facing left, opened by any shot.</summary>
    BlueDoorFacingLeft,

    /// <summary>A blue door cap facing right, opened by any shot.</summary>
    BlueDoorFacingRight,

    /// <summary>A blue door cap facing up, opened by any shot.</summary>
    BlueDoorFacingUp,

    /// <summary>A blue door cap facing down, opened by any shot.</summary>
    BlueDoorFacingDown,

    /// <summary>The blue door cap closing behind Samus after she comes through a left-facing door.</summary>
    BlueDoorClosingFacingLeft,

    /// <summary>The blue door cap closing behind Samus after she comes through a right-facing door.</summary>
    BlueDoorClosingFacingRight,

    /// <summary>The blue door cap closing behind Samus after she comes through an upward-facing door.</summary>
    BlueDoorClosingFacingUp,

    /// <summary>The blue door cap closing behind Samus after she comes through a downward-facing door.</summary>
    BlueDoorClosingFacingDown,

    /// <summary>
    /// The gate of the first room of the Tourian escape. Samus coming in from Mother Brain's room makes the door closing
    /// close it behind her.
    /// </summary>
    MotherBrainEscapeRoomGate,

    /// <summary>
    /// The escape gate closing behind Samus at a door whose block holds no resident cap.
    /// </summary>
    MotherBrainEscapeRoomGateClosing,

    /// <summary>
    /// The Speed Booster block Samus breaks in Brinstar, with the slower crumble, which comes back.
    /// </summary>
    SpeedBlockBrinstarSlowRespawning,

    /// <summary>
    /// The Speed Booster block Samus breaks in Brinstar, with the slower crumble, which stays broken.
    /// </summary>
    SpeedBlockBrinstarSlowPermanent,

    /// <summary>A Speed Booster block Samus breaks, which comes back.</summary>
    SpeedBlockRespawning,

    /// <summary>The Speed Booster block of Brinstar's Dachora room, which comes back.</summary>
    SpeedBlockDachoraRespawning,

    /// <summary>A Speed Booster block Samus breaks, which stays broken.</summary>
    SpeedBlockPermanent,

    /// <summary>A one-block crumble block Samus stands on, which comes back.</summary>
    ContactCrumble1x1Respawning,

    /// <summary>A two-block-wide crumble block Samus stands on, which comes back.</summary>
    ContactCrumble2x1Respawning,

    /// <summary>A two-block-tall crumble block Samus stands on, which comes back.</summary>
    ContactCrumble1x2Respawning,

    /// <summary>A two-by-two crumble block Samus stands on, which comes back.</summary>
    ContactCrumble2x2Respawning,

    /// <summary>A one-block crumble block Samus stands on, which stays broken.</summary>
    ContactCrumble1x1Permanent,

    /// <summary>A two-block-wide crumble block Samus stands on, which stays broken.</summary>
    ContactCrumble2x1Permanent,

    /// <summary>A two-block-tall crumble block Samus stands on, which stays broken.</summary>
    ContactCrumble1x2Permanent,

    /// <summary>A two-by-two crumble block Samus stands on, which stays broken.</summary>
    ContactCrumble2x2Permanent,

    /// <summary>The plug of the Lower Norfair Chozo's room, which crumbles once the Chozo lowers the acid.</summary>
    CrumbleLowerNorfairChozoRoomPlug,

    /// <summary>The Lower Norfair Chozo's hand, which carries Samus when she curls up in it.</summary>
    LowerNorfairChozoHand,

    /// <summary>The glass of Mother Brain's tank, which missiles break.</summary>
    MotherBrainGlass,

    /// <summary>The hand of Bomb Torizo's statue, which wakes Bomb Torizo when Samus takes its item.</summary>
    BombTorizoHand,

    /// <summary>The Wrecked Ship Chozo's hand, which carries Samus when she curls up in it.</summary>
    WreckedShipChozoHand,

    /// <summary>Clears the slope under the Wrecked Ship Chozo, letting Samus reach it.</summary>
    ClearSlopeAccessForWreckedShipChozo,

    /// <summary>Blocks the slope under the Wrecked Ship Chozo again.</summary>
    BlockSlopeAccessForWreckedShipChozo,

    /// <summary>Maridia's glass tube, which a Power Bomb shatters.</summary>
    NoobTube,

    /// <summary>
    /// Watches a Tourian Metroid room's kills and marks the room cleared once its authored count of Metroids is dead.
    /// </summary>
    MetroidRoomClearance,

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

    /// <summary>The Bombs lying in the open.</summary>
    ExposedBombs,

    /// <summary>The Charge Beam lying in the open.</summary>
    ExposedChargeBeam,

    /// <summary>The Ice Beam lying in the open.</summary>
    ExposedIceBeam,

    /// <summary>The Hi-Jump Boots lying in the open.</summary>
    ExposedHiJumpBoots,

    /// <summary>The Speed Booster lying in the open.</summary>
    ExposedSpeedBooster,

    /// <summary>The Wave Beam lying in the open.</summary>
    ExposedWaveBeam,

    /// <summary>The Spazer lying in the open.</summary>
    ExposedSpazerBeam,

    /// <summary>The Spring Ball lying in the open.</summary>
    ExposedSpringBall,

    /// <summary>The Varia Suit lying in the open.</summary>
    ExposedVariaSuit,

    /// <summary>The Gravity Suit lying in the open.</summary>
    ExposedGravitySuit,

    /// <summary>The X-Ray Scope lying in the open.</summary>
    ExposedXrayScope,

    /// <summary>The Plasma Beam lying in the open.</summary>
    ExposedPlasmaBeam,

    /// <summary>The Grapple Beam lying in the open.</summary>
    ExposedGrappleBeam,

    /// <summary>The Space Jump lying in the open.</summary>
    ExposedSpaceJump,

    /// <summary>The Screw Attack lying in the open.</summary>
    ExposedScrewAttack,

    /// <summary>The Morph Ball lying in the open.</summary>
    ExposedMorphBall,

    /// <summary>A Reserve Tank lying in the open.</summary>
    ExposedReserveTank,

    /// <summary>An Energy Tank held in a Chozo statue's orb.</summary>
    ChozoEnergyTank,

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

    /// <summary>The Morph Ball held in a Chozo statue's orb.</summary>
    ChozoMorphBall,

    /// <summary>A Reserve Tank held in a Chozo statue's orb.</summary>
    ChozoReserveTank,

    /// <summary>An Energy Tank hidden in a shot block.</summary>
    ShotBlockEnergyTank,

    /// <summary>A Missile Tank hidden in a shot block.</summary>
    ShotBlockMissileTank,

    /// <summary>A Super Missile Tank hidden in a shot block.</summary>
    ShotBlockSuperMissileTank,

    /// <summary>A Power Bomb Tank hidden in a shot block.</summary>
    ShotBlockPowerBombTank,

    /// <summary>The Bombs hidden in a shot block.</summary>
    ShotBlockBombs,

    /// <summary>The Charge Beam hidden in a shot block.</summary>
    ShotBlockChargeBeam,

    /// <summary>The Ice Beam hidden in a shot block.</summary>
    ShotBlockIceBeam,

    /// <summary>The Hi-Jump Boots hidden in a shot block.</summary>
    ShotBlockHiJumpBoots,

    /// <summary>The Speed Booster hidden in a shot block.</summary>
    ShotBlockSpeedBooster,

    /// <summary>The Wave Beam hidden in a shot block.</summary>
    ShotBlockWaveBeam,

    /// <summary>The Spazer hidden in a shot block.</summary>
    ShotBlockSpazerBeam,

    /// <summary>The Spring Ball hidden in a shot block.</summary>
    ShotBlockSpringBall,

    /// <summary>The Varia Suit hidden in a shot block.</summary>
    ShotBlockVariaSuit,

    /// <summary>The Gravity Suit hidden in a shot block.</summary>
    ShotBlockGravitySuit,

    /// <summary>The X-Ray Scope hidden in a shot block.</summary>
    ShotBlockXrayScope,

    /// <summary>The Plasma Beam hidden in a shot block.</summary>
    ShotBlockPlasmaBeam,

    /// <summary>The Grapple Beam hidden in a shot block.</summary>
    ShotBlockGrappleBeam,

    /// <summary>The Space Jump hidden in a shot block.</summary>
    ShotBlockSpaceJump,

    /// <summary>The Screw Attack hidden in a shot block.</summary>
    ShotBlockScrewAttack,

    /// <summary>The Morph Ball hidden in a shot block.</summary>
    ShotBlockMorphBall,

    /// <summary>A Reserve Tank hidden in a shot block.</summary>
    ShotBlockReserveTank,
}
