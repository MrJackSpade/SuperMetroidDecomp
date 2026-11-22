using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The state of a Chozo statue.</summary>
public sealed partial class ChozoStatueState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ChozoStatueSegment> Playback { get; set; }

    /// <summary>Where the statue stands in its wait for Samus at its hand and its activation (#2476).</summary>
    public ChozoStatuePhase Phase { get; internal set; } = ChozoStatuePhase.Placed;

    /// <summary>The site the statue's placement names, which decides what its hand's trigger records (#2558).</summary>
    public required ChozoStatueSite Site { get; init; }

    /// <summary>Samus has triggered the statue's hand: a waiting statue's next update starts its activation.</summary>
    internal void TriggerHand() => Phase = Phase.AfterHandTriggered;

    /// <summary>The statue's idle animation starts its wait for Samus at its hand, or its wake if she is already there.</summary>
    internal void AwaitHand() => Phase = Phase.AfterWaitStarts;
}
