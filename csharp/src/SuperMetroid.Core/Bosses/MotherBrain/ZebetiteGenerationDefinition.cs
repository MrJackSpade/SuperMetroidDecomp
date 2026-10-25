namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One active Zebetite generation's geometry and initial presentation binding.</summary>
internal readonly record struct ZebetiteGenerationDefinition(
    ZebetiteGenerationFlags GenerationFlags,
    int YRadius,
    ZebetiteProgram Program,
    int XPosition,
    int PrimaryYPosition,
    int LinkedYPosition)
{
    /// <summary>Returns the Y coordinate for the primary or linked physical half.</summary>
    internal int YPosition(bool linkedHalf) =>
        linkedHalf ? LinkedYPosition : PrimaryYPosition;
}
