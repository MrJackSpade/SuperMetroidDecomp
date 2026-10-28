using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MorphBallEye;
namespace SuperMetroid.Verification.Enemies.MorphBallEye;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the MorphBallEye values the cartridge keeps in the enemy's common record.</summary>
internal static class MorphBallEyeRecordStateVerification
{
    /// <summary>
    /// Installs a MorphBallEye state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyMorphBallEyeRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new MorphBallEyeEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.IsActivated = true;
        state.Angle = RotationAngle.FromTableIndex(1);
        state.FunctionTimer = WrappingCountdown.Start(1);
        state.Function = Enum.GetValues<MorphBallEyeAiFunction>()[^1];

        slot.Clear();
        AssertEqual<bool>(false, state.IsActivated, "a kill empties IsActivated");
        AssertEqual<RotationAngle>(RotationAngle.FromTableIndex(0), state.Angle, "a kill empties Angle");
        AssertEqual<WrappingCountdown>(WrappingCountdown.Start(0), state.FunctionTimer, "a kill empties FunctionTimer");
        AssertEqual<MorphBallEyeAiFunction?>(null, state.Function, "a kill empties Function");
        Console.WriteLine("MorphBallEye record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused MorphBallEye record-state check.</summary>
    [VerificationCommand("--morph-ball-eye-record-state")]
    internal static int MorphBallEyeRecordStateCommand()
    {
        VerifyMorphBallEyeRecordState();
        return 0;
    }
}
