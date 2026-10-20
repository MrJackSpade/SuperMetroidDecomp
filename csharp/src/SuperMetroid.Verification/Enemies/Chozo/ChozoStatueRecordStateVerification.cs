using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;
namespace SuperMetroid.Verification.Enemies.Chozo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the ChozoStatue values the cartridge keeps in the enemy's common record.</summary>
internal static class ChozoStatueRecordStateVerification
{
    /// <summary>
    /// Installs a ChozoStatue state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyChozoStatueRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new ChozoStatueState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.CarryRecord = 1;
        state.PreInstruction = Enum.GetValues<ChozoStatuePreInstruction>()[^1];

        slot.Clear();
        AssertEqual<int>(0, state.CarryRecord, "a kill empties CarryRecord");
        AssertEqual<ChozoStatuePreInstruction?>(null, state.PreInstruction, "a kill empties PreInstruction");
        Console.WriteLine("ChozoStatue record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused ChozoStatue record-state check.</summary>
    [VerificationCommand("--chozo-statue-record-state")]
    internal static int ChozoStatueRecordStateCommand()
    {
        VerifyChozoStatueRecordState();
        return 0;
    }
}
