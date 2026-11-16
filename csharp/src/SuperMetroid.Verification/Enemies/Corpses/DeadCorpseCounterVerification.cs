using SuperMetroid.AssetExtraction.Enemies.Corpses;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the corpse pre-rot contact counters: sixteen calls after Samus stands on a corpse start its rot.</summary>
internal static class DeadCorpseCounterVerification
{
    /// <summary>
    /// Drives the real dead-sidehopper dispatcher through the pre-rot delay from its start: fifteen calls hold the delay
    /// with Samus still colliding, and the sixteenth starts rotting and stops further collision (#1607: the count is a
    /// plain count, since nothing counts past sixteen).
    /// </summary>
    internal static void VerifyDeadSidehopperPreRotDelay()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        var state = new DeadSidehopperEnemyState(new CorpseRotSchedule(DeadSidehopperCorpseDefinitions.RowCount), DeadSidehopperAction.PreRotDelay);

        state.Action = DeadSidehopperAction.PreRotDelay;
        for (int call = 1; call < 16; call++)
            enemies.DeadSidehopperBehavior.RunDeadSidehopperState(place, state, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), default);
        AssertEqual(15, state.PreRotDelayCounter, "fifteen pre-rot calls");
        AssertEqual(DeadSidehopperAction.PreRotDelay, state.Action, "the delay holds below sixteen");
        AssertTrue(!place.Contact.HasAny(EnemyContact.IgnoresSamus), "the delay keeps Samus collision");
        enemies.DeadSidehopperBehavior.RunDeadSidehopperState(place, state, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), default);
        AssertEqual(16, state.PreRotDelayCounter, "the sixteenth call");
        AssertEqual(DeadSidehopperAction.Rotting, state.Action, "the sixteenth call starts rotting");
        AssertTrue(place.Contact.HasAny(EnemyContact.IgnoresSamus), "rotting stops further Samus collision");
        Console.WriteLine("  Dead sidehopper pre-rot delay: the sixteenth call through the real dispatcher starts the rot.");
    }

    /// <summary>Counts a dead Zoomer's pre-rot calls through the named operation, one call at a time.</summary>
    internal static void VerifyDeadTourianPreRotDelay()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        var state = new DeadTourianCorpseEnemyState(DeadTourianCorpseSpecies.Zoomer,
            new CorpseRotSchedule(DeadTourianCorpses.For(DeadTourianCorpseSpecies.Zoomer, 0).RowCount), DeadTourianCorpsePhase.PreRotDelay);

        state.PreRotDelayCounter = 15;
        state.CountPreRotDelayCall();
        AssertEqual(16, state.PreRotDelayCounter, "an ordinary pre-rot call counts one more");
        Console.WriteLine("  Dead Tourian pre-rot delay: the named count operation counts one call.");
    }

    /// <summary>Runs the corpse pre-rot counter checks; no cartridge is needed.</summary>
    [VerificationCommand("--dead-corpse-counters")]
    internal static int DeadCorpseCounterCommand()
    {
        Suite(nameof(VerifyDeadSidehopperPreRotDelay), () => VerifyDeadSidehopperPreRotDelay());
        Suite(nameof(VerifyDeadTourianPreRotDelay), () => VerifyDeadTourianPreRotDelay());
        return 0;
    }
}
