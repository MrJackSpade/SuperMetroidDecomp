using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Dead Torizo values the cartridge keeps in the enemy's common record.</summary>
internal static class DeadTorizoRecordStateVerification
{
    /// <summary>Installs a Dead Torizo state with its record-clearing operation, sets every record value and confirms a kill empties each.</summary>
    internal static void VerifyDeadTorizoRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new DeadTorizoEnemyState(slot, new CorpseRottingProgram { YLimit = 0, LateMoveEntryIndex = 0, WrapOffset = 0 });
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = DeadTorizoFunction.Rotting;
        state.Velocity = new(new SnesSignedEightEight(2), new SnesSignedEightEight(9));

        slot.Clear();
        AssertEqual<DeadTorizoFunction?>(null, state.Function, "a kill empties the function");
        AssertEqual(default(EightEightVector<RoomPixels>), state.Velocity, "a kill empties the velocity");
        Console.WriteLine("Dead Torizo record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Dead Torizo record-state check.</summary>
    [VerificationCommand("--dead-torizo-record-state")]
    internal static int DeadTorizoRecordStateCommand()
    {
        VerifyDeadTorizoRecordState();
        return 0;
    }
}
