using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Draygon's shared body word and that kills empty the body's and the eye's records.</summary>
internal static class DraygonRecordStateVerification
{
    /// <summary>
    /// Confirms the function timer and the swoop cursor are one record word, as the cartridge's are, that a body kill
    /// empties every body value, and that an eye kill empties the eye's function and look angle.
    /// </summary>
    internal static void VerifyDraygonRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        RoomEnemySlot eye = enemies.SlotTable.Slots[1];
        var state = new DraygonEnemyState(body) { Eye = eye };
        body.SetSpeciesState(state, state.ClearWithRecord);
        var eyeState = new DraygonEyeState { Function = DraygonEyeFunction.FacingLeft, LookAngle = 64 };
        eye.SetSpeciesState(eyeState, eyeState.ClearWithRecord);

        state.FunctionTimer = WrappingCountdown.Start(12);
        AssertEqual(12, state.SwoopOffset, "the function timer and the swoop cursor are one record word");

        state.Function = DraygonAiFunction.Dying;
        state.SwoopEndOffset = 40;
        state.SwoopVelocity = SixteenSixteen.Compose(2, 0x8000);
        body.Clear();
        AssertEqual((DraygonAiFunction)0, state.Function, "a body kill empties the function");
        AssertEqual(0, state.SwoopOffset, "a body kill empties the timer word");
        AssertEqual(0, state.SwoopEndOffset, "a body kill empties the swoop end");
        AssertEqual(0u, state.SwoopVelocity, "a body kill empties the velocity");
        AssertEqual<DraygonEyeFunction?>(DraygonEyeFunction.FacingLeft, eyeState.Function, "a body kill leaves the eye's record");

        eye.Clear();
        AssertEqual<DraygonEyeFunction?>(null, eyeState.Function, "an eye kill empties its function");
        AssertEqual(0, eyeState.LookAngle, "an eye kill empties its look angle");
        Console.WriteLine("Draygon record state: the timer word carries into the swoop cursor, and kills empty each record.");
    }

    /// <summary>Runs the focused Draygon record-state check.</summary>
    [VerificationCommand("--draygon-record-state")]
    internal static int DraygonRecordStateCommand()
    {
        VerifyDraygonRecordState();
        return 0;
    }
}
