using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the Mother Brain falling tube values the cartridge keeps in the enemy's common record.</summary>
internal static class MotherBrainFallingTubeRecordStateVerification
{
    /// <summary>Installs a Mother Brain falling tube state with its record-clearing operation, sets every record value and confirms a kill empties each.</summary>
    internal static void VerifyMotherBrainFallingTubeRecordState()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new MotherBrainFallingTubeState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = MotherBrainTubeFunction.MainTube_Falling;
        state.FloorY = 208;
        state.FallSpeed = new SnesSignedEightEight(6);
        state.SmokeDelay = NativeCountdown.Start(8);
        state.SmokePositionIndex = 3;

        slot.Clear();
        AssertEqual<MotherBrainTubeFunction?>(null, state.Function, "a kill empties the function");
        AssertEqual(0, state.FloorY, "a kill empties the floor");
        AssertEqual(default(SnesSignedEightEight), state.FallSpeed, "a kill empties the fall speed");
        AssertEqual(NativeCountdown.Start(0), state.SmokeDelay, "a kill empties the smoke delay");
        AssertEqual(0, state.SmokePositionIndex, "a kill empties the smoke position");
        Console.WriteLine("Mother Brain falling tube record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Mother Brain falling tube record-state check.</summary>
    [VerificationCommand("--mother-brain-falling-tube-record-state")]
    internal static int MotherBrainFallingTubeRecordStateCommand()
    {
        VerifyMotherBrainFallingTubeRecordState();
        return 0;
    }
}
