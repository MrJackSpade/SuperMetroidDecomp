namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The set, clear and test operations behaviours apply to an enemy's <see cref="EnemyContact"/>,
/// <see cref="EnemyProcessing"/> and <see cref="EnemyPresence"/>.
/// </summary>
public static class EnemyStateFlagExtensions
{
    /// <summary>Whether any of <paramref name="flags"/> is set.</summary>
    public static bool HasAny(this EnemyContact contact, EnemyContact flags) => (contact & flags) != EnemyContact.None;

    /// <summary>This contact with <paramref name="flags"/> set.</summary>
    public static EnemyContact With(this EnemyContact contact, EnemyContact flags) => contact | flags;

    /// <summary>This contact with <paramref name="flags"/> cleared.</summary>
    public static EnemyContact Without(this EnemyContact contact, EnemyContact flags) => contact & ~flags;

    /// <summary>This contact with <paramref name="clear"/> cleared, then <paramref name="set"/> set.</summary>
    public static EnemyContact Replace(this EnemyContact contact, EnemyContact clear, EnemyContact set) => (contact & ~clear) | set;

    /// <summary>Whether any of <paramref name="flags"/> is set.</summary>
    public static bool HasAny(this EnemyProcessing processing, EnemyProcessing flags) => (processing & flags) != EnemyProcessing.None;

    /// <summary>This processing with <paramref name="flags"/> set.</summary>
    public static EnemyProcessing With(this EnemyProcessing processing, EnemyProcessing flags) => processing | flags;

    /// <summary>This processing with <paramref name="flags"/> cleared.</summary>
    public static EnemyProcessing Without(this EnemyProcessing processing, EnemyProcessing flags) => processing & ~flags;

    /// <summary>This processing with <paramref name="clear"/> cleared, then <paramref name="set"/> set.</summary>
    public static EnemyProcessing Replace(this EnemyProcessing processing, EnemyProcessing clear, EnemyProcessing set) =>
        (processing & ~clear) | set;

    /// <summary>Whether any of <paramref name="flags"/> is set.</summary>
    public static bool HasAny(this EnemyPresence presence, EnemyPresence flags) => (presence & flags) != EnemyPresence.None;

    /// <summary>This presence with <paramref name="flags"/> set.</summary>
    public static EnemyPresence With(this EnemyPresence presence, EnemyPresence flags) => presence | flags;

    /// <summary>This presence with <paramref name="flags"/> cleared.</summary>
    public static EnemyPresence Without(this EnemyPresence presence, EnemyPresence flags) => presence & ~flags;

    /// <summary>This presence with <paramref name="clear"/> cleared, then <paramref name="set"/> set.</summary>
    public static EnemyPresence Replace(this EnemyPresence presence, EnemyPresence clear, EnemyPresence set) =>
        (presence & ~clear) | set;
}
