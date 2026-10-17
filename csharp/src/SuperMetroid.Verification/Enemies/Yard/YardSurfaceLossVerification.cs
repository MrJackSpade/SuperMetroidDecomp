using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Verification.Enemies.Yard;
using SuperMetroid.Verification.Harness;

/// <summary>A crawling Yard's typed missing-surface count drops it on the fourth consecutive miss, as the cartridge's does.</summary>
internal static class YardSurfaceLossVerification
{
    /// <summary>
    /// Runs the production crawling movement of a floor Yard in open air, so its surface probe misses. With two misses
    /// already counted it counts a third and turns around the outside corner; with three it counts the fourth and drops.
    /// The count's slot variable holds the same value.
    /// </summary>
    internal static void VerifyYardSurfaceLoss()
    {
        var air = new RoomLevelData(new Extent16<RoomBlocks>(64, 64), new RoomLevelPlanes(new ushort[4096], new byte[4096], new ushort[4096]), new byte[8]);
        foreach ((int counted, int expected, YardBehavior expectedBehavior, YardMovementFunction expectedMovement) in
                 new (int, int, YardBehavior, YardMovementFunction)[]
                 {
                     (2, 3, YardBehavior.NormalCrawling, YardMovementFunction.CrawlingUpsideUpMovingRight),
                     (3, 4, YardBehavior.Dropped, YardMovementFunction.Airborne),
                 })
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Position = slot.Position.WithWhole(new(512, 512));
            slot.Radius = new(8, 8);
            var state = new YardEnemyState(slot)
            {
                MovementFunction = YardMovementFunction.CrawlingUpsideUpMovingRight,
                Behavior = YardBehavior.NormalCrawling,
                CrawlingVelocity = new EightEightVector<RoomPixels>(256, 256),
                ConsecutiveTurnCounter = counted,
            };

            enemies.YardBehavior.RunYardCrawlingMovement(slot, state, air);

            string context = $"Yard missing its surface after {counted} misses";
            AssertEqual(expected, state.ConsecutiveTurnCounter, $"{context}: count");
            AssertEqual(checked((ushort)expected), slot.VariableE, $"{context}: slot variable");
            AssertEqual(expectedBehavior, state.Behavior, $"{context}: behavior");
            AssertEqual(expectedMovement, state.MovementFunction, $"{context}: movement");
        }
        Console.WriteLine("  Yard surface loss: the fourth consecutive missed surface drops the Yard.");
    }

    /// <summary>Runs the Yard surface-loss check.</summary>
    [VerificationCommand("--yard-surface-loss")]
    internal static int YardSurfaceLossCommand()
    {
        Suite(nameof(VerifyYardSurfaceLoss), () => VerifyYardSurfaceLoss());
        return 0;
    }
}
