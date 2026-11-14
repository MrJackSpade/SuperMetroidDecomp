using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Input;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>
/// Checks the title's skip window (#1715) against the original's. The original reads the skip once per title update,
/// after that update's title step and sprite objects have run (<c>$8B:9A3B</c>), and its check (<c>$8B:9A48</c>) skips on
/// a new B, Start or A press only while the running step precedes the one the final zoom's end installs
/// (<c>$8B:9F28</c>), and only while no skip is running. The steps before it are the four title cards and the four floor
/// scenes; the final zoom (<c>$8B:9E8B</c>) installs it on the first second update that finds the zoom at the floor's own
/// size, so the logo's and copyright's holds, the skip's own fades and the idle title screen all refuse the skip.
/// </summary>
internal static class TitleSkipWindowVerification
{
    /// <summary>The title steps whose updates accept the skip in the original: every step before the final zoom's end.</summary>
    private static readonly HashSet<TitleSequencePhase> AcceptingSteps =
    [
        TitleSequencePhase.YearText,
        TitleSequencePhase.LowerPan,
        TitleSequencePhase.NintendoText,
        TitleSequencePhase.UpperPan,
        TitleSequencePhase.PresentsText,
        TitleSequencePhase.DownwardPan,
        TitleSequencePhase.MetroidThreeText,
        TitleSequencePhase.FinalZoom,
    ];

    /// <summary>The buttons whose new presses the original's skip check reads.</summary>
    private static readonly ControllerButton[] SkipButtons = [ControllerButton.B, ControllerButton.Start, ControllerButton.A];

    /// <summary>Every other button, pressed together; the skip check reads none of them.</summary>
    private const ControllerButton OtherButtons = ControllerButton.Up | ControllerButton.Down | ControllerButton.Left |
        ControllerButton.Right | ControllerButton.Select | ControllerButton.X | ControllerButton.Y | ControllerButton.L |
        ControllerButton.R;

    /// <summary>Updates the skip may take from its press to the idle title screen.</summary>
    private const int SkipRouteBound = 24;

    /// <summary>Updates the natural title may take to reach its idle title screen.</summary>
    private const int NaturalTitleBound = 3_000;

    /// <summary>
    /// Presses each skip button on the first and last update of every title step and checks that the update skips
    /// exactly when the original's window accepts it: an accepted press starts the skip's fade-out on its own update and
    /// still reaches the idle title screen at full brightness through the rebuild and fade-in, and a refused press leaves
    /// the update as it is without the press. The final zoom holds the floor's own size for two updates before the logo
    /// shows, the last of them accepting the skip and the logo's first update refusing it; other buttons never skip.
    /// </summary>
    internal static void VerifyTitleSkipWindow()
    {
        List<TitleUpdate> natural = NaturalTitle();
        int titleScreen = natural.FindIndex(update => update.Phase == TitleSequencePhase.TitleScreen);

        // The final zoom reaches the floor's own size and completes on its next acting update, two updates later.
        int ownSize = natural.FindIndex(update => update.Zoom >= FloorZoom.OwnSize);
        int logo = natural.FindIndex(update => update.Phase == TitleSequencePhase.TitleLogoFade);
        AssertEqual(TitleSequencePhase.FinalZoom, natural[ownSize].Phase, "the zoom reaches the floor's own size during the final zoom");
        AssertEqual(ownSize + 2, logo, "the final zoom completes on the second update after the floor reaches its own size");

        int cases = 0;
        foreach (TitleSequencePhase step in natural.Select(update => update.Phase).Distinct())
        {
            int first = natural.FindIndex(update => update.Phase == step);
            int last = natural.FindLastIndex(update => update.Phase == step);
            foreach (int update in new[] { first, last }.Distinct())
            {
                foreach (ControllerButton button in SkipButtons)
                {
                    VerifyNaturalPress(natural, update, button, titleScreen);
                    cases++;
                }
            }
            if (AcceptingSteps.Contains(step))
            {
                AssertEqual(natural[first], StateAfter(at => at == first ? OtherButtons : ControllerButton.None, first),
                    $"pressing only other buttons on {step}'s first update (update {first}) does not skip");
                cases++;
            }
        }
        AssertTrue(AcceptingSteps.IsSubsetOf(natural.Select(update => update.Phase)), "the natural title runs every step that accepts the skip");

        // A press during the skip's own fade-out, on the update it reaches black, or during its fade-in does not restart it.
        List<TitleUpdate> skipping = Trace(at => at == 0 ? ControllerButton.A : ControllerButton.None,
            title => title.Phase == TitleSequencePhase.TitleScreen, SkipRouteBound);
        cases += VerifyRunningSkipRefuses(skipping, "the skip's fade-out", update => update.Skip == TitleSkipStage.FadingOut);
        cases += VerifyRunningSkipRefuses(skipping, "the skip's black", update => update.Skip == TitleSkipStage.Rebuilding);
        cases += VerifyRunningSkipRefuses(skipping, "the skip's rebuild and fade-in", update => update.Phase == TitleSequencePhase.TitleScreenFadeIn);

        // The idle title screen's own confirmation fades to file select; a press during that fade does not skip.
        int confirm = titleScreen + 1;
        List<TitleUpdate> confirming = Trace(at => at == confirm ? ControllerButton.Start : ControllerButton.None,
            _ => false, confirm + 4);
        int fading = confirm + 2;
        AssertEqual(TitleSequencePhase.TitleScreenFadeOut, confirming[fading].Phase, "the idle title screen's confirmation fades out");
        foreach (ControllerButton button in SkipButtons)
        {
            AssertEqual(confirming[fading], StateAfter(at => at == confirm ? ControllerButton.Start : at == fading ? button : ControllerButton.None, fading),
                $"a {button} press during the title screen's fade-out does not skip");
            cases++;
        }

        Console.WriteLine($"  Title skip window: {cases} presses skip exactly on the original's steps " +
            $"(the four title cards and four floor scenes, to the final zoom's end on update {logo}), and nowhere after.");
    }

