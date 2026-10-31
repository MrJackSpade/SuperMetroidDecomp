using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// What a n00b-tube shard or released air bubble keeps: its own horizontal position, which the flicker draws at or
/// mirrors, and how far along its swaying fall it is.
/// </summary>
/// <param name="column">The column the debris starts at.</param>
internal sealed class NoobTubeDebrisState(int column) : EnemyProjectileState
{
    /// <summary>How many sway steps make one full swing, in half-angle units.</summary>
    private const int SwayPeriod = 512;

    /// <summary>The debris's own horizontal position, with its fraction; the drawn position follows it or hides.</summary>
    internal FixedCoordinate X { get; set; } = new(CoordinateAxis.Wrap(column), 0);

    /// <summary>How far through its sway the falling debris is, in half-angle units, 0 through 511.</summary>
    internal int SwayPhase
    {
        get;
        set => field = Ensure.BetweenInclusive(0, SwayPeriod - 1, value);
    }

    /// <summary>The angle whose sine is the sway's sideways speed: the phase's angle with its quarter-turn bit set.</summary>
    internal ByteAngle SwayAngle => new(WordWrap.LowByte((SwayPhase >> 1) | QuarterTurn));

    /// <summary>Moves the sway on by <paramref name="step"/> half-angle units, wrapping at a full swing.</summary>
    internal void AdvanceSway(int step) => SwayPhase = (SwayPhase + step) % SwayPeriod;

    /// <summary>The quarter-turn bit set in the sway's angle.</summary>
    private const int QuarterTurn = 64;
}
