using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// What a n00b-tube piece keeps (#2571): a shard's or released air bubble's own horizontal position, which the flicker
/// draws at or mirrors, whether the crack's or a shard's flicker hides it, and how far along its swaying fall it is. The
/// crack keeps only whether its flicker hides it, which it keeps once it falls (#2455).
/// </summary>
internal sealed partial class NoobTubeDebrisState : EnemyProjectileState
{
    /// <summary>Creates a <see cref="NoobTubeDebrisState"/>.</summary>
    /// <param name="column">The column the debris starts at.</param>
    public NoobTubeDebrisState(int column)
    {
        this.X = FixedCoordinate.FromWhole(column);
    }

    /// <summary>Whether the crack's or a shard's flicker hides it on this update (#2455); a bubble never hides.</summary>
    public bool IsHidden { get; internal set; }

    /// <summary>The debris's own horizontal position, with its fraction; the drawn position follows it or hides.</summary>
    internal FixedCoordinate X { get; set; }

    /// <summary>
    /// How far through its sway the falling debris is, as an angle that goes once round a full turn each swing; it starts
    /// on a whole or a half step and moves on by whole steps.
    /// </summary>
    internal RotationAngle Sway { get; set; }

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

    /// <summary>Starts the sway at <paramref name="start"/> round the turn.</summary>
    internal void StartSway(RotationAngle start) => Sway = start;

    /// <summary>Moves the sway on by <paramref name="steps"/> whole steps, going on round the turn past a full swing.</summary>
    internal void AdvanceSway(int steps) => Sway = Sway.TurnedBySteps(steps);
}
