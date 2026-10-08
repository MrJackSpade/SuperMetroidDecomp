namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Botwoon's selector-reachable head movement,
/// hiding, and spit programs. Interleaved spritemap operands select installed presentation art.
/// </summary>
internal abstract class BotwoonInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
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

    public static int MechanicsWordCount => 74;
    public static int PresentationWordCount => 25;
    private static int PhysicalDirection(int index) => index < 3 ? index : index + 1;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address;
        if (index < 24)
        {
            int word = index % 3;
            address = (ushort)(MovingUpLeft + 8 * PhysicalDirection(index / 3) + (word == 0 ? 0 : word == 1 ? 2 : 6));
        }
        else if (index < 26) address = (ushort)(Hidden + 4 * (index - 24));
        else
        {
            int word = (index - 26) % 6;
            int offset = word == 0 ? 0 : word == 5 ? 14 : 2 + 2 * word;
            address = (ushort)(SpittingUpLeft + 16 * PhysicalDirection((index - 26) / 6) + offset);
        }
        return new(address, ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 8) return (ushort)(MovingUpLeft + 8 * PhysicalDirection(index) + 4);
        if (index == 8) return Hidden + 2;
        int frame = index - 9;
        return (ushort)(SpittingUpLeft + 16 * PhysicalDirection(frame / 2) + (frame % 2 == 0 ? 2 : 12));
    }
    internal static bool IsPresentationWord(ushort address) => address == Hidden + 2 ||
        (TryDecodeDirectional(address, out bool spitting, out _, out int offset) &&
            (spitting ? offset is 2 or 12 : offset == 4));

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
    private static bool TryDecodeDirectional(ushort address, out bool spitting, out ushort movement, out int offset)
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
    private static bool IsMechanicsWord(ushort address) => address == Hidden || address == Hidden + 4 ||
        (TryDecodeDirectional(address, out bool spitting, out _, out int offset) &&
            (spitting ? offset is 0 or 4 or 6 or 8 or 10 or 14 : offset is 0 or 2 or 6));
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xb30000 &&
        (IsMechanicsWord(unchecked((ushort)address)) || IsMechanicsWord(unchecked((ushort)(address - 1))));
}
