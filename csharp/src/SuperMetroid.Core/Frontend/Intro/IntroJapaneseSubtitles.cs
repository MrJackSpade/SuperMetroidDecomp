using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening's Japanese subtitle sequencer: it shows each page's first subtitle as the page begins, and on a page with a
/// second subtitle waits at the end of the page for a press that shows it, which then holds the page's input wait for
/// <see cref="SecondSubtitleHoldFrames"/>. English text shows no subtitle and leaves the hold at zero.
/// </summary>
/// <remarks>
/// At most one subtitle is showing at a time: a page's first subtitle has finished long before the page's text ends,
/// and the second subtitle starts the input wait that leads to the next page. The intro advances the subtitle after
/// its cinematic sprites and before the page text that begins and finishes subtitles, so a subtitle first advances on
/// the frame after it begins. Each of a subtitle's steps takes one update (#2488), so a step is taken on every update
/// the subtitle shows.
/// </remarks>
internal sealed partial class IntroJapaneseSubtitles
{
    /// <summary>Frames the second subtitle holds a page's input wait.</summary>
    public const int SecondSubtitleHoldFrames = 60;

    /// <summary>
    /// The subtitle showing, the page it belongs to and what it does on its next update, or null while none is showing: one
    /// at a time, so no two subtitles show at once (#2553).
    /// </summary>
    private ShowingSubtitle? showing;

    /// <summary>Selects Japanese subtitle handling from the opening's alternate-text setting.</summary>
    public IntroJapaneseSubtitles(TextLanguage language) => Language = language;

    /// <summary>The Japanese text option at the start of the intro.</summary>
    public bool Enabled => Language == TextLanguage.Japanese;

    /// <summary>The game's text language, which shows the subtitles in Japanese (#2560).</summary>
    public TextLanguage Language { get; }

    /// <summary>
    /// The second subtitle's hold on the page's input wait, counted once by each of the wait's hold checks; over when the
    /// intro starts.
    /// </summary>
    private UpdateCountdown inputWaitHold = UpdateCountdown.Ended();

    /// <summary>
    /// True on the one update the input wait's hold has counted its first update: the second subtitle showed on the update
    /// before. The next-page arrow hides for the hold from then (#1655).
    /// </summary>
    public bool HoldFirstCounted => inputWaitHold.HasCountedOnlyItsFirst;

    /// <summary>Begins <paramref name="page"/>'s text: Japanese text shows the page's first subtitle.</summary>
    public void BeginPage(IntroNarrationPageId page)
    {
        if (Enabled)
            Show(page, SubtitleStage.ShowFirstSubtitle);
    }

    /// <summary>
    /// Finishes <paramref name="page"/>'s text. Returns true when the page's input wait starts now; on a page with a second
    /// subtitle (<see cref="IntroNarrationPageClosing.AwaitsSecondSubtitle"/>) Japanese text instead waits for a press
    /// that shows it, and the subtitle starts the wait.
    /// </summary>
    public bool FinishPage(IntroNarrationPageId page)
    {
        if (!Enabled || IntroNarrationPageDefinitions.Pages[page].Closing != IntroNarrationPageClosing.AwaitsSecondSubtitle)
            return true;
        Show(page, SubtitleStage.AwaitSecondSubtitleInput);
        return false;
    }

    /// <summary>
    /// The hold check at the start of every intro input wait, in each page's setup.
    /// True while the wait is held: this update used one frame of the hold.
    /// </summary>
    public bool HoldInputWait()
    {
        if (inputWaitHold.IsOver)
            return false;
        inputWaitHold.Count();
        return true;
    }

    /// <summary>What <c>Step</c> does for each <see cref="SubtitleStage"/> member, each member's own arm, given its page.</summary>
    private IdentityTable<SubtitleStage, Action<IntroNarrationPageId, Action<IntroNarrationPageId>>> StepForSubtitleStage => field ??= new(new Dictionary<SubtitleStage, Action<IntroNarrationPageId, Action<IntroNarrationPageId>>>
    {
        [SubtitleStage.ShowFirstSubtitle] = (shownPage, startInputWait) => showing = new(shownPage, SubtitleStage.FinishFirstSubtitle),
        [SubtitleStage.AwaitSecondSubtitleInput] = (shownPage, startInputWait) => { },
        [SubtitleStage.FinishFirstSubtitle] = (shownPage, startInputWait) => showing = null,
        [SubtitleStage.StartInputWait] = (shownPage, startInputWait) =>
        {
            showing = null;
            startInputWait(shownPage);
        },
    });

    /// <summary>Takes the showing subtitle's step for this update.</summary>
    /// <param name="newlyPressed">This frame's newly pressed buttons, which show a waiting page's second subtitle.</param>
    /// <param name="startInputWait">Receives the page whose second subtitle has started its input wait.</param>
    /// <remarks>With no subtitle showing, the step does nothing.</remarks>
    public void Step(ControllerButton newlyPressed, Action<IntroNarrationPageId> startInputWait)
    {
        if (showing is not { } shown)
            return;
        if (shown.Stage == SubtitleStage.AwaitSecondSubtitleInput && newlyPressed != ControllerButton.None)
        {
            // The press shows the second subtitle, holds the input wait it will start, and moves to starting that
            // wait on this same frame.
            shown = shown with { Stage = SubtitleStage.StartInputWait };
            inputWaitHold = UpdateCountdown.Lasting(SecondSubtitleHoldFrames);
        }
        StepForSubtitleStage[shown.Stage](shown.Page, startInputWait);
    }

    /// <summary>Starts showing <paramref name="shownPage"/>'s subtitle at <paramref name="firstStage"/>, which its next update takes.</summary>
    private void Show(IntroNarrationPageId shownPage, SubtitleStage firstStage) => showing = new(shownPage, firstStage);

    /// <summary>A subtitle showing: the page it belongs to and what it does on its next update.</summary>
    /// <param name="Page">The page whose subtitle is showing.</param>
    /// <param name="Stage">What the subtitle does on its next update.</param>
    private readonly record struct ShowingSubtitle(IntroNarrationPageId Page, SubtitleStage Stage);

    /// <summary>The steps of a page's subtitles.</summary>
    private enum SubtitleStage
    {
        /// <summary>The page has begun: show its first subtitle.</summary>
        ShowFirstSubtitle,
        /// <summary>The first subtitle is shown: let the page's text continue, then stop showing.</summary>
        FinishFirstSubtitle,
        /// <summary>The page's text has finished: wait, an update at a time, for a press that shows the second subtitle.</summary>
        AwaitSecondSubtitleInput,
        /// <summary>The second subtitle is shown: start the page's input wait, then stop showing.</summary>
        StartInputWait,
    }
}
