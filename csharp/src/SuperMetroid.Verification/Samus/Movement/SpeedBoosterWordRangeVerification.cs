using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>
/// Checks the Speed Booster counter's stage borrow and range, and the speed echo index, speed divisor and deceleration
/// multiplier now that they are stored as ordinary integers, driven through the production operations.
/// </summary>
internal static class SpeedBoosterWordRangeVerification
{
    /// <summary>Exercises the cadence borrow, the counter's range, the echo index's alternation and departure, and the two widened quantities' ranges.</summary>
    internal static void VerifySpeedBoosterWordRange()
    {
        // A cadence countdown at zero borrows from the stage: stage two, cadence zero becomes stage one, cadence 255,
        // which is not a stage advance.
        foreach ((SamusSpeedBoostCounter before, SamusSpeedBoostCounter expected) in new[]
        {
            new VerifySpeedBoosterWordRangeCase(new SamusSpeedBoostCounter(2, 0), new SamusSpeedBoostCounter(1, 255)),
            new VerifySpeedBoosterWordRangeCase(new SamusSpeedBoostCounter(4, 0), new SamusSpeedBoostCounter(3, 255)),
        })
        {
            var speed = new SamusHorizontalSpeedState { HasRunningMomentum = true, SpeedBoostCounter = before };
            int frame = 7;
            bool advanced = speed.TryAdvanceSpeedBoosterAnimationStage(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusMovementType.Running, ControllerButton.B,
                animationFrameBuffer: 0, ref frame, out _, requestEchoSound: () => null);
            AssertTrue(!advanced, $"counter {before}: a borrowing countdown leaves a nonzero cadence, so no stage advances");
            AssertEqual(expected, speed.SpeedBoostCounter, $"counter {before}: the countdown borrows a stage");
        }

        // Stage zero with no cadence has no stage to borrow from.
        var empty = new SamusHorizontalSpeedState { HasRunningMomentum = true, SpeedBoostCounter = new SamusSpeedBoostCounter(0, 0) };
        int emptyFrame = 7;
        AssertThrows<InvalidOperationException>(() => empty.TryAdvanceSpeedBoosterAnimationStage(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusMovementType.Running,
            ControllerButton.B, animationFrameBuffer: 0, ref emptyFrame, out _, requestEchoSound: () => null), "a stage-zero counter with no cadence cannot borrow");

        // A cadence of one reaching zero advances the stage and reloads the cadence.
        var advancing = new SamusHorizontalSpeedState { HasRunningMomentum = true, SpeedBoostCounter = new SamusSpeedBoostCounter(1, 1) };
        int advancingFrame = 7;
        AssertTrue(advancing.TryAdvanceSpeedBoosterAnimationStage(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusMovementType.Running, ControllerButton.B,
            animationFrameBuffer: 0, ref advancingFrame, out _, requestEchoSound: () => null), "cadence one expires and advances the stage");
        AssertEqual(new SamusSpeedBoostCounter(2, SamusRunningCadences.CountdownAfterAdvance(2)), advancing.SpeedBoostCounter,
            "stage two with its reloaded cadence");

        var range = new SamusHorizontalSpeedState();
        AssertThrows<ArgumentOutOfRangeException>(() => range.SpeedBoostCounter = new SamusSpeedBoostCounter(SamusSpeedBoostCounter.FullStage + 1, 0),
            "no stage lies past full boost");
        AssertThrows<ArgumentOutOfRangeException>(() => range.SpeedBoostCounter = new SamusSpeedBoostCounter(0, byte.MaxValue + 1),
            "the cadence never exceeds 255");
        AssertThrows<ArgumentOutOfRangeException>(() => range.SpeedBoostCounter = new SamusSpeedBoostCounter(-1, 0), "the stage is never negative");
        AssertThrows<ArgumentOutOfRangeException>(() => range.SpeedDivisor = SamusHorizontalSpeedState.LargestSpeedDivisor + 1,
            "no more goop sticks to Samus than five");
        range.DecelerationMultiplier = SuperMetroid.Core.Compatibility.MultipliedDeceleration.LargestMultiplier;
        AssertEqual(SuperMetroid.Core.Compatibility.MultipliedDeceleration.LargestMultiplier, range.DecelerationMultiplier,
            "the multiplier holds the largest multiplier the multiplied deceleration scales by");
        AssertThrows<ArgumentOutOfRangeException>(() => range.DecelerationMultiplier = SuperMetroid.Core.Compatibility.MultipliedDeceleration.LargestMultiplier + 1,
            "the multiplier stays below a whole deceleration");

        // A full boost captures into the first slot, then the second, and the step past it returns the cursor to the first.
        var boosted = new SamusHorizontalSpeedState
        {
            SpeedBoostCounter = SamusSpeedBoostCounter.AtFullStage,
        };
        AssertTrue(!boosted.SpeedEchoesDeparting, "a Samus who has not boosted is not departing");
        AssertTrue(boosted.CaptureSpeedEchoPosition(0, new(40, 50)), "a full boost captures on a fourth frame");
        AssertEqual(new Position16<RoomPixels>(40, 50), boosted.FirstSpeedEchoPosition, "the first capture fills the first slot");
        AssertEqual(SpeedEchoCursor.SecondSlot, boosted.SpeedEchoIndex, "the first capture moves the cursor to the second slot");
        AssertTrue(boosted.CaptureSpeedEchoPosition(4, new(44, 50)), "the next fourth frame captures again");
        AssertEqual(new Position16<RoomPixels>(44, 50), boosted.SecondSpeedEchoPosition, "the second capture fills the second slot");
        AssertEqual(SpeedEchoCursor.FirstSlot, boosted.SpeedEchoIndex, "stepping past the second slot returns the cursor to the first");

        // Cancellation publishes the departure index, which stops capture.
        boosted.CancelRunningMomentum(SamusFacingDirection.Right);
        AssertEqual(SpeedEchoCursor.Departing, boosted.SpeedEchoIndex, "cancellation sends the echoes departing");
        AssertTrue(boosted.SpeedEchoesDeparting, "the departing cursor is the departure state");
        AssertTrue(!boosted.CaptureSpeedEchoPosition(4, new(60, 70)), "departing echoes are not recaptured");
    }

    /// <summary>Runs the focused Speed Booster word range check.</summary>
    [VerificationCommand("--speed-booster-word-range")]
    internal static int SpeedBoosterWordRangeCommand()
    {
        Suite(nameof(VerifySpeedBoosterWordRange), () => VerifySpeedBoosterWordRange());
        return 0;
    }
}
