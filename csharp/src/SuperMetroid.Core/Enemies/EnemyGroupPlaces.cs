namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The places of a group the population load places (#2554): its head's, already holding its identity and entry, and the
/// places the room lists its parts in right after it, in listing order, before any part is placed.
/// </summary>
/// <param name="Head">The head's place.</param>
/// <param name="Parts">The parts' places.</param>
internal sealed record EnemyGroupPlaces(RoomEnemyPlace Head, IReadOnlyList<RoomEnemyPlace> Parts);
