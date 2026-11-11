using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.Core.World;

/// <summary>The compatibility services a verification case hands an enemy system it builds without a room load.</summary>
internal static class RoomEnemyCompatibilityFixtures
{
    /// <summary>
    /// A fresh set, as a room load creates for each enemy system: no earlier kill, no earlier slot occupant and no
    /// sprite-object place taken, over a fade step count and an unauthored Speed Booster cadence of the case's own.
    /// </summary>
    internal static RoomEnemyCompatibility ForNewRoom() =>
        ForNewRoom(new SharedFadeStepCount(), new UnauthoredBoostCadence());

    /// <summary>
    /// A fresh set of the room's own services, as a room load creates for each enemy system, over the fade step count and
    /// the unauthored Speed Booster cadence the case's enemies share with the rest of its game, as a room load hands on
    /// its runtime's.
    /// </summary>
    /// <param name="fadeStepCount">The fade step count the case's enemies share, such as its runtime's.</param>
    /// <param name="unauthoredBoostCadence">The unauthored Speed Booster cadence the case shares with its Samus.</param>
    internal static RoomEnemyCompatibility ForNewRoom(SharedFadeStepCount fadeStepCount, UnauthoredBoostCadence unauthoredBoostCadence) => new()
    {
        PowerBombRadius = new PowerBombRadiusAfterKill(),
        BullInheritance = new SlotInheritance<BullEnemyState>(EnemySlotOrder.Capacity),
        BeetomInheritance = new SlotInheritance<BeetomEnemyState>(EnemySlotOrder.Capacity),
        PuyoCeilingTurn = new PuyoCeilingTurnFromLastDraw(),
        SpriteObjectPlaces = new SpriteObjectPlaces(),
        FadeStepCount = fadeStepCount,
        UnauthoredBoostCadence = unauthoredBoostCadence,
        KilledShaktoolSegment = new KilledShaktoolSegment<RoomEnemySlot, ShaktoolSegmentState>(),
    };
}
