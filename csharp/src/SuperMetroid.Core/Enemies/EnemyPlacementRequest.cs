using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An occupant being placed (#2554): the occupant its place's binding installed, how it comes to be set up, and the places
/// the room's population lists right after it, from which a group's head takes its parts' places.
/// </summary>
/// <param name="Occupant">The occupant being placed, which names its place, identity and entry.</param>
/// <param name="Setup">How the occupant comes to be set up.</param>
/// <param name="ListedAfter">The places the room's population lists right after the occupant; empty for a spawn.</param>
[RoomScopeBoundary("A per-call input, built for one placement and never kept.")]
internal sealed record EnemyPlacementRequest(RoomEnemyOccupant Occupant, EnemySetup Setup, IReadOnlyList<RoomEnemyPlace> ListedAfter)
{
    /// <summary>The occupant's place.</summary>
    internal RoomEnemyPlace Place => Occupant.Place;
}
