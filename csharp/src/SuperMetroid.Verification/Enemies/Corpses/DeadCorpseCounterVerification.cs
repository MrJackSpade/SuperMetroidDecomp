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
        var state = new DeadSidehopperEnemyState(new CorpseRotSchedule(DeadSidehopperCorpseDefinitions.RowCount), DeadSidehopperAction.PreRotDelay, SidehopperDrain.PlacedDead());

        state.Action = DeadSidehopperAction.PreRotDelay;
        for (int call = 1; call < 16; call++)
            enemies.DeadSidehopperBehavior.RunDeadSidehopperState(place, state, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), default);
        AssertEqual(15, state.PreRotDelay.Age, "fifteen pre-rot calls");
        AssertEqual(DeadSidehopperAction.PreRotDelay, state.Action, "the delay holds below sixteen");
        AssertTrue(!place.Contact.HasAny(EnemyContact.IgnoresSamus), "the delay keeps Samus collision");
        enemies.DeadSidehopperBehavior.RunDeadSidehopperState(place, state, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), default);
        AssertTrue(state.PreRotDelay.IsOver, "the sixteenth call ends the delay");
        AssertEqual(DeadSidehopperAction.Rotting, state.Action, "the sixteenth call starts rotting");
        AssertTrue(place.Contact.HasAny(EnemyContact.IgnoresSamus), "rotting stops further Samus collision");
        Console.WriteLine("  Dead sidehopper pre-rot delay: the sixteenth call through the real dispatcher starts the rot.");
    }

    /// <summary>Runs the corpse pre-rot counter checks; no cartridge is needed.</summary>
    [VerificationCommand("--dead-corpse-counters")]
    internal static int DeadCorpseCounterCommand()
    {
        Suite(nameof(VerifyDeadSidehopperPreRotDelay), () => VerifyDeadSidehopperPreRotDelay());
        return 0;
    }
}
