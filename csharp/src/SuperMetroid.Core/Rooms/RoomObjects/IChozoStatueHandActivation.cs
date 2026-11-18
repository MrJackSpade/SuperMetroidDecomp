using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Starting a Chozo statue's hand sequence once Samus has triggered its hand (#2015).</summary>
public interface IChozoStatueHandActivation
{
    /// <summary>
    /// Starts the statue's hand sequence and applies the room-object work it requests at once in <paramref name="level"/>.
    /// </summary>
    void ActivateChozoStatueHand(RoomLevelData level);
}
