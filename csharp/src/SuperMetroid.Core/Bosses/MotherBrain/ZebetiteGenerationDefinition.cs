namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One active Zebetite generation's geometry and initial presentation binding.</summary>
internal readonly record struct ZebetiteGenerationDefinition(
    ZebetiteGenerationFlags GenerationFlags,
    int YRadius,
    ZebetiteProgram Program,
    int XPosition,
    int PrimaryYPosition,
    int LinkedYPosition);
