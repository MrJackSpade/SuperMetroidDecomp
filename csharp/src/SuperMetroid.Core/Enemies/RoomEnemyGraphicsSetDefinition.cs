namespace SuperMetroid.Core.Enemies;

/// <summary>One immutable terminated bank-$B4 enemy graphics set.</summary>
/// <param name="name">The set's name, the disassembly's label for it without its prefix.</param>
/// <param name="records">The set's ordered records, retained without copying artwork pixels.</param>
public sealed class RoomEnemyGraphicsSetDefinition(string name, RoomEnemyGraphicsSetHeader[] records)
{
    /// <summary>Catalog-owned four-byte records in the order the engine stages their tiles.</summary>
    private readonly RoomEnemyGraphicsSetHeader[] records = records;
    /// <summary>The set's name, the disassembly's label for it without its prefix.</summary>
    public string Name { get; } = Ensure.NotNull(name);
    /// <summary>Read-only view of catalog-owned members in native staging order, excluding the $FFFF terminator and trailing metadata; duplicate source associations and palette sharing are retained.</summary>
    public ReadOnlyMemory<RoomEnemyGraphicsSetHeader> Records => records;
}
