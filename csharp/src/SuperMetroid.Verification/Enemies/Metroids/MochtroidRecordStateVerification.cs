using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metroids;
namespace SuperMetroid.Verification.Enemies.Metroids;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the Mochtroid values the cartridge keeps in the enemy's common record.</summary>
internal static class MochtroidRecordStateVerification
{
    /// <summary>
    /// Installs a Mochtroid state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyMochtroidRecordState()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new MochtroidEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Velocity = FixedVector<RoomPixels>.FromWords(1, 0, 0, 0);
        state.ShakeTimer = NativeCountdown.Start(1);
        state.MovementMode = Enum.GetValues<MochtroidMovementMode>()[^1];

        slot.Clear();
        AssertEqual<FixedVector<RoomPixels>>(FixedVector<RoomPixels>.Zero, state.Velocity, "a kill empties Velocity");
        AssertEqual<NativeCountdown>(NativeCountdown.Start(0), state.ShakeTimer, "a kill empties ShakeTimer");
        AssertEqual<MochtroidMovementMode>(default(MochtroidMovementMode), state.MovementMode, "a kill empties MovementMode");
        Console.WriteLine("Mochtroid record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Mochtroid record-state check.</summary>
    [VerificationCommand("--mochtroid-record-state")]
    internal static int MochtroidRecordStateCommand()
    {
        VerifyMochtroidRecordState();
        return 0;
    }
}
