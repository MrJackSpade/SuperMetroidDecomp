using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Frontend;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the one screen fade every scene uses (#2081): its cadence waits its plain counts before each step, and its
/// steps stop at black and at full and report reaching them.
/// </summary>
internal static class ScreenFadeVerification
{
    /// <summary>The first step waits the first wait, each later one the step delay, and an update that waits changes nothing.</summary>
    internal static void VerifyScreenFadeCadence()
    {
        var fade = new ScreenFade(DisplayBrightness.Black);
        fade.SetCadence(stepDelay: 2, firstWait: 0);
        fade.Brighten();
        AssertEqual(1, fade.Level.StepsAboveBlack, "no first wait steps on the first update");
        fade.Brighten();
        fade.Brighten();
        AssertEqual(1, fade.Level.StepsAboveBlack, "a step delay of two waits the next two updates");
        fade.Brighten();
        AssertEqual(2, fade.Level.StepsAboveBlack, "the update after the delay steps");

        fade.Restart(DisplayBrightness.Full, 1);
        AssertTrue(!fade.Darken() && fade.Level == DisplayBrightness.Full, "a restart's first update waits its delay");
        fade.Darken();
        AssertEqual(DisplayBrightness.Full.Darker(1), fade.Level, "the update after the delay steps once");

        fade.StepEvery(2);
        DisplayBrightness[] levels = [.. Enumerable.Range(0, 4).Select(_ => { fade.Darken(); return fade.Level; })];
        AssertTrue(levels.SequenceEqual([DisplayBrightness.Full.Darker(1), DisplayBrightness.Full.Darker(2), DisplayBrightness.Full.Darker(2), DisplayBrightness.Full.Darker(3)]),
            "stepping every two updates steps on the second update and every second update after it");

        fade.StepEvery(1);
        fade.Darken();
        fade.Darken();
        AssertEqual(DisplayBrightness.Full.Darker(5), fade.Level, "stepping every update steps on each update");
        AssertThrows<ArgumentOutOfRangeException>(() => fade.SetCadence(-1, 0), "a cadence never waits a negative count");
        AssertThrows<ArgumentOutOfRangeException>(() => fade.StepEvery(0), "a step takes at least one update");
    }

    /// <summary>Steps stop at black and at full, report reaching them, and may move several levels at once.</summary>
    internal static void VerifyScreenFadeLevels()
    {
        var fade = new ScreenFade(DisplayBrightness.Full.Darker(1));
        AssertTrue(fade.Brighten() && fade.IsFullyLit, "the step to full reports a fully lit screen");
        AssertTrue(fade.Brighten() && fade.Level == DisplayBrightness.Full, "a fully lit screen stays fully lit");
        fade.Show(DisplayBrightness.StepsAbove(1));
        AssertTrue(!fade.IsBlack && fade.Darken() && fade.Level.IsBlack, "the step from level one reports a black screen");
        AssertTrue(fade.Darken() && fade.Level.IsBlack, "a black screen stays black");
        fade.Show(DisplayBrightness.Full.Darker(1));
        AssertTrue(fade.Brighten(2) && fade.Level == DisplayBrightness.Full, "a two-level step stops at full");
        fade.Show(DisplayBrightness.StepsAbove(1));
        AssertTrue(fade.Darken(2) && fade.Level.IsBlack, "a two-level step stops at black");
        AssertThrows<ArgumentOutOfRangeException>(() => fade.Show(DisplayBrightness.StepsAbove(DisplayBrightness.StepsFromBlackToFull + 1)), "no level lies past full");
        AssertThrows<ArgumentOutOfRangeException>(() => fade.Show(DisplayBrightness.StepsAbove(-1)), "no level lies below black");
    }

    /// <summary>Runs the focused screen fade verification command.</summary>
    [VerificationCommand("--screen-fade")]
    internal static int ScreenFadeCommand()
    {
        Suite(nameof(VerifyScreenFadeCadence), () => VerifyScreenFadeCadence());
        Suite(nameof(VerifyScreenFadeLevels), () => VerifyScreenFadeLevels());
        return 0;
    }
}
