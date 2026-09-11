namespace SuperMetroid.Core.Assets;

/// <summary>PNG indexed-image wire values and bounded asset limits; no cartridge addresses.</summary>
internal static class IndexedPngFormat
{
    internal static ReadOnlySpan<byte> Signature => [137, 80, 78, 71, 13, 10, 26, 10];
    internal const int HeaderLength = 13;
    internal const byte IndexedColorType = 3;
    internal const int MaximumDimension = 2048;
    internal const int MaximumEncodedBytes = 16 * 1024 * 1024;
    internal const uint InitialCrc = uint.MaxValue;
}

/// <summary>PNG filter method zero's mutually exclusive scanline predictors.</summary>
internal enum PngRowFilter : byte { None, Sub, Up, Average, Paeth }
