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
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Zoomer, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnParameter1(EnemyDefinitionId.Zoomer, 2);
        slot.Contact = EnemyContact.None;
        slot.Processing = EnemyProcessing.None;
        slot.Presence = EnemyPresence.None;
        CrawlersBehavior.InitializeCrawler(slot, CrawlerAnimationFamily.Shared, null);
        CrawlerEnemyState state = enemies.CrawlerStates[0] ?? throw new InvalidOperationException("The crawler has no state.");
        int speed = CrawlerSpeeds.ForParameter(2);
        AssertEqual(new EightEightVector<RoomPixels>(new SignedEightEight(unchecked((ushort)-speed)), new SignedEightEight(speed)),
            state.Velocity, "the reset speed with the X sign the properties select");
        AssertEqual<CrawlerAction?>(CrawlerAction.AwaitingAnimationCue, state.Action, "a new crawler waits for its instruction");
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
