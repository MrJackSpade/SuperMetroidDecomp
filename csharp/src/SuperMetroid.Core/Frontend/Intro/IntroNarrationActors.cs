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
<<<<<<< ours
    private IntroActiveNarrationPage? activePage;
||||||| base
    private AnimationPlayer<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue>? typing;
=======
>>>>>>> theirs
    /// <summary>The installed narration artwork, rebound whenever the game is bound to its installed content.</summary>
    private IntroNarrationPresentation narrationPresentation;
    /// <summary>The eye frame the portrait shows, as an installed picture over it (#1467); null before the first frame.</summary>
    internal IntroEyeFrame? EyeFrame => eye.Shown;
<<<<<<< ours
||||||| base
    /// <summary>The page glyphs, built from the installed presentation when it is bound.</summary>
    private IntroNarrationCharacter[]? narrationAnimation;
    /// <summary>The bound narration page's characters.</summary>
    private IntroNarrationCharacter[] NarrationAnimation => narrationAnimation ??
        throw new InvalidOperationException("No narration page is bound.");
    /// <summary>Page whose glyphs and Japanese subtitle markers are active.</summary>
    private IntroNarrationPageId? narrationPage;
    /// <summary>The characters the active page has typed, which its typing shows; none before its first.</summary>
    private int TypedCharacters => typing?.Shown?.Letters ?? 0;
=======
    /// <summary>The page being typed, with its glyphs and typing, or null between pages.</summary>
    private IntroNarrationPage? narrationPage;
>>>>>>> theirs
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
<<<<<<< ours
        if (activePage is { } page)
||||||| base
        if (narrationAnimation is not null)
            StepInstalledNarration();
=======
        if (narrationPage is { } page)
>>>>>>> theirs
            StepInstalledNarration(page);
        textGlow?.Step(text);
    }

    /// <summary>Binds the installed narration content when the game is bound to its installed content.</summary>
    public void BindNarration(IntroNarrationPresentation presentation)
    {
        narrationPresentation = Ensure.NotNull(presentation);
<<<<<<< ours
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
||||||| base
        if (narrationPage is not { } page)
        {
            narrationAnimation = null;
            return;
        }
        narrationAnimation = presentation.Compile(page);
        if (TypedCharacters > narrationAnimation.Length)
        {
            throw new InvalidDataException(
                $"Saved narration cursor {TypedCharacters} exceeds the rebound {page} narration.");
        }
        AnimationSegments<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue> rebound =
            IntroNarrationTypingAnimations.Segments(page, narrationAnimation.Length);
        Typing.Rebind(rebound, IntroNarrationTypingAnimations.Typing(rebound, page));
=======
        narrationPage?.Rebind(presentation);
>>>>>>> theirs
    }

    /// <summary>
    /// Builds the glyphs of an installed page and starts its typing, which shows the page's subtitle once its opening pause
    /// has passed and types its first glyph after the pause again.
    /// </summary>
<<<<<<< ours
    private void StartNarration(IntroNarrationPageId page)
    {
        IntroNarrationCharacter[] characters = narrationPresentation.Compile(page);
        AnimationSegments<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue> segments =
            IntroNarrationTypingAnimations.Segments(page, characters.Length);
        activePage = new IntroActiveNarrationPage(page, characters,
            AnimationPlayer<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue>.StartingAfter(
                segments, IntroNarrationTypingAnimations.Typing(segments, page), IntroNarrationDefinitions.PageOpeningPauseFrames));
    }
||||||| base
    private void StartNarration(IntroNarrationPageId page)
    {
        narrationPage = page;
        narrationAnimation = narrationPresentation.Compile(page);
        AnimationSegments<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue> segments =
            IntroNarrationTypingAnimations.Segments(page, narrationAnimation.Length);
        typing = AnimationPlayer<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue>.StartingAfter(
            segments, IntroNarrationTypingAnimations.Typing(segments, page), IntroNarrationDefinitions.PageOpeningPauseFrames);
    }
=======
    private void StartNarration(IntroNarrationPageId page) => narrationPage = new IntroNarrationPage(page, narrationPresentation);
>>>>>>> theirs

<<<<<<< ours
    /// <summary>Runs the page's typing one update: its subtitle, its next glyph with its sound and caret, and its finish.</summary>
    private void StepInstalledNarration(IntroActiveNarrationPage page)
||||||| base
    /// <summary>Runs the page's typing one update: its subtitle, its next glyph with its sound and caret, and its finish.</summary>
    private void StepInstalledNarration()
=======
    /// <summary>Runs <paramref name="page"/>'s typing one update: its subtitle, its next glyph with its sound and caret, and its finish.</summary>
    private void StepInstalledNarration(IntroNarrationPage page)
>>>>>>> theirs
    {
<<<<<<< ours
        if (page.Typing.StepToNewFrame(cue => CarryOut(page.Page, cue)) is { } typed)
||||||| base
        if (Typing.StepToNewFrame(CarryOut) is { } typed)
            TypeCharacter(typed.Letters - 1);
=======
        if (page.Typing.StepToNewFrame(cue => CarryOutForIntroNarrationTypingCue[cue](page.Id)) is { } typed)
>>>>>>> theirs
            TypeCharacter(page.Characters, typed.Letters - 1);
    }

<<<<<<< ours
    /// <summary>Carries out <paramref name="cue"/> of the page's typing.</summary>
    /// <exception cref="InvalidOperationException">The cue is not one a page's typing raises.</exception>
    private void CarryOut(IntroNarrationPageId page, IntroNarrationTypingCue cue)
||||||| base
    /// <summary>Carries out <paramref name="cue"/> of the page's typing.</summary>
    /// <exception cref="InvalidOperationException">The cue is not one a page's typing raises.</exception>
    private void CarryOut(IntroNarrationTypingCue cue)
=======
    /// <summary>What the typing does at each <see cref="IntroNarrationTypingCue"/> it raises, each cue's own arm.</summary>
    private IdentityTable<IntroNarrationTypingCue, Action<IntroNarrationPageId>> CarryOutForIntroNarrationTypingCue => field ??= new(new Dictionary<IntroNarrationTypingCue, Action<IntroNarrationPageId>>
>>>>>>> theirs
    {
<<<<<<< ours
        switch (cue)
||||||| base
        IntroNarrationPageId page = narrationPage ?? throw new InvalidDataException(
            "Installed narration typed without an active page identity.");
        switch (cue)
=======
        [IntroNarrationTypingCue.ShowSubtitle] = page =>
>>>>>>> theirs
        {
<<<<<<< ours
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
||||||| base
            case IntroNarrationTypingCue.ShowSubtitle:
                // Each page opens by showing its subtitle.
                subtitles.BeginPage(page);
                return;
            case IntroNarrationTypingCue.BlinkCaret:
                SetCaretBlinking();
                return;
            case IntroNarrationTypingCue.FinishPage:
                FinishInstalledNarration(page);
                narrationAnimation = null;
                narrationPage = null;
                return;
            default:
                throw new InvalidOperationException($"Undefined narration typing cue {cue}.");
        }
    }
=======
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
>>>>>>> theirs

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
    private void SetCaretBlinking()
    {
        // The caret-blink cue starts the existing caret sprite's blinking animation, and the sprite's update that follows
        // shows its first frame at once.
        caretSprite.Start(IntroNarrationAnimation.CaretBlinking);
    }

}
