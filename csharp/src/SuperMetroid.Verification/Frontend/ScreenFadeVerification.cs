using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Verification.Frontend;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the screen fade's countdown wraps through the production fade steps: the word counter steps brightness only
/// when its decrement goes negative and reloads from the delay, and the slow fades count the delay's low byte down
/// beside an unchanged high byte.
/// </summary>
internal static class ScreenFadeVerification
{
    /// <summary>The word counter decides each fast fade dispatch by the sign of its decremented word.</summary>
    internal static void VerifyScreenFadeWordCounter()
    {
        var fade = new ScreenFade();
        fade.SetTiming(2, 0);
        int inidisp = 0;
        fade.FadeIn(ref inidisp);
        AssertEqual(1, inidisp, "a cleared counter wraps negative on its first dispatch and steps brightness");
        AssertEqual(NativeCountdown.FromRaw(2), fade.Counter, "the stepping dispatch reloads the counter from the delay");
        fade.FadeIn(ref inidisp);
        fade.FadeIn(ref inidisp);
        AssertEqual(1, inidisp, "a delay of two consumes the next two dispatches");
        AssertEqual(NativeCountdown.FromRaw(0), fade.Counter, "the consumed dispatches count the word down to zero");
        fade.FadeIn(ref inidisp);
        AssertEqual(2, inidisp, "the dispatch after zero wraps negative and steps brightness");

        fade.SetTiming(1, ushort.MaxValue);
        inidisp = ScreenFade.FullyLit;
        fade.FadeOut(ref inidisp);
        AssertEqual(ScreenFade.FullyLit - 1, inidisp, "a counter left at $FFFF decrements to a negative word and steps");

        fade.SetTiming(1, 0x8000);
        inidisp = ScreenFade.FullyLit;
        fade.FadeOut(ref inidisp);
        AssertEqual(ScreenFade.FullyLit, inidisp, "a counter at $8000 decrements to a non-negative word and only counts down");
        AssertEqual(NativeCountdown.FromRaw(0x7fff), fade.Counter, "the word counter keeps the wrapped word exactly");
    }

    /// <summary>The slow fades count the delay's low byte down, keep its high byte and reload only the low byte.</summary>
    internal static void VerifyScreenFadeSlowLowByte()
    {
        var fade = new ScreenFade();
        fade.SetTiming(0x0300, 0x1202);
        int inidisp = 0;
        AssertTrue(!fade.AdvanceSlowFadeIn(ref inidisp), "the slow fade-in has not reached full brightness");
        AssertEqual(1, inidisp, "a zero low byte wraps negative on its first dispatch and steps brightness");
        AssertEqual(NativeCountdown.FromRaw(0x0302), fade.Delay, "the reload copies the counter's low byte under the delay's high byte");
        fade.AdvanceSlowFadeIn(ref inidisp);
        AssertEqual(1, inidisp, "a positive decremented low byte only counts down");
        AssertEqual(NativeCountdown.FromRaw(0x0301), fade.Delay, "the low-byte decrement leaves the high byte unchanged");
        fade.AdvanceSlowFadeIn(ref inidisp);
        AssertEqual(2, inidisp, "the low byte reaching zero steps brightness");

        fade.SetTiming(0x0500, 0x0700);
        inidisp = 1;
        AssertTrue(fade.AdvanceSlowFadeOut(ref inidisp), "the slow fade-out reports completion when it writes forced blank");
        AssertEqual(ScreenFade.ForcedBlank, inidisp, "the last slow fade-out step writes forced blank");
        AssertEqual(NativeCountdown.FromRaw(0x0500), fade.Delay, "completion clears only the delay's low byte");
        AssertEqual(NativeCountdown.FromRaw(0x0700), fade.Counter, "completion clears only the counter's low byte");
    }

    /// <summary>Runs the focused screen fade verification command.</summary>
    [VerificationCommand("--screen-fade-countdowns")]
    internal static int ScreenFadeCountdownsCommand()
    {
        Suite(nameof(VerifyScreenFadeWordCounter), () => VerifyScreenFadeWordCounter());
        Suite(nameof(VerifyScreenFadeSlowLowByte), () => VerifyScreenFadeSlowLowByte());
        return 0;
    }
}
