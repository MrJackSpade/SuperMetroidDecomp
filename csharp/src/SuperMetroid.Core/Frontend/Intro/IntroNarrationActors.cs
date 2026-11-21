using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The narration pages' persistent actors: the portrait eye running its typed <see cref="IntroEyeAnimations"/>
/// animation, the typewriter narration typing the installed text's letters, the text caret, and the Japanese text's
/// next-page arrow at each page's input wait.
/// </summary>
internal sealed class IntroNarrationActors
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
    /// The page being typed (#2488), with its glyphs and its typing; none before the first page and once a page has
    /// finished.
    /// </summary>
    private IntroActiveNarrationPage? activePage;
    /// <summary>The installed narration artwork, rebound whenever the game is bound to its installed content.</summary>
    private IntroNarrationPresentation narrationPresentation;
    /// <summary>The eye frame the portrait shows, as an installed picture over it (#1467); null before the first frame.</summary>
    internal IntroEyeFrame? EyeFrame => eye.Shown;
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
        switch (opening)
        {
            case IntroNarrationPageOpening.KeepsSetupCaret:
                return;
            case IntroNarrationPageOpening.RestoresCaret:
                ResetCaret();
                return;
            case IntroNarrationPageOpening.ReplacesEarlierPages:
                ResetCaret();
                // The eye observes the last page's cinematic phase and permanently changes to its closed, half-open and
                // deadpan animation; the portrait keeps the eye's frame until that animation's first frame shows at the
                // eye's next update.
                eye.StartKeepingShown(IntroEyeAnimations.StartingWith(IntroEyeSegment.FinalPageClosing));
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(IntroNarrationPageOpening)} {opening}.");
        }
    }

    /// <summary>Runs the narration's actors in the cinematic game state's order.</summary>
    /// <param name="newlyPressed">This frame's controller rising edges, read by the Japanese subtitles.</param>
    public void Step(ControllerButton newlyPressed)
    {
        caretSprite.Step();
        StepNextPageArrow();
        subtitles.Step(newlyPressed, MarkAwaitingInput);
        eye.Step();
        if (activePage is { } page)
            StepInstalledNarration(page);
        textGlow?.Step(text);
    }

    /// <summary>Binds the installed narration content when the game is bound to its installed content.</summary>
    public void BindNarration(IntroNarrationPresentation presentation)
    {
        narrationPresentation = Ensure.NotNull(presentation);
        if (activePage is not { } active)
            return;
        IntroNarrationPageId page = active.Page;
        active.Characters = presentation.Compile(page);
        // The characters the page has typed, which its typing shows; none before its first.
        int typedCharacters = active.Typing.Shown?.Letters ?? 0;
        if (typedCharacters > active.Characters.Length)
        {
            throw new InvalidDataException(
                $"Saved narration cursor {typedCharacters} exceeds the rebound {page} narration.");
        }
        AnimationSegments<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue> rebound =
            IntroNarrationTypingAnimations.Segments(page, active.Characters.Length);
        active.Typing.Rebind(rebound, IntroNarrationTypingAnimations.Typing(rebound, page));
    }

    /// <summary>
    /// Builds the glyphs of an installed page and starts its typing, which shows the page's subtitle once its opening pause
    /// has passed and types its first glyph after the pause again.
    /// </summary>
    private void StartNarration(IntroNarrationPageId page)
    {
        IntroNarrationCharacter[] characters = narrationPresentation.Compile(page);
        AnimationSegments<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue> segments =
            IntroNarrationTypingAnimations.Segments(page, characters.Length);
        activePage = new IntroActiveNarrationPage(page, characters,
            AnimationPlayer<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue>.StartingAfter(
                segments, IntroNarrationTypingAnimations.Typing(segments, page), IntroNarrationDefinitions.PageOpeningPauseFrames));
    }

    /// <summary>Runs the page's typing one update: its subtitle, its next glyph with its sound and caret, and its finish.</summary>
    private void StepInstalledNarration(IntroActiveNarrationPage page)
    {
        if (page.Typing.StepToNewFrame(cue => CarryOut(page.Page, cue)) is { } typed)
            TypeCharacter(page.Characters, typed.Letters - 1);
    }

    /// <summary>Carries out <paramref name="cue"/> of the page's typing.</summary>
    /// <exception cref="InvalidOperationException">The cue is not one a page's typing raises.</exception>
    private void CarryOut(IntroNarrationPageId page, IntroNarrationTypingCue cue)
    {
        switch (cue)
        {
            case IntroNarrationTypingCue.ShowSubtitle:
                // Each page opens by showing its subtitle.
                subtitles.BeginPage(page);
                return;
            case IntroNarrationTypingCue.BlinkCaret:
                SetCaretBlinking();
                return;
            case IntroNarrationTypingCue.FinishPage:
                FinishInstalledNarration(page);
                activePage = null;
                return;
            default:
                throw new InvalidOperationException($"Undefined narration typing cue {cue}.");
        }
    }

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

    /// <summary>Publishes the completed page's input wait to the outer intro state machine.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The page ends the intro instead of waiting for input.</exception>
    private void MarkAwaitingInput(IntroNarrationPageId page) =>
        AwaitingInputPage = IntroNarrationPageDefinitions.Pages[page].Closing != IntroNarrationPageClosing.EndsIntro
            ? page
            : throw new ArgumentOutOfRangeException(nameof(page), page, "The intro's last page ends the intro instead of waiting for input.");

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
    /// next frame.
    /// </summary>
    private void SpawnNextPageArrow()
    {
        if (NextPageArrow is not null)
            throw new InvalidOperationException("The narration spawned a next-page arrow while one was still showing.");
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
    private void SetCaretBlinking()
    {
        // The caret-blink cue starts the existing caret sprite's blinking animation, and the sprite's update that follows
        // shows its first frame at once.
        caretSprite.Start(IntroNarrationAnimation.CaretBlinking);
    }

}
