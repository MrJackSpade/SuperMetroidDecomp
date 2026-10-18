namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The state of one Ceres door. Its function, drawing hand-off and destruction counters are values the cartridge keeps
/// in the enemy's common record, so they are emptied with the record when the door is killed
/// (<see cref="ClearWithRecord"/>).
/// </summary>
internal sealed class CeresDoorEnemyState
{
    /// <summary>The bank-$A6 main-AI function; none once a kill has emptied the record.</summary>
    public CeresDoorFunction? Function { get; internal set; }

    /// <summary>
    /// Whether the door's program has handed its drawing to Ridley's scene and not yet taken it back.
    /// </summary>
    public bool HandsDrawingToRidley { get; internal set; }

    /// <summary>
    /// Calls left in the elevator door's destruction rumble, decremented each call; the call that wraps it below zero
    /// ends the destruction.
    /// </summary>
    public int RumbleCallsLeft
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Calls left before the next destruction explosion, decremented each call and reloaded when it wraps.</summary>
    public int ExplosionIntervalLeft
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The destruction explosion position the next burst uses, counting down through the authored positions.</summary>
    public int ExplosionPositionIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        HandsDrawingToRidley = false;
        RumbleCallsLeft = 0;
        ExplosionIntervalLeft = 0;
        ExplosionPositionIndex = 0;
    }
}
