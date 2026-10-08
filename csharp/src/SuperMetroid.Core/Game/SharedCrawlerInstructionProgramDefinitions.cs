namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words shared by Zeela, Sova, Zoomer, and Stone Zoomer. The
/// twenty interleaved spritemap operands select the same installed compositions
/// used by the Wrecked Ship HZoomer.
/// </summary>
internal abstract class SharedCrawlerInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_Zeela_Zoomer_UpsideRight_0</c> at $A3:E25C.</summary>
    internal const ushort UpsideRight = 0xe25c;
    /// <summary><c>InstList_Zeela_Zoomer_UpsideLeft_0</c> at $A3:E278.</summary>
    internal const ushort UpsideLeft = 0xe278;
    /// <summary><c>InstList_Zeela_Zoomer_UpsideDown_0</c> at $A3:E294.</summary>
    internal const ushort UpsideDown = 0xe294;
    /// <summary><c>InstList_Zeela_Zoomer_UpsideUp_0</c> at $A3:E2B0.</summary>
    internal const ushort UpsideUp = 0xe2b0;

    // Authored animation cadence (reviewed under #1165): five poses are held for three ticks each.
    // The repeated program layout derives from those still-independent choices.
    private const int PoseCount = 5;
    private const ushort PoseHold = 3;
    private const int SurfaceCount = 4;
    private const int SetupBytes = 4;
    private const int PoseBytes = 4;
    private const int LoopBytes = 4;
    private const int ListBytes = SetupBytes + PoseCount * PoseBytes + LoopBytes;
    private const int WordsPerList = 2 + PoseCount + 2;

    public static int MechanicsWordCount => SurfaceCount * WordsPerList;
    public static int PresentationWordCount => SurfaceCount * PoseCount;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int part = index % WordsPerList;
        int offset = part < 2 ? part * 2
            : part < 2 + PoseCount ? SetupBytes + (part - 2) * PoseBytes
            : SetupBytes + PoseCount * PoseBytes + (part - 2 - PoseCount) * 2;
        ushort address = (ushort)(UpsideRight + index / WordsPerList * ListBytes + offset);
        return new(address, ReadMechanicsWord(address));
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(UpsideRight + index / PoseCount * ListBytes + SetupBytes + index % PoseCount * PoseBytes + 2);
    }

    internal static bool IsPresentationWord(ushort address)
    {
        int relative = address - UpsideRight;
        if ((uint)relative >= SurfaceCount * ListBytes) return false;
        int offset = relative % ListBytes - SetupBytes;
        return (uint)offset < PoseCount * PoseBytes && offset % PoseBytes == 2;
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int relative = address - UpsideRight;
        if ((uint)relative < SurfaceCount * ListBytes && (relative & 1) == 0 && !IsPresentationWord(address))
        {
            int surface = relative / ListBytes;
            int offset = relative % ListBytes;
            if (offset == 0) return EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY;
            if (offset == 2) return (ushort)(surface < 2
                ? CrawlerEnemyFunction.CrawlingVertically : CrawlerEnemyFunction.CrawlingHorizontally);
            if (offset < SetupBytes + PoseCount * PoseBytes) return PoseHold;
            if (offset == ListBytes - LoopBytes) return CommonEnemyInstructionCodes.Goto;
            return (ushort)(UpsideRight + surface * ListBytes + SetupBytes);
        }
        throw new InvalidDataException(
            $"Shared-crawler instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        ushort word = (ushort)(address & 0xfffe);
        int relative = word - UpsideRight;
        return (uint)relative < SurfaceCount * ListBytes && !IsPresentationWord(word);
    }
}
