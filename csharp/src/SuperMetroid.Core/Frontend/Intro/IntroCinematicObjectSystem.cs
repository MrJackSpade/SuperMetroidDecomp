using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The narration pages' persistent objects: the portrait eye running its typed <see cref="IntroEyeAnimations"/>
/// program, the typewriter narration written from installed glyph records, and the text caret.
/// </summary>
internal sealed class IntroCinematicObjectSystem
{
    /// <summary>Queues alternating non-space typewriter sounds when audio is present.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>The shown portrait and narration tilemaps: eye-frame rectangles and narration uploads land here.</summary>
    private readonly IntroTilemaps tilemaps;
    /// <summary>Receives narration glyph words at their authored row and column.</summary>
    private readonly ushort[] textTilemap;
    /// <summary>Where the persistent portrait eye's program resumes when its frame expires.</summary>
    internal AnimationProgramCursor<IntroEyeProgram> eyeProgram =
        AnimationProgramCursor<IntroEyeProgram>.Start(IntroEyeProgram.Blinking);
    /// <summary>Countdown before the eye shows its program's next frame.</summary>
    internal NativeCountdown eyeInstructionTimer = NativeCountdown.Start(1);
    /// <summary>Countdown for initial marker, glyph, or final page hold.</summary>
    private NativeCountdown textInstructionTimer;
    /// <summary>Caller-installed narration artwork, rebound after debugger restoration.</summary>
    [NonSerialized] private IntroNarrationPresentation? narrationPresentation;
    /// <summary>Caller-installed eye rectangles, rebound after debugger restoration.</summary>
    [NonSerialized] private IntroEyeTilemapPresentation? eyeArtwork;
    /// <summary>The eye frame on the portrait, redrawn when the eye artwork is rebound; null before the first frame.</summary>
    private IntroEyeFrame? currentEyeFrame;
    /// <summary>Compiled page glyphs, rebound from the installed presentation after restore.</summary>
    [NonSerialized] private IntroNarrationCharacter[]? narrationProgram;
    /// <summary>The bound narration page's characters.</summary>
    private IntroNarrationCharacter[] NarrationProgram => narrationProgram ??
        throw new InvalidOperationException("No narration page is bound.");
    /// <summary>Page whose glyph stream and Japanese subtitle markers are active.</summary>
    private IntroNarrationPageId? narrationPage;
    /// <summary>Next semantic glyph in the active narration page.</summary>
    private int narrationCharacterIndex;
    /// <summary>Whether the page's subtitle creation marker has yet to execute.</summary>
    private bool narrationInitialMarkerPending;
    /// <summary>Whether page six entered its final 128-frame caret hold.</summary>
    private bool narrationFinalHoldStarted;
    /// <summary>The persistent text-caret sprite object, running its typed program (#641); <see cref="Caret"/> places it.</summary>
    private readonly IntroDiscoverySprite caretSprite = new(default, 0, CinematicSpriteProgram.IntroTextCaret);
    /// <summary>Alternates typewriter audio calls while letters appear.</summary>
    private bool typewriterSoundToggle;
    /// <summary>Created by the first glyph to animate per-character glow effects.</summary>
    private CinematicTextGlowSystem? textGlow;
    /// <summary>Tracks subtitle page transitions and the Japanese input wait.</summary>
    private readonly IntroJapaneseSubtitles subtitles;

    /// <summary>Binds eye, caret, narration, shown tilemaps, subtitle, and optional audio owners.</summary>
    public IntroCinematicObjectSystem(
        IntroTilemaps tilemaps,
        ushort[] textTilemap,
        IntroJapaneseSubtitles subtitles,
        CartridgeAudioState? audio = null,
        IntroNarrationPresentation? narrationPresentation = null,
        IntroEyeTilemapPresentation? eyeArtwork = null)
    {
        this.subtitles = Ensure.NotNull(subtitles);
        this.tilemaps = Ensure.NotNull(tilemaps);
        this.textTilemap = Ensure.NotNull(textTilemap);
        this.audio = audio;
        this.narrationPresentation = narrationPresentation;
        this.eyeArtwork = eyeArtwork;
        Ensure.LengthEqual(textTilemap, IntroCinematicRomData.Layers.TextTilemapWordCount);
    }

