using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the corpse pre-rot contact counters, whose cartridge word wraps from 65535 to zero.</summary>
internal static class DeadCorpseCounterVerification
{
    /// <summary>
    /// Drives the real dead-sidehopper dispatcher through the pre-rot delay at both sides of the counter's wrap: a
    /// retained count of 65534 reaches 65535 and starts rotting at once, while a retained count of 65535 wraps to zero
    /// and restarts the full sixteen-call delay.
    /// </summary>
    internal static void VerifyDeadSidehopperPreRotDelayWrap()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new DeadSidehopperEnemyState(slot, 2,
            new CorpseRottingProgram { YLimit = 0, LateMoveEntryIndex = 0, WrapOffset = 0 });

        state.Function = DeadSidehopperAiFunction.PreRotDelay;
        state.PreRotDelayCounter = DeadSidehopperEnemyState.LargestPreRotDelayCount - 1;
        enemies.DeadSidehopperBehavior.DispatchDeadSidehopperState(slot, state, null, null, default);
        AssertEqual(DeadSidehopperEnemyState.LargestPreRotDelayCount, state.PreRotDelayCounter,
            "a retained count below the wrap advances to the largest count");
        AssertEqual(DeadSidehopperAiFunction.Rotting, state.Function, "the largest count passes the sixteen-call threshold");
        AssertTrue(slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "rotting stops further Samus collision");

        slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Function = DeadSidehopperAiFunction.PreRotDelay;
        state.PreRotDelayCounter = DeadSidehopperEnemyState.LargestPreRotDelayCount;
        enemies.DeadSidehopperBehavior.DispatchDeadSidehopperState(slot, state, null, null, default);
        AssertEqual(0, state.PreRotDelayCounter, "the largest retained count wraps to zero");
        AssertEqual(DeadSidehopperAiFunction.PreRotDelay, state.Function, "the wrapped count restarts the pre-rot delay");
        AssertTrue(!slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "the restarted delay keeps Samus collision");

        for (int call = 1; call < 16; call++)
            enemies.DeadSidehopperBehavior.DispatchDeadSidehopperState(slot, state, null, null, default);
        AssertEqual(15, state.PreRotDelayCounter, "fifteen more calls after the wrap");
        AssertEqual(DeadSidehopperAiFunction.PreRotDelay, state.Function, "the delay holds below sixteen");
        enemies.DeadSidehopperBehavior.DispatchDeadSidehopperState(slot, state, null, null, default);
        AssertEqual(16, state.PreRotDelayCounter, "the sixteenth call after the wrap");
        AssertEqual(DeadSidehopperAiFunction.Rotting, state.Function, "the sixteenth call after the wrap starts rotting");
        Console.WriteLine("  Dead sidehopper pre-rot delay: both sides of the 65535-to-0 wrap pass through the real dispatcher.");
    }

    /// <summary>Counts a dead Zoomer's pre-rot calls through the named operation, including its wrap from 65535 to zero.</summary>
    internal static void VerifyDeadTourianPreRotDelayWrap()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        DeadTourianBehavior.DeadTourianCorpseProfile profile = DeadTourianCorpseProfileDefinitions.DeadZoomerProfile;
        var state = new DeadTourianCorpseEnemyState(slot, profile.Species, 0,
            new CorpseRottingProgram { YLimit = 0, LateMoveEntryIndex = 0, WrapOffset = 0 }, profile);

        state.PreRotDelayCounter = 15;
        state.CountPreRotDelayCall();
        AssertEqual(16, state.PreRotDelayCounter, "an ordinary pre-rot call counts one more");

        state.PreRotDelayCounter = DeadTourianCorpseEnemyState.LargestPreRotDelayCount;
        state.CountPreRotDelayCall();
        AssertEqual(0, state.PreRotDelayCounter, "the largest count wraps to zero");

        state.Function = DeadTourianCorpseFunction.PreRotDelay;
        state.PreRotDelayCounter = 5;
        slot.SetSpeciesState(state, state.ClearWithRecord);
        slot.Clear();
        AssertEqual(0, state.PreRotDelayCounter, "emptying the corpse's record empties its count");
        AssertEqual(null, state.Function, "emptying the corpse's record empties its function");
        Console.WriteLine("  Dead Tourian pre-rot delay: the named count operation wraps from 65535 to 0, and an emptied record empties it.");
    }

    /// <summary>Runs the corpse pre-rot counter checks; no cartridge is needed.</summary>
    [VerificationCommand("--dead-corpse-counters")]
    internal static int DeadCorpseCounterCommand()
    {
        Suite(nameof(VerifyDeadSidehopperPreRotDelayWrap), () => VerifyDeadSidehopperPreRotDelayWrap());
        Suite(nameof(VerifyDeadTourianPreRotDelayWrap), () => VerifyDeadTourianPreRotDelayWrap());
        return 0;
    }
}
