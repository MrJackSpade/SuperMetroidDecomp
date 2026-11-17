namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The speed at one mark of the shared quadratic enemy speed curve, used in either direction (#2157): the speed toward
/// increasing coordinates, negated at use for the way back.
/// </summary>
/// <param name="Speed">The speed toward increasing coordinates, rightward or downward, in pixels per frame.</param>
/// <remarks>
/// Several jump and orbit calculations read a mark's speed at a coarser precision: <see cref="CoarseToward"/> is the
/// speed in 256ths of a pixel, its finest 256ths of that dropped. Those calculations (Beetom's and the hoppers' climbs,
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

    /// <summary>
    /// The speed in the direction <paramref name="sign"/> names, in 256ths of a pixel per frame, negative toward
    /// decreasing coordinates.
    /// </summary>
    internal int CoarseToward(EnemySpeedSign sign) => Toward(sign).InTwoFiftySixths;
}
