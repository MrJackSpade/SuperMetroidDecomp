using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The state of a Chozo statue.</summary>
public sealed class ChozoStatueState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ChozoStatueSegment> Playback { get; set; }

    /// <summary>What the statue waits for each frame, or null while it waits for nothing.</summary>
    public ChozoStatueFrameBehavior? FrameBehavior { get; internal set; }

    /// <summary>Whether this is the Wrecked Ship statue rather than Lower Norfair's.</summary>
    public bool IsWreckedShip { get; internal set; }

    /// <summary>Whether Samus has triggered the statue's hand, which its waiting phase wakes on.</summary>
    public bool HandTriggered { get; internal set; }
}
