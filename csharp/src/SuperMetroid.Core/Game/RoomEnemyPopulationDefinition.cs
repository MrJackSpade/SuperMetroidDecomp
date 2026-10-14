namespace SuperMetroid.Core.Game;

/// <summary>One immutable ordered bank-$A1 room population and its terminator quota.</summary>
public sealed class RoomEnemyPopulationDefinition
{
    /// <summary>Placement order captured from the bank-$A1 list before its terminator.</summary>
    private readonly RoomEnemyPopulationRecord[] records;
    /// <summary>Retains one compiled list's native identity, placement order, and trailing kill quota.</summary>
    internal RoomEnemyPopulationDefinition(ushort pointer, RoomEnemyPopulationRecord[] records, byte deathQuota)
    {
        Pointer = pointer;
        this.records = records;
        DeathQuota = deathQuota;
    }
    /// <summary>Native 16-bit list pointer within bank $A1; identifies the population without granting cartridge-read access.</summary>
    public ushort Pointer { get; }
    /// <summary>Read-only ordered placements compiled from native 16-byte records, excluding the $FFFF terminator; order controls initialization and slot allocation.</summary>
    public ReadOnlyMemory<RoomEnemyPopulationRecord> Records => records;
    /// <summary>Literal byte following the $FFFF terminator, used as the room kill-count requirement for nonempty populations; an empty load does not publish this byte.</summary>
    public byte DeathQuota { get; }
}
