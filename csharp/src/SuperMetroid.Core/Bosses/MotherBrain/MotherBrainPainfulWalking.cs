namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The timing of each stage of Mother Brain's drained stagger.</summary>
internal static class MotherBrainPainfulWalking
{
    /// <summary>How fast she staggers in <paramref name="stage"/>.</summary>
    internal static MotherBrainWalkSpeed Speed(int stage) => MotherBrainPainfulWalkingDefinitions.Speeds[Pair(stage)];

    /// <summary>How far the neck turns each update in <paramref name="stage"/>.</summary>
    internal static RotationAngle NeckTurn(int stage) => MotherBrainPainfulWalkingDefinitions.NeckTurns[Pair(stage)];

    /// <summary>The pause of <paramref name="stage"/>.</summary>
    internal static int PauseTimer(int stage) => MotherBrainPainfulWalkingDefinitions.PauseTimers[Pair(stage)];

    /// <summary>Maps each forward/backward stagger stage to its shared timing pair, rejecting stages outside its eight stages.</summary>
    private static int Pair(int stage) => stage is >= 0 and < MotherBrainPainfulWalkingDefinitions.StageCount
        ? stage / MotherBrainPainfulWalkingDefinitions.StagesPerPair : throw new ArgumentOutOfRangeException(nameof(stage));
}
