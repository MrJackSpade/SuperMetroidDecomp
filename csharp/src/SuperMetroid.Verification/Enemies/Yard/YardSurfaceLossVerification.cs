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
    /// The Yard crawls in open air from its first miss, so every crawl misses its surface.
    /// </summary>
    internal static void VerifyYardSurfaceLoss()
    {
        RoomLevelData air = VerificationHelpersVerification.CreateEmptyRoom(new(64, 64));
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
            };
            for (int miss = 1; miss <= 4; miss++)
            {
                enemies.YardBehavior.RunYardCrawlingMovement(place, state, air);
                string context = $"Yard missing its surface for the {miss} time in a row";
                AssertEqual(miss < 4 ? YardBehavior.NormalCrawling : YardBehavior.Dropped, state.Behavior, $"{context}: behavior");
                AssertEqual(miss < 4 ? YardMovement.Crawling : YardMovement.Airborne, state.Movement, $"{context}: movement");
            }
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
