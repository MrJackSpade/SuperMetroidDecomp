using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The narration pages' persistent actors: the portrait eye running its typed <see cref="IntroEyeAnimations"/>
/// animation, the typewriter narration typing the installed text's letters, the text caret, and the Japanese text's
/// next-page arrow at each page's input wait.
/// </summary>
internal sealed partial class IntroNarrationActors
{
    /// <summary>Queues the alternating non-space typewriter sounds.</summary>
    private readonly AudioQueues audio;
    /// <summary>The narration text the typewriter types into and its glows recolour (#1467).</summary>
    private readonly IntroNarrationText text;
    /// <summary>
    /// The persistent portrait eye's animation (#1600, #2488), played by the shared player: it shows its first frame on its
    /// first update, and rests on the final page's stare for good (#2196).
    /// </summary>
    private readonly AnimationPlayer<IntroEyeSegment, IntroEyeFrame, NoAnimationCue> eye =
        AnimationPlayer<IntroEyeSegment, IntroEyeFrame, NoAnimationCue>.Starting(IntroEyeAnimations.Segments, IntroEyeAnimations.StartingWith(IntroEyeSegment.Blinking));
    /// <summary>
    /// The active page's typing (#2488): its subtitle, its characters, the final page's caret hold and its finish, played by
    /// the shared player; none before the first page.
    /// </summary>
    /// <summary>The installed narration artwork, rebound whenever the game is bound to its installed content.</summary>
    private IntroNarrationPresentation narrationPresentation;
    /// <summary>The eye frame the portrait shows, as an installed picture over it (#1467); null before the first frame.</summary>
    internal IntroEyeFrame? EyeFrame => eye.Shown;
    /// <summary>The page being typed, with its glyphs and typing, or null between pages.</summary>
    private IntroNarrationPage? narrationPage;
    /// <summary>The persistent text-caret actor, running its typed animation (#641); <see cref="Caret"/> places it.</summary>
    private readonly CinematicActor<IntroNarrationAnimation, CinematicSpritePose, NoAnimationCue> caretSprite =
        new(default, IntroShades.Drawn(IntroSpriteColouring.CrossFadeActors), IntroNarrationAnimations.Narration, IntroNarrationAnimation.Caret);
    /// <summary>Alternates the typewriter's sound requests while letters appear.</summary>
    private bool typewriterSoundToggle;
    /// <summary>Created by the first glyph to animate per-character glow effects.</summary>
    private CinematicTextGlowSystem? textGlow;
    /// <summary>Tracks subtitle page transitions and the Japanese input wait.</summary>
    private readonly IntroJapaneseSubtitles subtitles;

    /// <summary>Binds caret, narration, narration text, subtitle, and audio owners.</summary>
    public IntroNarrationActors(
        IntroNarrationText text,
        IntroJapaneseSubtitles subtitles,
        AudioQueues audio,
        IntroNarrationPresentation narrationPresentation)
    {
        this.subtitles = Ensure.NotNull(subtitles);
        this.text = Ensure.NotNull(text);
        this.audio = Ensure.NotNull(audio);
        this.narrationPresentation = Ensure.NotNull(narrationPresentation);
    }

    /// <summary>
    /// The page whose input wait has started (#2359): set as its text ends, or, for a Japanese page that waits for it, as
    /// its second subtitle shows; null from the start of the next page until that page's wait starts.
    /// </summary>
    public IntroNarrationPageId? AwaitingInputPage { get; private set; }

    /// <summary>Set after the last page's final hold.</summary>
    public bool FinalHoldEnded { get; private set; }

    /// <summary>
    /// The pose the text caret shows: its animation's pose, or null while the caret is hidden or before its animation's first
    /// frame, when nothing is drawn.
    /// </summary>
    internal CinematicSpritePose? CaretPose => caretHidden ? null : caretSprite.Shown;

    /// <summary>
    /// Whether the caret is hidden while an illustration owns the screen; its animation keeps running, and the next page
    /// shows it again (#1625).
    /// </summary>
    private bool caretHidden;

    /// <summary>The Japanese text's next-page arrow while it shows (#1655); null with English text and between input waits.</summary>
    internal CinematicActor<IntroNarrationAnimation, CinematicSpritePose, NoAnimationCue>? NextPageArrow { get; private set; }

    /// <summary>
    /// A page's input wait accepted the press that turns the page (#1655, #2562): the page-turn setup starts the page's
    /// cross-fade, and the next-page arrow, which watches for the cross-fade's start, goes at once. The game's arrow deletes
    /// itself at its next update, later in the same frame and before anything is drawn, which nothing between reads.
    /// </summary>
    public void AcceptPageTurn()
    {
        NextPageArrow?.Delete();
        NextPageArrow = null;
    }

