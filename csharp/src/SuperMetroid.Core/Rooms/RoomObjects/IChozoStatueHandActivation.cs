using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Starting a Chozo statue's hand sequence from the hand block Samus landed on (#2015).</summary>
public interface IChozoStatueHandActivation
{
    /// <summary>
    /// Starts the statue's hand sequence from <paramref name="handBlock"/> and applies the room-object work it requests
    /// at once.
    /// </summary>
    void ActivateChozoStatueHand(RoomLevelData level, Position16<RoomBlocks> handBlock);
}
