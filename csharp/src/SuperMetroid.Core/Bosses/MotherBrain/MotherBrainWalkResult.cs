namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One update of Mother Brain's body walk: whether the target or the arena limit was reached,
/// and whether this update installed a walking animation.
/// </summary>
[IndependentFlag("independent outcomes of one walk step: whether the target was reached and whether a walk was requested")]
internal readonly record struct MotherBrainWalkResult(bool ReachedTarget)
{
    /// <summary>Signals that the walk target or arena limit was reached without installing a new animation.</summary>
    internal static MotherBrainWalkResult Reached => new(true);
    /// <summary>Signals that the walk remains incomplete and no new animation was installed this update.</summary>
    internal static MotherBrainWalkResult Waiting => new(false);
    /// <summary>Signals that a walking animation was installed while travel remains incomplete.</summary>
    internal static MotherBrainWalkResult WalkInstalled => new(false);
}
