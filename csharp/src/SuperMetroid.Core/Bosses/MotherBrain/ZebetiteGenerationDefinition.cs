namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One active Zebetite generation's geometry and initial presentation binding.</summary>
internal readonly record struct ZebetiteGenerationDefinition(
    ZebetiteGenerationFlags GenerationFlags,
    ushort YRadius,
    ZebetiteProgram Program,
    ushort XPosition,
    ushort PrimaryYPosition,
    ushort LinkedYPosition)
{
    /// <summary>Returns the Y coordinate for the primary or linked physical half.</summary>
    internal ushort YPosition(bool linkedHalf) =>
        linkedHalf ? LinkedYPosition : PrimaryYPosition;
}
