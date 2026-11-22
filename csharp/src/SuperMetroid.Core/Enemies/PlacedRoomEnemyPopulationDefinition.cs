namespace SuperMetroid.Core.Enemies;

/// <summary>A room state's enemy population that places enemies, and the kills that clear its room.</summary>
/// <param name="name">The list's name.</param>
/// <param name="placements">The list's placements in order, at least one; a list that places none is an <see cref="EmptyRoomEnemyPopulationDefinition"/>.</param>
/// <param name="deathQuota">The kills, zero or more, that clear the room.</param>
public sealed partial class PlacedRoomEnemyPopulationDefinition([InstalledKey] string name, RoomEnemyPlacement[] placements, int deathQuota)
    : RoomEnemyPopulationDefinition(name, Ensure.NotEmpty(placements))
{
    /// <summary>
    /// The kills that clear the room for its grey doors and clearance watches, which loading the population publishes once
    /// every placement has been initialized.
    /// </summary>
    public int DeathQuota { get; } = Ensure.AtLeastZero(deathQuota);
}
