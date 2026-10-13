using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Small bank-$8B cinematic-sprite interpreter shared by the SR388 egg actors.
/// </summary>
/// <remarks>
/// This is intentionally narrower than a pretend 65c816 VM. Every translated opcode below
/// is part of the retail egg/baby streams. Unknown words fail with their cartridge address,
/// keeping later scene work honest instead of silently treating an opcode as frame data.
/// </remarks>
internal sealed class IntroDiscoverySprite
{
    /// <summary>Countdown before the next sprite instruction is decoded; setup may stagger the first frame.</summary>
    private ushort instructionTimer = 1;

    /// <summary>Creates an egg-scene actor at a whole-pixel origin with its starting OBJ attributes and list cursor.</summary>
    public IntroDiscoverySprite(
        Position16<ScreenPixels> position,
        ushort paletteBits,
        ushort instructionPointer)
    {
        Position = new(new(position.X, 0), new(position.Y, 0));
        PaletteBits = paletteBits;
        InstructionPointer = instructionPointer;
    }

    /// <summary>Cleared when the list's delete opcode retires the actor.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>The native whole and subpixel words of the sprite's position on both axes.</summary>
    public FixedPosition<ScreenPixels> Position { get; set; }

    /// <summary>The whole-pixel words of <see cref="Position"/>; assigning replaces them and keeps both subpixel words.</summary>
    public Position16<ScreenPixels> WholePosition
    {
        get => Position.Whole;
        set => Position = new(Position.X.WithWhole(value.X), Position.Y.WithWhole(value.Y));
    }

    /// <summary>Native OBJ attribute bits applied to the current spritemap's tiles.</summary>
    public ushort PaletteBits { get; private set; }

    /// <summary>Applies a cinematic callback's native OBJ palette/attribute write.</summary>
    public void SetAttributes(SnesObjAttributeWord attributes) => PaletteBits = attributes.Raw;

    /// <summary>Frame pointer most recently published by a timed instruction.</summary>
    public ushort SpriteMapPointer { get; private set; }

    /// <summary>The next list word, matching <c>CinematicSpriteObject_InstListPointers</c>.</summary>
    public ushort InstructionPointer { get; private set; }

    /// <summary>The actor scratch word used by timer and motion opcodes.</summary>
    public ushort GeneralTimer { get; set; }

    /// <summary>The selected native pre-instruction address, when the list changes it.</summary>
    public ushort PreInstructionPointer { get; private set; }

    /// <summary>Starts decoding a new list on the next handler call, discarding the old frame countdown.</summary>
    public void Redirect(ushort pointer)
    {
        InstructionPointer = pointer;
        instructionTimer = 1;
    }

    /// <summary>
    /// Seeds the private instruction countdown used by the next generic sprite-handler
    /// call. Cinematic object setup routines write this word directly when several actors
    /// share one list but must begin on staggered frames.
    /// </summary>
    /// <remarks>
    /// This is deliberately not represented by <see cref="GeneralTimer"/>. The latter is
    /// the separate <c>CinematicSpriteObject_GotoTimers</c> scratch word consumed by list
    /// opcodes $94C3/$94D6; conflating the two makes a delayed actor interpret its first
    /// frame immediately while merely changing an unrelated loop counter.
    /// </remarks>
    public void DelayFirstInstruction(ushort frames)
    {
        if (frames == 0)
            throw new ArgumentOutOfRangeException(nameof(frames), "A native instruction delay must be nonzero.");
        instructionTimer = frames;
    }

    /// <summary>
    /// Applies a pre-instruction transition owned by this one translated scene. Keeping the
    /// setter named and internal prevents unrelated code from treating the native address as
    /// an arbitrary public state field.
    /// </summary>
    public void PreInstructionPointerForDiscovery(ushort pointer) =>
        PreInstructionPointer = pointer;

    /// <summary>Retires the actor and clears both its visible frame and instruction cursor.</summary>
    public void Delete()
    {
        IsActive = false;
        SpriteMapPointer = 0;
        InstructionPointer = 0;
    }

    /// <summary>
    /// Advances one generic cinematic-sprite handler call for a list that uses only the
    /// shared instructions; any other instruction word fails with its address.
    /// </summary>
    public void Step(Func<ushort, ushort>? instructionWord) =>
        Step<CinematicSpriteInstruction>(ownerInstruction: null, instructionWord);

