namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Sbug's movement constants that are masks or offsets rather than quantities.</summary>
internal static class SbugDefinitions
{
    /// <summary>Enemy frames in each leg of the zigzag; the legs alternate left and right as the frame counter advances.</summary>
    internal const int ZigZagLegFrames = 16;

    /// <summary>Facings a reversal advances the custom facing by: the reversal adds four bytes to the facing table's byte offset.</summary>
    internal const int ReversalFacingStep = 2;

    /// <summary>
    /// Facings a reversed custom facing wraps within: the reversal masks the byte offset with 7 rather than the expected
    /// 14, so a reversal folds every facing into the first four; preserved as the game does.
    /// </summary>
    internal const int FoldedFacingCount = 4;

    /// <summary>Rotates an angle by 48 before quantizing it to one of eight facings.</summary>
    internal const int FacingQuantizationOffset = 48;

    /// <summary>Heading table units covered by one of the eight facings.</summary>
    internal const int TableUnitsPerFacing = 32;
}
