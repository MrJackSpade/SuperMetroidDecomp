namespace SuperMetroid.Core.Game;

/// <summary>A gameplay cue of Mother Brain's body programs.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Pose">The pose a <see cref="MotherBrainBodyCueKind.SetPose"/> cue publishes.</param>
/// <param name="Offset">The body displacement, hand offset, or vertical move with its scroll bias as X.</param>
/// <param name="Dust">The particle a hand-dust cue spawns.</param>
internal readonly record struct MotherBrainBodyCue(
    MotherBrainBodyCueKind Kind, MotherBrainBodyPose Pose, Offset16<RoomPixels> Offset, MiscDustAnimation Dust)
{
    /// <summary>Publishes <paramref name="pose"/>.</summary>
    internal static MotherBrainBodyCue SetPose(MotherBrainBodyPose pose) => new(MotherBrainBodyCueKind.SetPose, pose, default, default);
    /// <summary>Moves the body by <paramref name="offset"/>.</summary>
    internal static MotherBrainBodyCue Move(Offset16<RoomPixels> offset) => new(MotherBrainBodyCueKind.Move, default, offset, default);
    /// <summary>Plants a foot, then moves the body by <paramref name="offset"/>.</summary>
    internal static MotherBrainBodyCue StepAndMove(Offset16<RoomPixels> offset) => new(MotherBrainBodyCueKind.StepAndMove, default, offset, default);
    /// <summary>Moves the body down by <paramref name="y"/>, scrolling BG2 with <paramref name="scrollBias"/>.</summary>
    internal static MotherBrainBodyCue MoveWithScrollBias(short y, short scrollBias) =>
        new(MotherBrainBodyCueKind.MoveWithScrollBias, default, new(scrollBias, y), default);
    /// <summary>Spawns <paramref name="dust"/> at <paramref name="offset"/> from the body.</summary>
    internal static MotherBrainBodyCue SpawnHandDust(Offset16<RoomPixels> offset, MiscDustAnimation dust) =>
        new(MotherBrainBodyCueKind.SpawnHandDust, default, offset, dust);
    /// <summary>Fires the red hand beam.</summary>
    internal static MotherBrainBodyCue FireHandBeam { get; } = new(MotherBrainBodyCueKind.FireHandBeam, default, default, default);
    /// <summary>Advances the hand-beam attack phase.</summary>
    internal static MotherBrainBodyCue AdvanceHandBeamPhase { get; } = new(MotherBrainBodyCueKind.AdvanceHandBeamPhase, default, default, default);
}
