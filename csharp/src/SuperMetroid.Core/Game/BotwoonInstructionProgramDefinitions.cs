namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Botwoon's selector-reachable head movement,
/// hiding, and spit programs. Interleaved spritemap operands select installed presentation art.
/// </summary>
internal abstract class BotwoonInstructionProgramDefinitions
{
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingUpLeft</c> at $B3:9341.</summary>
    internal const ushort MovingUpLeft = 0x9341;
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingLeft</c> at $B3:9349.</summary>
    internal const ushort MovingLeft = 0x9349;
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingDownLeft</c> at $B3:9351.</summary>
    internal const ushort MovingDownLeft = 0x9351;
    /// <summary>
    /// <c>InstList_Botwoon_MouthClosed_AimingDown_FacingRight</c> at $B3:9361.
    /// </summary>
    internal const ushort MovingDown = 0x9361;
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingDownRight</c> at $B3:9369.</summary>
    internal const ushort MovingDownRight = 0x9369;
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingRight</c> at $B3:9371.</summary>
    internal const ushort MovingRight = 0x9371;
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingUpRight</c> at $B3:9379.</summary>
    internal const ushort MovingUpRight = 0x9379;
    /// <summary>
    /// <c>InstList_Botwoon_MouthClosed_AimingUp_FacingRight</c> at $B3:9381.
    /// </summary>
    internal const ushort MovingUp = 0x9381;
    /// <summary><c>InstList_Botwoon_Hide</c> at $B3:9389.</summary>
    internal const ushort Hidden = 0x9389;

    /// <summary><c>InstList_Botwoon_Spit_AimingUpLeft</c> at $B3:939F.</summary>
    internal const ushort SpittingUpLeft = 0x939f;

    /// <summary>Number of editable presentation operands interleaved in the compiled movement, hidden, and spit programs.</summary>
    public static int PresentationWordCount => 25;

    /// <summary>Maps a selector direction index to its physical movement-list position, accounting for the absent list at physical index 3.</summary>
    /// <param name="index">The zero-based presentation direction index in the eight-direction list.</param>
    /// <returns>The corresponding physical movement-list position, shifted past index 3 where necessary.</returns>
    internal static int PhysicalDirection(int index) => index < 3 ? index : index + 1;

    /// <summary>Returns the instruction address of a visual operand in the compiled Botwoon programs.</summary>
    /// <param name="index">The zero-based operand index across movement, hidden, and spit presentation words.</param>
    /// <returns>The bank-local address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 8) return (ushort)(MovingUpLeft + 8 * PhysicalDirection(index) + 4);
        if (index == 8) return Hidden + 2;
        int frame = index - 9;
        return (ushort)(SpittingUpLeft + 16 * PhysicalDirection(frame / 2) + (frame % 2 == 0 ? 2 : 12));
    }

    /// <summary>Determines whether an instruction address contains a replaceable presentation operand rather than mechanics.</summary>
    /// <param name="address">The bank-local Botwoon instruction address to classify.</param>
    /// <returns><see langword="true"/> for movement spritemap operands, the hidden spritemap, or spit spritemap operands.</returns>
    internal static bool IsPresentationWord(ushort address) => address == Hidden + 2 ||
        (TryDecodeDirectional(address, out bool spitting, out _, out int offset) &&
            (spitting ? offset is 2 or 12 : offset == 4));

    /// <summary>Reads a compiled non-presentation instruction word while preserving Botwoon's native timing and side effects.</summary>
    /// <param name="address">The bank-local instruction address to resolve.</param>
    /// <returns>The mechanics operand or native instruction code compiled for that address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a compiled Botwoon mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address == Hidden) return 1;
        if (address == Hidden + 4) return CommonEnemyInstructionCodes.Sleep;
        if (TryDecodeDirectional(address, out bool spitting, out ushort movement, out int offset))
        {
            if (!spitting)
            {
                if (offset == 0) return RadiusInstruction(movement);
                if (offset == 2) return 1;
                if (offset == 6) return CommonEnemyInstructionCodes.Sleep;
            }
            else
            {
                switch (offset)
                {
                    case 0: return 32;
                    case 4: return RadiusInstruction(movement);
                    case 6: return BotwoonCodePointers.Instruction_Botwoon_QueueSpitSFX;
                    case 8: return BotwoonCodePointers.Instruction_Botwoon_SetSpittingFlag;
                    case 10: return (ushort)(movement == MovingLeft ? 25 : 16);
                    case 14: return CommonEnemyInstructionCodes.Sleep;
                }
            }
        }
        throw new InvalidDataException($"Botwoon instruction mechanics pointer $B3:{address:X4} is not compiled.");
    }
    /// <summary>Selects the native enemy-radius instruction associated with a Botwoon movement program.</summary>
    /// <param name="movement">The compiled directional movement-list pointer.</param>
    /// <returns>The native instruction code that sets that direction's collision radius.</returns>
    /// <exception cref="InvalidDataException">The movement pointer is not one of Botwoon's compiled directions.</exception>
    private static ushort RadiusInstruction(ushort movement) => movement switch
    {
        MovingUpLeft => BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC,
        MovingLeft => BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_10x8,
        MovingDownLeft => BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC_duplicate,
        MovingDown => BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_8x10_duplicate_again,
        MovingDownRight => BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC_duplicate_again,
        MovingRight => BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_10x8_duplicate,
        MovingUpRight => BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC_duplicate_again2,
        MovingUp => BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_8x10_duplicate_again2,
        _ => throw new InvalidDataException("Unknown Botwoon movement program."),
    };
    /// <summary>Decodes an address inside a directional movement or spit instruction list.</summary>
    /// <param name="address">The bank-local instruction address to decode.</param>
    /// <param name="spitting">Receives whether the address belongs to a spit program.</param>
    /// <param name="movement">Receives the corresponding directional movement-list pointer, or zero when the address is outside a list.</param>
    /// <param name="offset">Receives the byte offset within that list, or zero when the address is outside a list.</param>
    /// <returns><see langword="true"/> when the address falls inside one of the compiled directional lists.</returns>
    internal static bool TryDecodeDirectional(ushort address, out bool spitting, out ushort movement, out int offset)
    {
        spitting = address >= SpittingUpLeft;
        int relative = address - (spitting ? SpittingUpLeft : MovingUpLeft);
        int stride = spitting ? 16 : 8;
        int direction = relative / stride;
        if ((uint)relative >= 9 * stride || direction == 3)
        {
            movement = 0; offset = 0; return false;
        }
        movement = (ushort)(MovingUpLeft + 8 * direction);
        offset = relative % stride;
        return true;
    }
}
