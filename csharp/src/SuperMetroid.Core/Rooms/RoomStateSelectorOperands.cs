using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Fixed operands embedded by selectors rather than by room data.</summary>
public static class RoomStateSelectorOperands
{
    /// <summary>
    /// The main-area-boss-dead room state check always tests the area's main boss flag.
    /// </summary>
    public const BossBits MainAreaBoss = BossBits.AreaBoss;
}
