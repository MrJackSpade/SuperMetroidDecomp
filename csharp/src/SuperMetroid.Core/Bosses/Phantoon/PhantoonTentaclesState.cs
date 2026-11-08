namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's tentacles' own state: the shot that asks for a swoop, the round's damage, the hurt flash, the swoop's
/// target and velocity, and the next death explosion. The values are emptied when the tentacles are killed
/// (<see cref="ClearWithRecord"/>).
/// </summary>
/// <remarks>
/// Every phase loads the values it reads when it starts. Each count is never negative: the round's damage only adds
/// damage, wrapping as the game's sum does, and the explosion index steps through its table.
/// </remarks>
public sealed class PhantoonTentaclesState
{
    /// <summary>The tentacles' swoop target X position (<see cref="SwoopTargetX"/>).</summary>
    private int _swoopTargetX;

    /// <summary>Whether the tentacles have taken the shot that makes Phantoon swoop when its eye closes.</summary>
    public bool SwoopRequested { get; internal set; }

    /// <summary>The damage the tentacles have taken during the current exposure.</summary>
    public int RoundDamage { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The tentacles' signed 8.8 swoop velocity in pixels per update.</summary>
    public EightEightVector<RoomPixels> SwoopVelocity { get; internal set; }

    /// <summary>The tentacles' swoop target X position, which sweeps out to its turn and back.</summary>
    public int SwoopTargetX { get => _swoopTargetX; internal set => _swoopTargetX = Ensure.BetweenInclusive(0, short.MaxValue, value); }

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

    /// <summary>Empties the tentacles' values, as a kill empties their record.</summary>
    [IntendedDuplicate("each part's state clears its own record values; the shared shape is only the assignment of zero")]
    internal void ClearWithRecord()
    {
        SwoopRequested = false;
        RoundDamage = 0;
        SwoopVelocity = default;
        _swoopTargetX = 0;
        SwoopReturning = false;
        ExplosionIndex = 0;
        HurtFlashShown = false;
    }
}
