using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The state of a Chozo statue.</summary>
public sealed class ChozoStatueState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ChozoStatueSegment> Playback { get; set; }

    /// <summary>
    /// Whether the statue awaits Samus at its hand: from its idle animation's start until its activation starts. Lower
    /// Norfair's wakes once Samus triggers the hand, the Wrecked Ship's once Phantoon is also defeated.
    /// </summary>
    public bool AwaitingHand { get; internal set; }

    /// <summary>Which of the two statues this is (#2369), as its placement names it.</summary>
    public ChozoStatueSite Site { get; internal set; }

    /// <summary>Whether Samus has triggered the statue's hand, which its waiting phase wakes on.</summary>
    public bool HandTriggered { get; internal set; }
}
