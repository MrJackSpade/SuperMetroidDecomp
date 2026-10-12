namespace SuperMetroid.Core.Assets;

/// <summary>PNG indexed-image wire values and bounded asset limits; no cartridge addresses.</summary>
internal static class IndexedPngFormat
{
    /// <summary>Gets signature.</summary>
    internal static ReadOnlySpan<byte> Signature => [137, 80, 78, 71, 13, 10, 26, 10];
    /// <summary>Stores header length.</summary>
    internal const int HeaderLength = 13;
    /// <summary>Stores indexed color type.</summary>
    internal const byte IndexedColorType = 3;
    /// <summary>Stores maximum dimension.</summary>
    internal const int MaximumDimension = 2048;
    /// <summary>Stores maximum encoded bytes.</summary>
    internal const int MaximumEncodedBytes = 16 * 1024 * 1024;
    /// <summary>Stores initial crc.</summary>
    internal const uint InitialCrc = uint.MaxValue;
}

/// <summary>PNG filter method zero's mutually exclusive scanline predictors.</summary>
internal enum PngRowFilter : byte { /** <summary>Identifies none.</summary> */ None, /** <summary>Identifies sub.</summary> */ Sub, /** <summary>Identifies up.</summary> */ Up, /** <summary>Identifies average.</summary> */ Average, /** <summary>Identifies paeth.</summary> */ Paeth }
