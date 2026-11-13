using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Conditions a room-state selector fixes itself rather than reads from room data.</summary>
public static class RoomStateSelectorConditions
{
    /// <summary>
    /// The main-area-boss-dead room state check always tests the area's main boss flag.
    /// </summary>
    public const BossBits MainAreaBoss = BossBits.AreaBoss;
}
