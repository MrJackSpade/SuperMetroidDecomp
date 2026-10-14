namespace SuperMetroid.AssetExtraction.Compression;

/// <summary>Bit layout and bounds shared by every command in the cartridge format.</summary>
public static class SmCompressionCodes
{
    /// <summary><c>$FF</c>: ends a compressed stream; this value takes precedence over the long-header marker.</summary>
    public const byte Terminator = 0xff;
    /// <summary><c>$E0</c>: selects the upper three bits used to recognize a long command header.</summary>
    public const byte LongHeaderMarkerMask = 0xe0;
    /// <summary><c>$E0</c>: all three marker bits must be set to introduce a second length byte.</summary>
    public const byte LongHeaderMarker = 0xe0;
    /// <summary><c>$E0</c>: selects command bits in a short header or in the normalized long-header command.</summary>
    public const byte CommandMask = 0xe0;
    /// <summary><c>$1F</c>: selects the short header's five-bit output length minus one.</summary>
    public const byte ShortLengthMask = 0x1f;
    /// <summary><c>$03</c>: selects the high two bits of a long header's ten-bit output length minus one.</summary>
    public const byte LongLengthHighMask = 0x03;
    /// <summary>Left shift of three moves a long header's command field from bits 2-4 into normalized bits 5-7.</summary>
    public const int LongCommandShift = 3;
    /// <summary>Default decompression safety cap of 4 MiB, limiting expansion of malformed asset streams.</summary>
    public const int DefaultMaximumOutputBytes = 4 * 1024 * 1024;

    /// <summary>Checks whether a byte begins a two-byte header, excluding the <c>$FF</c> stream terminator.</summary>
    /// <param name="value">The first byte of a prospective command header.</param>
    /// <returns>True when bits 5-7 are set and the byte is not the terminator.</returns>
    public static bool IsLongHeader(byte value) =>
        value != Terminator &&
        (value & LongHeaderMarkerMask) == LongHeaderMarker;
}
