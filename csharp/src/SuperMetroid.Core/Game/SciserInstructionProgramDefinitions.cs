namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Sciser's four surface loops. The sixteen interleaved
/// spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class SciserInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_Sciser_UpsideRight_0</c> at $A3:967B.</summary>
    internal const ushort UpsideRight = 0x967b;
    /// <summary><c>InstList_Sciser_UpsideLeft_0</c> at $A3:9693.</summary>
    internal const ushort UpsideLeft = 0x9693;
    /// <summary><c>InstList_Sciser_UpsideDown_0</c> at $A3:96AB.</summary>
    internal const ushort UpsideDown = 0x96ab;
    /// <summary><c>InstList_Sciser_UpsideUp_0</c> at $A3:96C3.</summary>
    internal const ushort UpsideUp = 0x96c3;
    /// <summary>The final elevator return opcode immediately before Sciser's palette.</summary>
    internal const ushort AdjacentPreviousCode = 0x95eb;

    private const int SurfaceCount = 4;
    private const int ProgramBytes = 24;
    public static int MechanicsWordCount => SurfaceCount * 8;
    public static int PresentationWordCount => SurfaceCount * 4;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 8;
        int offset = word < 2 ? word * 2 : word < 6 ? 4 + (word - 2) * 4 : 20 + (word - 6) * 2;
        ushort address = (ushort)(UpsideRight + index / 8 * ProgramBytes + offset);
        return new(address, ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(UpsideRight + index / 4 * ProgramBytes + 6 + index % 4 * 4);
    }
    /// <summary>True only for a spritemap operand in one of the four native loops.</summary>
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - UpsideRight;
        int within = offset % ProgramBytes;
        return (uint)offset < SurfaceCount * ProgramBytes && within is >= 6 and <= 18 && within % 4 == 2;
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - UpsideRight;
        if ((uint)offset < SurfaceCount * ProgramBytes)
        {
            int surface = offset / ProgramBytes;
            switch (offset % ProgramBytes)
            {
                case 0: return EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY;
                case 2: return (ushort)(surface < 2 ? CrawlerEnemyFunction.CrawlingVertically : CrawlerEnemyFunction.CrawlingHorizontally);
                case 4: case 8: case 12: case 16: return 8;
                case 20: return CommonEnemyInstructionCodes.Goto;
                case 22: return (ushort)(UpsideRight + surface * ProgramBytes + 4);
            }
        }
        throw new InvalidDataException($"Sciser instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        int offset = (ushort)address - UpsideRight;
        return (uint)offset < SurfaceCount * ProgramBytes &&
            ((offset % ProgramBytes & ~1) is 0 or 2 or 4 or 8 or 12 or 16 or 20 or 22);
    }
}
