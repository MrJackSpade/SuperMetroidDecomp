namespace SuperMetroid.Core.Enemies;

/// <summary>A check's reach to the enemy a fixture place holds, which it set up before asking.</summary>
internal static class RoomEnemyPlaceOccupantAccess
{
    /// <summary>
    /// The enemy <paramref name="place"/> holds, which a check calling a species behaviour hands it with the place. A check
    /// that left the place unbound, using it only as the source its behaviour spawns from, gets the fixtures' stand-in
    /// (<see cref="EnemyPlaceFixtures.StandIn"/>) as an occupant no place holds, so the place and the states the check
    /// installed there stay as they are.
    /// </summary>
    internal static RoomEnemyOccupant HeldOccupant(this RoomEnemyPlace place) =>
        Ensure.NotNull(place).Occupant ?? EnemyOccupantFixtures.Of(EnemyPlaceFixtures.StandIn);
}
