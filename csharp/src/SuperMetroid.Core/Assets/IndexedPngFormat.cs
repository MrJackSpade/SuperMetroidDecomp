namespace SuperMetroid.Core.Assets;

/// <summary>PNG indexed-image wire values and bounded asset limits; no cartridge addresses.</summary>
internal static class IndexedPngFormat
{
    /// <summary>The eight-byte PNG file signature checked before chunk parsing and written before encoded chunks.</summary>
    internal static ReadOnlySpan<byte> Signature => [137, 80, 78, 71, 13, 10, 26, 10];
    /// <summary>Number of data bytes required in the PNG image-header chunk.</summary>
    internal const int HeaderLength = 13;
    /// <summary>PNG color-type value indicating palette-indexed pixels.</summary>
    internal const byte IndexedColorType = 3;
    /// <summary>Largest permitted width or height when decoding an indexed image asset.</summary>
    internal const int MaximumDimension = 2048;
    /// <summary>Maximum accumulated encoded PNG size accepted by the bounded asset reader.</summary>
    internal const int MaximumEncodedBytes = 16 * 1024 * 1024;
    /// <summary>Initial unsigned CRC-32 accumulator used for PNG chunk checksums.</summary>
    internal const uint InitialCrc = uint.MaxValue;
}

/// <summary>PNG filter method zero's mutually exclusive scanline predictors.</summary>
internal enum PngRowFilter : byte
{
    /// <summary>Uses each filtered scanline byte unchanged, with no neighboring-byte predictor.</summary>
    None,

    /// <summary>Predicts from the reconstructed byte immediately to the left in the same scanline.</summary>
    Sub,

    /// <summary>Predicts from the reconstructed byte in the preceding scanline.</summary>
    Up,

    /// <summary>Predicts from the integer average of the reconstructed left and upper bytes.</summary>
    Average,

    /// <summary>Predicts from the Paeth-selected nearest value among the left, upper, and upper-left bytes.</summary>
    Paeth
}
