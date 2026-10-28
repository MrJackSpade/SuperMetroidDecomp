namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The timing of each stage of Mother Brain's drained stagger.</summary>
internal static class MotherBrainPainfulWalking
{
    /// <summary>$A9:BEEE: the animation delay of <paramref name="stage"/>.</summary>
    internal static ushort AnimationDelay(int stage) => MotherBrainPainfulWalkingDefinitions.AnimationDelays[Pair(stage)];

    /// <summary>$A9:BEFE: the neck angle step of <paramref name="stage"/>.</summary>
    internal static ushort NeckAngleDelta(int stage) => MotherBrainPainfulWalkingDefinitions.NeckAngleDeltas[Pair(stage)];

    /// <summary>$A9:C049: the pause of <paramref name="stage"/>.</summary>
    internal static ushort FunctionTimer(int stage) => MotherBrainPainfulWalkingDefinitions.FunctionTimers[Pair(stage)];

    /// <summary>Maps each forward/backward stagger stage to its shared timing pair, rejecting stages outside the eight-entry sequence.</summary>
    private static int Pair(int stage) => (uint)stage < MotherBrainPainfulWalkingDefinitions.StageCount
        ? stage / MotherBrainPainfulWalkingDefinitions.StagesPerPair : throw new ArgumentOutOfRangeException(nameof(stage));
}
