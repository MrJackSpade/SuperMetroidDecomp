using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

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
    /// <summary>Updates remaining before the current instruction-list entry is consumed.</summary>
    private ushort instructionTimer = 1;

    /// <summary>Creates a cinematic sprite at its initial world position with native palette and list state.</summary>
    /// <param name="xPosition">Initial whole-pixel world X coordinate.</param>
    /// <param name="yPosition">Initial whole-pixel world Y coordinate.</param>
    /// <param name="paletteBits">OBJ attribute palette/priority bits used for emitted sprites.</param>
    /// <param name="instructionPointer">Bank-$8B list address to interpret on the first due update.</param>
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

    /// <summary>Whether this sprite remains allocated for instruction stepping and drawing.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Whole-pixel world X coordinate before camera subtraction.</summary>
    public ushort XPosition { get; set; }

    /// <summary>Fractional X accumulator used by cinematic motion callbacks.</summary>
    public ushort XSubPosition { get; set; }

    /// <summary>Whole-pixel world Y coordinate before camera subtraction.</summary>
    public ushort YPosition { get; set; }

    /// <summary>Fractional Y accumulator used by cinematic motion callbacks.</summary>
    public ushort YSubPosition { get; set; }

    /// <summary>OBJ palette and attribute bits applied to each drawn sprite component.</summary>
    public ushort PaletteBits { get; private set; }

    /// <summary>Applies a cinematic callback's native OBJ palette/attribute write.</summary>
    public void SetAttributes(SnesObjAttributeWord attributes) => PaletteBits = attributes.Raw;

    /// <summary>Current native spritemap pointer, or zero when no frame is selected.</summary>
    public ushort SpriteMapPointer { get; private set; }

    /// <summary>The next list word, matching <c>CinematicSpriteObject_InstListPointers</c>.</summary>
    public ushort InstructionPointer { get; private set; }

    /// <summary>The actor scratch word used by timer and motion opcodes.</summary>
    public ushort GeneralTimer { get; set; }

    /// <summary>The selected native pre-instruction address, when the list changes it.</summary>
    public ushort PreInstructionPointer { get; private set; }

    /// <summary>Replaces the instruction-list cursor and makes its next entry due immediately.</summary>
    /// <param name="pointer">New bank-$8B instruction-list address.</param>
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

    /// <summary>Deactivates the sprite and clears its current frame and instruction cursor.</summary>
    public void Delete()
    {
        IsActive = false;
        SpriteMapPointer = 0;
        InstructionPointer = 0;
    }

    /// <summary>
    /// Advances one generic cinematic-sprite handler call. The callback handles only
    /// scene-specific opcodes and returns their next list cursor, or null when unhandled.
    /// An owner may supply its compiled instruction-word reader; other scenes retain
    /// the ROM-backed path until their own bounded lists have been migrated.
    /// </summary>
    public void Step(
        ISnesAddressSpace bus,
        Func<ushort, ushort, ushort?>? specialInstruction = null,
        Func<ushort, ushort>? instructionWord = null)
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
                    PreInstructionPointer = Read(Add(cursor, 2));
                    cursor = Add(cursor, 4);
                    break;

                case CinematicCodePointers.CinematicSpriteObject_Instruction_Goto:
                    cursor = Read(Add(cursor, 2));
                    break;

                case CinematicCodePointers.CinematicSpriteObject_Instruction_DecrementTimerAndGoto:
                    GeneralTimer = unchecked((ushort)(GeneralTimer - 1));
                    cursor = GeneralTimer != 0
                        ? Read(Add(cursor, 2))
                        : Add(cursor, 4);
                    break;

                case CinematicCodePointers.CinematicSpriteObject_Instruction_SetTimer:
                    GeneralTimer = Read(Add(cursor, 2));
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

    /// <summary>Emits the active spritemap after camera offset and native screen-edge clipping are applied.</summary>
    /// <param name="bus">Address space retained by the shared drawing interface; installed artwork supplies the sprite data.</param>
    /// <param name="oam">Object attribute buffer receiving visible spritemap components.</param>
    /// <param name="cameraX">Horizontal camera origin subtracted from the world coordinate.</param>
    /// <param name="cameraY">Vertical camera origin subtracted from the world coordinate.</param>
    /// <param name="installedArt">Installed presentation that resolves the spritemap pointer.</param>
    public void Draw(ISnesAddressSpace bus, OamBuffer oam, ushort cameraX = 0,
        ushort cameraY = 0, IIntroCinematicSpritePresentation? installedArt = null)
    {
        if (!IsActive || SpriteMapPointer == 0)
            return;
        ushort x = unchecked((ushort)(XPosition - cameraX));
        ushort y = unchecked((ushort)(YPosition - cameraY));
        if (unchecked((ushort)(y + CinematicSpriteDrawDefinitions.OriginYBias)) >=
            CinematicSpriteDrawDefinitions.BiasedOriginYLimit)
            return;
        // A negative origin still has visible component tiles, but their low-byte Y
        // arithmetic needs the opposite clipping branch to prevent bottom-edge wrap.
        bool originIsOnScreen =
            (y & CinematicSpriteDrawDefinitions.OriginYHighByteMask) == 0;
        (installedArt ?? throw new InvalidOperationException(
            "Cinematic sprites require installed artwork."))
            .Draw(SpriteMapPointer, oam, x, y, PaletteBits, originIsOnScreen);
    }

    /// <summary>Advances a bank-local list cursor with native 16-bit wrapping.</summary>
    /// <param name="pointer">Current instruction-list offset.</param>
    /// <param name="bytes">Byte distance to advance.</param>
    /// <returns>The wrapped bank offset.</returns>
    private static ushort Add(ushort pointer, int bytes) =>
        unchecked((ushort)(pointer + bytes));
}
