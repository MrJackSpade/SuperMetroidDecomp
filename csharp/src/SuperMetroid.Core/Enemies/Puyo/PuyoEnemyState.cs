using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>The state of one Puyo.</summary>
public sealed class PuyoEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PuyoSegment> Playback { get; set; }

    /// <summary>
    /// The Puyo's place along the shared quadratic speed curve, whose whole marks choose its vertical speed, from the curve's
    /// start through <see cref="LargestYSpeedCurvePlace"/>: a hop starts below its 96th mark, and any step that would leave
    /// that range restarts the fall at the curve's start (<see cref="StepAirbornePlace"/>).
    /// </summary>
    public QuadraticSpeedCurvePlace YSpeedCurvePlace { get; internal set; }

    /// <summary>
    /// The furthest place the Puyo keeps, the last before 128 whole marks: a step past it, as a step back past the
    /// curve's start, restarts the fall.
    /// </summary>
    internal static readonly QuadraticSpeedCurvePlace LargestYSpeedCurvePlace =
        QuadraticSpeedCurvePlace.FromMarks(128) - QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(1);

    /// <summary>Grounded-frame countdown copied from its spawn hop cooldown.</summary>
    public int HopCooldownTimer { get; internal set; }

    /// <summary>Whether the Puyo is grounded or airborne.</summary>
    public PuyoAction? Action { get; internal set; }

    /// <summary>The current hop's airborne behaviour.</summary>
    public PuyoAirborneMovement? AirborneMovement { get; internal set; }

    /// <summary>
    /// The hop being made, whose definition drives the airborne motion; it is kept when a collision changes <see
    /// cref="HopType"/>.
    /// </summary>
    public PuyoHopType CurrentHop { get; internal set; }

    /// <summary>The mark selected for the next hop.</summary>
    public PuyoHopType HopType { get; internal set; }

    /// <summary>
    /// True means the hopping pose is still active, while false permits
    /// the grounded-animation handoff.
    /// </summary>
    public bool HoppingAnimationActive { get; internal set; }

    /// <summary>The direction of the current hop.</summary>
    public PuyoDirection Direction { get; internal set; }

    /// <summary>Selects the curve's downward speeds.</summary>
    public bool Falling { get; internal set; }

    /// <summary>A terrain collision requests that the next hop use the precomputed opposite direction instead of re-aiming at Samus.</summary>
    public bool InvertDirection { get; internal set; }

    /// <summary>Opposite direction captured when Puyo strikes a wall or ceiling.</summary>
    public PuyoDirection InvertedDirection { get; internal set; }

    /// <summary>Three-quarter point of the initial vertical speed-curve place.</summary>
    public QuadraticSpeedCurvePlace InitialYSpeedCurvePlaceThreeQuarters { get; internal set; }

    /// <summary>One-half point of the initial vertical speed-curve place.</summary>
    public QuadraticSpeedCurvePlace InitialYSpeedCurvePlaceHalf { get; internal set; }

    /// <summary>
    /// One airborne update's change to the speed-curve place: rising lowers it by <paramref name="delta"/>, falling raises it.
    /// A step back past the curve's start, or on past <see cref="LargestYSpeedCurvePlace"/>, restarts
    /// the fall at the curve's start: the apex while rising, and a restart of an overlong fall.
    /// </summary>
    /// <param name="delta">The hop's step along the curve, at most three whole marks.</param>
    internal void StepAirbornePlace(QuadraticSpeedCurvePlace delta)
    {
        YSpeedCurvePlace = Falling ? YSpeedCurvePlace + delta : YSpeedCurvePlace - delta;
        if (!YSpeedCurvePlace.IsNegative && YSpeedCurvePlace <= LargestYSpeedCurvePlace)
            return;
        Falling = true;
        YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
    }
}
