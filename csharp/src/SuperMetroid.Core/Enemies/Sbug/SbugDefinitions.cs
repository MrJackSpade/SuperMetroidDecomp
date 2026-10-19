namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Sbug's native movement constants that are masks or offsets rather than quantities.</summary>
internal static class SbugDefinitions
{
    /// <summary>Bit four of the enemy frame counter alternates the zigzag's 16-frame left and right legs.</summary>
    internal const int ZigZagLegFrameBit = 0x0010;

    /// <summary>Facings a reversal advances the custom facing by: $A3:A565 adds four bytes to the facing table's byte offset.</summary>
    internal const int ReversalFacingStep = 2;

    /// <summary>
    /// Facings a reversed custom facing wraps within: $A3:A565 masks the byte offset with $0007 rather than the expected
    /// $000E, so a reversal folds every facing into the first four; preserved as the cartridge does.
    /// </summary>
    internal const int FoldedFacingCount = 4;

    /// <summary>Rotates an angle by $30 before quantizing it to one of eight facings.</summary>
    internal const int FacingQuantizationOffset = 0x30;

    /// <summary>Heading table units covered by one of the eight facings.</summary>
    internal const int TableUnitsPerFacing = 32;
}
