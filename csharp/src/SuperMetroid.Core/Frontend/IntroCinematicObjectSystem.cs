using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

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
    private const ushort SpriteGoto = 0x94bc;
    private const ushort SpriteDelete = 0x9438;
    private const ushort SpriteSleep = 0x9442;
    private const ushort BgDelete = 0x9698;
    private const ushort BgGoto = 0x971e;
    private const ushort BeginEnglishPageOne = 0xae43;
    private const ushort FinishEnglishPageOne = 0xae5b;
    private const ushort BeginEnglishPageTwo = 0xae79;
    private const ushort FinishEnglishPageTwo = 0xae91;
    private const ushort BeginEnglishPageThree = 0xb074;
    private const ushort FinishEnglishPageThree = 0xb08c;
    private const ushort BeginEnglishPageFour = 0xb0b3;
    private const ushort FinishEnglishPageFour = 0xb0cb;
    private const ushort BeginEnglishPageFive = 0xb19b;
    private const ushort FinishEnglishPageFive = 0xb1b3;
    private const ushort BeginEnglishPageSix = 0xb228;
    private const ushort FinishIntro = 0xb240;
    private const ushort SetCaretBlinkingInstruction = 0xadd4;

    private const ushort DrawNothing = 0x8849;
    private const ushort DrawCharacter = 0x884d;
    private const ushort DrawToTextTilemap = 0x88b7;
    private const ushort DrawToPortraitTilemap = 0x88fd;

    private readonly ISnesAddressSpace bus;
    private readonly SnesVram vram;
    private readonly ushort[] textTilemap;
    private ushort eyeInstructionPointer = 0xd5df;
    private ushort eyeInstructionTimer = 1;
    private ushort textInstructionPointer;
    private ushort textInstructionTimer;
    private ushort spriteInstructionPointer = 0xcbfb;
    private ushort spriteInstructionTimer = 1;
    private ushort spriteMapPointer;
    private ushort caretX = 8;
    private ushort caretY = 24;

    public IntroCinematicObjectSystem(
        ISnesAddressSpace bus,
        SnesVram vram,
        ushort[] textTilemap)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.vram = vram ?? throw new ArgumentNullException(nameof(vram));
        this.textTilemap = textTilemap ?? throw new ArgumentNullException(nameof(textTilemap));
        if (textTilemap.Length != 0x400)
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

    /// <summary>
    /// Implements <c>PlaceIntroTextCaretOffScreen</c> at $8B:ADE1. The retail game keeps
    /// the same object slot alive between narration pages and visibly moves it to Y=$F8
    /// while a gameplay or scientist illustration owns the screen.
    /// </summary>
    public void PlaceCaretOffScreen()
    {
        caretX = 8;
        caretY = 0x00f8;
    }

    /// <summary>
    /// Starts the letter-by-letter narration stream from definition $8B:CF3F. The eye-blink
    /// stream from $8B:CF63 and the portrait-border sprite are already active at page setup.
    /// </summary>
    public void StartEnglishPageOne()
    {
        textInstructionPointer = 0xc383;
        textInstructionTimer = 1;
    }

    /// <summary>
    /// Spawns definition $8B:CF45 and restores the existing caret object as $8B:B35F/B3F0 do.
    /// </summary>
    public void StartEnglishPageTwo()
    {
        textInstructionPointer = 0xc797;
        textInstructionTimer = 1;
        PageTwoAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>
    /// Spawns definition $8B:CF4B after the egg actor requests page three at $8B:B33E.
    /// </summary>
    public void StartEnglishPageThree()
    {
        textInstructionPointer = 0xcb45;
        textInstructionTimer = 1;
        PageThreeAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>Spawns the page-four text definition at $8B:CF51 / $8C:CE33.</summary>
    public void StartEnglishPageFour()
    {
        textInstructionPointer = 0xce33;
        textInstructionTimer = 1;
        PageFourAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>Spawns the page-five text definition at $8B:CF57 / $8C:D15D.</summary>
    public void StartEnglishPageFive()
    {
        textInstructionPointer = 0xd15d;
        textInstructionTimer = 1;
        PageFiveAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>Clears pages one-through-five and starts final text definition $8C:D511.</summary>
    public void StartEnglishPageSix()
    {
        Array.Fill(textTilemap, (ushort)0x002f, startIndex: 128, count: 640);
        textInstructionPointer = 0xd511;
        textInstructionTimer = 1;
        IntroFinishRequested = false;
        ResetCaret();

        // B4BC observes the page-six cinematic function and permanently changes the eye
        // object to its closed/half-open/deadpan sequence at $8C:D613.
        eyeInstructionPointer = 0xd613;
        eyeInstructionTimer = 1;
    }

    /// <summary>Runs the same post-state-function object order as game state $25.</summary>
    public void Step()
    {
        StepSpriteObject();
        StepBgObject(ref eyeInstructionPointer, ref eyeInstructionTimer);
        if (textInstructionPointer != 0)
            StepBgObject(ref textInstructionPointer, ref textInstructionTimer);

        // UpdateCinematicBgTilemap queues $780 bytes from $7E:3000 to VMADD $4C00.
        // Applying it immediately is the desktop equivalent of observing the following NMI.
        vram.ExecuteWordTransfer(textTilemap.AsSpan(0, 0x3c0), 0x4c00, 1);
    }

    private void StepSpriteObject()
    {
        if (spriteInstructionPointer == 0 || spriteInstructionTimer-- != 1)
            return;

        ushort pointer = spriteInstructionPointer;
        while (true)
        {
            ushort instructionOrDuration = ReadBank8B(pointer);
            if ((instructionOrDuration & 0x8000) == 0)
            {
                spriteInstructionTimer = instructionOrDuration;
                spriteMapPointer = ReadBank8B(Add(pointer, 2));
                spriteInstructionPointer = Add(pointer, 4);
                return;
            }

            // The opening border animation uses only the common goto. Delete and sleep are
            // retained because they are fundamental interpreter control flow, not page lore.
            switch (instructionOrDuration)
            {
                case SpriteGoto:
                    pointer = ReadBank8B(Add(pointer, 2));
                    break;
                case SpriteDelete:
                    spriteMapPointer = 0;
                    spriteInstructionPointer = 0;
                    return;
                case SpriteSleep:
                    spriteInstructionPointer = pointer;
                    return;
                default:
                    throw Unsupported("sprite", instructionOrDuration, pointer);
            }
        }
    }

    private void StepBgObject(ref ushort instructionPointer, ref ushort instructionTimer)
    {
        if (instructionPointer == 0 || instructionTimer-- != 1)
            return;

        ushort pointer = instructionPointer;
        while (true)
        {
            ushort instructionOrDuration = ReadBank8C(pointer);
            if ((instructionOrDuration & 0x8000) == 0)
            {
                instructionTimer = instructionOrDuration;
                ushort tilePosition = ReadBank8C(Add(pointer, 2));
                ushort dataPointer = ReadBank8C(Add(pointer, 4));
                ProcessTileData(pointer, tilePosition, dataPointer);
                instructionPointer = Add(pointer, 6);
                return;
            }

            switch (instructionOrDuration)
            {
                case BgGoto:
                    pointer = ReadBank8C(Add(pointer, 2));
                    break;
                case BgDelete:
                    instructionPointer = 0;
                    return;
                case BeginEnglishPageOne:
                    // English skips the Japanese Mode-7 glyph object spawned by this opcode.
                    pointer = Add(pointer, 2);
                    break;
                case FinishEnglishPageOne:
                    // $8B:AE5B switches the cinematic function to its input-wait routine.
                    // The page marker sprite is Japanese-only, but the state change is not.
                    PageOneAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case BeginEnglishPageTwo:
                    // $AE79 differs only in the Japanese subtitle object it conditionally
                    // spawns. The default English route consumes no operands.
                    pointer = Add(pointer, 2);
                    break;
                case FinishEnglishPageTwo:
                    // $AE91 selects the baby-Metroid-discovery input wait and makes the
                    // same existing caret object blink; page-three setup is the next slice.
                    PageTwoAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case BeginEnglishPageThree:
                    // $B074 clears the Japanese click flag and conditionally starts a Mode
                    // 7 subtitle object. English has no extra actor or operands here.
                    pointer = Add(pointer, 2);
                    break;
                case FinishEnglishPageThree:
                    // $B08C selects the page-three input wait that proceeds to the Ceres
                    // delivery scene; the optional subtitle/arrow branch is Japanese-only.
                    PageThreeAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case BeginEnglishPageFour:
                    // English skips the optional page-four Japanese Mode-7 subtitle actor.
                    pointer = Add(pointer, 2);
                    break;
                case FinishEnglishPageFour:
                    PageFourAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case BeginEnglishPageFive:
                    pointer = Add(pointer, 2);
                    break;
                case FinishEnglishPageFive:
                    PageFiveAwaitingInput = true;
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case BeginEnglishPageSix:
                    pointer = Add(pointer, 2);
                    break;
                case SetCaretBlinkingInstruction:
                    SetCaretBlinking();
                    pointer = Add(pointer, 2);
                    break;
                case FinishIntro:
                    IntroFinishRequested = true;
                    pointer = Add(pointer, 2);
                    break;
                default:
                    throw Unsupported("background", instructionOrDuration, pointer);
            }
        }
    }

    private void ProcessTileData(
        ushort instructionRecordPointer,
        ushort packedPosition,
        ushort dataPointer)
    {
        ushort drawFunction = ReadBank8C(dataPointer);
        if (drawFunction == DrawNothing)
            return;

        byte width = bus.ReadByte(0x8c0000 | Add(dataPointer, 2));
        byte height = bus.ReadByte(0x8c0000 | Add(dataPointer, 3));
        if (width == 0 || height == 0)
            throw new InvalidDataException($"Cinematic tile data $8C:{dataPointer:X4} has a zero-sized rectangle.");

        int destinationX = packedPosition & 0xff;
        int destinationY = packedPosition >> 8;
        ushort source = Add(dataPointer, 4);
        switch (drawFunction)
        {
            case DrawCharacter:
                // `$8B:884D-$8B:889F` does more than copy the glyph. It looks ahead from
                // the *current six-byte BG-object record* to the following record and moves
                // cinematic sprite slot $1E to that next character cell. If the following
                // word is an opcode instead of a duration, the caret wraps to column one of
                // the next text row. This is why the object's own pre-instruction can be an
                // RTS while the visible typewriter block still walks across every line.
                UpdateCaretAfterCharacter(instructionRecordPointer, packedPosition);
                CopyRectangleToText(destinationX, destinationY, width, height, source);
                return;
            case DrawToTextTilemap:
                CopyRectangleToText(destinationX, destinationY, width, height, source);
                return;
            case DrawToPortraitTilemap:
                CopyRectangleToPortrait(destinationX, destinationY, width, height, source);
                return;
            default:
                // Every active bank-$8C rectangle names one of the three indirect drawing
                // functions above. An arbitrary function word would make its payload shape
                // unknowable and is malformed cartridge data for this object class.
                throw new InvalidDataException(
                    $"Cinematic tile-data function $8B:{drawFunction:X4} at $8C:{dataPointer:X4} is invalid.");
        }
    }

    /// <summary>
    /// Translates the caret side effect embedded in the native draw-character indirect
    /// instruction at <c>$8B:884D</c>.
    /// </summary>
    private void UpdateCaretAfterCharacter(ushort instructionRecordPointer, ushort packedPosition)
    {
        // A normal record is [duration:2, packed X/Y:2, indirect-data pointer:2]. The next
        // duration therefore begins six bytes after this record; its packed coordinates
        // begin eight bytes after this record. ROM words with bit 15 set are interpreter
        // opcodes, so there is no following cell to read in that branch.
        ushort nextDurationOrOpcode = ReadBank8C(Add(instructionRecordPointer, 6));
        if ((nextDurationOrOpcode & 0x8000) == 0)
        {
            caretX = unchecked((ushort)(
                bus.ReadByte(0x8c0000 | Add(instructionRecordPointer, 8)) * 8));
            caretY = unchecked((ushort)(
                bus.ReadByte(0x8c0000 | Add(instructionRecordPointer, 9)) * 8 - 8));
            return;
        }

        // At an instruction boundary `$8B:888A-$889F` retains the native left margin and
        // derives the following line from the just-drawn record's Y byte: (Y + 2)*8 - 8.
        // packedPosition is already the little-endian form of that exact X/Y operand.
        caretX = 8;
        int currentTileY = packedPosition >> 8;
        caretY = unchecked((ushort)((currentTileY + 1) * 8));
    }

    private void CopyRectangleToText(int destinationX, int destinationY, int width, int height, ushort source)
    {
        ValidateRectangle(destinationX, destinationY, width, height);
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                textTilemap[(destinationY + row) * 32 + destinationX + column] = ReadBank8C(source);
                source = Add(source, 2);
            }
        }
    }

    private void CopyRectangleToPortrait(int destinationX, int destinationY, int width, int height, ushort source)
    {
        ValidateRectangle(destinationX, destinationY, width, height);
        for (int row = 0; row < height; row++)
        {
            var words = new ushort[width];
            for (int column = 0; column < width; column++)
            {
                words[column] = ReadBank8C(source);
                source = Add(source, 2);
            }

            // BG2SC=$48 selects word $4800. Updating only the script-authored rectangle is
            // equivalent for visible pixels and avoids inventing values for unrelated
            // $7E:3800 staging words that this focused state does not yet own.
            ushort destinationWord = (ushort)(0x4800 + (destinationY + row) * 32 + destinationX);
            vram.ExecuteWordTransfer(words, destinationWord, 1);
        }
    }

    private void ResetCaret()
    {
        // RestIntroTextCaret ($8B:ADEE) moves the persistent slot back from Y=$F8 before
        // restoring its non-blinking list. Position is state, not a renderer constant.
        caretX = 8;
        caretY = 24;
        spriteInstructionPointer = 0xcbfb;
        spriteInstructionTimer = 1;
    }

    private void SetCaretBlinking()
    {
        // Instruction_SetCaretToBlink points the existing slot at $CC03 and primes timer
        // one, allowing the generic sprite list handler to select the first frame now.
        spriteInstructionPointer = 0xcc03;
        spriteInstructionTimer = 1;
    }

    private static void ValidateRectangle(int x, int y, int width, int height)
    {
        if (x + width > 32 || y + height > 32)
            throw new InvalidDataException($"Cinematic rectangle ({x},{y}) {width}x{height} leaves its 32x32 tilemap.");
    }

    private ushort ReadBank8B(ushort pointer) => RomDataReader.ReadWordFixedBank(bus, 0x8b0000 | pointer);

    private ushort ReadBank8C(ushort pointer) => RomDataReader.ReadWordFixedBank(bus, 0x8c0000 | pointer);

    private static ushort Add(ushort pointer, int byteCount) => unchecked((ushort)(pointer + byteCount));

    private static InvalidDataException Unsupported(string kind, ushort opcode, ushort pointer) =>
        new($"Cinematic {kind} opcode $8B:{opcode:X4}, read from ${pointer:X4}, is invalid for the active retail stream.");
}
