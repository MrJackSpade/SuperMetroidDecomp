namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What an enemy's death leaves (#2558): nothing, with no roll made, or a pickup its drop chances roll for. An enemy that
/// drops nothing has no chances to roll, so none stand for its absence.
/// </summary>
public abstract record EnemyDrops
{
    /// <summary>Only the two cases below are drops.</summary>
    private EnemyDrops()
    {
    }

    /// <summary>The death drops nothing, and no roll is made.</summary>
    public static EnemyDrops Nothing { get; } = new DropsNothing();

    /// <summary>The death rolls <paramref name="chances"/> for its pickup.</summary>
    /// <param name="chances">The chances the roll reads.</param>
    public static EnemyDrops Rolling(EnemyDropChances chances) => new Rolled(chances);

    /// <summary>The death drops nothing.</summary>
    public sealed record DropsNothing : EnemyDrops;

    /// <summary>The death rolls <paramref name="Chances"/> for its pickup.</summary>
    /// <param name="Chances">The chances the roll reads.</param>
    public sealed record Rolled(EnemyDropChances Chances) : EnemyDrops;
}
