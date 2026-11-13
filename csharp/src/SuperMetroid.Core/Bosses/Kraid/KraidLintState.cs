namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's lints' own state beside the function every part runs: its X offset from his body and how far it has
/// travelled out of his mouth.
/// </summary>
/// <remarks>
/// The game keeps the X offset in the value his body keeps its head timer in, and the travel in the one
/// a nail keeps its horizontal fraction in (#1476); a lint reads only its own, so each is held on its own.
/// </remarks>
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
