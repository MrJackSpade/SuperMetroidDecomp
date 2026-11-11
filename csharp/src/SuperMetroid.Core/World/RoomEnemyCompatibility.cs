using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Bull;

namespace SuperMetroid.Core.World;

/// <summary>
/// The compatibility services a room's enemy system consults. The room's own services the room's loader creates fresh
/// with each enemy system it builds, and they live exactly as long as that system: a room's power-bomb sweep, its slots
/// and its sprite-object places start with no earlier kill, no earlier occupant and no place taken. The services that
/// outlive every room, the runtime's shared fade step count and the session's unauthored Speed Booster cadence, the loader
/// hands on to every enemy system it builds.
/// </summary>
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
    /// The room's thirty-two sprite-object places, whose place a dud flash on Kraid's mouth takes bounds the rest of that
    /// collision pass; a new room starts with every place free.
    /// </summary>
    public required SpriteObjectPlaces SpriteObjectPlaces { get; init; }

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
}
