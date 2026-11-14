using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>
/// Checks the title skip's fade-out (#1719) against the original's. The original runs the skip's handler
/// (<c>$8B:9A6C</c>) after each title update's running step, sprite objects and colour effects (<c>$8B:9A22</c>), so its
/// fade-out (<c>$8B:9A83</c>, two levels per update through <c>$8B:90B8</c>) darkens the screen while the running step
/// keeps running: the pans keep moving, the title cards keep advancing and starting their scenes, and the final zoom can
/// complete and show the logo with its fade. Only at black does the handler replace the running step; the next update's
/// handler (<c>$8B:9A9C</c>) clears the objects and shows the immediate logo and copyright at the floor's own size, and the
/// fade-in (<c>$8B:9B53</c>) starts on the update after that.
/// </summary>
internal static class TitleSkipFadeVerification
{
    /// <summary>The title steps whose updates accept the skip in the original: every step before the final zoom's end.</summary>
    private static readonly TitleSequencePhase[] AcceptingSteps =
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

    /// <summary>Updates the natural title may take to reach its idle title screen.</summary>
    private const int NaturalTitleBound = 3_000;

    /// <summary>
    /// Presses Start on the first and last update of every step that accepts the skip, and on each of the final zoom's
    /// last updates whose fade-out the zoom completes in, and checks every update from the press to the idle title screen:
    /// through the fade-out the running step runs and draws exactly what the unskipped title draws on that update (never the
    /// copyright), at two levels darker per update; the update after black rebuilds the logo and copyright at the floor's
    /// own size, still black; and the fade-in brightens by two levels per update to the idle title screen.
    /// </summary>
    internal static void VerifyTitleSkipFade()
    {
        SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.Title);
        var pictures = new TitlePictures(
            sheet.Frame(TitleSpriteDefinitions.FrameName(TitleSprite.SuperMetroidLogo)),
            sheet.Frame(TitleSpriteDefinitions.FrameName(TitleSprite.NintendoCopyright)));
        List<TitleFrame> natural = NaturalTitle();
        TitleFrame idle = natural[^1];
        int logo = natural.FindIndex(frame => frame.Phase == TitleSequencePhase.TitleLogoFade);
        AssertTrue(logo > 0, "the natural title completes its final zoom");

        var presses = new SortedSet<int>();
        foreach (TitleSequencePhase step in AcceptingSteps)
        {
            presses.Add(natural.FindIndex(frame => frame.Phase == step));
            presses.Add(natural.FindLastIndex(frame => frame.Phase == step));
        }
        // The final zoom completes within the fade-out of a press on any of its last seven updates.
        for (int before = 1; before < 8; before++)
            presses.Add(logo - before);

        var coverage = new SkipFadeCoverage();
        foreach (int press in presses)
            VerifyPress(natural, idle, pictures, press, coverage);

