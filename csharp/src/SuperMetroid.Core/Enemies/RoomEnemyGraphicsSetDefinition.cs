namespace SuperMetroid.Core.Enemies;

/// <summary>One immutable terminated bank-$B4 enemy graphics set.</summary>
public sealed class RoomEnemyGraphicsSetDefinition
{
    /// <summary>Catalog-owned four-byte records in the order the engine stages their tiles.</summary>
    private readonly RoomEnemyGraphicsSetHeader[] records;
    /// <summary>Retains one graphics set's name and its ordered records without copying artwork pixels.</summary>
    internal RoomEnemyGraphicsSetDefinition(string name, RoomEnemyGraphicsSetHeader[] records) =>
        (Name, this.records) = (Ensure.NotNull(name), records);
    /// <summary>The set's name, the disassembly's label for it without its prefix.</summary>
    public string Name { get; }
    /// <summary>Read-only view of catalog-owned members in native staging order, excluding the $FFFF terminator and trailing metadata; duplicate source associations and palette sharing are retained.</summary>
    public ReadOnlyMemory<RoomEnemyGraphicsSetHeader> Records => records;
}
