namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's tentacles' own state: the shot that asks for a swoop, the round's damage, the hurt flash, the swoop's
/// target and velocity, and the next death explosion.
/// </summary>
/// <remarks>
/// Every phase loads the values it reads when it starts. Each count is never negative: the round's damage only adds
/// damage, wrapping as the game's sum does, and the explosion index steps through its table.
/// </remarks>
public sealed class PhantoonTentaclesState
{
    /// <summary>Whether the tentacles have taken the shot that makes Phantoon swoop when its eye closes.</summary>
    public bool SwoopRequested { get; internal set; }

    /// <summary>The damage the tentacles have taken during the current exposure.</summary>
    public int RoundDamage { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The tentacles' signed 1/256-pixel swoop velocity in pixels per update.</summary>
    public EightEightVector<RoomPixels> SwoopVelocity { get; internal set; }

    /// <summary>
    /// The tentacles' swoop target X position, which sweeps out by two pixels an update to its turn at
    /// <see cref="PhantoonMotionDefinitions.SwoopTargetTurnX"/> and back to zero.
    /// </summary>
    public int SwoopTargetX { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether the tentacles' swoop target has reached its turn and is counting back toward zero.</summary>
    public bool SwoopReturning { get; internal set; }

    /// <summary>The tentacles' next death explosion.</summary>
    public int ExplosionIndex { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>
    /// A landed hit lets the next hurt flash paint the body again: the game writes a hit marker over the value that
    /// holds <see cref="HurtFlashShown"/>, and nothing reads the marker itself.
    /// </summary>
    internal void RearmHurtFlash() => HurtFlashShown = false;

    /// <summary>Whether the hurt flash has painted the body white since the health palette was last restored.</summary>
    public bool HurtFlashShown { get; internal set; }
}
