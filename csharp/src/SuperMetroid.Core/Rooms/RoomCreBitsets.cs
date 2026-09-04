namespace SuperMetroid.Core.Rooms;

/// <summary>Named bits in a room header's common-room-element bitset.</summary>
public static class RoomCreBitsets
{
    /// <summary>
    /// Bit tested by door IRQ handlers $80:96F1/$80:9771/$80:97DA. If either adjacent
    /// room sets it, the transition disables BG1 below the HUD and displays OBJ alone.
    /// </summary>
    public const byte SuppressDoorTransitionBg1 = 0x01;
}
