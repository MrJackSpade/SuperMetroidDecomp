namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The speed at one mark of the shared quadratic enemy speed curve, used in either direction (#2157): the speed toward
/// increasing coordinates, negated at use for the way back.
/// </summary>
/// <param name="Speed">The speed toward increasing coordinates, rightward or downward, in pixels per frame.</param>
/// <remarks>
/// Several jump and orbit calculations choose their place along the curve by summing a mark's coarse speed
/// (<see cref="CoarseRise"/>), the speed kept to a 256th of a pixel: Beetom's and the hoppers'
/// climbs, <see cref="QuadraticCurveClimb"/>, Puyo's hop estimate, and Ki-Hunter wing orbits.
/// </remarks>
public readonly record struct QuadraticCurveSpeed(SignedSixteenSixteen Speed)
{
    /// <summary>The speed toward increasing coordinates: rightward or downward.</summary>
    internal SignedSixteenSixteen Positive => Speed;

    /// <summary>The same speed toward decreasing coordinates: leftward or upward.</summary>
    internal SignedSixteenSixteen Negative => -Speed;

    /// <summary>The speed toward <paramref name="direction"/> (#2473): as it is downward, reversed upward.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is neither up nor down.</exception>
    internal SignedSixteenSixteen Toward(VerticalDirection direction) => Speed.Toward(direction);

    /// <summary>The speed toward <paramref name="direction"/> (#2569): as it is rightward, reversed leftward.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is neither left nor right.</exception>
    internal SignedSixteenSixteen Toward(HorizontalDirection direction) => Speed.Toward(direction);

    /// <summary>
    /// How far the coarse speed toward increasing coordinates carries a mover in one update (#2537): the speed with its
    /// finest fraction dropped toward decreasing coordinates. A climb's search sums it for each place it passes
    /// (<see cref="QuadraticCurveClimb"/>, the Puyo's hop).
    /// </summary>
    internal SignedSixteenSixteen CoarseRise => Speed.RoundedDownToTwoFiftySixths();

    /// <summary>
    /// The turn an orbit takes each update that turns one step for each pixel of the coarse speed, toward
    /// <paramref name="direction"/> (#2439, #2569), as a Ki-Hunter's detached wing orbits: a 256th of a step for each 256th of
    /// a pixel, counterclockwise as the screen shows it toward increasing trigonometric angles and clockwise toward
    /// decreasing ones, the speed's fraction finer than a 256th dropped after it is turned that way.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is neither clockwise nor counterclockwise.</exception>
    internal AngularSpeed TurnOfAStepAPixelToward(RotationDirection direction)
    {
        SignedSixteenSixteen speed = direction switch
        {
            RotationDirection.Counterclockwise => Speed,
            RotationDirection.Clockwise => -Speed,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Not a rotation direction."),
        };
        return AngularSpeed.StepPerPixelOf(speed.RoundedDownToTwoFiftySixths());
    }
}
