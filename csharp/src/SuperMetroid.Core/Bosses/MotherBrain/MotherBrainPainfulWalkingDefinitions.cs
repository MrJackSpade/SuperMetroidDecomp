namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Paired forward/backward stages of Mother Brain's drained stagger.</summary>
internal static class MotherBrainPainfulWalkingDefinitions
{
    /// <summary>The painful-walk tables each describe eight stages, paired by walking direction.</summary>
    internal const int StageCount = 8;

    /// <summary>Each forward/backward pair of stages shares one timing.</summary>
    internal const int StagesPerPair = 2;

    /// <summary>By pair: the first pair is fastest; subsequent pairs add two animation ticks.</summary>
    internal static readonly int[] AnimationDelays = [2, 6, 8, 10];

    /// <summary>By pair: how far the neck turns each update, slowing from five steps to two, three quarters and a quarter of a step.</summary>
    internal static readonly RotationAngle[] NeckTurns =
        [RotationAngle.FromSteps(5), RotationAngle.FromSteps(2), RotationAngle.FromSteps(0, 192), RotationAngle.FromSteps(0, 64)];

    /// <summary>By pair: each successive forward/backward pair adds sixteen pause ticks.</summary>
    internal static readonly int[] PauseTimers = [16, 32, 48, 64];
}
