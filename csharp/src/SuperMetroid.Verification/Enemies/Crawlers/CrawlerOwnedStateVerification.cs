using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Crawlers;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the crawler state the families own: the initial velocity reset.</summary>
internal static class CrawlerOwnedStateVerification
{
    /// <summary>
    /// Initializes a Zoomer whose properties select a reversed X and confirms its velocity is the parameter's speed on
    /// both axes with that sign.
    /// </summary>
    internal static void VerifyCrawlerOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Zoomer, place.Definition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        place.SetSpawnParameter1(EnemyDefinitionId.Zoomer, 2);
        place.Contact = EnemyContact.None;
        place.Processing = EnemyProcessing.None;
        place.Presence = EnemyPresence.None;
        CrawlersBehavior.InitializeCrawler(place.HeldOccupant(), place, CrawlerAnimationFamily.Shared);
        CrawlerEnemyState state = enemies.CrawlerStates[0] ?? throw new InvalidOperationException("The crawler has no state.");
        int speed = CrawlerSpeeds.ForParameter(2);
        AssertEqual(new PixelVelocity<RoomPixels>(new SignedEightEight(unchecked((ushort)-speed)), new SignedEightEight(speed)),
            state.Velocity, "the reset speed with the X sign the properties select");
        AssertEqual<CrawlerAction?>(CrawlerAction.Settling, state.Action, "a new crawler settles before it crawls");
        Console.WriteLine("Crawler owned state: the velocity reset matches.");
    }

    /// <summary>Runs the focused crawler owned-state check.</summary>
    [VerificationCommand("--crawler-owned-state")]
    internal static int CrawlerOwnedStateCommand()
    {
        VerifyCrawlerOwnedState();
        return 0;
    }
}
