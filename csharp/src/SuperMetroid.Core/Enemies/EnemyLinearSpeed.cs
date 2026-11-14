namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy's constant speed along one axis, used in either direction (#1804): the speed toward increasing
/// coordinates, negated at use for the way back.
/// </summary>
/// <param name="Speed">The speed toward increasing coordinates, rightward or downward, in pixels per frame.</param>
public readonly record struct EnemyLinearSpeed(SignedSixteenSixteen Speed)
{
    /// <summary>The speed toward increasing coordinates: rightward or downward.</summary>
    internal SignedSixteenSixteen Positive => Speed;

    /// <summary>The same speed toward decreasing coordinates: leftward or upward.</summary>
    internal SignedSixteenSixteen Negative => -Speed;
}
