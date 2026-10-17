using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
namespace SuperMetroid.Verification.Enemies.Crawlers;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the crawler state the families own: the initial velocity reset and the cleared record.</summary>
internal static class CrawlerOwnedStateVerification
{
    /// <summary>
    /// Initializes a Zoomer whose properties select a reversed X and confirms its velocity is the parameter's speed on
    /// both axes with that sign, then confirms a kill empties the velocity and behaviour kept in the common record while
    /// the saved surface behaviour outlives it.
    /// </summary>
    internal static void VerifyCrawlerOwnedState()
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Zoomer, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = CrawlerFrameToolingDefinitions.Bank });
        slot.Parameter1 = 2;
        slot.Properties = EnemyPropertyWord.FromNativeWord(0);
        CrawlersBehavior.InitializeCrawler(slot, CrawlerAnimationFamily.Shared, null);
        CrawlerEnemyState state = enemies.CrawlerStates[0] ?? throw new InvalidOperationException("The crawler has no state.");
        ushort speed = CrawlerSpeedDefinitions.ForParameter(2);
        AssertEqual(new EightEightVector<RoomPixels>(new SnesSignedEightEight(unchecked((ushort)-speed)), new SnesSignedEightEight(speed)),
            state.Velocity, "the reset speed with the X sign the properties select");
        AssertEqual<CrawlerEnemyFunction?>(CrawlerEnemyFunction.InstructionPending, state.Function, "a new crawler waits for its instruction");

        state.NonFallingFunction = CrawlerEnemyFunction.CrawlingVertically;
        slot.Clear();
        AssertEqual(default(EightEightVector<RoomPixels>), state.Velocity, "a kill empties the velocity");
        AssertEqual<CrawlerEnemyFunction?>(null, state.Function, "a kill empties the behaviour");
        AssertEqual<CrawlerEnemyFunction?>(CrawlerEnemyFunction.CrawlingVertically, state.NonFallingFunction, "the saved surface behaviour outlives the kill");
        Console.WriteLine("Crawler owned state: the velocity reset and the cleared record match.");
    }

    /// <summary>Runs the focused crawler owned-state check.</summary>
    [VerificationCommand("--crawler-owned-state")]
    internal static int CrawlerOwnedStateCommand()
    {
        VerifyCrawlerOwnedState();
        return 0;
    }
}
