namespace SuperMetroid.AssetExtraction.Compression;

/// <summary>
/// The eight commands of the native Super Metroid compressed-stream format. <see cref="SmCompressionHeader"/> owns the
/// three-bit command field that selects each: a short header's bits 5-7, where command 7 is the long-header marker, so
/// <see cref="RelativeCopyInverted"/> is encodable only in a long header's bits 2-4.
/// </summary>
public enum SmCompressionCommand
{
    /// <summary>Copy literal bytes from the compressed stream.</summary>
    Literal,
    /// <summary>Repeat one source byte.</summary>
    RepeatByte,
    /// <summary>Alternate two source bytes.</summary>
    AlternatePair,
    /// <summary>Emit an incrementing sequence beginning with one source byte.</summary>
    IncrementingSequence,
    /// <summary>Copy from an absolute offset in the decompressed output.</summary>
    AbsoluteCopy,
    /// <summary>Copy and invert bytes from an absolute output offset.</summary>
    AbsoluteCopyInverted,
    /// <summary>Copy from a one-byte backwards distance.</summary>
    RelativeCopy,
    /// <summary>Copy and invert bytes from a one-byte backwards distance (long headers only).</summary>
    RelativeCopyInverted,
}
