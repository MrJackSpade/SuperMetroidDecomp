using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// Confirms #1655: with Japanese text, each narration page's input wait shows the next-page arrow ($8B:CE73) at the
/// screen's middle below the text, and with English text it never shows. The cartridge's "spawn blinking markers and wait
/// for input" instructions spawn it as the page's text ends; it loops $8B:CC0F's frames, and the update after a page's
/// second subtitle starts holding the input wait it restarts on $8B:CC23, which shows nothing for the hold before the
/// loop; the press that turns the page deletes it.
/// <para>
/// A Japanese and an English narration run side by side over the same pages: the English page's input wait marks the
/// update both texts end on, as the narration's timing does not depend on the text option.
/// </para>
/// </summary>
internal static class IntroNextPageArrowVerification
{
    /// <summary>The poses the arrow's loop ($8B:CC0F) shows, in order: $8C:8CCF, $8C:8CDB, $8C:8CE7 and $8C:8CDB.</summary>
    private static readonly CinematicSpritePose[] LoopPoses = [CinematicSpritePose.NextPageArrow1, CinematicSpritePose.NextPageArrow2, CinematicSpritePose.NextPageArrow3, CinematicSpritePose.NextPageArrow2];

    /// <summary>The updates each frame of the arrow's loop shows for ($8B:CC0F's durations).</summary>
    private const int LoopFrameUpdates = 10;

    /// <summary>The updates the arrow's delayed list ($8B:CC23) shows nothing for before its loop.</summary>
    private const int HiddenUpdates = 60;

    /// <summary>The whole loops compared at each stage.</summary>
    private const int ComparedLoops = 2;

    /// <summary>The updates the arrow loops on page two before the press that shows the second subtitle: into its third frame.</summary>
    private const int UpdatesBeforeSecondSubtitle = 2 * LoopFrameUpdates + 3;

    /// <summary>Where the arrow shows: the screen's middle, below the page's text ($8B:AE07).</summary>
    private static readonly Position16<ScreenPixels> ArrowPosition = new(128, 216);

    /// <summary>Runs pages one and two with Japanese and English text and checks the arrow at their input waits.</summary>
    internal static void VerifyIntroNextPageArrow(ISnesAddressSpace rom)
    {
        IntroNarrationPresentation narration = IntroNarrationFile.Read(
            new MemoryStream(IntroNarrationExtractor.Extract(rom), writable: false));
        VerifyPageOne(narration);
        VerifyPageTwo(narration);
        Console.WriteLine("Intro next-page arrow: with Japanese text pages one and two show it at their input waits on the cartridge's cadence, page two hides it while its second subtitle holds the wait, the page turn deletes it, and English text never shows it.");
    }

    /// <summary>
    /// Page one has no second subtitle: its Japanese input wait starts as its text ends, with the arrow spawned, which loops
    /// from its next update until the page turns.
    /// </summary>
    private static void VerifyPageOne(IntroNarrationPresentation narration)
    {
        (IntroCinematicObjectSystem japanese, IntroCinematicObjectSystem english, _) = Pair(narration);
        japanese.StartEnglishPageOne();
        english.StartEnglishPageOne();
        RunToTextEnd(japanese, english, () => english.PageOneAwaitingInput, "page one");
        AssertTrue(japanese.PageOneAwaitingInput, "Japanese page one's input wait starts as its text ends");
        RequireSpawned(japanese, "page one");
        CompareLoop(japanese, english, "page one's input wait");
        TurnPage(japanese, english, "page one");
    }

    /// <summary>
    /// Page two's Japanese text ends waiting for the press that shows its second subtitle, with the arrow looping. The
    /// subtitle holds the input wait for its hold; the update after it shows, the arrow restarts hidden for the hold and
    /// then loops again until the page turns.
    /// </summary>
    private static void VerifyPageTwo(IntroNarrationPresentation narration)
    {
        (IntroCinematicObjectSystem japanese, IntroCinematicObjectSystem english, IntroJapaneseSubtitles subtitles) = Pair(narration);
        japanese.StartEnglishPageTwo();
        english.StartEnglishPageTwo();
        RunToTextEnd(japanese, english, () => english.PageTwoAwaitingInput, "page two");
        AssertTrue(!japanese.PageTwoAwaitingInput, "Japanese page two waits for its second subtitle before its input wait");
        RequireSpawned(japanese, "page two");

        for (int update = 0; update < UpdatesBeforeSecondSubtitle; update++)
        {
            Step(japanese, english, ControllerButton.None);
            AssertArrowShows(japanese, LoopPose(update), $"page two before its second subtitle, update {update}");
        }
        // The press shows the second subtitle after the arrow's update, which still shows its loop.
        Step(japanese, english, ControllerButton.A);
        AssertArrowShows(japanese, LoopPose(UpdatesBeforeSecondSubtitle), "page two on the second subtitle's press");
        AssertTrue(japanese.PageTwoAwaitingInput, "the second subtitle starts Japanese page two's input wait");
        AssertEqual(IntroJapaneseSubtitles.SecondSubtitleHoldFrames, subtitles.Timer, "the second subtitle holds the input wait");

        // The page's input wait takes its hold check before the narration's objects update, as each page's setup does.
        for (int update = 0; update < HiddenUpdates; update++)
        {
            AssertTrue(subtitles.HoldInputWait(), $"the second subtitle still holds the input wait at hidden update {update}");
            Step(japanese, english, ControllerButton.None);
            AssertArrowShows(japanese, CinematicSpritePose.Blank, $"page two while the second subtitle holds the wait, update {update}");
        }
        for (int update = 0; update < ComparedLoops * LoopPoses.Length * LoopFrameUpdates; update++)
        {
            AssertTrue(!subtitles.HoldInputWait(), $"the hold has ended at update {update} after it");
            Step(japanese, english, ControllerButton.None);
            AssertArrowShows(japanese, LoopPose(update), $"page two after the hold, update {update}");
        }
        TurnPage(japanese, english, "page two");
    }

