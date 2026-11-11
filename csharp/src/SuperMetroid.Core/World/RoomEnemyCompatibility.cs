using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.Core.World;

/// <summary>
/// The compatibility services a room's enemy system consults. The room's loader creates a fresh set with each enemy
/// system it builds, and the set lives exactly as long as that system: a room's power-bomb sweep, its Shaktool chain
/// and its slots start with no earlier kill and no earlier occupant.
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
    /// The Shaktool segment a hit last killed, whose emptied remains the rest of its chain reads for the rest of that
    /// update; a new one holds no kill.
    /// </summary>
    public required KilledShaktoolSegment<RoomEnemySlot, ShaktoolSegmentState> KilledShaktoolSegment { get; init; }
}
