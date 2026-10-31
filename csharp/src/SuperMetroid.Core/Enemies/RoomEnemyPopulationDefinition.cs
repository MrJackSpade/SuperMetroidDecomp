namespace SuperMetroid.Core.Enemies;

/// <summary>One room state's ordered enemy population and the kills that clear the room.</summary>
/// <param name="name">The list's name, the disassembly's label for it without its prefix.</param>
/// <param name="records">The list's placements in order.</param>
/// <param name="deathQuota">The kills, 0 through 255, that clear a room with a nonempty population.</param>
public sealed class RoomEnemyPopulationDefinition(string name, RoomEnemyPopulationRecord[] records, int deathQuota)
{
    /// <summary>The placements in order.</summary>
    private readonly RoomEnemyPopulationRecord[] placements = records;
    /// <summary>The list's name, the disassembly's label for it without its prefix.</summary>
    public string Name { get; } = Ensure.NotNull(name);
    /// <summary>The placements in order; the order controls initialization and slot allocation.</summary>
    public ReadOnlyMemory<RoomEnemyPopulationRecord> Records => placements;
    /// <summary>The kills, 0 through 255, that clear the room when the population is not empty; an empty load does not publish it.</summary>
    public int DeathQuota { get; } = Ensure.BetweenInclusive(0, byte.MaxValue, deathQuota);
}
