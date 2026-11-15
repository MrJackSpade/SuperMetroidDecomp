namespace SuperMetroid.Core.Enemies;

/// <summary>One room state's ordered enemy population and the kills that clear the room.</summary>
/// <param name="name">The list's name.</param>
/// <param name="placements">The list's placements in order.</param>
/// <param name="deathQuota">The kills, zero or more, that clear a room whose population places enemies.</param>
public sealed class RoomEnemyPopulationDefinition(string name, RoomEnemyPlacement[] placements, int deathQuota)
{
    /// <summary>The placements in order.</summary>
    private readonly RoomEnemyPlacement[] placements = placements;
    /// <summary>The list's name.</summary>
    public string Name { get; } = Ensure.NotNull(name);
    /// <summary>The placements in order; the order controls initialization and slot allocation.</summary>
    public ReadOnlyMemory<RoomEnemyPlacement> Placements => placements;
    /// <summary>The kills that clear the room when the population is not empty; an empty load does not publish it.</summary>
    public int DeathQuota { get; } = Ensure.AtLeastZero(deathQuota);
}
