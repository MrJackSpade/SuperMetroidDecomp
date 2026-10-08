namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Puyo's three grounded loops and five airborne pose
/// programs. Their interleaved spritemap operands select installed presentation frames.
/// </summary>
internal abstract class PuyoInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_Puyo_GroundedDropping_Fast</c> at $A2:99AD.</summary>
    internal const ushort GroundedFast = 0x99ad;

    /// <summary><c>InstList_Puyo_GroundedDropping_Medium</c> at $A2:99C1.</summary>
    internal const ushort GroundedMedium = 0x99c1;

    /// <summary><c>InstList_Puyo_GroundedDropping_Slow</c> at $A2:99D5.</summary>
    internal const ushort GroundedSlow = 0x99d5;

    /// <summary><c>InstList_Puyo_HoppingRight_0_HoppingLeft_4</c> at $A2:99E9.</summary>
    internal const ushort RightFrame0LeftFrame4 = 0x99e9;

    /// <summary><c>InstList_Puyo_HoppingRight_1_HoppingLeft_3</c> at $A2:99EF.</summary>
    internal const ushort RightFrame1LeftFrame3 = 0x99ef;

    /// <summary><c>InstList_Puyo_Hopping_2</c> at $A2:99F5.</summary>
    internal const ushort Frame2 = 0x99f5;

    /// <summary><c>InstList_Puyo_HoppingRight_3_HoppingLeft_1</c> at $A2:99FB.</summary>
    internal const ushort RightFrame3LeftFrame1 = 0x99fb;

    /// <summary><c>InstList_Puyo_HoppingRight_4_HoppingLeft_0</c> at $A2:9A01.</summary>
    internal const ushort RightFrame4LeftFrame0 = 0x9a01;

    public static int MechanicsWordCount => 28;
    public static int PresentationWordCount => 17;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int pointer;
        if (index < 18)
        {
            int local = index % 6;
            pointer = GroundedFast + index / 6 * 20 + (local < 5 ? local * 4 : 18);
        }
        else pointer = RightFrame0LeftFrame4 + (index - 18) / 2 * 6 + (index % 2) * 4;
        return new((ushort)pointer, ReadMechanicsWord((ushort)pointer));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 12 ? GroundedFast + index / 4 * 20 + index % 4 * 4 + 2 :
            RightFrame0LeftFrame4 + (index - 12) * 6 + 2);
    }
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - GroundedFast;
        if ((uint)offset < 60) return offset % 20 < 16 && offset % 20 % 4 == 2;
        offset = address - RightFrame0LeftFrame4;
        return (uint)offset < 30 && offset % 6 == 2;
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - GroundedFast;
        if ((uint)offset < 60)
        {
            int local = offset % 20;
            ushort start = (ushort)(GroundedFast + offset / 20 * 20);
            if (local < 16 && local % 4 == 0)
                return start == GroundedFast ? (ushort)5 : start == GroundedMedium ? (ushort)8 : (ushort)10;
            if (local == 16) return CommonEnemyInstructionCodes.Goto;
            if (local == 18) return start;
        }
        offset = address - RightFrame0LeftFrame4;
        if ((uint)offset < 30)
        {
            if (offset % 6 == 0) return 1;
            if (offset % 6 == 4) return CommonEnemyInstructionCodes.Sleep;
        }
        throw new InvalidDataException($"Puyo instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = (ushort)address - GroundedFast;
        if ((uint)offset < 60) return offset % 20 >= 16 || offset % 20 % 4 < 2;
        offset = (ushort)address - RightFrame0LeftFrame4;
        return (uint)offset < 30 && offset % 6 is not (2 or 3);
    }
}
