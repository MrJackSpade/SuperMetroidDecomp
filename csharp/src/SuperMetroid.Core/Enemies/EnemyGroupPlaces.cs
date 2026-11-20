namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The places of a group the population load places (#2554): its head's occupant, which its place's binding installed, and
/// the places the room lists its parts in right after it, in listing order, before any part is placed.
/// </summary>
/// <param name="HeadOccupant">The head's occupant.</param>
/// <param name="Parts">The parts' places.</param>
internal sealed record EnemyGroupPlaces(RoomEnemyOccupant HeadOccupant, IReadOnlyList<RoomEnemyPlace> Parts)
{
    /// <summary>The head's place.</summary>
    internal RoomEnemyPlace Head => HeadOccupant.Place;
}