    /// <summary>Set when opcode $8B:AE5B reaches the English page-one input marker.</summary>
    public bool PageOneAwaitingInput { get; private set; }

    /// <summary>Set when opcode $8B:AE91 reaches the English page-two input marker.</summary>
    public bool PageTwoAwaitingInput { get; private set; }

    /// <summary>Set when opcode $8B:B08C completes English page three.</summary>
    public bool PageThreeAwaitingInput { get; private set; }

    /// <summary>Set when opcode $8B:B0CB completes English page four.</summary>
    public bool PageFourAwaitingInput { get; private set; }

    /// <summary>Set when opcode $8B:B1B3 completes English page five.</summary>
    public bool PageFiveAwaitingInput { get; private set; }

    /// <summary>Set by $8B:B240 after page six's final 128-frame hold.</summary>
    public bool IntroFinishRequested { get; private set; }

    /// <summary>The pose the text caret's program shows; null before its first frame.</summary>
    internal CinematicSpritePose? CaretPose => caretSprite.Pose;

    /// <summary>Current native screen position of the persistent text caret object; Y=$F8 deliberately hides it below the viewport.</summary>
    public Position16<ScreenPixels> Caret { get; private set; } =
        new(IntroCinematicRomData.ObjectSystem.CaretLeftX, IntroCinematicRomData.ObjectSystem.CaretFirstTextY);

    /// <summary>Reapply the active eye rectangle after restoring a state or changing art.</summary>
    public void BindEyeArtwork(IntroEyeTilemapPresentation? value)
    {
        eyeArtwork = value;
        if (value is not null && currentEyeFrame is { } frame)
            CopyRectangleToPortrait(
                IntroEyeAnimationDefinitions.PortraitCell,
                IntroEyeAnimationDefinitions.FrameSize,
                value.FrameWords(EyeArtworkIndex(frame)));
    }

    /// <summary>
    /// Implements <c>PlaceIntroTextCaretOffScreen</c> at $8B:ADE1. The retail game keeps
    /// the same object slot alive between narration pages and visibly moves it to Y=$F8
    /// while a gameplay or scientist illustration owns the screen.
    /// </summary>
    public void PlaceCaretOffScreen()
    {
        Caret = new(IntroCinematicRomData.ObjectSystem.CaretLeftX, IntroCinematicRomData.ObjectSystem.CaretInitialY);
    }

    /// <summary>
    /// Starts the letter-by-letter narration stream from definition $8B:CF3F. The eye-blink
    /// stream from $8B:CF63 and the portrait-border sprite are already active at page setup.
    /// </summary>
    public void StartEnglishPageOne()
    {
        StartNarration(
            IntroNarrationPageId.Page1);
    }

