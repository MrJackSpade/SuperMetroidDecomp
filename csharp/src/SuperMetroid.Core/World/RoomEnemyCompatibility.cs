using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.Core.World;

/// <summary>
/// The compatibility services a room's enemy system consults. The room's own services the room's loader creates fresh
/// with each enemy system it builds, and they live exactly as long as that system: a room's power-bomb sweep, its Shaktool
/// chain, its slots and its effect-actor places start with no earlier kill, no earlier occupant and no place taken. The services that
/// outlive every room, the runtime's shared fade step count and Yard kick camera measure and the session's unauthored Speed
/// Booster cadence, the loader hands on to every enemy system it builds.
/// </summary>
[ArchitectureReview("context-bag", "A constructor parameter object: production builds it only for RoomEnemySystem's " +
    "constructor, and the one other method taking it is Verification's EnemySystemFixtures.New, which forwards it to " +
    "that same constructor.")]
internal sealed class RoomEnemyCompatibility
{
    /// <summary>The horizontal reach a power bomb's sweep continues with after the sweep kills an enemy.</summary>
    public required PowerBombRadiusAfterKill PowerBombRadius { get; init; }

    /// <summary>The Bull a new Bull inherits its untouched values from, per slot.</summary>
    public required SlotInheritance<BullEnemyState> BullInheritance { get; init; }

    /// <summary>The Beetom a new Beetom inherits its untouched values from, per slot.</summary>
    public required SlotInheritance<BeetomEnemyState> BeetomInheritance { get; init; }

    /// <summary>
    /// Whether a Puyo striking a ceiling turns back, from what the room's draws and HUD redraws left since its load; a new
    /// one holds what the room's load leaves.
    /// </summary>
    public required PuyoCeilingTurnFromLastDraw PuyoCeilingTurn { get; init; }

    /// <summary>
    /// The room's thirty-two effect-actor places, whose place a dud flash on Kraid's mouth takes bounds the rest of that
    /// collision pass; a new room starts with every place free.
    /// </summary>
    public required EffectActorPlaces EffectActorPlaces { get; init; }

    /// <summary>
    /// The runtime's shared fade step count, which Kraid's backdrop fades and the Torizos' body fades count their steps on
    /// with the runtime's door, death and time-up fades; it outlives every room.
    /// </summary>
    public required SharedFadeStepCount FadeStepCount { get; init; }

    /// <summary>
    /// The session's unauthored Speed Booster cadence, whose delays Ceres Ridley's getaway wing frames leave behind for
    /// every later Samus; it outlives every room and every runtime.
    /// </summary>
    public required UnauthoredBoostCadence UnauthoredBoostCadence { get; init; }

    /// <summary>
    /// The Shaktool segment a hit last killed, whose emptied remains the rest of its chain reads for the rest of that
    /// update; a new one holds no kill.
    /// </summary>
    public required KilledShaktoolSegment<RoomEnemySlot, ShaktoolSegmentState> KilledShaktoolSegment { get; init; }

    /// <summary>
    /// The runtime's measure of how far the camera last saw Samus move sideways, plus one pixel, which a kicked Yard flies
    /// off with; the runtime's scrolling records it and it outlives every room.
    /// </summary>
    public required YardKickTrackingDistance YardKickDistance { get; init; }
}
