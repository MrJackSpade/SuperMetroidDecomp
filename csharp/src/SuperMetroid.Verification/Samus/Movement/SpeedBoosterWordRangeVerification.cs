using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>
/// Checks the Speed Booster counter, speed echo index, speed divisor and deceleration multiplier now that they are
/// stored as ordinary integers: each keeps the sixteen-bit (or eight-bit) range and wrap the cartridge's words had,
/// driven through the production operations.
/// </summary>
internal static class SpeedBoosterWordRangeVerification
{
    /// <summary>Counter value for stage <paramref name="stage"/> with the given cadence countdown.</summary>
    private static int Counter(int stage, int cadence) => stage * 256 + cadence;

    /// <summary>Exercises the cadence borrow, the counter's range, the crash residue in the echo index and the two widened quantities' ranges.</summary>
    internal static void VerifySpeedBoosterWordRange()
    {
        // A cadence countdown at zero borrows from the stage, exactly as the cartridge's
        // sixteen-bit decrement does: stage two, cadence zero becomes stage one, cadence 255,
        // which is not a stage advance. Zero wraps to the largest counter.
        foreach ((int before, int expected) in new[]
        {
            new VerifySpeedBoosterWordRangeCase(Counter(2, 0), Counter(1, 255)),
            new VerifySpeedBoosterWordRangeCase(0, ushort.MaxValue),
            new VerifySpeedBoosterWordRangeCase(Counter(4, 0), Counter(3, 255)),
        })
        {
            var speed = new SamusHorizontalSpeedState { HasRunningMomentum = true, SpeedBoostCounter = new SamusSpeedBoostCounter(before) };
            int frame = 7;
            bool advanced = speed.TryAdvanceSpeedBoosterAnimationStage(null, SamusMovementType.Running, ControllerButton.B,
                animationFrameBuffer: 0, ref frame, out _);
            AssertTrue(!advanced, $"counter {before}: a borrowing decrement leaves a nonzero cadence, so no stage advances");
            AssertEqual(expected, speed.SpeedBoostCounter.RawValue, $"counter {before}: the decrement wraps across the sixteen-bit counter");
        }

        // A cadence of one reaching zero advances the stage and reloads the cadence.
        var advancing = new SamusHorizontalSpeedState { HasRunningMomentum = true, SpeedBoostCounter = new SamusSpeedBoostCounter(Counter(1, 1)) };
        int advancingFrame = 7;
        AssertTrue(advancing.TryAdvanceSpeedBoosterAnimationStage(null, SamusMovementType.Running, ControllerButton.B,
            animationFrameBuffer: 0, ref advancingFrame, out _), "cadence one expires and advances the stage");
        AssertEqual(Counter(2, SamusRunningCadenceDefinitions.ReadResetWord(2)), advancing.SpeedBoostCounter.RawValue,
            "stage two with its reloaded cadence");

        var range = new SamusHorizontalSpeedState();
        AssertThrows<ArgumentOutOfRangeException>(() => range.SpeedBoostCounter = new SamusSpeedBoostCounter(ushort.MaxValue + 1),
            "the counter never holds more than sixteen bits");
        AssertThrows<ArgumentOutOfRangeException>(() => range.SpeedBoostCounter = new SamusSpeedBoostCounter(-1), "the counter is never negative");
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
            SpeedBoostCounter = new SamusSpeedBoostCounter(SamusSpeedBoostCounterFixtures.ActiveStageWord),
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
