namespace SuperMetroid.Core.Enemies;

/// <summary>A room state's enemy population that places enemies, and the kills that clear its room.</summary>
public sealed class PlacedRoomEnemyPopulationDefinition : RoomEnemyPopulationDefinition
{
    /// <summary>Names the list and holds its placements, at least one, and the kills that clear its room.</summary>
    /// <param name="name">The list's name.</param>
    /// <param name="placements">The list's placements in order, at least one.</param>
    /// <param name="deathQuota">The kills, zero or more, that clear the room.</param>
    /// <exception cref="ArgumentException"><paramref name="placements"/> places no enemy.</exception>
    public PlacedRoomEnemyPopulationDefinition([InstalledKey] string name, RoomEnemyPlacement[] placements, int deathQuota)
        : base(name, AtLeastOne(placements))
    {
        DeathQuota = Ensure.AtLeastZero(deathQuota);
    }

    /// <summary>
    /// The kills that clear the room for its grey doors and clearance watches, which loading the population publishes once
    /// every placement has been initialized.
    /// </summary>
    public int DeathQuota { get; }

    /// <summary>Requires a list that places at least one enemy; a list that places none is an <see cref="EmptyRoomEnemyPopulationDefinition"/>.</summary>
    private static RoomEnemyPlacement[] AtLeastOne(RoomEnemyPlacement[] placements) =>
        Ensure.NotNull(placements).Length > 0
            ? placements
            : throw new ArgumentException("A placed population places at least one enemy.", nameof(placements));
}
