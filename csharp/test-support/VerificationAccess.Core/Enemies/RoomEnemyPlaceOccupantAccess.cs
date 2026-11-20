namespace SuperMetroid.Core.Enemies;

/// <summary>A check's reach to the enemy a fixture place holds, which it set up before asking.</summary>
internal static class RoomEnemyPlaceOccupantAccess
{
    /// <summary>The enemy <paramref name="place"/> holds, which a check calling a species behaviour hands it with the place.</summary>
    /// <exception cref="InvalidOperationException">The check asks before binding an enemy in the place.</exception>
    internal static RoomEnemyOccupant HeldOccupant(this RoomEnemyPlace place) =>
        Ensure.NotNull(place).Occupant ?? throw new InvalidOperationException(
            $"A check asks for the enemy in place {place.Ordinal} before binding one there.");
}