    /// <summary>The persistent text caret's place, in signed screen pixels from the screen's top-left corner.</summary>
    public Offset16<ScreenPixels> Caret { get; private set; } =
        new(IntroCinematicDefinitions.NarrationLayout.CaretLeftX, IntroCinematicDefinitions.NarrationLayout.CaretFirstTextY);

    /// <summary>
    /// Hides the intro text caret while a gameplay or scientist illustration owns the screen: the caret is not
    /// drawn until the next narration page restores it (#1625).
    /// </summary>
    public void HideCaret() => caretHidden = true;

    /// <summary>What <see cref="StartPage"/> does for each <see cref="IntroNarrationPageOpening"/> member, each member's own arm.</summary>
    private IdentityTable<IntroNarrationPageOpening, Action> StartPageForIntroNarrationPageOpening => field ??= new(new Dictionary<IntroNarrationPageOpening, Action>
    {
        [IntroNarrationPageOpening.KeepsSetupCaret] = () => { },
        [IntroNarrationPageOpening.RestoresCaret] = () => ResetCaret(),
        [IntroNarrationPageOpening.ReplacesEarlierPages] = () =>
        {
            ResetCaret();
            // The eye observes the last page's cinematic phase and permanently changes to its closed, half-open and
            // deadpan animation; the portrait keeps the eye's frame until that animation's first frame shows at the
            // eye's next update.
            eye.StartKeepingShown(IntroEyeAnimations.StartingWith(IntroEyeSegment.FinalPageClosing));
        },
    });

    /// <summary>
    /// Starts <paramref name="page"/>'s letter-by-letter narration as its definition says (#2489): the first page keeps the
    /// caret its setup started, with the eye's blinking and the portrait already showing; a page after an illustration
    /// shows the caret again at the first line; the last page clears the earlier pages' text first and closes the eye for
    /// good.
    /// </summary>
    public void StartPage(IntroNarrationPageId page)
    {
        IntroNarrationPageOpening opening = IntroNarrationPageDefinitions.Pages[page].Opening;
        if (opening == IntroNarrationPageOpening.ReplacesEarlierPages)
            text.ClearText();
        StartNarration(page);
        AwaitingInputPage = null;
        FinalHoldEnded = false;
        StartPageForIntroNarrationPageOpening[opening]();
    }

    /// <summary>Runs the narration's actors in the cinematic game state's order.</summary>
    /// <param name="newlyPressed">This frame's controller rising edges, read by the Japanese subtitles.</param>
    public void Step(ControllerButton newlyPressed)
    {
        caretSprite.Step();
        StepNextPageArrow();
        subtitles.Step(newlyPressed, MarkAwaitingInput);
        eye.Step();
        if (narrationPage is { } page)
            StepInstalledNarration(page);
        textGlow?.Step(text);
    }

    /// <summary>Binds the installed narration content when the game is bound to its installed content.</summary>
    public void BindNarration(IntroNarrationPresentation presentation)
    {
        narrationPresentation = Ensure.NotNull(presentation);
        narrationPage?.Rebind(presentation);
    }

    /// <summary>
    /// Builds the glyphs of an installed page and starts its typing, which shows the page's subtitle once its opening pause
    /// has passed and types its first glyph after the pause again.
    /// </summary>
    private void StartNarration(IntroNarrationPageId page) => narrationPage = new IntroNarrationPage(page, narrationPresentation);

    /// <summary>Runs <paramref name="page"/>'s typing one update: its subtitle, its next glyph with its sound and caret, and its finish.</summary>
    private void StepInstalledNarration(IntroNarrationPage page)
    {
        if (page.Typing.StepToNewFrame(cue => CarryOutForIntroNarrationTypingCue[cue](page.Id)) is { } typed)
            TypeCharacter(page.Characters, typed.Letters - 1);
    }

    /// <summary>What the typing does at each <see cref="IntroNarrationTypingCue"/> it raises, each cue's own arm.</summary>
    private IdentityTable<IntroNarrationTypingCue, Action<IntroNarrationPageId>> CarryOutForIntroNarrationTypingCue => field ??= new(new Dictionary<IntroNarrationTypingCue, Action<IntroNarrationPageId>>
    {
        [IntroNarrationTypingCue.ShowSubtitle] = page =>
        {
            // Each page opens by showing its subtitle.
            subtitles.BeginPage(page);
        },
        [IntroNarrationTypingCue.BlinkCaret] = page => SetCaretBlinking(),
        [IntroNarrationTypingCue.FinishPage] = page =>
        {
            FinishInstalledNarration(page);
            narrationPage = null;
        },
    });

