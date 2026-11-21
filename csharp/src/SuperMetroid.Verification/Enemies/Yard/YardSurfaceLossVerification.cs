using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Yard;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

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
        RoomLevelData air = VerificationHelpersVerification.CreateEmptyRoom(new(64, 64));
        foreach ((int counted, int expected, YardBehavior expectedBehavior, YardMovement expectedMovement) in
                 new VerifyYardSurfaceLossCase[]
                 {
                     new VerifyYardSurfaceLossCase(2, 3, YardBehavior.NormalCrawling, YardMovement.Crawling),
                     new VerifyYardSurfaceLossCase(3, 4, YardBehavior.Dropped, YardMovement.Airborne),
                 })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Position = place.Position.WithWhole(new(512, 512));
            place.Radius = new(8, 8);
            var state = new YardEnemyState
            {
                Motion = YardMotion.Crawling,
                Crawl = YardDirection.OnFloorMovingRight,
                Behavior = YardBehavior.NormalCrawling,
                CrawlingVelocity = new PixelVelocity<RoomPixels>(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(256), SignedEightEight.FromTwoHundredFiftySixthsOfPixel(256)),
                ConsecutiveTurnCounter = counted,
            };

            enemies.YardBehavior.RunYardCrawlingMovement(place, state, air);

            string context = $"Yard missing its surface after {counted} misses";
            AssertEqual(expected, state.ConsecutiveTurnCounter, $"{context}: count");
            AssertEqual(expectedBehavior, state.Behavior, $"{context}: behavior");
            AssertEqual(expectedMovement, state.Movement, $"{context}: movement");
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
