using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The state of a Chozo statue.</summary>
public sealed class ChozoStatueState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ChozoStatueSegment> Playback { get; set; }

    /// <summary>The carry step the statue last moved by.</summary>
    public ChozoCarryStep CarryStep { get; internal set; }

    /// <summary>The frame behaviour the statue runs, or none.</summary>
    public ChozoStatueFrameBehavior? FrameBehavior { get; internal set; }

    /// <summary>Whether this is the Wrecked Ship statue (parameter two zero) rather than Lower Norfair's (two).</summary>
    public bool IsWreckedShip { get; internal set; }

    /// <summary>Whether Samus has triggered the statue's hand (parameter one), which its waiting phase wakes on.</summary>
    public bool HandTriggered { get; internal set; }
}
