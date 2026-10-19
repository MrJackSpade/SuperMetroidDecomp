using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the Shitroid values the cartridge keeps in the enemy's common record.</summary>
internal static class ShitroidRecordStateVerification
{
    /// <summary>
    /// Installs a Shitroid state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyShitroidRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new ShitroidEnemyState(slot);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<ShitroidAiFunction>()[^1];
        state.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(1), default);
        state.PalettePhase = 1;
        state.PaletteTimer = 1;
        state.PaletteDelay = 1;
        state.StateTimer = NativeCountdown.Start(1);

        slot.Clear();
        AssertEqual<ShitroidAiFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<EightEightVector<RoomPixels>>(default(EightEightVector<RoomPixels>), state.Velocity, "a kill empties Velocity");
        AssertEqual<int>(0, state.PalettePhase, "a kill empties PalettePhase");
        AssertEqual<int>(0, state.PaletteTimer, "a kill empties PaletteTimer");
        AssertEqual<int>(0, state.PaletteDelay, "a kill empties PaletteDelay");
        AssertEqual<NativeCountdown>(NativeCountdown.Start(0), state.StateTimer, "a kill empties StateTimer");
        Console.WriteLine("Shitroid record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Shitroid record-state check.</summary>
    [VerificationCommand("--shitroid-record-state")]
    internal static int ShitroidRecordStateCommand()
    {
        VerifyShitroidRecordState();
        return 0;
    }
}