    /// <summary>
    /// Presses each skip button again on the first and last update after the first skip's press on which
    /// <paramref name="during"/> holds, and checks that the running skip goes on as it is.
    /// </summary>
    /// <returns>The presses checked.</returns>
    private static int VerifyRunningSkipRefuses(List<TitleUpdate> skipping, string stage, Predicate<TitleUpdate> during)
    {
        int first = skipping.FindIndex(1, during);
        int last = skipping.FindLastIndex(during);
        AssertTrue(first > 0, $"the skip runs {stage} after the update of its press");
        int cases = 0;
        foreach (int update in new[] { first, last }.Distinct())
        {
            foreach (ControllerButton button in SkipButtons)
            {
                AssertEqual(skipping[update], StateAfter(at => at == 0 ? ControllerButton.A : at == update ? button : ControllerButton.None, update),
                    $"a second {button} press on {stage}'s update {update} leaves the running skip as it is");
                cases++;
            }
        }
        return cases;
    }

    /// <summary>
    /// Presses <paramref name="button"/> on natural title update <paramref name="update"/> and checks the update against the
    /// window: the skip on an accepting step, the idle title screen's own confirmation from its second update, and otherwise
    /// the natural update unchanged.
    /// </summary>
    private static void VerifyNaturalPress(List<TitleUpdate> natural, int update, ControllerButton button, int titleScreen)
    {
        TitleUpdate expected = natural[update];
        string place = $"a {button} press on {expected.Phase} update {update}";
        TitleSequenceState title = InstalledFixtures.CreateTitle();
        for (int at = 0; at < update; at++)
            title.Step(ControllerButton.None);
        title.Step(button);
        if (AcceptingSteps.Contains(expected.Phase))
        {
            AssertEqual(TitleSkipStage.FadingOut, title.SkipStage, $"{place} skips");
            AssertEqual(expected.Phase, title.Phase, $"{place} keeps running its step alongside the skip's fade-out");
            AssertEqual(expected.Brightness - TitleSequenceDefinitions.Timing.SkipFadeBrightnessStep, (int)title.Brightness,
                $"{place} takes the skip's first fade step on its own update");
            bool fadedIn = false;
            for (int route = 0; title.Phase != TitleSequencePhase.TitleScreen; route++)
            {
                AssertTrue(route < SkipRouteBound, $"{place} reaches the idle title screen");
                title.Step(ControllerButton.None);
                fadedIn |= title.Phase == TitleSequencePhase.TitleScreenFadeIn;
            }
            AssertTrue(fadedIn, $"{place} rebuilds the title objects and fades back in");
            AssertEqual(TitleSequenceDefinitions.Timing.MaximumBrightness, (int)title.Brightness, $"{place} returns at full brightness");
            return;
        }
        if (expected.Phase == TitleSequencePhase.TitleScreen && update > titleScreen)
        {
            AssertEqual(TitleSequencePhase.TitleScreenFadeOut, title.Phase, $"{place} is the idle title screen's confirmation, not the skip");
            return;
        }
        AssertEqual(expected, Capture(title), $"{place} does not skip");
    }

    /// <summary>The natural title, without input, from its first update to the second update of its idle title screen.</summary>
    private static List<TitleUpdate> NaturalTitle()
    {
        List<TitleUpdate> natural = Trace(_ => ControllerButton.None, title => title.Phase == TitleSequencePhase.TitleScreen, NaturalTitleBound);
        natural.Add(StateAfter(_ => ControllerButton.None, natural.Count));
        return natural;
    }

    /// <summary>
    /// Steps a new title with <paramref name="input"/> on each update, recording the state after each, until
    /// <paramref name="until"/> holds after an update or <paramref name="bound"/> updates have run.
    /// </summary>
    private static List<TitleUpdate> Trace(Func<int, ControllerButton> input, Func<TitleSequenceState, bool> until, int bound)
    {
        TitleSequenceState title = InstalledFixtures.CreateTitle();
        var updates = new List<TitleUpdate>();
        for (int update = 0; update < bound; update++)
        {
            title.Step(input(update));
            updates.Add(Capture(title));
            if (until(title))
                return updates;
        }
        return updates;
    }

    /// <summary>The state after update <paramref name="update"/> of a new title stepped with <paramref name="input"/>.</summary>
    private static TitleUpdate StateAfter(Func<int, ControllerButton> input, int update) =>
        Trace(input, _ => false, update + 1)[update];

    /// <summary>What the skip check's outcome changes in one update: the title's step, the skip's stage, brightness and floor zoom.</summary>
    private static TitleUpdate Capture(TitleSequenceState title) => new(title.Phase, title.SkipStage, title.Brightness, title.FloorZoom);

    /// <summary>The title's step, the skip's stage, brightness and floor zoom after one update.</summary>
    /// <param name="Phase">The step the update ends in.</param>
    /// <param name="Skip">How far the skip has run alongside the step (#1719).</param>
    /// <param name="Brightness">The screen brightness the update ends at.</param>
    /// <param name="Zoom">The floor's zoom the update ends at.</param>
    private readonly record struct TitleUpdate(TitleSequencePhase Phase, TitleSkipStage Skip, int Brightness, FloorZoom Zoom);
}
