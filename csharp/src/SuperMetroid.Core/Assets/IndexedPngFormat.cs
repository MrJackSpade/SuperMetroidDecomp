namespace SuperMetroid.Core.Assets;

/// <summary>PNG indexed-image wire values and bounded asset limits; no cartridge addresses.</summary>
internal static class IndexedPngFormat
{
    /// <summary>The eight-byte PNG signature, including its line-ending check bytes.</summary>
    internal static ReadOnlySpan<byte> Signature => [137, 80, 78, 71, 13, 10, 26, 10];
    /// <summary>Required byte count of an IHDR payload.</summary>
    internal const int HeaderLength = 13;
    /// <summary>PNG color-type value for palette-indexed pixels.</summary>
    internal const byte IndexedColorType = 3;
    /// <summary>Asset loader limit for either image dimension.</summary>
    internal const int MaximumDimension = 2048;
    /// <summary>Asset loader's 16 MiB limit for the complete encoded PNG.</summary>
    internal const int MaximumEncodedBytes = 16 * 1024 * 1024;
    /// <summary>Initial accumulator for each PNG chunk's CRC-32 calculation.</summary>
    internal const uint InitialCrc = uint.MaxValue;
}
