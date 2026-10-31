namespace SuperMetroid.Core.Enemies;

/// <summary>The set, clear and test operations behaviours apply to an enemy's <see cref="EnemyProperties"/> and <see cref="EnemyExtraProperties"/>.</summary>
public static class EnemyPropertyFlagExtensions
{
    /// <summary>Whether any of <paramref name="flags"/> is set.</summary>
    public static bool HasAny(this EnemyProperties properties, EnemyProperties flags) => (properties & flags) != EnemyProperties.None;

    /// <summary>These properties with <paramref name="flags"/> set.</summary>
    public static EnemyProperties With(this EnemyProperties properties, EnemyProperties flags) => properties | flags;

    /// <summary>These properties with <paramref name="flags"/> cleared.</summary>
    public static EnemyProperties Without(this EnemyProperties properties, EnemyProperties flags) => properties & ~flags;

    /// <summary>These properties with <paramref name="clear"/> cleared, then <paramref name="set"/> set.</summary>
    public static EnemyProperties Replace(this EnemyProperties properties, EnemyProperties clear, EnemyProperties set) =>
        (properties & ~clear) | set;

    /// <summary>Whether any of <paramref name="flags"/> is set.</summary>
    public static bool HasAny(this EnemyExtraProperties properties, EnemyExtraProperties flags) =>
        (properties & flags) != EnemyExtraProperties.None;

    /// <summary>These additional properties with <paramref name="flags"/> set.</summary>
    public static EnemyExtraProperties With(this EnemyExtraProperties properties, EnemyExtraProperties flags) => properties | flags;

    /// <summary>These additional properties with <paramref name="flags"/> cleared.</summary>
    public static EnemyExtraProperties Without(this EnemyExtraProperties properties, EnemyExtraProperties flags) => properties & ~flags;
}
