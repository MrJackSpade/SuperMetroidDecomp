namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>The state of one Fune, which spits each time its cooldown runs out.</summary>
public sealed class FuneEnemyState : FuneNamiheEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal FuneEnemyState(FuneAction action) => Action = action;

    /// <summary>The waiting or spitting behaviour.</summary>
    public FuneAction Action { get; internal set; }

    /// <summary>
    /// The count of waiting updates, reset to zero when the wait ends: it counts up to <see cref="CooldownTime"/>, a
    /// spawn setting of at most 255 updates, so it never reaches the 32768 at which the game's signed comparison of the
    /// two would differ from a plain one, and never wraps (#2045).
    /// </summary>
    public int CooldownTimer
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>The number of updates the Fune waits, from the spawn settings, 0 through 255.</summary>
    public int CooldownTime
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <inheritdoc/>
    internal override int MouthDrop => 0;

    /// <inheritdoc/>
    internal override FuneNamiheSegment Animation => ShowsActiveAnimation
        ? FacingRight ? FuneNamiheSegment.FuneActiveRight : FuneNamiheSegment.FuneActiveLeft
        : FacingRight ? FuneNamiheSegment.FuneIdleRight : FuneNamiheSegment.FuneIdleLeft;

    /// <inheritdoc/>
    internal override void FinishSpit()
    {
        ShowsActiveAnimation = false;
        Action = FuneAction.WaitForCooldown;
    }
}
