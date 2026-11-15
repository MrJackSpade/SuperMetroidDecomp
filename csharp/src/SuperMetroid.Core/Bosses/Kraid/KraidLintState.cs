namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's lints' own state beside the phase every part runs: its X offset from his body and how far it has
/// travelled out of his mouth.
/// </summary>
public sealed class KraidLintState : KraidPartState
{
    /// <summary>A lint's X offset from the body in pixels: zero, or sixteen to the left.</summary>
    public int LintXOffset { get; internal set; }

    /// <summary>The pixels a lint has travelled out of Kraid's mouth while it is produced; never negative.</summary>
    public int LintTravel
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
}
