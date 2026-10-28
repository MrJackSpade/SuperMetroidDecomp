namespace SuperMetroid.Core.Enemies;

/// <summary>One immutable ordered bank-$A1 room population and its terminator quota.</summary>
public sealed class RoomEnemyPopulationDefinition
{
    /// <summary>Placement order captured from the bank-$A1 list before its terminator.</summary>
    private readonly RoomEnemyPopulationRecord[] records;
    /// <summary>Retains one compiled list's name, placement order, and trailing kill quota.</summary>
    internal RoomEnemyPopulationDefinition(string name, RoomEnemyPopulationRecord[] records, int deathQuota) =>
        (Name, this.records, DeathQuota) = (Ensure.NotNull(name), records, deathQuota);
    /// <summary>The list's name, the disassembly's label for it without its prefix.</summary>
    public string Name { get; }
    /// <summary>Read-only ordered placements compiled from native 16-byte records, excluding the $FFFF terminator; order controls initialization and slot allocation.</summary>
    public ReadOnlyMemory<RoomEnemyPopulationRecord> Records => records;
    /// <summary>Literal byte following the $FFFF terminator, used as the room kill-count requirement for nonempty populations; an empty load does not publish it. 0 through 255.</summary>
    public int DeathQuota { get; }
}
