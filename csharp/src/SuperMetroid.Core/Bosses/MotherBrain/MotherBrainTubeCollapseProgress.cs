namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Where the collapse of the tubes about Mother Brain's tank stands (#2258): the tube it is on among
/// <see cref="MotherBrainTubeCollapseDefinitions.Drops"/> and whether that tube still stands or has fallen.
/// </summary>
/// <param name="Tube">The tube the collapse is on, or null once every tube has fallen and its place has been cleared.</param>
/// <param name="Stage">Whether the tube still stands, so it falls next, or has fallen, so its place clears next.</param>
internal readonly record struct MotherBrainTubeCollapseProgress(ListStep<MotherBrainTubeDrop>? Tube, MotherBrainTubeDropStage Stage)
{
    /// <summary>The tube the collapse drops first, which waits for free enemy projectiles rather than a wait (#2470).</summary>
    internal static ListStep<MotherBrainTubeDrop> FirstTube => ListStep<MotherBrainTubeDrop>.FirstOf(MotherBrainTubeCollapseDefinitions.Drops);

    /// <summary>The collapse as her fake-death flash starts it: its first tube, still standing.</summary>
    internal static MotherBrainTubeCollapseProgress Start => new(FirstTube, MotherBrainTubeDropStage.Standing);
}
