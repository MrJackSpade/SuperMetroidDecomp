using SuperMetroid.Core.Enemies.Chozo;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Starting a Chozo statue's hand sequence once Samus has triggered its hand (#2015).</summary>
public interface IChozoStatueHandActivation
{
    /// <summary>
    /// Starts the statue's hand sequence; returns the room objects it requests at once, which the statue's room objects
    /// spawn.
    /// </summary>
    IReadOnlyList<RoomObjectSpawnRequest<ChozoStatueRoomObject>> ActivateChozoStatueHand();
}
