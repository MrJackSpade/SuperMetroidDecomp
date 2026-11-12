using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening's Japanese subtitle sequencer: it shows each page's first subtitle as the page begins, and on pages two
/// to five waits at the end of the page for a press that shows the second subtitle, which then holds the page's input
/// wait for <see cref="SecondSubtitleHoldFrames"/>. English text shows no subtitle and leaves the hold at zero.
/// </summary>
/// <remarks>
/// At most one subtitle is showing at a time: a page's first subtitle has finished long before the page's text ends,
/// and the second subtitle starts the input wait that leads to the next page. The intro advances the subtitle after
/// its cinematic sprites and before the page text that begins and finishes subtitles, so a subtitle first advances on
/// the frame after it begins.
/// </remarks>
internal sealed class IntroJapaneseSubtitles
{
    /// <summary>Frames the second subtitle holds a page's input wait.</summary>
    public const int SecondSubtitleHoldFrames = 60;

    /// <summary>The page whose subtitle is showing, or null while <see cref="stage"/> is <see cref="SubtitleStage.None"/>.</summary>
    private IntroNarrationPageId? page;
    /// <summary>What the showing subtitle does next, or <see cref="SubtitleStage.None"/> when no subtitle is showing.</summary>
    private SubtitleStage stage;
    /// <summary>Frames left before the showing subtitle takes its next step.</summary>
    private int frameHold;

    /// <summary>Selects Japanese subtitle handling from the opening's alternate-text setting.</summary>
    public IntroJapaneseSubtitles(bool enabled) => Enabled = enabled;

    /// <summary>The Japanese text option at the start of the intro.</summary>
    public bool Enabled { get; }

    /// <summary>Frames the second subtitle still holds the page's input wait; zero when the intro starts.</summary>
    public int Timer { get; private set; }

    /// <summary>
    /// True on the one update the input wait's hold has counted down once: the second subtitle showed on the update
    /// before. The next-page arrow hides for the hold from then (#1655).
    /// </summary>
    public bool HoldFirstCounted => Timer == SecondSubtitleHoldFrames - 1;

    /// <summary>Begins <paramref name="page"/>'s text: Japanese text shows the page's first subtitle.</summary>
    public void BeginPage(IntroNarrationPageId page)
    {
        if (Enabled)
            Show(page, SubtitleStage.ShowFirstSubtitle);
    }

    /// <summary>
    /// Finishes <paramref name="page"/>'s text. Returns true when the page's input wait starts now; on pages two to five
    /// Japanese text instead waits for a press that shows the second subtitle, and the subtitle starts the wait.
    /// </summary>
    public bool FinishPage(IntroNarrationPageId page)
    {
        if (!Enabled || page is IntroNarrationPageId.Page1 or IntroNarrationPageId.Page6)
            return true;
        Show(page, SubtitleStage.AwaitSecondSubtitleInput);
        return false;
    }

    /// <summary>
    /// The hold check at the start of every intro input wait, in each page's setup.
    /// True while the wait is held: the call consumed one frame of the hold.
    /// </summary>
    public bool HoldInputWait()
    {
        if (Timer == 0)
            return false;
        Timer--;
        return true;
    }

    /// <summary>Advances the showing subtitle by one frame.</summary>
    /// <param name="newlyPressed">This frame's newly pressed buttons, which show a waiting page's second subtitle.</param>
    /// <param name="startInputWait">Receives the page whose second subtitle has started its input wait.</param>
    public void Step(ControllerButton newlyPressed, Action<IntroNarrationPageId> startInputWait)
    {
        if (stage == SubtitleStage.None)
            return;
        if (stage == SubtitleStage.AwaitSecondSubtitleInput && newlyPressed != ControllerButton.None)
        {
            // The press shows the second subtitle, holds the input wait it will start, and moves to starting that
            // wait on this same frame.
            stage = SubtitleStage.StartInputWait;
            frameHold = 1;
            Timer = SecondSubtitleHoldFrames;
        }
        if (--frameHold != 0)
            return;
        switch (stage)
        {
            case SubtitleStage.ShowFirstSubtitle:
                stage = SubtitleStage.FinishFirstSubtitle;
                frameHold = 1;
                return;
            case SubtitleStage.AwaitSecondSubtitleInput:
                frameHold = 1;
                return;
            case SubtitleStage.FinishFirstSubtitle:
                stage = SubtitleStage.None;
                page = null;
                return;
            case SubtitleStage.StartInputWait:
                IntroNarrationPageId waitingPage = page
                    ?? throw new InvalidOperationException("A subtitle starting an input wait has no page.");
                stage = SubtitleStage.None;
                page = null;
                startInputWait(waitingPage);
                return;
            case SubtitleStage.None:
                throw new InvalidOperationException("The intro advanced a subtitle that is not showing.");
            default:
                throw new InvalidOperationException($"Unknown intro subtitle stage {stage}.");
        }
    }

    /// <summary>Starts showing <paramref name="shownPage"/>'s subtitle at <paramref name="firstStage"/>, one frame from its next step.</summary>
    /// <exception cref="InvalidOperationException">Another subtitle is still showing; the intro never shows two.</exception>
    private void Show(IntroNarrationPageId shownPage, SubtitleStage firstStage)
    {
        if (stage != SubtitleStage.None)
            throw new InvalidOperationException(
                $"The intro began page {shownPage}'s subtitle while page {page}'s subtitle was still showing.");
        page = shownPage;
        stage = firstStage;
        frameHold = 1;
    }

    /// <summary>The steps of a page's subtitles.</summary>
    private enum SubtitleStage
    {
        /// <summary>No subtitle is showing.</summary>
        None,
        /// <summary>The page has begun: show its first subtitle, taking one frame.</summary>
        ShowFirstSubtitle,
        /// <summary>The first subtitle is shown: let the page's text continue, then stop showing.</summary>
        FinishFirstSubtitle,
        /// <summary>The page's text has finished: wait, one frame at a time, for a press that shows the second subtitle.</summary>
        AwaitSecondSubtitleInput,
        /// <summary>The second subtitle is shown: start the page's input wait, then stop showing.</summary>
        StartInputWait,
    }
}
