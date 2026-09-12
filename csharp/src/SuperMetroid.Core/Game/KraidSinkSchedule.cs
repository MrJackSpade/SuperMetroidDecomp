namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanical callbacks in Kraid's sinking schedule.</summary>
internal static class KraidSinkSchedule
{
    /// <summary>
    /// $A7:C5E7..C68F, ShrinkingKraidTable: twenty-eight rows spaced eight pixels
    /// apart, terminated by FFFF. Six rows crumble platforms; the remaining rows
    /// invoke the distinct empty RTS. Tilemap offsets are presentation data.
    /// </summary>
    public static ushort? CallbackAt(ushort y)
    {
        if (y < 0x130 || y > 0x208 || (y & 7) != 0)
            return null;
        return y switch
        {
            0x130 => KraidSinkCallbacks.CrumbleLeftPlatformLeft,
            0x148 => KraidSinkCallbacks.CrumbleRightPlatformMiddle,
            0x160 => KraidSinkCallbacks.CrumbleRightPlatformLeft,
            0x180 => KraidSinkCallbacks.CrumbleLeftPlatformRight,
            0x198 => KraidSinkCallbacks.CrumbleLeftPlatformMiddle,
            0x1b0 => KraidSinkCallbacks.CrumbleRightPlatformRight,
            _ => KraidSinkCallbacks.NoOperation,
        };
    }
}
