using static SuperMetroid.Core.Samus.Abilities.CrystalFlashPaletteTimingDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>How long each step of the Crystal Flash palette shows.</summary>
internal static class CrystalFlashPaletteTiming
{
    /// <summary>Returns the duration selected by an aligned byte offset into the records.</summary>
    /// <remarks>
    /// All ten timer words at <c>$91:DC02</c> plus a four-byte record stride
    /// are <c>$000A</c> in the pinned NTSC J/U v1.0 ROM. Thus every aligned
    /// offset from 0 through 36 returns ten after validation; the native
    /// handler advances and wraps that offset at 40. A complete 100-call
    /// production cycle matches with timer-word ROM reads forbidden.
    /// Investigation: #625 / #668.
    /// </remarks>
    public static ushort DurationForByteOffset(int byteOffset)
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
