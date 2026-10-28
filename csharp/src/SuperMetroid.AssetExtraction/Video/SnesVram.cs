using SuperMetroid.Core.Hardware;

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
        return SnesLittleEndian.Word(_bytes[byteOffset], _bytes[byteOffset + 1]);
    }

    /// <summary>
    /// Executes a word-oriented DMA staging slice with the VMAIN increment already decoded.
    /// </summary>
    /// <param name="words">Complete source-word slice, in DMA order.</param>
    /// <param name="destinationWord">Initial 15-bit VMADD word address.</param>
    /// <param name="wordIncrement">One for a row (VMAIN=$80), 32 for a column (VMAIN=$81).</param>
    public void ExecuteWordTransfer(ReadOnlySpan<ushort> words, int destinationWord, int wordIncrement)
    {
        writeGeneration++;
        if (wordIncrement is not (1 or 32))
            throw new ArgumentOutOfRangeException(nameof(wordIncrement), "Known tilemap DMA increments are one or 32 words.");

        int destination = destinationWord & WordAddressMask;
        foreach (int word in words)
        {
            int byteOffset = destination * 2;
            _bytes[byteOffset] = SnesLittleEndian.LowByte(word);
            _bytes[byteOffset + 1] = SnesLittleEndian.HighByte(word);
            destination = (destination + wordIncrement) & WordAddressMask;
        }
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

    /// <summary>Loads bytes through Mode 7's high-byte-only $2119 DMA port.</summary>
    public void LoadFloorCharacterBytes(ReadOnlySpan<byte> bytes, int destinationWord = 0)
    {
        writeGeneration++;
        if (bytes.Length > WordCount)
            throw new ArgumentOutOfRangeException(nameof(bytes));
        int destination = destinationWord & WordAddressMask;
        for (int index = 0; index < bytes.Length; index++)
        {
            _bytes[destination * 2 + 1] = bytes[index];
            destination = (destination + 1) & WordAddressMask;
        }
    }

    /// <summary>Loads bytes through Mode 7's low-byte-only $2118 DMA port.</summary>
    public void LoadFloorMapBytes(ReadOnlySpan<byte> bytes, int destinationWord = 0)
    {
        writeGeneration++;
        if (bytes.Length > WordCount)
            throw new ArgumentOutOfRangeException(nameof(bytes));
        int destination = destinationWord & WordAddressMask;
        for (int index = 0; index < bytes.Length; index++)
        {
            _bytes[destination * 2] = bytes[index];
            destination = (destination + 1) & WordAddressMask;
        }
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
