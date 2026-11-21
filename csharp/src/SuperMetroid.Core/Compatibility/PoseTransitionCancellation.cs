namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A control setup's cancellation of Samus's pending pose transition during the enemies (#2562): the setup clears the
/// prospective poses her input phase chose, which the movement phase's resample after the enemies holds, so the
/// cancellation waits for that resample; an update whose movement is disabled skips the resample, and the cancellation
/// waits for the next one.
/// </summary>
/// <remarks>
/// Evidence: the game's control setup run from an enemy's AI clears Samus's prospective pose variables and momentum
/// responses at once, so the pose update after the enemies commits none of them. The prospective poses here are her
/// frame's, which only the resample after the enemies reaches.
/// Lifetime: Samus's.
/// </remarks>
[CompatibilityService("a control setup during the enemies cancels Samus's pending pose transition")]
public sealed class PoseTransitionCancellation
{
    /// <summary>Whether a cancellation waits for the resample.</summary>
    internal bool IsWaiting { get; set; }

    /// <summary>A control setup cancels the pending pose transition.</summary>
    internal void Cancel() => IsWaiting = true;

    /// <summary>The resample takes the cancellation: whether one waited, which then no longer does.</summary>
    internal bool Take()
    {
        bool waiting = IsWaiting;
        IsWaiting = false;
        return waiting;
    }
}
