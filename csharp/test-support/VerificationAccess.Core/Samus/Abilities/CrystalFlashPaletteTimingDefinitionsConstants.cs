namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Cartridge places of the Crystal Flash body pulse's timing that only verification reads; the port plays the pulse as a
/// named shade sequence (#2387).
/// </summary>
internal static class CrystalFlashPaletteTimingDefinitionsConstants
{
    /// <summary>
    /// Timer words in the ten four-byte pointer/timer records at $91:DC00-$91:DC27.
    /// The first timer is at $91:DC02 and subsequent timers have a four-byte stride.
    /// </summary>
    public const int NativeFirstTimerAddress = 0x91dc02;

    /// <summary>The bytes of one native pointer/timer record; the port plays the pulse's shades instead (#1512, #2387).</summary>
    public const int RecordByteCount = 4;

    /// <summary>The native pulse's records, one per body shade.</summary>
    public const int RecordCount = 10;
}
