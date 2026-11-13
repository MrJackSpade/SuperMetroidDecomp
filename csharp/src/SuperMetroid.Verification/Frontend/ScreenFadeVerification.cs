using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
namespace SuperMetroid.Verification.Frontend;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the screen fade's countdown wraps through the production fade steps: the word counter steps brightness only
/// when its decrement goes negative and reloads from the delay, and the slow fades count the delay down, reload it from
/// the counter and refuse a delay longer than they count.
/// </summary>
internal static class ScreenFadeVerification
{
    /// <summary>The word counter decides each fast fade dispatch by the sign of its decremented word.</summary>
    internal static void VerifyScreenFadeWordCounter()
    {
        var fade = new ScreenFade();
        fade.SetTiming(2, 0);
        ScreenBrightness brightness = ScreenBrightness.Lit(0);
        fade.FadeIn(ref brightness);
        AssertEqual(ScreenBrightness.Lit(1), brightness, "a cleared counter wraps negative on its first dispatch and steps brightness");
        AssertEqual(WrappingCountdown.Start(2), fade.Counter, "the stepping dispatch reloads the counter from the delay");
        fade.FadeIn(ref brightness);
        fade.FadeIn(ref brightness);
        AssertEqual(ScreenBrightness.Lit(1), brightness, "a delay of two consumes the next two dispatches");
        AssertEqual(WrappingCountdown.Start(0), fade.Counter, "the consumed dispatches count the word down to zero");
        fade.FadeIn(ref brightness);
        AssertEqual(ScreenBrightness.Lit(2), brightness, "the dispatch after zero wraps negative and steps brightness");

        fade.SetTiming(1, ushort.MaxValue);
        brightness = ScreenBrightness.FullyLit;
        fade.FadeOut(ref brightness);
        AssertEqual(ScreenBrightness.Lit(ScreenBrightness.FullLevel - 1), brightness, "a counter left at $FFFF decrements to a negative word and steps");

        fade.SetTiming(1, 0x8000);
        brightness = ScreenBrightness.FullyLit;
        fade.FadeOut(ref brightness);
        AssertEqual(ScreenBrightness.FullyLit, brightness, "a counter at $8000 decrements to a non-negative word and only counts down");
        AssertEqual(WrappingCountdown.Start(0x7fff), fade.Counter, "the word counter keeps the wrapped word exactly");
    }

    /// <summary>The slow fades count the delay down, reload it from the counter, clear both at the end and refuse a longer delay.</summary>
    internal static void VerifyScreenFadeSlowDelay()
    {
        var fade = new ScreenFade();
        fade.SetTiming(0, 2);
        ScreenBrightness brightness = ScreenBrightness.Lit(0);
        AssertTrue(!fade.AdvanceSlowFadeIn(ref brightness), "the slow fade-in has not reached full brightness");
        AssertEqual(ScreenBrightness.Lit(1), brightness, "a zero delay runs past zero on its first dispatch and steps brightness");
        AssertEqual(WrappingCountdown.Start(2), fade.Delay, "the step reloads the delay from the counter");
        fade.AdvanceSlowFadeIn(ref brightness);
        AssertEqual(ScreenBrightness.Lit(1), brightness, "a positive decremented delay only counts down");
        AssertEqual(WrappingCountdown.Start(1), fade.Delay, "the dispatch counts the delay down by one");
        fade.AdvanceSlowFadeIn(ref brightness);
        AssertEqual(ScreenBrightness.Lit(2), brightness, "the delay reaching zero steps brightness");

        fade.SetTiming(2, 2);
        brightness = ScreenBrightness.Lit(1);
        AssertTrue(!fade.AdvanceSlowFadeOut(ref brightness), "a delay of two holds the first slow fade-out dispatch");
        AssertTrue(fade.AdvanceSlowFadeOut(ref brightness), "the slow fade-out reports completion when it blanks the screen");
        AssertEqual(ScreenBrightness.Blanked, brightness, "the last slow fade-out step blanks the screen");
        AssertEqual(WrappingCountdown.Start(0), fade.Delay, "completion clears the delay");
        AssertEqual(WrappingCountdown.Start(0), fade.Counter, "completion clears the counter");

        fade.SetTiming(ScreenFade.LongestSlowDelay + 1, 2);
        brightness = ScreenBrightness.Lit(1);
        AssertThrows<InvalidOperationException>(() => fade.AdvanceSlowFadeIn(ref brightness),
            "a slow fade refuses a delay longer than it counts");
    }

    /// <summary>Runs the focused screen fade verification command.</summary>
    [VerificationCommand("--screen-fade-countdowns")]
    internal static int ScreenFadeCountdownsCommand()
    {
        Suite(nameof(VerifyScreenFadeWordCounter), () => VerifyScreenFadeWordCounter());
        Suite(nameof(VerifyScreenFadeSlowDelay), () => VerifyScreenFadeSlowDelay());
        return 0;
    }
}
