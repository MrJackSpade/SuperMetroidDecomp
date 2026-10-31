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
    /// <summary>Exercises the cadence borrow, the counter's range, the crash residue in the echo index and the two widened quantities' ranges.</summary>
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
                animationFrameBuffer: 0, ref frame, out _);
            AssertTrue(!advanced, $"counter {before}: a borrowing countdown leaves a nonzero cadence, so no stage advances");
            AssertEqual(expected, speed.SpeedBoostCounter, $"counter {before}: the countdown borrows a stage");
        }

        // Stage zero with no cadence has no stage to borrow from.
        var empty = new SamusHorizontalSpeedState { HasRunningMomentum = true, SpeedBoostCounter = new SamusSpeedBoostCounter(0, 0) };
        int emptyFrame = 7;
        AssertThrows<InvalidOperationException>(() => empty.TryAdvanceSpeedBoosterAnimationStage(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusMovementType.Running,
            ControllerButton.B, animationFrameBuffer: 0, ref emptyFrame, out _), "a stage-zero counter with no cadence cannot borrow");

        // A cadence of one reaching zero advances the stage and reloads the cadence.
        var advancing = new SamusHorizontalSpeedState { HasRunningMomentum = true, SpeedBoostCounter = new SamusSpeedBoostCounter(1, 1) };
        int advancingFrame = 7;
        AssertTrue(advancing.TryAdvanceSpeedBoosterAnimationStage(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusMovementType.Running, ControllerButton.B,
            animationFrameBuffer: 0, ref advancingFrame, out _), "cadence one expires and advances the stage");
        AssertEqual(new SamusSpeedBoostCounter(2, SamusRunningCadences.CountdownAfterAdvance(2)), advancing.SpeedBoostCounter,
            "stage two with its reloaded cadence");

        var range = new SamusHorizontalSpeedState();
        AssertThrows<ArgumentOutOfRangeException>(() => range.SpeedBoostCounter = new SamusSpeedBoostCounter(SamusSpeedBoostCounter.FullStage + 1, 0),
            "no stage lies past full boost");
        AssertThrows<ArgumentOutOfRangeException>(() => range.SpeedBoostCounter = new SamusSpeedBoostCounter(0, byte.MaxValue + 1),
            "the cadence never exceeds 255");
        AssertThrows<ArgumentOutOfRangeException>(() => range.SpeedBoostCounter = new SamusSpeedBoostCounter(-1, 0), "the stage is never negative");
        AssertThrows<ArgumentOutOfRangeException>(() => range.SpeedDivisor = ushort.MaxValue + 1,
            "the divisor never holds more than sixteen bits");
        AssertThrows<ArgumentOutOfRangeException>(() => range.DecelerationMultiplier = byte.MaxValue + 1,
            "the multiplier never holds more than eight bits");

        // The crash leaves its orbit radius plus 256 times its orbit stage in the echo index. The
        // next boost capture sees a nonzero index, writes the second slot first, and the step past
        // slot two returns the index to zero, as the cartridge's signed comparison does.
        const int crashResidue = 16 + 2 * 256;
        var boosted = new SamusHorizontalSpeedState
        {
            SpeedBoostCounter = SamusSpeedBoostCounter.AtFullStage,
        };
        boosted.SetShinesparkCrashEchoState(crashResidue, Position16<RoomPixels>.Zero, Position16<RoomPixels>.Zero);
        AssertTrue(!boosted.SpeedEchoesDeparting, "the crash residue is not the departure state");
        AssertTrue(boosted.CaptureSpeedEchoPosition(0, new(40, 50)), "a full boost captures on a fourth frame");
        AssertEqual(new Position16<RoomPixels>(40, 50), boosted.SecondSpeedEchoPosition,
            "a nonzero residue captures into the second slot");
        AssertEqual(Position16<RoomPixels>.Zero, boosted.FirstSpeedEchoPosition, "the first slot is untouched");
        AssertEqual(0, boosted.SpeedEchoIndex, "stepping past the second slot returns the index to zero");

        // Cancellation publishes the departure index, which stops capture.
        boosted.CancelRunningMomentum(SamusFacingDirection.Right);
        AssertEqual(ushort.MaxValue, boosted.SpeedEchoIndex, "cancellation publishes the largest index");
        AssertTrue(boosted.SpeedEchoesDeparting, "the largest index is the departure state");
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
