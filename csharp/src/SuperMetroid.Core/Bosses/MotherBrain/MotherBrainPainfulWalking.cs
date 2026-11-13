namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The timing of each stage of Mother Brain's drained stagger.</summary>
internal static class MotherBrainPainfulWalking
{
    /// <summary>The animation delay of <paramref name="stage"/>.</summary>
    internal static int AnimationDelay(int stage) => MotherBrainPainfulWalkingDefinitions.AnimationDelays[Pair(stage)];

    /// <summary>The neck angle step of <paramref name="stage"/>.</summary>
    internal static int NeckAngleDelta(int stage) => MotherBrainPainfulWalkingDefinitions.NeckAngleDeltas[Pair(stage)];

    /// <summary>The pause of <paramref name="stage"/>.</summary>
    internal static int PauseTimer(int stage) => MotherBrainPainfulWalkingDefinitions.PauseTimers[Pair(stage)];

    /// <summary>Maps each forward/backward stagger stage to its shared timing pair, rejecting stages outside the eight-entry sequence.</summary>
    private static int Pair(int stage) => (uint)stage < MotherBrainPainfulWalkingDefinitions.StageCount
        ? stage / MotherBrainPainfulWalkingDefinitions.StagesPerPair : throw new ArgumentOutOfRangeException(nameof(stage));
}
