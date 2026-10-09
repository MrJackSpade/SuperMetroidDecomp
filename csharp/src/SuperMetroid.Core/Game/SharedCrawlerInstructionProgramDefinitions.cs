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
    /// <summary>Number of authored animation pose words in each surface program.</summary>
    internal const int PoseCount = 5;
    /// <summary>Instruction ticks for which each authored pose remains active.</summary>
    private const ushort PoseHold = 3;
    /// <summary>Number of surface-specific crawler instruction lists sharing this layout.</summary>
    internal const int SurfaceCount = 4;
    /// <summary>Byte length of the native setup instructions preceding a pose sequence.</summary>
    internal const int SetupBytes = 4;
    /// <summary>Byte stride of one pose record, including its timing and spritemap operand.</summary>
    internal const int PoseBytes = 4;
    /// <summary>Byte length of the native loop instruction following the pose sequence.</summary>
    private const int LoopBytes = 4;
    /// <summary>Total byte length of one setup, pose sequence, and loop instruction list.</summary>
    internal const int ListBytes = SetupBytes + PoseCount * PoseBytes + LoopBytes;

    /// <summary>Tests whether an address within the shared lists contains an interleaved spritemap operand.</summary>
    /// <param name="address">Bank-relative instruction-list address to classify.</param>
    /// <returns><see langword="true"/> only for a pose record's presentation word.</returns>
    internal static bool IsPresentationWord(ushort address)
    {
        int relative = address - UpsideRight;
        if ((uint)relative >= SurfaceCount * ListBytes) return false;
        int offset = relative % ListBytes - SetupBytes;
        return (uint)offset < PoseCount * PoseBytes && offset % PoseBytes == 2;
    }

    /// <summary>Returns a compiled engine-control or timing word for a valid shared-crawler list address.</summary>
    /// <param name="address">Bank-relative $A3 address of the word requested by the interpreter.</param>
    /// <returns>The reconstructed mechanics word; spritemap operands are intentionally excluded.</returns>
    /// <exception cref="InvalidDataException">The address is outside these lists, unaligned, or identifies presentation data.</exception>
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
