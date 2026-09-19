namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing metadata for the Crystal Flash body-palette cycle. Palette pointers
/// remain presentation data; this catalog owns only the fixed delays interleaved with
/// those pointers by <c>HandleCrystalFlashPalette</c> at <c>$91:D9B2</c>.
/// </summary>
internal static class CrystalFlashPaletteTimingDefinitions
{
    /// <summary>
    /// Timer words in the ten four-byte pointer/timer records at $91:DC00-$91:DC27.
    /// The first timer is at $91:DC02 and subsequent timers have a four-byte stride.
    /// </summary>
    public const int NativeFirstTimerAddress = 0x91dc02;

    /// <summary>Number of authored Crystal Flash body-palette timing records.</summary>
    public const ushort RecordCount = 10;

    /// <summary>Bytes occupied by each interleaved palette-pointer/timer record.</summary>
    public const ushort RecordByteCount = 4;

    /// <summary>Every authored body-palette record remains active for ten handler calls.</summary>
    public const ushort AuthoredDuration = 10;

    /// <summary>Returns the duration selected by an aligned byte offset into the records.</summary>
    public static ushort DurationForByteOffset(ushort byteOffset)
    {
        if (byteOffset % RecordByteCount != 0 ||
            byteOffset >= RecordCount * RecordByteCount)
        {
            throw new InvalidDataException(
                $"Crystal Flash palette timing offset {byteOffset} is not one of the " +
                $"{RecordCount} authored {RecordByteCount}-byte records.");
        }

        return AuthoredDuration;
    }
}
