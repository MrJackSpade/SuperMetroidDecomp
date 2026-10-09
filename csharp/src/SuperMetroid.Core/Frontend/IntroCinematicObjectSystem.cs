using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

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
    /// <summary>ROM address space used to read the compiled cinematic instruction streams.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Optional audio queue used for alternating typewriter key sounds.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>VRAM target for portrait rectangles drawn by the eye animation.</summary>
    private readonly SnesVram vram;
    /// <summary>Staging tilemap updated by narration and uploaded during each object step.</summary>
    private readonly ushort[] textTilemap;
    /// <summary>Current bank-$8B eye animation-list pointer.</summary>
    private ushort eyeInstructionPointer = CinematicCodePointers.BackgroundLists.SamusBlinking;
    /// <summary>Countdown until the eye instruction list consumes its next record.</summary>
    private ushort eyeInstructionTimer = 1;
    /// <summary>Current bank-$8C text-background instruction pointer, when legacy text is active.</summary>
    private ushort textInstructionPointer;
    /// <summary>Countdown governing either a text instruction or the next narration character.</summary>
    private ushort textInstructionTimer;
    /// <summary>Host-provided localized narration records, rebound after state restoration.</summary>
    [NonSerialized] private IntroNarrationPresentation? narrationPresentation;
    /// <summary>Host-provided eye frame pixels used to render compiled eye-list selections.</summary>
    [NonSerialized] private IntroEyeTilemapPresentation? eyeArtwork;
    /// <summary>Current eye frame data pointer retained so artwork can be rebound and redrawn.</summary>
    private ushort currentEyeFramePointer;
    /// <summary>Packed destination position associated with the retained eye frame.</summary>
    private ushort currentEyePackedPosition;
    /// <summary>Compiled semantic narration for the active English page.</summary>
    [NonSerialized] private IntroNarrationCharacter[]? narrationProgram;
    /// <summary>Page identity needed to resume or finish the active narration program.</summary>
    private IntroNarrationPageId? narrationPage;
    /// <summary>Index of the next semantic character to reveal.</summary>
    private int narrationCharacterIndex;
    /// <summary>Whether the page's authored initial marker delay remains to be consumed.</summary>
    private bool narrationInitialMarkerPending;
    /// <summary>Whether page six has entered its final caret-visible hold.</summary>
    private bool narrationFinalHoldStarted;
    /// <summary>Current bank-$8B caret sprite-list pointer.</summary>
    private ushort spriteInstructionPointer =
        CinematicCodePointers.Lists.IntroTextCaret;
    /// <summary>Countdown until the caret sprite list advances.</summary>
    private ushort spriteInstructionTimer = 1;
    /// <summary>Current bank-$8B sprite-map pointer selected for the caret object.</summary>
    private ushort spriteMapPointer;
    /// <summary>Persistent caret object's native screen X coordinate.</summary>
    private ushort caretX = IntroCinematicRomData.ObjectSystem.CaretLeftX;
    /// <summary>Persistent caret object's native screen Y coordinate.</summary>
    private ushort caretY = IntroCinematicRomData.ObjectSystem.CaretFirstTextY;
    /// <summary>Alternates typewriter audio admission between eligible characters.</summary>
    private bool typewriterSoundToggle;
    // Nullable for older debugger snapshots that predate text-glow state.
    /// <summary>Optional per-character glow state, absent in older restored snapshots.</summary>
    private CinematicTextGlowSystem? textGlow;

    /// <summary>Creates the intro object interpreter with host resources for its two instruction streams.</summary>
    /// <param name="bus">Address space containing the compiled bank-$8B and bank-$8C words.</param>
    /// <param name="vram">Video memory receiving the interpreted eye-frame rectangles.</param>
    /// <param name="textTilemap">Exactly $400 words of staging storage for the cinematic text tilemap.</param>
    /// <param name="audio">Optional sound queue for typewriter effects.</param>
    /// <param name="narrationPresentation">Localized narration source used to compile semantic page records.</param>
    /// <param name="eyeArtwork">Eye-animation frames copied into the portrait tilemap.</param>
    public IntroCinematicObjectSystem(
        ISnesAddressSpace bus,
        SnesVram vram,
        ushort[] textTilemap,
        CartridgeAudioState? audio = null,
        IntroNarrationPresentation? narrationPresentation = null,
        IntroEyeTilemapPresentation? eyeArtwork = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.vram = vram ?? throw new ArgumentNullException(nameof(vram));
        this.textTilemap = textTilemap ?? throw new ArgumentNullException(nameof(textTilemap));
        this.audio = audio;
        this.narrationPresentation = narrationPresentation;
        this.eyeArtwork = eyeArtwork;
        if (textTilemap.Length != IntroCinematicRomData.Layers.TextTilemapWordCount)
            throw new ArgumentException("The cinematic tilemap staging buffer must contain $400 words.", nameof(textTilemap));
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
    public ushort SpriteMapPointer => spriteMapPointer;

    /// <summary>Current native X coordinate of the persistent text caret object.</summary>
    public ushort CaretX => caretX;

    /// <summary>Current native Y coordinate; $F8 deliberately hides it below the viewport.</summary>
    public ushort CaretY => caretY;

    /// <summary>Reapply the active eye rectangle after restoring a state or changing art.</summary>
    public void BindEyeArtwork(IntroEyeTilemapPresentation? value)
    {
        eyeArtwork = value;
        if (value is not null &&
            IntroEyeAnimationDefinitions.TryFrameIndex(currentEyeFramePointer, out int index))
            CopyRectangleToPortrait(
                currentEyePackedPosition & IntroCinematicRomData.ObjectSystem.PackedPositionXMask,
                currentEyePackedPosition >> 8,
                IntroEyeAnimationDefinitions.FrameColumns,
                IntroEyeAnimationDefinitions.FrameRows,
                value.FrameWords(index));
    }

    /// <summary>
    /// Implements <c>PlaceIntroTextCaretOffScreen</c> at $8B:ADE1. The retail game keeps
    /// the same object slot alive between narration pages and visibly moves it to Y=$F8
    /// while a gameplay or scientist illustration owns the screen.
    /// </summary>
    public void PlaceCaretOffScreen()
    {
        caretX = IntroCinematicRomData.ObjectSystem.CaretLeftX;
        caretY = IntroCinematicRomData.ObjectSystem.CaretInitialY;
    }

    /// <summary>
    /// Starts the letter-by-letter narration stream from definition $8B:CF3F. The eye-blink
    /// stream from $8B:CF63 and the portrait-border sprite are already active at page setup.
    /// </summary>
    public void StartEnglishPageOne()
    {
        StartNarration(
            IntroNarrationPageId.Page1,
            CinematicCodePointers.BackgroundLists.IntroTextPage1);
    }

    /// <summary>
    /// Spawns definition $8B:CF45 and restores the existing caret object as $8B:B35F/B3F0 do.
    /// </summary>
    public void StartEnglishPageTwo()
    {
        StartNarration(
            IntroNarrationPageId.Page2,
            CinematicCodePointers.BackgroundLists.IntroTextPage2);
        PageTwoAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>
    /// Spawns definition $8B:CF4B after the egg actor requests page three at $8B:B33E.
    /// </summary>
    public void StartEnglishPageThree()
    {
        StartNarration(
            IntroNarrationPageId.Page3,
            CinematicCodePointers.BackgroundLists.IntroTextPage3);
        PageThreeAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>Spawns the page-four text definition at $8B:CF51 / $8C:CE33.</summary>
    public void StartEnglishPageFour()
    {
        StartNarration(
            IntroNarrationPageId.Page4,
            CinematicCodePointers.BackgroundLists.IntroTextPage4);
        PageFourAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>Spawns the page-five text definition at $8B:CF57 / $8C:D15D.</summary>
    public void StartEnglishPageFive()
    {
        StartNarration(
            IntroNarrationPageId.Page5,
            CinematicCodePointers.BackgroundLists.IntroTextPage5);
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
            IntroNarrationPageId.Page6,
            CinematicCodePointers.BackgroundLists.IntroTextPage6);
        IntroFinishRequested = false;
        ResetCaret();

        // B4BC observes the page-six cinematic function and permanently changes the eye
        // object to its closed/half-open/deadpan sequence at $8C:D613.
        eyeInstructionPointer = CinematicCodePointers.BackgroundLists.SamusBlinkingPage6;
        eyeInstructionTimer = 1;
    }

    /// <summary>Runs the same post-state-function object order as game state $25.</summary>
    public void Step()
    {
        if (narrationPage is not null && narrationProgram is null)
        {
            throw new InvalidOperationException(
                "Opening narration host content must be rebound after state restoration.");
        }
        StepSpriteObject();
        StepBgObject(ref eyeInstructionPointer, ref eyeInstructionTimer);
        if (narrationProgram is not null)
            StepInstalledNarration();
        else if (textInstructionPointer != 0)
            StepBgObject(ref textInstructionPointer, ref textInstructionTimer);
        textGlow?.Step(textTilemap);

        // UpdateCinematicBgTilemap queues $780 bytes from $7E:3000 to VMADD $4C00.
        // Applying it immediately is the desktop equivalent of observing the following NMI.
        vram.ExecuteWordTransfer(
            textTilemap.AsSpan(0, IntroCinematicRomData.Layers.VisibleTextTransferWordCount),
            IntroCinematicRomData.Layers.NarrationTilemapWord,
            1);
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

    /// <summary>Initializes page-local timing and binds its compiled narration records.</summary>
    private void StartNarration(IntroNarrationPageId page, ushort nativePointer)
    {
        textInstructionTimer = IntroNarrationDefinitions.InitialMarkerDelayFrames;
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

    /// <summary>Consumes one narration timing tick, reveals due characters, and handles page completion.</summary>
    private void StepInstalledNarration()
    {
        if (textInstructionTimer-- != 1)
            return;
        if (narrationInitialMarkerPending)
        {
            narrationInitialMarkerPending = false;
            textInstructionTimer = IntroNarrationDefinitions.InitialMarkerDelayFrames;
            return;
        }

        IntroNarrationCharacter[] program = narrationProgram!;
        if (narrationCharacterIndex < program.Length)
        {
            IntroNarrationCharacter character = program[narrationCharacterIndex];
            IntroNarrationCharacter? next = narrationCharacterIndex + 1 < program.Length
                ? program[narrationCharacterIndex + 1]
                : null;
            (textGlow ??= new()).Spawn(character.Column, character.Row, 1, 1);
            if (next is { } following)
            {
                caretX = unchecked((ushort)(following.Column *
                    IntroCinematicRomData.ObjectSystem.CharacterPixelSize));
                caretY = unchecked((ushort)(following.Row *
                    IntroCinematicRomData.ObjectSystem.CharacterPixelSize -
                    IntroCinematicRomData.ObjectSystem.CharacterBaselineOffset));
            }
            else
            {
                caretX = IntroCinematicRomData.ObjectSystem.CaretLeftX;
                caretY = unchecked((ushort)((character.Row + 1) *
                    IntroCinematicRomData.ObjectSystem.CharacterPixelSize));
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
            textInstructionTimer = IntroNarrationDefinitions.CharacterDelayFrames;
            return;
        }

        if (narrationPage == IntroNarrationPageId.Page6 && !narrationFinalHoldStarted)
        {
            SetCaretBlinking();
            narrationFinalHoldStarted = true;
            textInstructionTimer = IntroNarrationDefinitions.FinalPageHoldFrames;
            return;
        }

        FinishInstalledNarration(narrationPage ?? throw new InvalidDataException(
            "Installed narration completed without an active page identity."));
        narrationProgram = null;
        narrationPage = null;
    }

    /// <summary>Publishes the page's input-wait or intro-finish signal after its final hold.</summary>
    private void FinishInstalledNarration(IntroNarrationPageId page)
    {
        switch (page)
        {
            case IntroNarrationPageId.Page1:
                PageOneAwaitingInput = true;
                SetCaretBlinking();
                break;
            case IntroNarrationPageId.Page2:
                PageTwoAwaitingInput = true;
                SetCaretBlinking();
                break;
            case IntroNarrationPageId.Page3:
                PageThreeAwaitingInput = true;
                SetCaretBlinking();
                break;
            case IntroNarrationPageId.Page4:
                PageFourAwaitingInput = true;
                SetCaretBlinking();
                break;
            case IntroNarrationPageId.Page5:
                PageFiveAwaitingInput = true;
                SetCaretBlinking();
                break;
            case IntroNarrationPageId.Page6:
                IntroFinishRequested = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(page), page, null);
        }
    }

    /// <summary>Advances the persistent caret sprite list, including its goto, delete, and sleep controls.</summary>
    private void StepSpriteObject()
    {
        if (spriteInstructionPointer == 0 || spriteInstructionTimer-- != 1)
            return;

        ushort pointer = spriteInstructionPointer;
        while (true)
        {
            ushort instructionOrDuration = ReadBank8B(pointer);
            if ((instructionOrDuration & CinematicCodePointers.InstructionCommandBit) == 0)
            {
                spriteInstructionTimer = instructionOrDuration;
                spriteMapPointer = ReadBank8B(Add(
                    pointer,
                    IntroCinematicRomData.ObjectSystem.RecordDurationToPositionByteCount));
                spriteInstructionPointer = Add(
                    pointer,
                    IntroCinematicRomData.ObjectSystem.RecordDurationToDataPointerByteCount);
                return;
            }

            // The opening border animation uses only the common goto. Delete and sleep are
            // retained because they are fundamental interpreter control flow, not page lore.
            switch (instructionOrDuration)
            {
                case CinematicCodePointers.CinematicSpriteObject_Instruction_Goto:
                    pointer = ReadBank8B(Add(
                        pointer,
                        IntroCinematicRomData.ObjectSystem.RecordDurationToPositionByteCount));
                    break;
                case CinematicCodePointers.CinematicSpriteObject_Instruction_Delete:
                    spriteMapPointer = 0;
                    spriteInstructionPointer = 0;
                    return;
                case CinematicCodePointers.CinematicSpriteObject_Instruction_Sleep:
                    spriteInstructionPointer = pointer;
                    return;
                default:
                    throw Unsupported("sprite", instructionOrDuration, pointer);
            }
        }
    }

    /// <summary>Advances one bank-$8C background list and dispatches its compiled page-control opcodes.</summary>
    private void StepBgObject(ref ushort instructionPointer, ref ushort instructionTimer)
    {
        if (instructionPointer == 0 || instructionTimer-- != 1)
            return;

        ushort pointer = instructionPointer;
        while (true)
        {
            ushort instructionOrDuration = ReadBank8C(pointer);
            if ((instructionOrDuration & CinematicCodePointers.InstructionCommandBit) == 0)
            {
                instructionTimer = instructionOrDuration;
                ushort tilePosition = ReadBank8C(Add(
                    pointer,
                    IntroCinematicRomData.ObjectSystem.RecordDurationToPositionByteCount));
                ushort dataPointer = ReadBank8C(Add(
                    pointer,
                    IntroCinematicRomData.ObjectSystem.RecordDurationToDataPointerByteCount));
                ProcessTileData(pointer, tilePosition, dataPointer);
                instructionPointer = Add(
                    pointer,
                    IntroCinematicRomData.ObjectSystem.BackgroundRecordByteCount);
                return;
            }

            switch (instructionOrDuration)
            {
                case CinematicCodePointers.CinematicBackgroundObject_Instruction_Goto:
                    pointer = ReadBank8C(Add(
                        pointer,
                        IntroCinematicRomData.ObjectSystem.RecordDurationToPositionByteCount));
                    break;
                case CinematicCodePointers.CinematicBackgroundObject_Instruction_Delete:
                    instructionPointer = 0;
                    return;
                case CinematicCodePointers.Instruction_BeginEnglishPage1:
                    // English skips the Japanese Mode-7 glyph object spawned by this opcode.
                    pointer = Add(pointer, 2);
                    break;
                case CinematicCodePointers.Instruction_FinishEnglishPage1:
                    // $8B:AE5B switches the cinematic function to its input-wait routine.
                    // The page marker sprite is Japanese-only, but the state change is not.
                    PageOneAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case CinematicCodePointers.Instruction_BeginEnglishPage2:
                    // $AE79 differs only in the Japanese subtitle object it conditionally
                    // spawns. The default English route consumes no operands.
                    pointer = Add(pointer, 2);
                    break;
                case CinematicCodePointers.Instruction_FinishEnglishPage2:
                    // $AE91 selects the baby-Metroid-discovery input wait and makes the
                    // same existing caret object blink; page-three setup is the next slice.
                    PageTwoAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case CinematicCodePointers.Instruction_BeginEnglishPage3:
                    // $B074 clears the Japanese click flag and conditionally starts a Mode
                    // 7 subtitle object. English has no extra actor or operands here.
                    pointer = Add(pointer, 2);
                    break;
                case CinematicCodePointers.Instruction_FinishEnglishPage3:
                    // $B08C selects the page-three input wait that proceeds to the Ceres
                    // delivery scene; the optional subtitle/arrow branch is Japanese-only.
                    PageThreeAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case CinematicCodePointers.Instruction_BeginEnglishPage4:
                    // English skips the optional page-four Japanese Mode-7 subtitle actor.
                    pointer = Add(pointer, 2);
                    break;
                case CinematicCodePointers.Instruction_FinishEnglishPage4:
                    PageFourAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case CinematicCodePointers.Instruction_BeginEnglishPage5:
                    pointer = Add(pointer, 2);
                    break;
                case CinematicCodePointers.Instruction_FinishEnglishPage5:
                    PageFiveAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case CinematicCodePointers.Instruction_BeginEnglishPage6:
                    pointer = Add(pointer, 2);
                    break;
                case CinematicCodePointers.Instruction_SetCaretToBlink:
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case CinematicCodePointers.Instruction_FinishIntro:
                    IntroFinishRequested = true;
                    pointer = Add(pointer, 2);
                    break;
                default:
                    throw Unsupported("background", instructionOrDuration, pointer);
            }
        }
    }

    /// <summary>Resolves a compiled eye frame and copies its rectangle to the portrait tilemap.</summary>
    private void ProcessTileData(
        ushort instructionRecordPointer,
        ushort packedPosition,
        ushort dataPointer)
    {
        if (IntroEyeAnimationDefinitions.TryFrameIndex(dataPointer, out int eyeFrame))
        {
            currentEyeFramePointer = dataPointer;
            currentEyePackedPosition = packedPosition;
            IntroEyeTilemapPresentation content = eyeArtwork ?? throw new InvalidOperationException(
                "Intro eye animation requires installed eye artwork.");
            int x = packedPosition & IntroCinematicRomData.ObjectSystem.PackedPositionXMask;
            int y = packedPosition >> 8;
            CopyRectangleToPortrait(x, y,
                IntroEyeAnimationDefinitions.FrameColumns,
                IntroEyeAnimationDefinitions.FrameRows,
                content.FrameWords(eyeFrame));
            return;
        }
        // Installed narration executes as semantic glyph records. The only BG-object
        // bytecode still interpreted here is the bounded, compiled eye-animation list.
        throw new InvalidDataException(
            $"Cinematic tile data $8C:{dataPointer:X4} has no installed definition.");
    }

    /// <summary>Copies a validated rectangular frame into consecutive portrait tilemap rows.</summary>
    private void CopyRectangleToPortrait(int destinationX, int destinationY, int width, int height,
        ReadOnlySpan<ushort> source)
    {
        ValidateRectangle(destinationX, destinationY, width, height);
        if (source.Length != width * height)
            throw new InvalidDataException("Opening eye frame does not match its native rectangle size.");
        for (int row = 0; row < height; row++)
        {
            ushort destinationWord = (ushort)(
                IntroCinematicRomData.Layers.PortraitTilemapWord +
                (destinationY + row) * IntroCinematicRomData.Layers.TilemapWidth +
                destinationX);
            vram.ExecuteWordTransfer(source.Slice(row * width, width), destinationWord, 1);
        }
    }

    /// <summary>Restores the persistent caret to the initial text position and non-blinking list.</summary>
    private void ResetCaret()
    {
        // RestIntroTextCaret ($8B:ADEE) moves the persistent slot back from Y=$F8 before
        // restoring its non-blinking list. Position is state, not a renderer constant.
        caretX = IntroCinematicRomData.ObjectSystem.CaretLeftX;
        caretY = IntroCinematicRomData.ObjectSystem.CaretFirstTextY;
        spriteInstructionPointer = CinematicCodePointers.Lists.IntroTextCaret;
        spriteInstructionTimer = 1;
    }

    /// <summary>Selects the existing caret's blink list and primes its first animation step.</summary>
    private void SetCaretBlinking()
    {
        // Instruction_SetCaretToBlink points the existing slot at $CC03 and primes timer
        // one, allowing the generic sprite list handler to select the first frame now.
        spriteInstructionPointer = CinematicCodePointers.Lists.IntroTextCaretBlink;
        spriteInstructionTimer = 1;
    }

    /// <summary>Rejects a tile rectangle that would extend beyond the 32-by-32 cinematic map.</summary>
    private static void ValidateRectangle(int x, int y, int width, int height)
    {
        if (x + width > IntroCinematicRomData.Layers.TilemapWidth ||
            y + height > IntroCinematicRomData.Layers.TilemapWidth)
            throw new InvalidDataException($"Cinematic rectangle ({x},{y}) {width}x{height} leaves its 32x32 tilemap.");
    }

    /// <summary>Reads a compiled word from the bank-$8B sprite instruction stream.</summary>
    private static ushort ReadBank8B(ushort pointer) =>
        IntroCaretInstructionDefinitions.TryReadWord(pointer, out ushort word)
            ? word
            : throw new InvalidDataException(
                $"Cinematic caret instruction $8B:{pointer:X4} is not compiled.");

    /// <summary>Reads a compiled word from the bank-$8C background instruction stream.</summary>
    private static ushort ReadBank8C(ushort pointer) =>
        IntroEyeAnimationDefinitions.TryReadWord(pointer, out ushort word)
            ? word
            : throw new InvalidDataException(
                $"Cinematic eye instruction $8C:{pointer:X4} is not compiled.");

    /// <summary>Advances a bank-local instruction pointer with native 16-bit wrapping.</summary>
    private static ushort Add(ushort pointer, int byteCount) => unchecked((ushort)(pointer + byteCount));

    /// <summary>Builds the format error reported when an instruction is not valid for its active stream.</summary>
    private static InvalidDataException Unsupported(string kind, ushort opcode, ushort pointer) =>
        new($"Cinematic {kind} opcode $8B:{opcode:X4}, read from ${pointer:X4}, is invalid for the active retail stream.");
}
