namespace SuperMetroid.Core.Assets;

/// <summary>
/// Three-bit command field from the native Super Metroid compressed-stream format, normalized
/// into bits 5-7. A short header's command field is bits 5-7 of its first byte; command 7
/// there is the long-header marker, so <see cref="RelativeCopyInverted"/> is encodable only in
/// a long header's bits 2-4.
/// </summary>
public enum SmCompressionCommand : byte
{
    /// <summary>Copy literal bytes from the compressed stream.</summary>
    Literal = 0x00,
    /// <summary>Repeat one source byte.</summary>
    RepeatByte = 0x20,
    /// <summary>Alternate two source bytes.</summary>
    AlternatePair = 0x40,
    /// <summary>Emit an incrementing sequence beginning with one source byte.</summary>
    IncrementingSequence = 0x60,
    /// <summary>Copy from an absolute offset in the decompressed output.</summary>
    AbsoluteCopy = 0x80,
    /// <summary>Copy and invert bytes from an absolute output offset.</summary>
    AbsoluteCopyInverted = 0xa0,
    /// <summary>Copy from a one-byte backwards distance.</summary>
    RelativeCopy = 0xc0,
    /// <summary>Copy and invert bytes from a one-byte backwards distance (long headers only).</summary>
    RelativeCopyInverted = 0xe0,
}
