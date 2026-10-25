using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the wrapping steps timers and counters take through the compatibility stubs (#1419): frames added to or
/// taken from a countdown wrap at the word, a stepped word counter wraps past the top, a loop count loaded at zero and
/// decremented becomes $FFFF, and a saturating countdown stops at zero.
/// </summary>
internal static class WrappingTimerStepsVerification
{
    /// <summary>Adds and removes frames across both ends of the countdown word.</summary>
    internal static void VerifyCountdownFrameAddition()
    {
        AssertEqual(0x0010, WrappingCountdown.FromRaw(0xFFF0).AddFrames(32).RawValue, "frames added past $FFFF wrap to the bottom");
        AssertEqual(0x0028, WrappingCountdown.Start(8).AddFrames(32).RawValue, "frames added inside the word add plainly");
        WrappingCountdown shortened = WrappingCountdown.Start(4).AddFrames(-6);
        AssertEqual(0xFFFE, shortened.RawValue, "frames taken below zero wrap to the top");
        AssertTrue(shortened.IsNegative, "a countdown shortened below zero reads as negative");
        AssertEqual(0x0002, WrappingCountdown.Start(8).AddFrames(-6).RawValue, "frames taken inside the word subtract plainly");
        AssertThrows<ArgumentOutOfRangeException>(() => WrappingCountdown.Start(0).AddFrames(ushort.MaxValue + 1),
            "adding more than a word of frames is rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => WrappingCountdown.Start(0).AddFrames(-ushort.MaxValue - 1),
            "taking more than a word of frames is rejected");
    }

    /// <summary>Steps a word counter past the top, and inside the word.</summary>
    internal static void VerifyWordCounterStep()
    {
        AssertEqual(0x010F, WrappingCounter.AddWord(0xFFFF, 0x0110), "a step past $FFFF wraps to the bottom");
        AssertEqual(0x0410, WrappingCounter.AddWord(0x0300, 0x0110), "a step inside the word adds plainly");
        AssertThrows<ArgumentOutOfRangeException>(() => WrappingCounter.AddWord(ushort.MaxValue + 1, 1),
            "a counter above a word is rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => WrappingCounter.AddWord(0, -1), "a negative step is rejected");
    }

    /// <summary>Loads a zero loop count and decrements it, and saturates a countdown at zero.</summary>
    internal static void VerifyCountdownLoadAndSaturation()
    {
        AssertEqual(0xFFFF, WrappingCountdown.FromRaw(0).Decrement().Remaining.RawValue,
            "a loop count loaded at zero and decremented runs for $FFFF loops");
        AssertEqual(0, WrappingCountdown.Start(1).DecrementToZero().RawValue, "a saturating countdown reaches zero");
        AssertEqual(0, WrappingCountdown.Start(0).DecrementToZero().RawValue, "a saturating countdown stays at zero");
    }

    /// <summary>Runs the focused wrapping timer-step checks.</summary>
    [VerificationCommand("--wrapping-timer-steps")]
    internal static int WrappingTimerStepsCommand()
    {
        Suite(nameof(VerifyCountdownFrameAddition), () => VerifyCountdownFrameAddition());
        Suite(nameof(VerifyWordCounterStep), () => VerifyWordCounterStep());
        Suite(nameof(VerifyCountdownLoadAndSaturation), () => VerifyCountdownLoadAndSaturation());
        return 0;
    }
}
