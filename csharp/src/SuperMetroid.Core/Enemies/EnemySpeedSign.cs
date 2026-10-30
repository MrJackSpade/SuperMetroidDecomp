namespace SuperMetroid.Core.Enemies;

/// <summary>Which of a shared speed-table row's two speeds a movement takes.</summary>
internal enum EnemySpeedSign
{
    /// <summary>The row's speed toward increasing coordinates: rightward or downward.</summary>
    Positive,

    /// <summary>The row's speed toward decreasing coordinates: leftward or upward.</summary>
    Negative,
}