    /// <summary>A Japanese and an English narration over <paramref name="narration"/>, with the Japanese one's subtitles.</summary>
    private static NarrationPair Pair(IntroNarrationPresentation narration)
    {
        var subtitles = new IntroJapaneseSubtitles(enabled: true);
        return new(new IntroCinematicObjectSystem(new IntroNarrationText(), subtitles, narrationPresentation: narration),
            new IntroCinematicObjectSystem(new IntroNarrationText(), new IntroJapaneseSubtitles(enabled: false), narrationPresentation: narration),
            subtitles);
    }

    /// <summary>Steps both narrations until the English one's input wait starts, neither showing the arrow while its text is typed.</summary>
    private static void RunToTextEnd(IntroCinematicObjectSystem japanese, IntroCinematicObjectSystem english, Func<bool> englishWaiting, string page)
    {
        while (!englishWaiting())
        {
            AssertTrue(japanese.NextPageArrow is null, $"Japanese {page} shows no next-page arrow while its text is typed");
            Step(japanese, english, ControllerButton.None);
        }
        AssertTrue(english.NextPageArrow is null, $"English {page} shows no next-page arrow at its input wait");
    }

    /// <summary>Requires the Japanese page's arrow spawned as its text ended, at its place, not yet updated.</summary>
    private static void RequireSpawned(IntroCinematicObjectSystem japanese, string page)
    {
        IntroDiscoverySprite arrow = japanese.NextPageArrow
            ?? throw new InvalidOperationException($"Japanese {page} spawned no next-page arrow as its text ended.");
        AssertEqual(ArrowPosition, arrow.WholePosition, $"Japanese {page}'s next-page arrow shows at the screen's middle below the text");
        AssertTrue(arrow.Pose is null, $"Japanese {page}'s next-page arrow first updates on the update after its spawn");
    }

    /// <summary>Compares <see cref="ComparedLoops"/> whole loops of the Japanese arrow from its first update.</summary>
    private static void CompareLoop(IntroCinematicObjectSystem japanese, IntroCinematicObjectSystem english, string at)
    {
        for (int update = 0; update < ComparedLoops * LoopPoses.Length * LoopFrameUpdates; update++)
        {
            Step(japanese, english, ControllerButton.None);
            AssertArrowShows(japanese, LoopPose(update), $"{at}, update {update}");
        }
    }

    /// <summary>Accepts the press that turns the page: the arrow deletes itself on that update's object update.</summary>
    private static void TurnPage(IntroCinematicObjectSystem japanese, IntroCinematicObjectSystem english, string page)
    {
        japanese.AcceptPageTurn();
        english.AcceptPageTurn();
        Step(japanese, english, ControllerButton.A);
        AssertTrue(japanese.NextPageArrow is null, $"turning Japanese {page} deletes its next-page arrow on the press's update");
        AssertTrue(english.NextPageArrow is null, $"turning English {page} shows no next-page arrow");
    }

    /// <summary>Runs one update of both narrations with <paramref name="newlyPressed"/>; the English one never shows the arrow.</summary>
    private static void Step(IntroCinematicObjectSystem japanese, IntroCinematicObjectSystem english, ControllerButton newlyPressed)
    {
        japanese.Step(newlyPressed);
        english.Step(newlyPressed);
        AssertTrue(english.NextPageArrow is null, "English text shows no next-page arrow");
    }

    /// <summary>The pose the arrow's loop shows on its <paramref name="update"/>th update from the loop's start.</summary>
    private static CinematicSpritePose LoopPose(int update) => LoopPoses[update / LoopFrameUpdates % LoopPoses.Length];

    /// <summary>Requires the Japanese arrow present and showing <paramref name="expected"/>.</summary>
    private static void AssertArrowShows(IntroCinematicObjectSystem japanese, CinematicSpritePose expected, string at)
    {
        IntroDiscoverySprite arrow = japanese.NextPageArrow
            ?? throw new InvalidOperationException($"The Japanese next-page arrow is gone at {at}.");
        AssertEqual<CinematicSpritePose?>(expected, arrow.Pose, $"the Japanese next-page arrow's pose at {at}");
    }

    /// <summary>Runs the Japanese text's next-page arrow over pages one and two (#1655).</summary>
    [VerificationCommand("--intro-next-page-arrow")]
    internal static int IntroNextPageArrowCommand()
    {
        Suite(nameof(VerifyIntroNextPageArrow), () => VerifyIntroNextPageArrow(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>A Japanese and an English narration run side by side.</summary>
    /// <param name="Japanese">The narration with Japanese text.</param>
    /// <param name="English">The narration with English text.</param>
    /// <param name="Subtitles">The Japanese narration's subtitles, whose hold the page's input wait checks.</param>
    private sealed record NarrationPair(IntroCinematicObjectSystem Japanese, IntroCinematicObjectSystem English, IntroJapaneseSubtitles Subtitles);
}
