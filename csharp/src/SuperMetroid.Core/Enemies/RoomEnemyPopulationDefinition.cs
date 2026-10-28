namespace SuperMetroid.Core.Enemies;

/// <summary>One immutable ordered bank-$A1 room population and its terminator quota.</summary>
/// <param name="name">The list's name, the disassembly's label for it without its prefix.</param>
/// <param name="records">The list's placements in order.</param>
/// <param name="deathQuota">The list's trailing kill quota.</param>
public sealed class RoomEnemyPopulationDefinition(string name, RoomEnemyPopulationRecord[] records, int deathQuota)
{
    /// <summary>Placement order captured from the bank-$A1 list before its terminator.</summary>
    private readonly RoomEnemyPopulationRecord[] placements = records;
    /// <summary>The list's name, the disassembly's label for it without its prefix.</summary>
    public string Name { get; } = Ensure.NotNull(name);
    /// <summary>Read-only ordered placements compiled from native 16-byte records, excluding the $FFFF terminator; order controls initialization and slot allocation.</summary>
    public ReadOnlyMemory<RoomEnemyPopulationRecord> Records => placements;
    /// <summary>Literal byte following the $FFFF terminator, used as the room kill-count requirement for nonempty populations; an empty load does not publish it. 0 through 255.</summary>
    public int DeathQuota { get; } = deathQuota;
}
