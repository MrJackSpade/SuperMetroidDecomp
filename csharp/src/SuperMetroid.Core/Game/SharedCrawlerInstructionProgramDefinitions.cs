namespace SuperMetroid.Core.Game;

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

    // Authored animation cadence (reviewed under #1165): five poses are held for three ticks each.
    // The repeated program layout derives from those still-independent choices.
    internal const int PoseCount = 5;
    private const ushort PoseHold = 3;
    internal const int SurfaceCount = 4;
    internal const int SetupBytes = 4;
    internal const int PoseBytes = 4;
    private const int LoopBytes = 4;
    internal const int ListBytes = SetupBytes + PoseCount * PoseBytes + LoopBytes;

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
}
