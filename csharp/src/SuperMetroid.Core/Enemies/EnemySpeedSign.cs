namespace SuperMetroid.Core.Enemies;

/// <summary>Which way a movement applies a shared speed: as it is, or negated.</summary>
internal enum EnemySpeedSign
{
    /// <summary>The speed as it is, toward increasing coordinates: rightward or downward.</summary>
    Positive,

    /// <summary>The speed negated, toward decreasing coordinates: leftward or upward.</summary>
    Negative,
}
