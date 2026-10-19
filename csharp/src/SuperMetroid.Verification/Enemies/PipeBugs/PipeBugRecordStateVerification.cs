using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.PipeBugs;
namespace SuperMetroid.Verification.Enemies.PipeBugs;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the Pipe Bug values the cartridge keeps in the enemy's common record.</summary>
internal static class PipeBugRecordStateVerification
{
    /// <summary>
    /// Installs a Pipe Bug state with its record-clearing operation, sets its function and Brinstar flight direction,
    /// and confirms a kill empties both.
    /// </summary>
    internal static void VerifyPipeBugRecordState()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new PipeBugEnemyState(slot);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<PipeBugEnemyFunction>()[^1];
        state.BrinstarFliesLeft = true;

        slot.Clear();
        AssertEqual<PipeBugEnemyFunction?>(null, state.Function, "a kill empties the function");
        AssertEqual(false, state.BrinstarFliesLeft, "a kill empties the Brinstar flight direction");
        Console.WriteLine("PipeBug record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Pipe Bug record-state check.</summary>
    [VerificationCommand("--pipe-bug-record-state")]
    internal static int PipeBugRecordStateCommand()
    {
        VerifyPipeBugRecordState();
        return 0;
    }
}
