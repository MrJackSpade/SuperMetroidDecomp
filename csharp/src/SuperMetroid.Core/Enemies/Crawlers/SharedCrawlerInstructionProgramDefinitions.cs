namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// Compiled engine-control words shared by Zeela, Sova, Zoomer, and Stone Zoomer. The
/// twenty interleaved spritemap operands select the same installed compositions
/// used by the Wrecked Ship HZoomer.
/// </summary>
internal abstract class SharedCrawlerInstructionProgramDefinitions
{
    /// <summary><c>InstList_Zeela_Zoomer_UpsideRight_0</c> at $A3:E25C.</summary>
    internal const ushort UpsideRight = 0xe25c;
    /// <summary><c>InstList_Zeela_Zoomer_UpsideLeft_0</c> at $A3:E278.</summary>
    internal const ushort UpsideLeft = 0xe278;
    /// <summary><c>InstList_Zeela_Zoomer_UpsideDown_0</c> at $A3:E294.</summary>
    internal const ushort UpsideDown = 0xe294;
    /// <summary><c>InstList_Zeela_Zoomer_UpsideUp_0</c> at $A3:E2B0.</summary>
    internal const ushort UpsideUp = 0xe2b0;

    /// <summary>Five authored poses form each surface's repeated animation cycle.</summary>
    internal const int PoseCount = 5;
    /// <summary>Each crawler pose is held for three interpreter ticks; this is authored animation cadence.</summary>
    private const ushort PoseHold = 3;
    /// <summary>Four orientation-specific lists select the ceiling and wall crawl directions.</summary>
    internal const int SurfaceCount = 4;
    /// <summary>Each list begins with a function opcode and its movement-handler pointer.</summary>
    internal const int SetupBytes = 4;
    /// <summary>Each timed pose occupies a duration word and a spritemap operand.</summary>
    internal const int PoseBytes = 4;
    /// <summary>Each list ends with a jump opcode and a pointer back to its first pose.</summary>
    private const int LoopBytes = 4;
    /// <summary>One setup, five timed poses, and one loop comprise an orientation's list.</summary>
    internal const int ListBytes = SetupBytes + PoseCount * PoseBytes + LoopBytes;

    /// <summary>Recognizes the interleaved spritemap operands across all four orientation lists.</summary>
    internal static bool IsPresentationWord(ushort address)
    {
        int relative = address - UpsideRight;
        if ((uint)relative >= SurfaceCount * ListBytes) return false;
        int offset = relative % ListBytes - SetupBytes;
        return (uint)offset < PoseCount * PoseBytes && offset % PoseBytes == 2;
    }

    /// <summary>Compiles function setup, pose holds, and loop control while rejecting visual operands.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int relative = address - UpsideRight;
        if ((uint)relative < SurfaceCount * ListBytes && (relative & 1) == 0 && !IsPresentationWord(address))
        {
            int surface = relative / ListBytes;
            int offset = relative % ListBytes;
            if (offset == 0) return (ushort)CrawlerInstruction.FunctionInY;
            if (offset == 2) return (ushort)(surface < 2
                ? CrawlerEnemyFunction.CrawlingVertically : CrawlerEnemyFunction.CrawlingHorizontally);
            if (offset < SetupBytes + PoseCount * PoseBytes) return PoseHold;
            if (offset == ListBytes - LoopBytes) return (ushort)CommonEnemyInstruction.Goto;
            return (ushort)(UpsideRight + surface * ListBytes + SetupBytes);
        }
        throw new InvalidDataException(
            $"Shared-crawler instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
}
