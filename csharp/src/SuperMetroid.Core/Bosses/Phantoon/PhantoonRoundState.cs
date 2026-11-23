namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One round of Phantoon's vulnerability (#1933): how long his eye stays closed before it opens, the damage he has taken
/// while it is open, and whether his hurt flash has painted him white.
/// </summary>
public sealed partial class PhantoonRoundState
{
    /// <summary>
    /// The round's wait before his eye opens (#2538): the round's pattern starts it for one of
    /// <see cref="PhantoonTimerDefinitions.EyeClosed"/>'s durations, and the eye opens on its last update.
    /// </summary>
    public BossCountdown EyeClosedWait { get; } = new();

    /// <summary>The damage he has taken during the current round.</summary>
    public int Damage { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether the hurt flash has painted his body white since his health colours were last restored.</summary>
    public bool HurtFlashShown { get; internal set; }

    /// <summary>A landed hit lets the next hurt flash paint his body again.</summary>
    internal void RearmHurtFlash() => HurtFlashShown = false;
}
