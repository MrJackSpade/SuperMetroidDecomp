using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>The state of one Puyo.</summary>
public sealed class PuyoEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<PuyoProgram> Program { get; set; }

    /// <summary>
    /// Unsigned 8.8 index whose whole part chooses a row of the shared quadratic speed table, from 0 through 32767:
    /// a hop starts below 24576, and any step that would leave the range restarts the fall at zero
    /// (<see cref="StepAirborneIndex"/>).
    /// </summary>
    public int YSpeedTableIndex { get; internal set; }

    /// <summary>Grounded-frame countdown copied from population parameter one.</summary>
    public int HopCooldownTimer { get; internal set; }

    /// <summary>Whether the Puyo is grounded or airborne.</summary>
    public PuyoEnemyFunction? Function { get; internal set; }

    /// <summary>The current hop's airborne behaviour.</summary>
    public PuyoAirborneFunction? AirborneFunction { get; internal set; }

    /// <summary>
    /// The hop being made, whose definition drives the airborne motion; it is kept when a collision changes <see
    /// cref="HopType"/>.
    /// </summary>
    public PuyoHopType CurrentHop { get; internal set; }

    /// <summary>The record selected for the next hop.</summary>
    public PuyoHopType HopType { get; internal set; }

    /// <summary>
    /// True means the hopping pose is still active, while false permits
    /// the grounded-list handoff.
    /// </summary>
    public bool HoppingAnimationActive { get; internal set; }

    /// <summary>The direction of the current hop.</summary>
    public PuyoDirection Direction { get; internal set; }

    /// <summary>Selects positive gravity-table rows.</summary>
    public bool Falling { get; internal set; }

    /// <summary>A terrain collision requests that the next hop use the precomputed opposite direction instead of re-aiming at Samus.</summary>
    public bool InvertDirection { get; internal set; }

    /// <summary>Opposite direction captured when Puyo strikes a wall or ceiling.</summary>
    public PuyoDirection InvertedDirection { get; internal set; }

    /// <summary>Three-quarter point of the initial vertical speed index.</summary>
    public int InitialYSpeedTableIndexThreeQuarters { get; internal set; }

    /// <summary>One-half point of the initial vertical speed index.</summary>
    public int InitialYSpeedTableIndexHalf { get; internal set; }

    /// <summary>
    /// One airborne update's change to the speed index: rising lowers it by <paramref name="delta"/>, falling raises it.
    /// A step that leaves 0 through 32767 (the game tests its word's sign bit) restarts the fall at zero: the apex
    /// while rising, and a restart of an overlong fall.
    /// </summary>
    /// <param name="delta">The hop record's 8.8 index step, at most $300.</param>
    internal void StepAirborneIndex(int delta)
    {
        YSpeedTableIndex = Falling ? YSpeedTableIndex + delta : YSpeedTableIndex - delta;
        if (YSpeedTableIndex is >= 0 and <= short.MaxValue)
            return;
        Falling = true;
        YSpeedTableIndex = 0;
    }
}
