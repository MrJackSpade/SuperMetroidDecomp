using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Fixed operands embedded by selector routines rather than by room data.</summary>
public static class RoomStateSelectorOperands
{
    /// <summary>
    /// <c>RoomStateCheck_MainAreaBossIsDead</c> always tests bit zero of the area's boss byte.
    /// </summary>
    public const BossBits MainAreaBoss = BossBits.AreaBoss;
}
