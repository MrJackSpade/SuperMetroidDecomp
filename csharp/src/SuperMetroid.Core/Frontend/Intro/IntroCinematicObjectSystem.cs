using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The two small ROM-script interpreters used by the first illustrated intro page.
/// </summary>
/// <remarks>
/// Bank $8B owns the object definitions and sprite instruction lists, while cinematic BG
/// lists and their rectangular tile data live in bank $8C. That split is easy to miss: the
/// native BG handler deliberately calls <c>RomPtr_8C</c> even though its opcodes dispatch
/// back to bank-$8B functions. Keeping the banks explicit here prevents a plausible-looking
/// but completely unrelated stream from being interpreted.
/// </remarks>
internal sealed class IntroCinematicObjectSystem
{
    /// <summary>Supplies compiled cinematic instruction words through the active address space.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Queues alternating non-space typewriter sounds when audio is present.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>Receives eye-frame rectangles in the portrait tilemap.</summary>
    private readonly SnesVram vram;
    /// <summary>Receives narration glyph words at their authored row and column.</summary>
    private readonly ushort[] textTilemap;
    /// <summary>Bank-$8C cursor for the persistent blinking-eye background object.</summary>
    internal ushort eyeInstructionPointer = CinematicCodePointers.BackgroundLists.SamusBlinking;
    /// <summary>Countdown before the eye object's next instruction record.</summary>
    internal NativeCountdown eyeInstructionTimer = NativeCountdown.Start(1);
    /// <summary>Installed narration cursor; semantic glyph records replace the old BG bytecode.</summary>
    private ushort textInstructionPointer;
    /// <summary>Countdown for initial marker, glyph, or final page hold.</summary>
    private NativeCountdown textInstructionTimer;
    /// <summary>Caller-installed narration artwork, rebound after debugger restoration.</summary>
    [NonSerialized] private IntroNarrationPresentation? narrationPresentation;
    /// <summary>Caller-installed eye rectangles, rebound after debugger restoration.</summary>
    [NonSerialized] private IntroEyeTilemapPresentation? eyeArtwork;
    /// <summary>Currently displayed eye-frame identity for artwork rebinding.</summary>
    private ushort currentEyeFramePointer;
    /// <summary>Packed eye rectangle origin retained for artwork rebinding.</summary>
    private ushort currentEyePackedPosition;
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

