using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the DeadSidehopper values the cartridge keeps in the enemy's common record.</summary>
internal static class DeadSidehopperRecordStateVerification
{
    /// <summary>
    /// Installs a DeadSidehopper state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyDeadSidehopperRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new DeadSidehopperEnemyState(slot, 0, new CorpseRottingProgram { YLimit = 0, LateMoveEntryIndex = 0, WrapOffset = 0 });
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<DeadSidehopperAiFunction>()[^1];
        state.PreRotDelayCounter = 1;
        state.StateTimer = NativeCountdown.Start(1);

        slot.Clear();
        AssertEqual<DeadSidehopperAiFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<int>(0, state.PreRotDelayCounter, "a kill empties PreRotDelayCounter");
        AssertEqual<NativeCountdown>(NativeCountdown.Start(0), state.StateTimer, "a kill empties StateTimer");
        Console.WriteLine("DeadSidehopper record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused DeadSidehopper record-state check.</summary>
    [VerificationCommand("--dead-sidehopper-record-state")]
    internal static int DeadSidehopperRecordStateCommand()
    {
        VerifyDeadSidehopperRecordState();
        return 0;
    }
}
