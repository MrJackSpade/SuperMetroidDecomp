using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties a Phantoon part's own state.</summary>
internal static class PhantoonRecordStateVerification
{
    /// <summary>
    /// Confirms that a kill empties every value of the killed part's own state (#1476, #1542), and leaves another
    /// part's state alone.
    /// </summary>
    internal static void VerifyPhantoonRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot bodySlot = enemies.SlotTable.Slots[0];
        RoomEnemySlot eyeSlot = enemies.SlotTable.Slots[1];
        var body = new PhantoonBodyState();
        var eye = new PhantoonEyeState();
        bodySlot.SetSpeciesState(body, body.ClearWithRecord);
        eyeSlot.SetSpeciesState(eye, eye.ClearWithRecord);

        body.PathCursor = 533;
        body.FigureEightSpeed = SignedSixteenSixteen.FromParts(1, 0x8000);
        body.SpeedPhase = 3;
        body.FunctionTimer = 120;
        body.Function = PhantoonAiFunction.SpawnStartingFlames;
        eye.FadeComplete = true;
        eye.ClosedTimer = 40;
        bodySlot.Clear();
        AssertEqual(0, body.PathCursor, "a kill empties the path cursor");
        AssertEqual(SignedSixteenSixteen.Zero, body.FigureEightSpeed, "a kill empties the speed");
        AssertEqual(0, body.SpeedPhase, "a kill empties the speed phase");
        AssertEqual(0, body.FunctionTimer, "a kill empties the function timer");
        AssertEqual(default(PhantoonAiFunction), body.Function, "a kill empties the function");
        AssertTrue(eye.FadeComplete, "the body's kill leaves the eye's fade-complete flag alone");
        AssertEqual(40, eye.ClosedTimer, "the body's kill leaves the eye's closed timer alone");
        eyeSlot.Clear();
        AssertTrue(!eye.FadeComplete, "a kill empties the fade-complete flag");
        AssertEqual(0, eye.ClosedTimer, "a kill empties the closed timer");
        Console.WriteLine("Phantoon record state: a kill empties the killed part's own state and leaves the others alone.");
    }

    /// <summary>Runs the focused Phantoon record-state check.</summary>
    [VerificationCommand("--phantoon-record-state")]
    internal static int PhantoonRecordStateCommand()
    {
        VerifyPhantoonRecordState();
        return 0;
    }
}
