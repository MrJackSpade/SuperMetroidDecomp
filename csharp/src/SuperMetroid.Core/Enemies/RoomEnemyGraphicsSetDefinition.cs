namespace SuperMetroid.Core.Enemies;

/// <summary>One immutable enemy graphics set: an ordered list of the members whose installed sheets its rooms draw from.</summary>
/// <param name="name">The set's name.</param>
/// <param name="records">The set's ordered members.</param>
public sealed class RoomEnemyGraphicsSetDefinition(string name, RoomEnemyGraphicsSetMember[] records)
{
    /// <summary>Catalog-owned members in the set's order.</summary>
    private readonly RoomEnemyGraphicsSetMember[] members = records;
    /// <summary>The set's name.</summary>
    public string Name { get; } = Ensure.NotNull(name);
    /// <summary>Read-only view of catalog-owned members in the set's order; a member listed twice is retained twice.</summary>
    public ReadOnlyMemory<RoomEnemyGraphicsSetMember> Records => members;
}
