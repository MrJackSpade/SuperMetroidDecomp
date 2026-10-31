namespace SuperMetroid.Core.Enemies;

/// <summary>One row of the shared quadratic enemy speed table: a speed and its negation.</summary>
/// <param name="Positive">
/// The speed toward increasing coordinates, rightward or downward, as a signed 16.16 displacement per frame.
/// </param>
/// <param name="Negative">The same speed toward decreasing coordinates, leftward or upward.</param>
/// <remarks>
/// Several jump and orbit calculations read a row at a coarser precision: <see cref="CoarseToward"/> is the speed in
/// 256ths of a pixel, the finest fraction byte dropped. Those calculations (Beetom's and the hoppers' jump heights,
/// Puyo's hop estimate, and Ki-Hunter wing orbits) choose their table position by summing coarse speeds, so the
/// coarse value is the authored rule they follow, not an approximation of the full speed.
/// </remarks>
public readonly record struct EnemyQuadraticSpeedRow(int Positive, int Negative)
{
    /// <summary>The row's speed in the direction <paramref name="sign"/> names.</summary>
    internal int Toward(EnemySpeedSign sign) => sign switch
    {
        EnemySpeedSign.Positive => Positive,
        EnemySpeedSign.Negative => Negative,
        _ => throw new ArgumentOutOfRangeException(nameof(sign), sign, "Undefined enemy speed sign."),
    };

    /// <summary>
    /// The row's speed in the direction <paramref name="sign"/> names, in 256ths of a pixel per frame, as an unsigned
    /// sixteen-bit 8.8 value: the whole pixels' low byte over the fraction's high byte.
    /// </summary>
    internal int CoarseToward(EnemySpeedSign sign)
    {
        int speed = Toward(sign);
        return WordWrap.Unsigned(EightEight.FromSixteenSixteen(SixteenSixteen.WholeOf(speed), SixteenSixteen.FractionOf(speed)));
    }
}
