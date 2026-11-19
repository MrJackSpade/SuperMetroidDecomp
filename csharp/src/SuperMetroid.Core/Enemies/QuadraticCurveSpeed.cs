namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The speed at one mark of the shared quadratic enemy speed curve, used in either direction (#2157): the speed toward
/// increasing coordinates, negated at use for the way back.
/// </summary>
/// <param name="Speed">The speed toward increasing coordinates, rightward or downward, in pixels per frame.</param>
/// <remarks>
/// Several jump and orbit calculations read a mark's speed at a coarser precision: <see cref="CoarseToward"/> is the
/// speed to a 256th of a pixel, its finer fraction dropped. Those calculations (Beetom's and the hoppers' climbs,
/// <see cref="QuadraticCurveClimb"/>, Puyo's hop estimate, and Ki-Hunter wing orbits) choose their place along the curve by summing coarse speeds,
/// so the coarse value is the authored rule they follow, not an approximation of the full speed.
/// </remarks>
public readonly record struct QuadraticCurveSpeed(SignedSixteenSixteen Speed)
{
    /// <summary>The speed toward increasing coordinates: rightward or downward.</summary>
    internal SignedSixteenSixteen Positive => Speed;

    /// <summary>The same speed toward decreasing coordinates: leftward or upward.</summary>
    internal SignedSixteenSixteen Negative => -Speed;

    /// <summary>The speed in the direction <paramref name="sign"/> names: as it is, or negated.</summary>
    internal SignedSixteenSixteen Toward(EnemySpeedSign sign) => sign switch
    {
        EnemySpeedSign.Positive => Speed,
        EnemySpeedSign.Negative => -Speed,
        _ => throw new ArgumentOutOfRangeException(nameof(sign), sign, "Undefined enemy speed sign."),
    };

    /// <summary>The speed toward <paramref name="direction"/> (#2473): as it is downward, negated upward.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is neither up nor down.</exception>
    internal SignedSixteenSixteen Toward(VerticalDirection direction) => direction switch
    {
        VerticalDirection.Down => Speed,
        VerticalDirection.Up => -Speed,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Not a vertical direction."),
    };

    /// <summary>
    /// The coarse speed in the direction <paramref name="sign"/> names: the speed with its fraction finer than a 256th of a
    /// pixel dropped toward decreasing coordinates.
    /// </summary>
    internal SignedSixteenSixteen CoarseToward(EnemySpeedSign sign) => Toward(sign).RoundedDownToTwoFiftySixths();

    /// <summary>
    /// How far the coarse speed toward increasing coordinates carries a mover in one update (#2537): the speed with its
    /// finest fraction dropped, as <see cref="CoarseToward"/> drops it. A climb's search sums it for each place it passes
    /// (<see cref="QuadraticCurveClimb"/>, the Puyo's hop).
    /// </summary>
    internal SignedSixteenSixteen CoarseRise => Speed.RoundedDownToTwoFiftySixths();

    /// <summary>
    /// The turn an orbit takes each update that turns one step for each pixel of the coarse speed in the direction
    /// <paramref name="sign"/> names (#2439), as a Ki-Hunter's detached wing orbits: a 256th of a step for each 256th of a
    /// pixel, back toward decreasing angles for a speed toward decreasing coordinates.
    /// </summary>
    internal AngularSpeed TurnOfAStepAPixelToward(EnemySpeedSign sign) => AngularSpeed.StepPerPixelOf(CoarseToward(sign));
}
