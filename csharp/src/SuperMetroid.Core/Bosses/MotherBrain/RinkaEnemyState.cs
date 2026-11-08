using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The state of a Rinka.</summary>
public sealed class RinkaEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the instruction pointer.</summary>
    internal AnimationProgramCursor<RinkaProgram> Program { get; set; }

    /// <summary>
    /// Routine dispatched by main AI, changed by initialization, the fire step, aim completion, or the special death
    /// tail.
    /// </summary>
    public RinkaEnemyFunction? Function { get; internal set; }

    /// <summary>Signed 8.8 velocity: X produced by the sine multiply, Y by the cosine multiply.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// Reservation token of the spawn location a special Mother Brain Rinka holds: one of the even tokens 2 through 22
    /// from the spawn-point table, or zero when it holds none.
    /// </summary>
    public int SpawnResourceToken
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Updates left before flight or before a special respawn, counted down past zero.</summary>
    public int DelayTimer { get; internal set; }
}
