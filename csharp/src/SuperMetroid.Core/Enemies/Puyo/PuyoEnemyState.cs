using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>The state of one Puyo.</summary>
public sealed class PuyoEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PuyoSegment> Playback { get; set; }

    /// <summary>
    /// The Puyo's place along the shared quadratic speed table, whose whole rows choose its vertical speed, from the first
    /// row through <see cref="LargestYSpeedCurvePlace"/>: a hop starts below its 96th row, and any step that would leave
    /// that range restarts the fall at the first row (<see cref="StepAirborneIndex"/>).
    /// </summary>
    public EnemyQuadraticSpeedIndex YSpeedCurvePlace { get; internal set; }

    /// <summary>
    /// The furthest place the Puyo's index keeps: a step past it, as past the first row, restarts the fall, as the game's
    /// check for a negative index does.
    /// </summary>
    internal static readonly EnemyQuadraticSpeedIndex LargestYSpeedCurvePlace =
        EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(short.MaxValue);

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

    /// <summary>The record selected for the next hop.</summary>
    public PuyoHopType HopType { get; internal set; }

    /// <summary>
    /// True means the hopping pose is still active, while false permits
    /// the grounded-animation handoff.
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
    public EnemyQuadraticSpeedIndex InitialYSpeedCurvePlaceThreeQuarters { get; internal set; }

    /// <summary>One-half point of the initial vertical speed index.</summary>
    public EnemyQuadraticSpeedIndex InitialYSpeedCurvePlaceHalf { get; internal set; }

    /// <summary>
    /// One airborne update's change to the speed index: rising lowers it by <paramref name="delta"/>, falling raises it.
    /// A step that leaves the first row through <see cref="LargestYSpeedCurvePlace"/> (the game checks whether it is negative) restarts
    /// the fall at the first row: the apex while rising, and a restart of an overlong fall.
    /// </summary>
    /// <param name="delta">The hop record's index step, at most three whole rows.</param>
    internal void StepAirborneIndex(EnemyQuadraticSpeedIndex delta)
    {
        YSpeedCurvePlace = Falling ? YSpeedCurvePlace + delta : YSpeedCurvePlace - delta;
        if (!YSpeedCurvePlace.IsNegative && YSpeedCurvePlace <= LargestYSpeedCurvePlace)
            return;
        Falling = true;
        YSpeedCurvePlace = EnemyQuadraticSpeedIndex.Start;
    }
}
