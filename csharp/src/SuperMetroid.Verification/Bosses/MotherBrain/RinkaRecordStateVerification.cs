using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the Rinka values the cartridge keeps in the enemy's common record.</summary>
internal static class RinkaRecordStateVerification
{
    /// <summary>
    /// Installs a Rinka state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyRinkaRecordState()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new RinkaEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<RinkaEnemyFunction>()[^1];
        state.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(1), default);
        state.SpawnResourceToken = 1;
        state.DelayTimer = NativeCountdown.Start(1);

        slot.Clear();
        AssertEqual<RinkaEnemyFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<EightEightVector<RoomPixels>>(default(EightEightVector<RoomPixels>), state.Velocity, "a kill empties Velocity");
        AssertEqual<int>(0, state.SpawnResourceToken, "a kill empties SpawnResourceToken");
        AssertEqual<NativeCountdown>(NativeCountdown.Start(0), state.DelayTimer, "a kill empties DelayTimer");
        Console.WriteLine("Rinka record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Rinka record-state check.</summary>
    [VerificationCommand("--rinka-record-state")]
    internal static int RinkaRecordStateCommand()
    {
        VerifyRinkaRecordState();
        return 0;
    }
}
