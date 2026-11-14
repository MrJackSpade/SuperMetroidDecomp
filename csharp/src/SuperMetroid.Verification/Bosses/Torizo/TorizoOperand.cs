namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>What one operand word of a native Torizo instruction carries.</summary>
internal enum TorizoOperand
{
    /// <summary>A movement index, spawn parameter or walking step: the cue's value.</summary>
    Value,
    /// <summary>A main-AI function.</summary>
    Function,
    /// <summary>A pre-instruction.</summary>
    FrameBehavior,
    /// <summary>A list pointer: the cue's target program.</summary>
    Target,
    /// <summary>A second list pointer: the cue's alternate program.</summary>
    Alternate,
    /// <summary>A list pointer: the list a replay cue's run of steps begins at, which it plays again (#1788).</summary>
    ReplayedRun,
    /// <summary>A byte offset into the swipe placement tables at $86:A738 and $86:A74E: the swipe cue's hand placement (#1867).</summary>
    SwipePlacement,
    /// <summary>A zero or one the routine never reads, authored on every sonic boom and eye beam; the cue has no value (#1867).</summary>
    Unread,
}
