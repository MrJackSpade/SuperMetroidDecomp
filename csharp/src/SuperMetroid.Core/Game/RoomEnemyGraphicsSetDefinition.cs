namespace SuperMetroid.Core.Game;

/// <summary>One immutable terminated bank-$B4 enemy graphics set.</summary>
public sealed class RoomEnemyGraphicsSetDefinition
{
    /// <summary>Catalog-owned four-byte records in the order the engine stages their tiles.</summary>
    private readonly RoomEnemyGraphicsSetHeader[] records;
    /// <summary>Retains one bank-$B4 list identity and its ordered records without copying artwork pixels.</summary>
    internal RoomEnemyGraphicsSetDefinition(ushort pointer, RoomEnemyGraphicsSetHeader[] records)
    {
        Pointer = pointer;
        this.records = records;
    }
    /// <summary>Native 16-bit list identity in bank $B4, beginning at its first four-byte member record or the $FFFF terminator for an empty set.</summary>
    public ushort Pointer { get; }
    /// <summary>Read-only view of catalog-owned members in native staging order, excluding the $FFFF terminator and trailing metadata; duplicate source associations and palette sharing are retained.</summary>
    public ReadOnlyMemory<RoomEnemyGraphicsSetHeader> Records => records;
}
