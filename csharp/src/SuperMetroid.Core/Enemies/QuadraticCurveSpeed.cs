namespace SuperMetroid.Core.Enemies;

/// <summary>The speeds at one mark of the shared quadratic enemy speed curve: a speed and its negation.</summary>
/// <param name="Positive">The speed toward increasing coordinates, rightward or downward, in pixels per frame.</param>
/// <param name="Negative">The same speed toward decreasing coordinates, leftward or upward.</param>
/// <remarks>
/// Several jump and orbit calculations read a mark's speed at a coarser precision: <see cref="CoarseToward"/> is the
/// speed in 256ths of a pixel, its finest 256ths of that dropped. Those calculations (Beetom's and the hoppers' climbs,
/// <see cref="QuadraticCurveClimb"/>, Puyo's hop estimate, and Ki-Hunter wing orbits) choose their place along the curve by summing coarse speeds,
/// so the coarse value is the authored rule they follow, not an approximation of the full speed.
/// </remarks>
public readonly record struct QuadraticCurveSpeed(SignedSixteenSixteen Positive, SignedSixteenSixteen Negative)
{
    /// <summary>The speed in the direction <paramref name="sign"/> names.</summary>
    internal SignedSixteenSixteen Toward(EnemySpeedSign sign) => sign switch
    {
        EnemySpeedSign.Positive => Positive,
        EnemySpeedSign.Negative => Negative,
        _ => throw new ArgumentOutOfRangeException(nameof(sign), sign, "Undefined enemy speed sign."),
    };

    /// <summary>
    /// The speed in the direction <paramref name="sign"/> names, in 256ths of a pixel per frame, negative toward
    /// decreasing coordinates. Every mark's speed lies well within 128 pixels a frame, so this is exactly the game's
    /// 1/256-pixel word read signed.
    /// </summary>
    internal int CoarseToward(EnemySpeedSign sign) => Toward(sign).InTwoFiftySixths;
}
