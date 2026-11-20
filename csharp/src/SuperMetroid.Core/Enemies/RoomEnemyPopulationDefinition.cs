namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One room state's ordered enemy population: a <see cref="PlacedRoomEnemyPopulationDefinition"/>, which places enemies and
/// states the kills that clear its room, or an <see cref="EmptyRoomEnemyPopulationDefinition"/>, which places none and has
/// no quota.
/// </summary>
public abstract class RoomEnemyPopulationDefinition
{
    /// <summary>The placements in order.</summary>
    private readonly RoomEnemyPlacement[] placements;

    /// <summary>Names the list and holds its placements in order.</summary>
    /// <param name="name">The list's name.</param>
    /// <param name="placements">The list's placements in order.</param>
    private protected RoomEnemyPopulationDefinition([InstalledKey] string name, RoomEnemyPlacement[] placements)
    {
        Name = Ensure.NotNull(name);
        this.placements = Ensure.NotNull(placements);
    }

    /// <summary>The list's name.</summary>
    public string Name { get; }
    /// <summary>The placements in order; the order controls initialization and place allocation.</summary>
    public ReadOnlyMemory<RoomEnemyPlacement> Placements => placements;
}