    /// <summary>
    /// Spawns definition $8B:CF45 and restores the existing caret object as $8B:B35F/B3F0 do.
    /// </summary>
    public void StartEnglishPageTwo()
    {
        StartNarration(
            IntroNarrationPageId.Page2);
        PageTwoAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>
    /// Spawns definition $8B:CF4B after the egg actor requests page three at $8B:B33E.
    /// </summary>
    public void StartEnglishPageThree()
    {
        StartNarration(
            IntroNarrationPageId.Page3);
        PageThreeAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>Spawns the page-four text definition at $8B:CF51 / $8C:CE33.</summary>
    public void StartEnglishPageFour()
    {
        StartNarration(
            IntroNarrationPageId.Page4);
        PageFourAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>Spawns the page-five text definition at $8B:CF57 / $8C:D15D.</summary>
    public void StartEnglishPageFive()
    {
        StartNarration(
            IntroNarrationPageId.Page5);
        PageFiveAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>Clears pages one-through-five and starts final text definition $8C:D511.</summary>
    public void StartEnglishPageSix()
    {
        Array.Fill(
            textTilemap,
            IntroCinematicRomData.Text.Blank.Raw,
            startIndex: IntroCinematicRomData.Text.GameplayBlankStartIndex,
            count: IntroCinematicRomData.Text.GameplayBlankWordCount);
        StartNarration(
            IntroNarrationPageId.Page6);
        IntroFinishRequested = false;
        ResetCaret();

        // B4BC observes the page-six cinematic function and permanently changes the eye
        // object to its closed/half-open/deadpan program.
        eyeProgram = AnimationProgramCursor<IntroEyeProgram>.Start(IntroEyeProgram.FinalPageClosing);
        eyeInstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Runs the same post-state-function object order as game state $25.</summary>
    /// <param name="newlyPressed">This frame's controller rising edges, read by the Japanese subtitles.</param>
    public void Step(SnesButton newlyPressed)
    {
        if (narrationPage is not null && narrationProgram is null)
        {
            throw new InvalidOperationException(
                "Opening narration host content must be rebound after state restoration.");
        }
        caretSprite.Step();
        subtitles.Step(newlyPressed, MarkAwaitingInput);
        StepEye();
        if (narrationProgram is not null)
            StepInstalledNarration();
        textGlow?.Step(textTilemap);

        // UpdateCinematicBgTilemap queues $780 bytes from $7E:3000 to VMADD $4C00.
        // Applying it immediately is the desktop equivalent of observing the following NMI.
        textTilemap.AsSpan(0, IntroCinematicRomData.Layers.VisibleTextTransferWordCount).CopyTo(tilemaps.Narration);
    }

    /// <summary>Rebinds host-owned narration content after override reload or state restore.</summary>
    public void BindNarration(IntroNarrationPresentation? presentation)
    {
        narrationPresentation = presentation;
        narrationProgram = narrationPage is { } page && presentation is not null
            ? presentation.Compile(page)
            : null;
        if (narrationProgram is not null && narrationCharacterIndex > narrationProgram.Length)
        {
            throw new InvalidDataException(
                $"Saved narration cursor {narrationCharacterIndex} exceeds the rebound {narrationPage} program.");
        }
    }

    /// <summary>Compiles an installed page and primes its subtitle marker before the first glyph.</summary>
    private void StartNarration(IntroNarrationPageId page)
    {
        textInstructionTimer = NativeCountdown.Start(IntroNarrationDefinitions.InitialMarkerDelayFrames);
        narrationCharacterIndex = 0;
        narrationInitialMarkerPending = true;
        narrationFinalHoldStarted = false;
        if (narrationPresentation is null)
            throw new InvalidOperationException(
                "Opening narration requires installed text presentation assets.");

        narrationPage = page;
        narrationProgram = narrationPresentation.Compile(page);
    }

    /// <summary>Advances marker, glyph, typewriter sound, caret, and final-hold timing.</summary>
    private void StepInstalledNarration()
    {
        textInstructionTimer = textInstructionTimer.Decrement().Remaining;
        if (!textInstructionTimer.IsZero)
            return;
        if (narrationInitialMarkerPending)
        {
            // Each page list opens with Instruction_HandleCreatingSubtitle_PageN.
            subtitles.BeginPage(narrationPage ?? throw new InvalidDataException(
                "Installed narration started without an active page identity."));
            narrationInitialMarkerPending = false;
            textInstructionTimer = NativeCountdown.Start(IntroNarrationDefinitions.InitialMarkerDelayFrames);
            return;
        }

        IntroNarrationCharacter[] program = NarrationProgram;
        if (narrationCharacterIndex < program.Length)
        {
            IntroNarrationCharacter character = program[narrationCharacterIndex];
            IntroNarrationCharacter? next = narrationCharacterIndex + 1 < program.Length
                ? program[narrationCharacterIndex + 1]
                : null;
            (textGlow ??= new()).Spawn(new(checked((ushort)character.Column), checked((ushort)character.Row)), new(1, 1));
            if (next is { } following)
            {
                Caret = new(
                    unchecked((ushort)(following.Column *
                        IntroCinematicRomData.ObjectSystem.CharacterPixelSize)),
                    unchecked((ushort)(following.Row *
                        IntroCinematicRomData.ObjectSystem.CharacterPixelSize -
                        IntroCinematicRomData.ObjectSystem.CharacterBaselineOffset)));
            }
            else
            {
                Caret = new(
                    IntroCinematicRomData.ObjectSystem.CaretLeftX,
                    unchecked((ushort)((character.Row + 1) *
                        IntroCinematicRomData.ObjectSystem.CharacterPixelSize)));
            }

            typewriterSoundToggle = !typewriterSoundToggle;
            if (!character.IsSpace && typewriterSoundToggle)
            {
                audio?.QueueSound(
                    IntroCinematicRomData.Objects.Typewriter,
                    maximumQueued: IntroCinematicRomData.Objects.MaximumQueuedSounds);
            }
            textTilemap[character.Row * IntroCinematicRomData.Layers.TilemapWidth +
                character.Column] = character.TilemapWord.Raw;
            narrationCharacterIndex++;
            textInstructionTimer = NativeCountdown.Start(IntroNarrationDefinitions.CharacterDelayFrames);
            return;
        }

        if (narrationPage == IntroNarrationPageId.Page6 && !narrationFinalHoldStarted)
        {
            SetCaretBlinking();
            narrationFinalHoldStarted = true;
            textInstructionTimer = NativeCountdown.Start(IntroNarrationDefinitions.FinalPageHoldFrames);
            return;
        }

        FinishInstalledNarration(narrationPage ?? throw new InvalidDataException(
            "Installed narration completed without an active page identity."));
        narrationProgram = null;
        narrationPage = null;
    }

    /// <summary>Starts the input wait or marks the sixth page complete after its hold.</summary>
    private void FinishInstalledNarration(IntroNarrationPageId page)
    {
        if (page == IntroNarrationPageId.Page6)
        {
            IntroFinishRequested = true;
            return;
        }
        // Instruction_SpawnBlinkingMarkers_WaitForInput_PageN ($8B:AE5B and siblings) first
        // sets the caret blinking. Japanese pages two to five then wait for the press that
        // shows their second subtitle, whose object selects the page's input wait.
        SetCaretBlinking();
        if (subtitles.FinishPage(page))
            MarkAwaitingInput(page);
    }

    /// <summary>Publishes the completed page's input marker to the outer intro state machine.</summary>
    private void MarkAwaitingInput(IntroNarrationPageId page)
    {
        switch (page)
        {
            case IntroNarrationPageId.Page1: PageOneAwaitingInput = true; break;
            case IntroNarrationPageId.Page2: PageTwoAwaitingInput = true; break;
            case IntroNarrationPageId.Page3: PageThreeAwaitingInput = true; break;
            case IntroNarrationPageId.Page4: PageFourAwaitingInput = true; break;
            case IntroNarrationPageId.Page5: PageFiveAwaitingInput = true; break;
            default: throw new ArgumentOutOfRangeException(nameof(page), page, null);
        }
    }

    /// <summary>
    /// Advances the portrait eye one cinematic update: when its frame expires, the program shows its next frame for
    /// that frame's duration and the frame's rectangle is drawn on the portrait.
    /// </summary>
    private void StepEye()
    {
        eyeInstructionTimer = eyeInstructionTimer.Decrement().Remaining;
        if (!eyeInstructionTimer.IsZero)
            return;

        // The eye programs load no loop counter; the program set still takes one.
        NativeCountdown loopCounter = default;
        AnimationProgramAdvance<IntroEyeFrame> advance = IntroEyeAnimations.Programs.Advance(
            ref eyeProgram, ref loopCounter, static (_, _) => null);
        if (advance.Kind != AnimationProgramAdvanceKind.Show)
            throw new InvalidOperationException($"The intro eye program {eyeProgram.Program} ended an update with {advance.Kind}.");
        eyeInstructionTimer = NativeCountdown.Start(advance.Duration);
        ShowEyeFrame(advance.Frame);
    }

    /// <summary>Draws <paramref name="frame"/>'s installed rectangle on the portrait and keeps it for artwork rebinding.</summary>
    internal void ShowEyeFrame(IntroEyeFrame frame)
    {
        currentEyeFrame = frame;
        IntroEyeTilemapPresentation content = eyeArtwork ?? throw new InvalidOperationException(
            "Intro eye animation requires installed eye artwork.");
        CopyRectangleToPortrait(
            IntroEyeAnimationDefinitions.PortraitCell,
            IntroEyeAnimationDefinitions.FrameSize,
            content.FrameWords(EyeArtworkIndex(frame)));
    }

    /// <summary>The position of <paramref name="frame"/> in the installed eye artwork, which keeps the cartridge's draw-record order.</summary>
    private static int EyeArtworkIndex(IntroEyeFrame frame) => frame switch
    {
        IntroEyeFrame.Open => 0,
        IntroEyeFrame.HalfOpen => 1,
        IntroEyeFrame.Closed => 2,
        IntroEyeFrame.Deadpan => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined intro eye frame."),
    };

    /// <summary>Validates and writes each eye tilemap row into the portrait VRAM region.</summary>
    private void CopyRectangleToPortrait(Position16<TilemapCells> destination, Extent16<TilemapCells> size,
        ReadOnlySpan<ushort> source)
    {
        ValidateRectangle(destination, size);
        if (source.Length != size.Area)
            throw new InvalidDataException("Opening eye frame does not match its native rectangle size.");
        Span<ushort> portrait = tilemaps.Portrait;
        for (int row = 0; row < size.Y; row++)
        {
            source.Slice(row * size.X, size.X).CopyTo(
                portrait[((destination.Y + row) * IntroCinematicRomData.Layers.TilemapWidth + destination.X)..]);
        }
    }

    /// <summary>Moves the persistent caret onscreen and restarts its non-blinking sprite list.</summary>
    private void ResetCaret()
    {
        // RestIntroTextCaret ($8B:ADEE) moves the persistent slot back from Y=$F8 before
        // restoring its non-blinking list. Position is state, not a renderer constant.
        Caret = new(IntroCinematicRomData.ObjectSystem.CaretLeftX, IntroCinematicRomData.ObjectSystem.CaretFirstTextY);
        caretSprite.Start(CinematicSpriteProgram.IntroTextCaret);
    }

    /// <summary>Points the existing caret slot at its blinking list and primes its timer.</summary>
    internal void SetCaretBlinking()
    {
        // Instruction_SetCaretToBlink starts the existing slot's blinking program and primes timer
        // one, allowing the generic sprite handler to select the first frame now.
        caretSprite.Start(CinematicSpriteProgram.IntroTextCaretBlink);
    }

    /// <summary>Rejects any eye rectangle extending beyond the 32-by-32 portrait tilemap.</summary>
    private static void ValidateRectangle(Position16<TilemapCells> origin, Extent16<TilemapCells> size)
    {
        if (origin.X + size.X > IntroCinematicRomData.Layers.TilemapWidth ||
            origin.Y + size.Y > IntroCinematicRomData.Layers.TilemapWidth)
            throw new InvalidDataException(
                $"Cinematic rectangle ({origin.X},{origin.Y}) {size.X}x{size.Y} leaves its 32x32 tilemap.");
    }
}
