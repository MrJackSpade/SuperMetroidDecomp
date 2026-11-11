using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.Core.World;

/// <summary>The compatibility services a verification case hands an enemy system it builds without a room load.</summary>
internal static class RoomEnemyCompatibilityFixtures
{
    /// <summary>A fresh set, as a room load creates for each enemy system: no earlier kill and no earlier slot occupant.</summary>
    internal static RoomEnemyCompatibility ForNewRoom() => new()
    {
        PowerBombRadius = new PowerBombRadiusAfterKill(),
        BullInheritance = new SlotInheritance<BullEnemyState>(EnemySlotOrder.Capacity),
        BeetomInheritance = new SlotInheritance<BeetomEnemyState>(EnemySlotOrder.Capacity),
        PuyoCeilingTurn = new PuyoCeilingTurnFromLastDraw(),
        KilledShaktoolSegment = new KilledShaktoolSegment<RoomEnemySlot, ShaktoolSegmentState>(),
    };
}