    /// <summary>
    /// Advances one generic cinematic-sprite handler call. Words outside the shared set are
    /// decoded into the owner's closed <typeparamref name="TInstruction"/> set and passed to
    /// <paramref name="ownerInstruction"/>, which returns the next list cursor. A word in
    /// neither set means the wrong owner and list were composed, and fails with its address.
    /// </summary>
    public void Step<TInstruction>(
        Func<TInstruction, ushort, ushort>? ownerInstruction,
        Func<ushort, ushort>? instructionWord)
        where TInstruction : struct, Enum
    {
        if (!IsActive)
            return;

        instructionTimer = unchecked((ushort)(instructionTimer - 1));
        if (instructionTimer != 0)
            return;

        ushort Read(ushort pointer) => (instructionWord ?? throw new InvalidOperationException(
            "Cinematic sprites require compiled instruction definitions."))(pointer);

        ushort cursor = InstructionPointer;
        while (true)
        {
            ushort word = Read(cursor);
            if ((word & CinematicCodePointers.InstructionCommandBit) == 0)
            {
                instructionTimer = word;
                SpriteMapPointer = Read(Add(cursor, 2));
                InstructionPointer = Add(cursor, 4);
                return;
            }

            if (!CinematicInstructionWords.TryDecode(word, out CinematicSpriteInstruction shared))
            {
                // Private opcodes are owned by the containing cinematic object.
                if (ownerInstruction is null)
                    throw new InvalidDataException(
                        $"Cinematic sprite opcode $8B:{word:X4} at $8B:{cursor:X4} has no owner.");
                cursor = ownerInstruction(
                    CinematicInstructionWords.Decode<TInstruction>(word, cursor), Add(cursor, 2));
                continue;
            }

            switch (shared)
            {
                case CinematicSpriteInstruction.Delete:
                    Delete();
                    return;

                case CinematicSpriteInstruction.Sleep:
                    // Sleep returns the opcode's own address so it is encountered again
                    // after an external owner primes the instruction timer/list pointer.
                    InstructionPointer = cursor;
                    return;

                case CinematicSpriteInstruction.SetPreInstruction:
                    PreInstructionPointer = Read(Add(cursor, 2));
                    cursor = Add(cursor, 4);
                    break;

                case CinematicSpriteInstruction.Goto:
                    cursor = Read(Add(cursor, 2));
                    break;

                case CinematicSpriteInstruction.DecrementTimerAndGoto:
                    GeneralTimer = unchecked((ushort)(GeneralTimer - 1));
                    cursor = GeneralTimer != 0
                        ? Read(Add(cursor, 2))
                        : Add(cursor, 4);
                    break;

                case CinematicSpriteInstruction.SetTimer:
                    GeneralTimer = Read(Add(cursor, 2));
                    cursor = Add(cursor, 4);
                    break;

                default:
                    throw new InvalidOperationException($"Undefined CinematicSpriteInstruction {shared}.");
            }
        }
    }

    /// <summary>Draws the current frame at the whole position minus <paramref name="scroll"/>.</summary>
    public void Draw(OamBuffer oam, Offset16<ScreenPixels> scroll = default,
        IIntroCinematicSpritePresentation? installedArt = null)
    {
        if (!IsActive || SpriteMapPointer == 0)
            return;
        Position16<ScreenPixels> origin = Position.Whole - scroll;
        ushort y = origin.Y;
        if (unchecked((ushort)(y + CinematicSpriteDrawDefinitions.OriginYBias)) >=
            CinematicSpriteDrawDefinitions.BiasedOriginYLimit)
            return;
        // A negative origin still has visible component tiles, but their low-byte Y
        // arithmetic needs the opposite clipping branch to prevent bottom-edge wrap.
        bool originIsOnScreen =
            ((y & CinematicSpriteDrawDefinitions.OriginYHighByteMask)) == 0;
        (installedArt ?? throw new InvalidOperationException(
            "Cinematic sprites require installed artwork."))
            .Draw(SpriteMapPointer, oam, origin.ToOam(), PaletteBits, originIsOnScreen);
    }

    /// <summary>Advances a bank-$8B list cursor with native 16-bit address wraparound.</summary>
    private static ushort Add(ushort pointer, int bytes) =>
        unchecked((ushort)(pointer + bytes));
}