        AssertTrue(coverage.LogoFadesDuringFadeOut, "a skip's fade-out shows the final zoom's logo with its fade");
        AssertTrue(coverage.FloorMovesDuringFadeOut, "a skip's fade-out shows a pan moving the floor");
        AssertTrue(coverage.CardAdvancesDuringFadeOut, "a skip's fade-out shows a title card advancing");
        AssertTrue(coverage.StepChangesDuringFadeOut, "a skip's fade-out shows the running step handing over to the next");
        Console.WriteLine($"  Title skip fade: {presses.Count} skips run their step through the fade-out as the unskipped title " +
            "does, never drawing the copyright, rebuild the title objects on the update after black and fade in to the idle title screen.");
    }

    /// <summary>
    /// Presses Start on natural title update <paramref name="press"/> and checks every update from it to the idle title
    /// screen against the original's skip.
    /// </summary>
    private static void VerifyPress(List<TitleFrame> natural, TitleFrame idle, TitlePictures pictures, int press, SkipFadeCoverage coverage)
    {
        TitleSequenceState title = InstalledFixtures.CreateTitle();
        for (int at = 0; at < press; at++)
            title.Step(ControllerButton.None);
        string pressed = $"a skip pressed on {natural[press].Phase} update {press}";
        AssertEqual(TitleSequenceDefinitions.Timing.MaximumBrightness, natural[press].Brightness, $"the unskipped title shows {pressed}'s update at full brightness");

        // The fade-out: the running step runs and draws as the unskipped title does, darker by two levels per update.
        int update = press;
        int brightness = TitleSequenceDefinitions.Timing.MaximumBrightness;
        while (true)
        {
            title.Step(update == press ? ControllerButton.Start : ControllerButton.None);
            brightness = Math.Max(0, brightness - TitleSequenceDefinitions.Timing.SkipFadeBrightnessStep);
            TitleFrame shown = Capture(title);
            string place = $"{pressed}, fade-out update {update}";
            AssertEqual(brightness, shown.Brightness, $"{place} fades two levels darker per update");
            AssertEqual(brightness == 0 ? TitleSkipStage.Rebuilding : TitleSkipStage.FadingOut, shown.Skip,
                $"{place} fades out until black and rebuilds only on the next update");
            AssertSameShow(natural[update], shown, $"{place} runs and draws its running step as the unskipped title does");
            AssertTrue(!shown.Draws.Any(draw => ((ImageSpriteDraw)draw).Image == pictures.Copyright), $"{place} does not draw the copyright");
            if (update > press && brightness > 0)
                coverage.Record(natural[update - 1], shown, pictures);
            if (brightness == 0)
                break;
            update++;
        }

        // The update after black rebuilds the logo and copyright at the floor's own size, still black.
        update++;
        title.Step(ControllerButton.None);
        TitleFrame rebuilt = Capture(title);
        string rebuild = $"{pressed}, rebuild update {update}";
        AssertEqual(TitleSequencePhase.TitleScreenFadeIn, rebuilt.Phase, $"{rebuild} hands the title to the fade-in");
        AssertEqual(TitleSkipStage.NotSkipping, rebuilt.Skip, $"{rebuild} ends the skip's fade-out");
        AssertEqual(0, rebuilt.Brightness, $"{rebuild} stays black");
        AssertEqual(idle.Floor, rebuilt.Floor, $"{rebuild} shows the floor at its own size, centred, as the idle title screen does");
        AssertEqual(idle.Gradient, rebuilt.Gradient, $"{rebuild} shows the idle title screen's gradient");
        AssertEqual(natural[update].BabyFrame, rebuilt.BabyFrame, $"{rebuild} keeps the Baby Metroid's animation running");
        AssertEqual(2, rebuilt.Draws.Length, $"{rebuild} draws the logo and the copyright");
        AssertRebuiltDraw(idle.Draws[0], pictures.Logo, PictureShading.Shaded(TitleShades.Drawn(TitleSpriteColouring.Logo)),
            rebuilt.Draws[0], $"{rebuild} draws the logo in its drawn colours");
        AssertRebuiltDraw(idle.Draws[1], pictures.Copyright,
            PictureShading.Shaded(TitleShades.Drawn(TitleSpriteColouring.Copyright)).With(TitleShades.RestoredCopyright),
            rebuilt.Draws[1], $"{rebuild} draws the copyright in its restored colours");

        // The fade-in brightens by two levels per update, from the update after the rebuild, to the idle title screen.
        while (brightness < TitleSequenceDefinitions.Timing.MaximumBrightness)
        {
            update++;
            title.Step(ControllerButton.None);
            brightness = Math.Min(TitleSequenceDefinitions.Timing.MaximumBrightness, brightness + TitleSequenceDefinitions.Timing.SkipFadeBrightnessStep);
            TitleFrame shown = Capture(title);
            string place = $"{pressed}, fade-in update {update}";
            AssertEqual(brightness, shown.Brightness, $"{place} fades two levels brighter per update");
            AssertEqual(brightness == TitleSequenceDefinitions.Timing.MaximumBrightness ? TitleSequencePhase.TitleScreen : TitleSequencePhase.TitleScreenFadeIn,
                shown.Phase, $"{place} fades in until the idle title screen");
            AssertTrue(rebuilt.Draws.SequenceEqual(shown.Draws), $"{place} draws the rebuilt logo and copyright");
            AssertEqual(rebuilt.Floor, shown.Floor, $"{place} keeps the rebuilt floor");
        }
        AssertEqual(TitleSequenceDefinitions.Timing.TitleScreenNtscFrames, title.TitleScreenFramesRemaining,
            $"{pressed} reaches the idle title screen with its demo countdown");
    }

    /// <summary>Checks that <paramref name="shown"/> runs the same step as <paramref name="expected"/> and draws the same frame, at any brightness.</summary>
    private static void AssertSameShow(TitleFrame expected, TitleFrame shown, string place)
    {
        AssertEqual(expected.Phase, shown.Phase, $"{place}: step");
        AssertTrue(expected.Draws.SequenceEqual(shown.Draws), $"{place}: sprites ({Describe(expected.Draws)} expected, {Describe(shown.Draws)} drawn)");
        AssertEqual(expected.Floor, shown.Floor, $"{place}: floor");
        AssertEqual(expected.Gradient, shown.Gradient, $"{place}: gradient");
        AssertEqual(expected.BabyFrame, shown.BabyFrame, $"{place}: Baby Metroid frame");
        AssertEqual(expected.FloorShading, shown.FloorShading, $"{place}: floor shading");
    }

    /// <summary>Checks that a rebuilt draw shows <paramref name="picture"/> where the idle title screen does, in <paramref name="shading"/>.</summary>
    private static void AssertRebuiltDraw(SpriteDraw idle, SpriteSheetFrame picture, PictureShading shading, SpriteDraw shown, string place)
    {
        var image = (ImageSpriteDraw)shown;
        AssertTrue(image.Image == picture, $"{place}: picture");
        AssertEqual(idle.Origin, image.Origin, $"{place}: where the idle title screen draws it");
        AssertEqual(shading, image.Shading, $"{place}: colours");
    }

    /// <summary>The draws' pictures and places, for a failure message.</summary>
    private static string Describe(ImmutableArray<SpriteDraw> draws) =>
        draws.Length == 0 ? "nothing" : string.Join(", ", draws.Select(draw => draw.ToString()));

    /// <summary>The natural title, without input, from its first update to the first update of its idle title screen.</summary>
    private static List<TitleFrame> NaturalTitle()
    {
        TitleSequenceState title = InstalledFixtures.CreateTitle();
        var frames = new List<TitleFrame>();
        while (title.Phase != TitleSequencePhase.TitleScreen)
        {
            AssertTrue(frames.Count < NaturalTitleBound, "the natural title reaches its idle title screen");
            title.Step(ControllerButton.None);
            frames.Add(Capture(title));
        }
        return frames;
    }

    /// <summary>What the title runs and shows after one update.</summary>
    private static TitleFrame Capture(TitleSequenceState title)
    {
        FloorScene scene = title.CaptureRenderSnapshot();
        return new TitleFrame(title.Phase, title.SkipStage, scene.Brightness, scene.Paint.SpriteDraws, scene.Background, scene.Gradient,
            title.BabyFrame, title.FloorShading);
    }

    /// <summary>What the title runs and shows after one update.</summary>
    /// <param name="Phase">The step the update ends in.</param>
    /// <param name="Skip">How far the skip has run.</param>
    /// <param name="Brightness">The screen brightness the frame shows.</param>
    /// <param name="Draws">The sprites drawn, frontmost first, with their colours.</param>
    /// <param name="Floor">How the floor is shown, or null when it is hidden.</param>
    /// <param name="Gradient">The gradient shown over the floor, or null for none.</param>
    /// <param name="BabyFrame">The frame of the Baby Metroid's pulse the floor shows.</param>
    /// <param name="FloorShading">The console lights' shading of the floor.</param>
    private sealed record TitleFrame(
        TitleSequencePhase Phase,
        TitleSkipStage Skip,
        int Brightness,
        ImmutableArray<SpriteDraw> Draws,
        FloorView? Floor,
        ScreenGradientLayer? Gradient,
        int BabyFrame,
        PictureShading FloorShading);

    /// <summary>The logo's and the copyright's pictures in the title sheet.</summary>
    /// <param name="Logo">The Super Metroid logo.</param>
    /// <param name="Copyright">The Nintendo copyright.</param>
    private sealed record TitlePictures(SpriteSheetFrame Logo, SpriteSheetFrame Copyright);

    /// <summary>Which of the running step's behaviours the checked fade-outs showed while the screen was still lit.</summary>
    private sealed class SkipFadeCoverage
    {
        /// <summary>Whether a fade-out showed the final zoom's logo with its fade.</summary>
        internal bool LogoFadesDuringFadeOut { get; private set; }
        /// <summary>Whether a fade-out showed a pan moving the floor.</summary>
        internal bool FloorMovesDuringFadeOut { get; private set; }
        /// <summary>Whether a fade-out showed a title card showing its next picture.</summary>
        internal bool CardAdvancesDuringFadeOut { get; private set; }
        /// <summary>Whether a fade-out showed the running step handing over to the next step.</summary>
        internal bool StepChangesDuringFadeOut { get; private set; }

        /// <summary>Records what changed between the previous fade-out update, as the unskipped title shows it, and <paramref name="shown"/>.</summary>
        internal void Record(TitleFrame previous, TitleFrame shown, TitlePictures pictures)
        {
            LogoFadesDuringFadeOut |= shown.Phase == TitleSequencePhase.TitleLogoFade &&
                shown.Draws.Any(draw => ((ImageSpriteDraw)draw).Image == pictures.Logo);
            FloorMovesDuringFadeOut |= previous.Phase == shown.Phase && previous.Floor is not null && previous.Floor != shown.Floor;
            CardAdvancesDuringFadeOut |= previous.Phase == shown.Phase && previous.Floor is null && !previous.Draws.SequenceEqual(shown.Draws);
            StepChangesDuringFadeOut |= previous.Phase != shown.Phase;
        }
    }
}
