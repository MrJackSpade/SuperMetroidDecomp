namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Sbug's native movement constants that are masks or offsets rather than quantities.</summary>
internal static class SbugDefinitions
{
    /// <summary>Bit four of the enemy frame counter alternates the zigzag's 16-frame left and right legs.</summary>
    internal const int ZigZagLegFrameBit = 0x0010;

    /// <summary>
    /// $A3:A565 masks the reversed custom facing with $0007 rather than the expected $000E, so directions in the upper
    /// half fold down; preserved as the cartridge does.
    /// </summary>
    internal const int CustomFacingFoldMask = 0x0007;

    /// <summary>Rotates an angle by $30 before quantizing it to one of eight facings.</summary>
    internal const int FacingQuantizationOffset = 0x30;
}
