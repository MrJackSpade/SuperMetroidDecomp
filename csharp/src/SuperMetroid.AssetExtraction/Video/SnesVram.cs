using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// The SNES PPU's 64 KiB of video RAM, exposed as bytes while retaining word-addressed
/// DMA behavior.
/// </summary>
public sealed class SnesVram
{
    /// <summary>The PPU contains 32,768 16-bit VRAM words.</summary>
    public const int WordCount = 32 * 1024;

    /// <summary>Two bytes per word gives the physical 64 KiB storage size.</summary>
    public const int ByteCount = WordCount * sizeof(ushort);

    /// <summary>VMADD's fifteen address bits: register arithmetic wraps within the VRAM words.</summary>
    private const int WordAddressMask = WordCount - 1;

    /// <summary>Physical VRAM image addressed as 64 KiB of bytes behind the PPU's word ports.</summary>
    private readonly byte[] _bytes = new byte[ByteCount];

    /// <summary>Transient write generation incremented before each mutation so render capture can detect changes.</summary>
    [NonSerialized] private long writeGeneration;
    /// <summary>Transient immutable copy reused while no VRAM write has occurred; excluded from saved state.</summary>
    [NonSerialized] private byte[]? capturedImage;
    /// <summary>Write generation represented by the cached immutable image; excluded from saved state.</summary>
    [NonSerialized] private long capturedGeneration;

    /// <summary>
    /// An immutable copy of the current 64 KiB image. Consecutive captures with no write in
    /// between return the same array, so static scenes stop copying VRAM every frame.
    /// </summary>
    internal byte[] CaptureImage()
    {
        if (capturedImage is null || capturedGeneration != writeGeneration)
        {
            capturedImage = _bytes.ToArray();
            capturedGeneration = writeGeneration;
        }
        return capturedImage;
    }


    /// <summary>Reads one little-endian PPU word for debugger inspection and verification.</summary>
    public ushort ReadWord(int wordAddress)
    {
        if ((uint)wordAddress >= WordCount)
            throw new ArgumentOutOfRangeException(nameof(wordAddress));
        int byteOffset = wordAddress * 2;
        return LittleEndianBytes.Word(_bytes[byteOffset], _bytes[byteOffset + 1]);
    }

    /// <summary>
    /// Loads a consecutive decompressed byte range as the VRAM-targeting decompressor does.
    /// This is used for room character graphics, whose command stream is expanded directly
    /// to the PPU rather than represented by the ordinary seven-byte write queue.
    /// </summary>
    public void LoadBytes(int destinationByteOffset, ReadOnlySpan<byte> bytes)
    {
        writeGeneration++;
        if (destinationByteOffset < 0 || destinationByteOffset + bytes.Length > ByteCount)
            throw new ArgumentOutOfRangeException(nameof(destinationByteOffset));
        bytes.CopyTo(_bytes.AsSpan(destinationByteOffset));
    }

    /// <summary>
    /// Writes <paramref name="bytes"/> low byte then high byte of each word from <paramref name="firstWord"/>, advancing
    /// <paramref name="wordStep"/> words after each high byte.
    /// </summary>
    public void WriteThroughPort(ReadOnlySpan<byte> bytes, int firstWord, int wordStep)
    {
        writeGeneration++;
        if (bytes.Length is <= 0 or > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(bytes));
        int destinationWord = firstWord & WordAddressMask;
        int increment = wordStep;
        for (int i = 0; i < bytes.Length; i++)
        {
            bool high = (i & 1) != 0;
            _bytes[destinationWord * 2 + (high ? 1 : 0)] = bytes[i];
            if (high) destinationWord = (destinationWord + increment) & WordAddressMask;
        }
    }
}
