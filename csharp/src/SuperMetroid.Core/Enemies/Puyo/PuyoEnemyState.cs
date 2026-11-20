using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>The state of one Puyo.</summary>
public sealed class PuyoEnemyState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal PuyoSpawnSettings Settings { get; }

    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal PuyoEnemyState(PuyoAction action, PuyoSpawnSettings settings)
    {
        Settings = Ensure.NotNull(settings);
        Action = action;
    }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PuyoSegment> Playback { get; set; }

    /// <summary>
    /// The Puyo's place along the shared quadratic speed curve, whose whole marks choose its vertical speed: a hop starts
    /// below its 96th mark and rises back to the curve's start, where it falls from (<see cref="StepAirbornePlace"/>).
    /// </summary>
    public QuadraticSpeedCurvePlace YSpeedCurvePlace { get; internal set; }

    /// <summary>
    /// The grounded updates left before the next hop, the one it hops on included: its placement's hop cooldown and the
    /// update it runs out on (<see cref="EnemyWaitDefinitions.RunOutUpdate"/>, #2281).
    /// </summary>
    public UpdateCountdown HopCooldownTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>Whether the Puyo is grounded, or airborne rising or falling (#2512).</summary>
    public PuyoAction Action { get; internal set; }

    /// <summary>The current hop's airborne behaviour, chosen as each hop starts.</summary>
    public PuyoAirborneMovement AirborneMovement { get; internal set; }

    /// <summary>
    /// The hop being made, whose definition drives the airborne motion; it is kept when a collision changes <see
    /// cref="HopType"/>.
    /// </summary>
    public PuyoHopType CurrentHop { get; internal set; }

    /// <summary>The mark selected for the next hop.</summary>
    public PuyoHopType HopType { get; internal set; }

    /// <summary>The direction of the current hop.</summary>
    public HorizontalDirection Direction { get; internal set; }

    /// <summary>
    /// The way the next hop goes after a wall, or a ceiling that turns it back, cut this one short: the way back from the
    /// hop it was making (#2450). None while the next hop aims at Samus; the hop that takes it clears it.
    /// </summary>
    public HorizontalDirection? TurnBack { get; internal set; }

    /// <summary>Three-quarter point of the initial vertical speed-curve place.</summary>
    public QuadraticSpeedCurvePlace InitialYSpeedCurvePlaceThreeQuarters { get; internal set; }

    /// <summary>One-half point of the initial vertical speed-curve place.</summary>
    public QuadraticSpeedCurvePlace InitialYSpeedCurvePlaceHalf { get; internal set; }

    /// <summary>
    /// One airborne update's change to the speed-curve place: rising lowers it by <paramref name="delta"/>, and a rise
    /// past the curve's start is the hop's apex, where it falls from the start; falling raises it, and an overlong fall
    /// starts again from rest (<see cref="PuyoFallRestart"/>).
    /// </summary>
    /// <param name="delta">The hop's step along the curve, at most three whole marks.</param>
    /// <exception cref="InvalidOperationException">The Puyo is grounded.</exception>
    internal void StepAirbornePlace(QuadraticSpeedCurvePlace delta)
    {
        switch (Action)
        {
            case PuyoAction.Falling:
                YSpeedCurvePlace += delta;
                if (PuyoFallRestart.StartsOver(YSpeedCurvePlace.WholeMarks))
                    YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
                return;
            case PuyoAction.Rising:
                YSpeedCurvePlace -= delta;
                if (!YSpeedCurvePlace.IsNegative)
                    return;
                Action = PuyoAction.Falling;
                YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
                return;
            case PuyoAction.Grounded:
                throw new InvalidOperationException("A grounded Puyo has no airborne place to step.");
            default:
                throw new InvalidDataException($"Undefined {nameof(PuyoAction)} {Action}.");
        }
    }
}
