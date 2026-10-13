namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanical callbacks in Kraid's sinking schedule.</summary>
internal static class KraidSinkSchedule
{
    /// <summary>
    /// $A7:C5E7..C68F, ShrinkingKraidTable: twenty-eight rows spaced eight pixels
    /// apart, terminated by FFFF. Six rows crumble platforms; the remaining rows
    /// invoke the distinct empty RTS. Tilemap offsets are presentation data.
    /// </summary>
    public static KraidSinkCallback? CallbackAt(ushort y)
    {
        if (y < 304 || y > 520 || (y & 7) != 0)
            return null;
        return y switch
        {
            304 => KraidSinkCallback.CrumbleLeftPlatformLeft,
            328 => KraidSinkCallback.CrumbleRightPlatformMiddle,
            352 => KraidSinkCallback.CrumbleRightPlatformLeft,
            384 => KraidSinkCallback.CrumbleLeftPlatformRight,
            408 => KraidSinkCallback.CrumbleLeftPlatformMiddle,
            432 => KraidSinkCallback.CrumbleRightPlatformRight,
            _ => KraidSinkCallback.NoOperation,
        };
    }
}