    /// <summary>
    /// Types the page's character at <paramref name="index"/>: starts its glow, moves the caret past it, plays the
    /// alternating typewriter sound and shows it in the text.
    /// </summary>
    private void TypeCharacter(IntroNarrationCharacter[] animation, int index)
    {
        IntroNarrationCharacter character = animation[index];
        IntroNarrationCharacter? next = index + 1 < animation.Length
            ? animation[index + 1]
            : null;
        (textGlow ??= new()).Spawn(character.Column, character.Row);
        if (next is { } following)
        {
            // The caret stands a baseline above the next character's cell.
            Caret = new(
                following.Column * IntroCinematicDefinitions.NarrationLayout.CharacterPixelSize,
                following.Row * IntroCinematicDefinitions.NarrationLayout.CharacterPixelSize -
                    IntroCinematicDefinitions.NarrationLayout.CharacterBaselineOffset);
        }
        else
        {
            Caret = new(
                IntroCinematicDefinitions.NarrationLayout.CaretLeftX,
                (character.Row + 1) * IntroCinematicDefinitions.NarrationLayout.CharacterPixelSize);
        }

        typewriterSoundToggle = !typewriterSoundToggle;
        if (!character.IsSpace && typewriterSoundToggle)
        {
            audio.QueueSound(
                IntroCinematicDefinitions.Sounds.Typewriter,
                priority: IntroCinematicDefinitions.Sounds.SoundPriority);
        }
        text.Type(character);
    }

    /// <summary>Starts the input wait, or marks the intro's last page complete after its hold.</summary>
    private void FinishInstalledNarration(IntroNarrationPageId page)
    {
        if (IntroNarrationPageDefinitions.Pages[page].Closing == IntroNarrationPageClosing.EndsIntro)
        {
            FinalHoldEnded = true;
            return;
        }
        // Once the page is typed the caret starts blinking, and Japanese text shows the next-page arrow. A Japanese page
        // with a second subtitle then waits for the press that shows it, which selects the page's input wait.
        SetCaretBlinking();
        if (subtitles.Enabled)
            SpawnNextPageArrow();
        if (subtitles.FinishPage(page))
            MarkAwaitingInput(page);
    }

    /// <summary>
    /// Publishes the completed page's input wait to the outer intro state machine: only a page that waits for input reaches
    /// its wait, as the intro's last page ends the intro before it (<see cref="FinishInstalledNarration"/>).
    /// </summary>
    private void MarkAwaitingInput(IntroNarrationPageId page) => AwaitingInputPage = page;

    /// <summary>Shows the persistent caret again at the first text line and restarts its non-blinking sprite animation.</summary>
    private void ResetCaret()
    {
        caretHidden = false;
        Caret = new(IntroCinematicDefinitions.NarrationLayout.CaretLeftX, IntroCinematicDefinitions.NarrationLayout.CaretFirstTextY);
        caretSprite.Start(IntroNarrationAnimation.Caret);
    }

    /// <summary>
    /// Spawns the next-page arrow at the screen's middle below the text, its looping animation primed to show its first
    /// frame at its first update. The narration spawns it after this frame's sprite updates, so it first updates on the
    /// next frame. Each page's arrow goes as its page turns (<see cref="AcceptPageTurn"/>), before the next page is typed.
    /// </summary>
    private void SpawnNextPageArrow()
    {
        NextPageArrow = new(
            new(IntroCinematicDefinitions.NarrationLayout.NextPageArrowX, IntroCinematicDefinitions.NarrationLayout.NextPageArrowY),
            IntroShades.Drawn(IntroSpriteColouring.CrossFadeActors), IntroNarrationAnimations.Narration, IntroNarrationAnimation.NextPageArrow);
    }

    /// <summary>
    /// Runs the next-page arrow's update, its own check first: the update after a page's second subtitle starts holding
    /// the input wait, it restarts on its delayed animation, which shows nothing for the hold before the arrow loops
    /// again. The press that turns the page deletes it (<see cref="AcceptPageTurn"/>).
    /// </summary>
    private void StepNextPageArrow()
    {
        if (NextPageArrow is not { } arrow)
            return;
        if (subtitles.HoldFirstCounted)
            arrow.Start(IntroNarrationAnimation.NextPageArrowDelayed);
        arrow.Step();
    }

    /// <summary>Starts the existing caret sprite's blinking animation, whose first frame it shows on this update.</summary>
    internal void SetCaretBlinking()
    {
        // The caret-blink cue starts the existing caret sprite's blinking animation, and the sprite's update that follows
        // shows its first frame at once.
        caretSprite.Start(IntroNarrationAnimation.CaretBlinking);
    }

}
