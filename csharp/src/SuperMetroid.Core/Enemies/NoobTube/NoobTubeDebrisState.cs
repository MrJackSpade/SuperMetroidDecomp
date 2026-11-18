using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// What a n00b-tube shard or released air bubble keeps: its own horizontal position, which the flicker draws at or
/// mirrors, whether a shard's hiding flicker hides it, and how far along its swaying fall it is.
/// </summary>
/// <param name="column">The column the debris starts at.</param>
internal sealed class NoobTubeDebrisState(int column) : EnemyProjectileState, IFlickeringProjectileState
{
    /// <summary>Whether a shard's hiding flicker hides it on this update (#2455); a bubble never hides.</summary>
    public bool IsHidden { get; internal set; }

    /// <summary>The debris's own horizontal position, with its fraction; the drawn position follows it or hides.</summary>
    internal FixedCoordinate X { get; set; } = new(column, 0);

    /// <summary>
    /// How far through its sway the falling debris is, as an angle that goes once round a full turn each swing; it starts
    /// on a whole or a half step and moves on by whole steps.
    /// </summary>
    internal RotationAngle Sway { get; private set; }

    /// <summary>
    /// The angle whose sine is the sway's sideways speed: the sway's whole steps, kept to the turn's second and fourth
    /// quarters, so an angle in the first or third quarter is turned on a quarter turn (#1847).
    /// </summary>
    internal TrigonometryAngle SwayAngle
    {
        get
        {
            TrigonometryAngle angle = Sway.ToTrigonometryAngle();
            return angle.IsInFirstOrThirdQuarter ? angle.TurnedBy(TrigonometryAngle.QuarterTurn) : angle;
        }
    }

    /// <summary>Starts the sway <paramref name="halfSteps"/> half steps round the turn, 0 through 511.</summary>
    internal void StartSway(int halfSteps) => Sway = RotationAngle.FromHalfSteps(halfSteps);

    /// <summary>Moves the sway on by <paramref name="steps"/> whole steps, going on round the turn past a full swing.</summary>
    internal void AdvanceSway(int steps) => Sway = Sway.TurnedBySteps(steps);
}
