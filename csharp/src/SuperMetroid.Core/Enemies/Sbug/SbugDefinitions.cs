namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Sbug's zigzag cadence and the division of the turn into its eight facings.</summary>
internal static class SbugDefinitions
{
    /// <summary>Enemy frames in each leg of the zigzag; the legs alternate left and right as the frame counter advances.</summary>
    internal const int ZigZagLegFrames = 16;

    /// <summary>
    /// The heading, in 256ths of a turn, at which the first facing's eighth of the turn begins; each later facing's
    /// eighth follows counter-clockwise.
    /// </summary>
    internal const int FirstFacingStart = 48;

    /// <summary>Heading table units covered by one of the eight facings.</summary>
    internal const int StepsPerFacing = 32;
}
