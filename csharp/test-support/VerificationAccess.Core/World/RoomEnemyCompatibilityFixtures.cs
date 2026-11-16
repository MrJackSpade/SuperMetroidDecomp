using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Bull;

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
    /// A fresh set of the room's own services, as <see cref="ForNewRoom(SharedFadeStepCount, UnauthoredBoostCadence)"/>
    /// builds, over a Yard kick camera measure of the case's own.
    /// </summary>
    /// <param name="fadeStepCount">The fade step count the case's enemies share, such as its runtime's.</param>
    /// <param name="unauthoredBoostCadence">The unauthored Speed Booster cadence the case shares with its Samus.</param>
    internal static RoomEnemyCompatibility ForNewRoom(SharedFadeStepCount fadeStepCount, UnauthoredBoostCadence unauthoredBoostCadence) =>
        ForNewRoom(fadeStepCount, unauthoredBoostCadence, new YardKickTrackingDistance());

    /// <summary>
    /// A fresh set of the room's own services, as a room load creates for each enemy system, over the fade step count,
    /// the unauthored Speed Booster cadence and the Yard kick camera measure the case's enemies share with the rest of its
    /// game, as a room load hands on its runtime's.
    /// </summary>
    /// <param name="fadeStepCount">The fade step count the case's enemies share, such as its runtime's.</param>
    /// <param name="unauthoredBoostCadence">The unauthored Speed Booster cadence the case shares with its Samus.</param>
    /// <param name="yardKickDistance">The camera measure the case's kicked Yards fly off with, such as its runtime's.</param>
    internal static RoomEnemyCompatibility ForNewRoom(SharedFadeStepCount fadeStepCount, UnauthoredBoostCadence unauthoredBoostCadence,
        YardKickTrackingDistance yardKickDistance) => new()
    {
        PowerBombRadius = new PowerBombRadiusAfterKill(),
        BullInheritance = new PlaceInheritance<BullEnemyState>(EnemyPlaceOrder.Capacity),
        BeetomInheritance = new PlaceInheritance<BeetomEnemyState>(EnemyPlaceOrder.Capacity),
        PuyoCeilingTurn = new PuyoCeilingTurnFromLastDraw(),
        EffectActorPlaces = new EffectActorPlaces(),
        FadeStepCount = fadeStepCount,
        UnauthoredBoostCadence = unauthoredBoostCadence,
        YardKickDistance = yardKickDistance,
        FallingSparkFloorProbe = new FallingSparkFloorProbe(),
    };
}
