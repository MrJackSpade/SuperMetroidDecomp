namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The walking animation each of Mother Brain's walking speeds plays, forwards and backwards (#2471).</summary>
internal static partial class MotherBrainWalkDefinitions
{
    /// <summary>Each walking speed's animations: every speed walks in a forward and a backward animation of its own.</summary>
    internal static IdentityTable<MotherBrainWalkSpeed, MotherBrainWalkGait> Gaits { get; } = new(new Dictionary<MotherBrainWalkSpeed, MotherBrainWalkGait>
    {
        [MotherBrainWalkSpeed.ReallyFast] = new(MotherBrainBodySegment.WalkForwardReallyFast, MotherBrainBodySegment.WalkBackwardReallyFast),
        [MotherBrainWalkSpeed.Fast] = new(MotherBrainBodySegment.WalkForwardFast, MotherBrainBodySegment.WalkBackwardFast),
        [MotherBrainWalkSpeed.Medium] = new(MotherBrainBodySegment.WalkForwardMedium, MotherBrainBodySegment.WalkBackwardMedium),
        [MotherBrainWalkSpeed.Slow] = new(MotherBrainBodySegment.WalkForwardSlow, MotherBrainBodySegment.WalkBackwardSlow),
        [MotherBrainWalkSpeed.ReallySlow] = new(MotherBrainBodySegment.WalkForwardReallySlow, MotherBrainBodySegment.WalkBackwardReallySlow),
    });
}
