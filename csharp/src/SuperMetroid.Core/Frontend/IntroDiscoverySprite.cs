using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

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
    private ushort instructionTimer = 1;

    public IntroDiscoverySprite(
        ushort xPosition,
        ushort yPosition,
        ushort paletteBits,
        ushort instructionPointer)
    {
        XPosition = xPosition;
        YPosition = yPosition;
        PaletteBits = paletteBits;
        InstructionPointer = instructionPointer;
    }

    public bool IsActive { get; private set; } = true;

    public ushort XPosition { get; set; }

    public ushort XSubPosition { get; set; }

    public ushort YPosition { get; set; }

    public ushort YSubPosition { get; set; }

    public ushort PaletteBits { get; private set; }

    /// <summary>Applies a cinematic callback's native OBJ palette/attribute write.</summary>
    public void SetAttributes(SnesObjAttributeWord attributes) => PaletteBits = attributes.Raw;

    public ushort SpriteMapPointer { get; private set; }

    /// <summary>The next list word, matching <c>CinematicSpriteObject_InstListPointers</c>.</summary>
    public ushort InstructionPointer { get; private set; }

    /// <summary>The actor scratch word used by timer and motion opcodes.</summary>
    public ushort GeneralTimer { get; set; }

    /// <summary>The selected native pre-instruction address, when the list changes it.</summary>
    public ushort PreInstructionPointer { get; private set; }

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

    public void Delete()
    {
        IsActive = false;
        SpriteMapPointer = 0;
        InstructionPointer = 0;
    }

    /// <summary>
    /// Advances one generic cinematic-sprite handler call. The callback handles only
    /// scene-specific opcodes and returns their next list cursor, or null when unhandled.
    /// </summary>
    public void Step(
        ISnesAddressSpace bus,
        Func<ushort, ushort, ushort?>? specialInstruction = null)
    {
        if (!IsActive)
            return;

        instructionTimer = unchecked((ushort)(instructionTimer - 1));
        if (instructionTimer != 0)
            return;

        ushort cursor = InstructionPointer;
        while (true)
        {
            ushort word = ReadWord(bus, cursor);
            if ((word & CinematicCodePointers.InstructionCommandBit) == 0)
            {
                instructionTimer = word;
                SpriteMapPointer = ReadWord(bus, Add(cursor, 2));
                InstructionPointer = Add(cursor, 4);
                return;
            }

            switch (word)
            {
                case CinematicCodePointers.CinematicSpriteObject_Instruction_Delete:
                    Delete();
                    return;

                case CinematicCodePointers.CinematicSpriteObject_Instruction_Sleep:
                    // Sleep returns the opcode's own address so it is encountered again
                    // after an external owner primes the instruction timer/list pointer.
                    InstructionPointer = cursor;
                    return;

                case CinematicCodePointers.CinematicSpriteObject_Instruction_SetPreInstruction:
                    PreInstructionPointer = ReadWord(bus, Add(cursor, 2));
                    cursor = Add(cursor, 4);
                    break;

                case CinematicCodePointers.CinematicSpriteObject_Instruction_Goto:
                    cursor = ReadWord(bus, Add(cursor, 2));
                    break;

                case CinematicCodePointers.CinematicSpriteObject_Instruction_DecrementTimerAndGoto:
                    GeneralTimer = unchecked((ushort)(GeneralTimer - 1));
                    cursor = GeneralTimer != 0
                        ? ReadWord(bus, Add(cursor, 2))
                        : Add(cursor, 4);
                    break;

                case CinematicCodePointers.CinematicSpriteObject_Instruction_SetTimer:
                    GeneralTimer = ReadWord(bus, Add(cursor, 2));
                    cursor = Add(cursor, 4);
                    break;

                default:
                    ushort? next = specialInstruction?.Invoke(word, Add(cursor, 2));
                    if (next is null)
                    {
                        // Private opcodes are owned by the containing cinematic object.
                        // A null callback result means the wrong owner/list were composed.
                        throw new InvalidOperationException(
                            $"Baby-discovery sprite opcode $8B:{word:X4} at $8B:{cursor:X4} was not handled by its owner.");
                    }
                    cursor = next.Value;
                    break;
            }
        }
    }

    public void Draw(ISnesAddressSpace bus, OamBuffer oam, ushort cameraX = 0, ushort cameraY = 0)
    {
        if (!IsActive || SpriteMapPointer == 0)
            return;
        ushort x = unchecked((ushort)(XPosition - cameraX));
        ushort y = unchecked((ushort)(YPosition - cameraY));
        if (unchecked((ushort)(y + CinematicSpriteDrawDefinitions.OriginYBias)) >=
            CinematicSpriteDrawDefinitions.BiasedOriginYLimit)
            return;
        int address = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps, SpriteMapPointer);
        // A negative origin still has visible component tiles, but their low-byte Y
        // arithmetic needs the opposite clipping branch to prevent bottom-edge wrap.
        if ((y & CinematicSpriteDrawDefinitions.OriginYHighByteMask) != 0)
            oam.AddOffScreenSpritemap(bus, address, x, y, PaletteBits);
        else
            oam.AddOnScreenSpritemap(bus, address, x, y, PaletteBits);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        RomDataReader.ReadWordFixedBank(
            bus,
            IntroCinematicRomData.Banks.CinematicCode | pointer);

    private static ushort Add(ushort pointer, int bytes) =>
        unchecked((ushort)(pointer + bytes));
}
