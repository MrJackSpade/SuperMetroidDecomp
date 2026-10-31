namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Sbug's native movement constants that are masks or offsets rather than quantities.</summary>
internal static class SbugDefinitions
{
    /// <summary>Enemy frames in each leg of the zigzag; the legs alternate left and right as the frame counter advances.</summary>
    internal const int ZigZagLegFrames = 16;

    /// <summary>Facings a reversal advances the custom facing by: $A3:A565 adds four bytes to the facing table's byte offset.</summary>
    internal const int ReversalFacingStep = 2;

    /// <summary>
    /// Facings a reversed custom facing wraps within: $A3:A565 masks the byte offset with $0007 rather than the expected
    /// $000E, so a reversal folds every facing into the first four; preserved as the cartridge does.
    /// </summary>
    internal const int FoldedFacingCount = 4;

    /// <summary>Rotates an angle by $30 before quantizing it to one of eight facings.</summary>
    internal const int FacingQuantizationOffset = 48;

    /// <summary>Heading table units covered by one of the eight facings.</summary>
    internal const int TableUnitsPerFacing = 32;
}
