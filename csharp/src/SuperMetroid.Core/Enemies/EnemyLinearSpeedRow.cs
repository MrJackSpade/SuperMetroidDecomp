namespace SuperMetroid.Core.Enemies;

/// <summary>One row of the shared linear enemy speed table: a speed and its negation.</summary>
/// <param name="Positive">The speed toward increasing coordinates, rightward or downward, in pixels per frame.</param>
/// <param name="Negative">The same speed toward decreasing coordinates, leftward or upward.</param>
public readonly record struct EnemyLinearSpeedRow(SignedSixteenSixteen Positive, SignedSixteenSixteen Negative)
{
    /// <summary>The row's speed in the direction <paramref name="sign"/> names.</summary>
    internal SignedSixteenSixteen Toward(EnemySpeedSign sign) => sign switch
    {
        EnemySpeedSign.Positive => Positive,
        EnemySpeedSign.Negative => Negative,
        _ => throw new ArgumentOutOfRangeException(nameof(sign), sign, "Undefined enemy speed sign."),
    };
}
