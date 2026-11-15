namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One round of Phantoon's vulnerability (#1933): how long his eye stays closed before it opens, the damage he has taken
/// while it is open, and whether his hurt flash has painted him white.
/// </summary>
public sealed class PhantoonRoundState
{
    /// <summary>The updates before his eye opens.</summary>
    public int EyeClosedTimer { get; internal set; }

    /// <summary>The damage he has taken during the current round.</summary>
    public int Damage { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether the hurt flash has painted his body white since his health colours were last restored.</summary>
    public bool HurtFlashShown { get; internal set; }

    /// <summary>A landed hit lets the next hurt flash paint his body again.</summary>
    internal void RearmHurtFlash() => HurtFlashShown = false;
}
