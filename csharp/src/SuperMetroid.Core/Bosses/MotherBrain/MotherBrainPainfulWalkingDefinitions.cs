namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Paired forward/backward stages of Mother Brain's drained stagger.</summary>
internal static class MotherBrainPainfulWalkingDefinitions
{
    /// <summary>$A9:BEEE/$BEFE/$C049 each describe eight stages, paired by walking direction.</summary>
    internal const int StageCount = 8;

    /// <summary>Each forward/backward pair of stages shares one timing record.</summary>
    internal const int StagesPerPair = 2;

    /// <summary>$A9:BEEE, by pair: the first pair is fastest; subsequent pairs add two animation ticks.</summary>
    internal static readonly ushort[] AnimationDelays = [2, 6, 8, 10];

    /// <summary>$A9:BEFE, by pair: each pair halves angular scale while its gain decreases from five to two.</summary>
    internal static readonly ushort[] NeckAngleDeltas = [5 * 256, 4 * 256 / 2, 3 * 256 / 4, 2 * 256 / 8];

    /// <summary>$A9:C049, by pair: each successive forward/backward pair adds sixteen pause ticks.</summary>
    internal static readonly ushort[] FunctionTimers = [16, 32, 48, 64];
}
