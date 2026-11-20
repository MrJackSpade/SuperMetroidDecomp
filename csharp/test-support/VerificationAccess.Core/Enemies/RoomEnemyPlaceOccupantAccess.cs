namespace SuperMetroid.Core.Enemies;

/// <summary>A check's reach to the enemy a fixture place holds, which it set up before asking.</summary>
internal static class RoomEnemyPlaceOccupantAccess
{
    /// <summary>
    /// The enemy <paramref name="place"/> holds, which a check calling a species behaviour hands it with the place. A check
    /// that left the place unbound, using it only as the source its behaviour spawns from, gets the fixtures' stand-in
    /// (<see cref="EnemyPlaceFixtures.StandIn"/>) bound there, which runs nothing of its own.
    /// </summary>
    internal static RoomEnemyOccupant HeldOccupant(this RoomEnemyPlace place)
    {
        if (Ensure.NotNull(place).Occupant is { } held)
            return held;
        place.Bind(EnemyPlaceFixtures.StandIn, place.Definition);
        return place.Occupant ?? throw new InvalidOperationException($"Binding the stand-in in enemy place {place.Ordinal} left it empty.");
    }
}
