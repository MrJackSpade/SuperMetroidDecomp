using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The state of a Chozo statue.</summary>
public sealed class ChozoStatueState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<ChozoStatueProgram> Program { get; set; }

    /// <summary>The carry-motion record, 0 through 31, the statue last moved by.</summary>
    public int CarryRecord { get; internal set; }

    /// <summary>The literal pre-instruction pointer.</summary>
    public ChozoStatueFrameBehavior? FrameBehavior { get; internal set; }

    /// <summary>Whether this is the Wrecked Ship statue (parameter two zero) rather than Lower Norfair's (two).</summary>
    public bool IsWreckedShip { get; internal set; }

    /// <summary>Whether Samus has triggered the statue's hand (parameter one), which its waiting phase wakes on.</summary>
    public bool HandTriggered { get; internal set; }
}