    /// <summary>Binds eye, caret, narration, VRAM, subtitle, and optional audio owners.</summary>
    public IntroCinematicObjectSystem(
        ISnesAddressSpace bus,
        SnesVram vram,
        ushort[] textTilemap,
        IntroJapaneseSubtitles subtitles,
        CartridgeAudioState? audio = null,
        IntroNarrationPresentation? narrationPresentation = null,
        IntroEyeTilemapPresentation? eyeArtwork = null)
    {
        this.bus = Ensure.NotNull(bus);
        this.subtitles = Ensure.NotNull(subtitles);
        this.vram = Ensure.NotNull(vram);
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

    /// <summary>The bank-$8C spritemap selected by the current sprite timing record.</summary>
    public ushort SpriteMapPointer => caretSprite.SpriteMapPointer;

    /// <summary>Current native screen position of the persistent text caret object; Y=$F8 deliberately hides it below the viewport.</summary>
    public Position16<ScreenPixels> Caret { get; private set; } =
        new(IntroCinematicRomData.ObjectSystem.CaretLeftX, IntroCinematicRomData.ObjectSystem.CaretFirstTextY);

    /// <summary>Reapply the active eye rectangle after restoring a state or changing art.</summary>
    public void BindEyeArtwork(IntroEyeTilemapPresentation? value)
    {
        eyeArtwork = value;
        if (value is not null &&
            IntroEyeAnimationDefinitions.TryFrameIndex(currentEyeFramePointer, out int index))
            CopyRectangleToPortrait(
                UnpackCell(currentEyePackedPosition),
                IntroEyeAnimationDefinitions.FrameSize,
                value.FrameWords(index));
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
        // object to its closed/half-open/deadpan sequence at $8C:D613.
        eyeInstructionPointer = CinematicCodePointers.BackgroundLists.SamusBlinkingPage6;
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
        StepBgObject(ref eyeInstructionPointer, ref eyeInstructionTimer);
        if (narrationProgram is not null)
            StepInstalledNarration();
        else if (textInstructionPointer != 0)
            StepBgObject(ref textInstructionPointer, ref textInstructionTimer);
        textGlow?.Step(textTilemap);

        // UpdateCinematicBgTilemap queues $780 bytes from $7E:3000 to VMADD $4C00.
        // Applying it immediately is the desktop equivalent of observing the following NMI.
        new IntroVram(vram).NarrationTilemap.Write(
            textTilemap.AsSpan(0, IntroCinematicRomData.Layers.VisibleTextTransferWordCount));
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
        textInstructionPointer = 0;
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
                character.Column] = character.TilemapWord;
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

    /// <summary>Interprets compiled bank-$8C eye records and writes their portrait tile rectangles.</summary>
    private void StepBgObject(ref ushort instructionPointer, ref NativeCountdown instructionTimer)
    {
        if (instructionPointer == 0 || !(instructionTimer = instructionTimer.Decrement().Remaining).IsZero)
            return;

        ushort pointer = instructionPointer;
        while (true)
        {
            ushort instructionOrDuration = ReadBank8C(pointer);
            if ((instructionOrDuration & CinematicCodePointers.InstructionCommandBit) == 0)
            {
                instructionTimer = NativeCountdown.FromRaw(instructionOrDuration);
                ushort tilePosition = ReadBank8C(Add(
                    pointer,
                    IntroCinematicRomData.ObjectSystem.RecordDurationToPositionByteCount));
                ushort dataPointer = ReadBank8C(Add(
                    pointer,
                    IntroCinematicRomData.ObjectSystem.RecordDurationToDataPointerByteCount));
                ProcessTileData(tilePosition, dataPointer);
                instructionPointer = Add(
                    pointer,
                    IntroCinematicRomData.ObjectSystem.BackgroundRecordByteCount);
                return;
            }

            switch (CinematicInstructionWords.Decode<CinematicBackgroundInstruction>(instructionOrDuration, pointer))
            {
                case CinematicBackgroundInstruction.Goto:
                    pointer = ReadBank8C(Add(
                        pointer,
                        IntroCinematicRomData.ObjectSystem.RecordDurationToPositionByteCount));
                    break;
                case CinematicBackgroundInstruction.Delete:
                    instructionPointer = 0;
                    return;
                case CinematicBackgroundInstruction.BeginPage1:
                    // English skips the Japanese Mode-7 glyph object spawned by this opcode.
                    pointer = Add(pointer, 2);
                    break;
                case CinematicBackgroundInstruction.FinishPage1:
                    // $8B:AE5B switches the cinematic function to its input-wait routine.
                    // The page marker sprite is Japanese-only, but the state change is not.
                    PageOneAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case CinematicBackgroundInstruction.BeginPage2:
                    // $AE79 differs only in the Japanese subtitle object it conditionally
                    // spawns. The default English route consumes no operands.
                    pointer = Add(pointer, 2);
                    break;
                case CinematicBackgroundInstruction.FinishPage2:
                    // $AE91 selects the baby-Metroid-discovery input wait and makes the
                    // same existing caret object blink; page-three setup is the next slice.
                    PageTwoAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case CinematicBackgroundInstruction.BeginPage3:
                    // $B074 clears the Japanese click flag and conditionally starts a Mode
                    // 7 subtitle object. English has no extra actor or operands here.
                    pointer = Add(pointer, 2);
                    break;
                case CinematicBackgroundInstruction.FinishPage3:
                    // $B08C selects the page-three input wait that proceeds to the Ceres
                    // delivery scene; the optional subtitle/arrow branch is Japanese-only.
                    PageThreeAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case CinematicBackgroundInstruction.BeginPage4:
                    // English skips the optional page-four Japanese Mode-7 subtitle actor.
                    pointer = Add(pointer, 2);
                    break;
                case CinematicBackgroundInstruction.FinishPage4:
                    PageFourAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case CinematicBackgroundInstruction.BeginPage5:
                    pointer = Add(pointer, 2);
                    break;
                case CinematicBackgroundInstruction.FinishPage5:
                    PageFiveAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case CinematicBackgroundInstruction.BeginPage6:
                    pointer = Add(pointer, 2);
                    break;
                case CinematicBackgroundInstruction.SetCaretToBlink:
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case CinematicBackgroundInstruction.FinishIntro:
                    IntroFinishRequested = true;
                    pointer = Add(pointer, 2);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined CinematicBackgroundInstruction {instructionOrDuration}.");
            }
        }
    }

    /// <summary>Resolves an installed eye frame from its data pointer and copies it to the portrait.</summary>
    internal void ProcessTileData(
        ushort packedPosition,
        ushort dataPointer)
    {
        if (IntroEyeAnimationDefinitions.TryFrameIndex(dataPointer, out int eyeFrame))
        {
            currentEyeFramePointer = dataPointer;
            currentEyePackedPosition = packedPosition;
            IntroEyeTilemapPresentation content = eyeArtwork ?? throw new InvalidOperationException(
                "Intro eye animation requires installed eye artwork.");
            CopyRectangleToPortrait(
                UnpackCell(packedPosition),
                IntroEyeAnimationDefinitions.FrameSize,
                content.FrameWords(eyeFrame));
            return;
        }
        // Installed narration executes as semantic glyph records. The only BG-object
        // bytecode still interpreted here is the bounded, compiled eye-animation list.
        throw new InvalidDataException(
            $"Cinematic tile data $8C:{dataPointer:X4} has no installed definition.");
    }

    /// <summary>Unpacks a BG record's low-byte column and high-byte row.</summary>
    private static Position16<TilemapCells> UnpackCell(ushort packedPosition) =>
        new(checked((ushort)(packedPosition & IntroCinematicRomData.ObjectSystem.PackedPositionXMask)), checked((ushort)(SnesLittleEndian.HighByte(packedPosition))));

    /// <summary>Validates and writes each eye tilemap row into the portrait VRAM region.</summary>
    private void CopyRectangleToPortrait(Position16<TilemapCells> destination, Extent16<TilemapCells> size,
        ReadOnlySpan<ushort> source)
    {
        ValidateRectangle(destination, size);
        if (source.Length != size.Area)
            throw new InvalidDataException("Opening eye frame does not match its native rectangle size.");
        VramRegion portrait = new IntroVram(vram).PortraitTilemap;
        for (int row = 0; row < size.Y; row++)
        {
            portrait.Write(source.Slice(row * size.X, size.X),
                (destination.Y + row) * IntroCinematicRomData.Layers.TilemapWidth + destination.X);
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

    /// <summary>Requires a compiled eye instruction word from bank $8C.</summary>
    private static ushort ReadBank8C(ushort pointer) =>
        IntroEyeAnimationDefinitions.TryReadWord(pointer, out ushort word)
            ? word
            : throw new InvalidDataException(
                $"Cinematic eye instruction $8C:{pointer:X4} is not compiled.");

    /// <summary>Advances a bank-relative script cursor with native 16-bit wrap.</summary>
    private static ushort Add(ushort pointer, int byteCount) => unchecked((ushort)(pointer + byteCount));
}
