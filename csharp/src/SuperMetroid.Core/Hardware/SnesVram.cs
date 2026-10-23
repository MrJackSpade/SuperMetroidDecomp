namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The SNES PPU's 64 KiB of video RAM, exposed as bytes while retaining word-addressed
/// DMA behavior.
/// </summary>
[PpuLayoutAccess]
public sealed class SnesVram
{
    /// <summary>The PPU contains 32,768 16-bit VRAM words.</summary>
    public const int WordCount = SnesPpuLayout.VramWordCount;

    /// <summary>Two bytes per word gives the physical 64 KiB storage size.</summary>
    public const int ByteCount = SnesPpuLayout.VramByteCount;

    /// <summary>VMADD's fifteen address bits: register arithmetic wraps within the VRAM words.</summary>
    private const int WordAddressMask = WordCount - 1;

    /// <summary>Physical VRAM image addressed as 64 KiB of bytes behind the PPU's word ports.</summary>
    private readonly byte[] _bytes = new byte[ByteCount];

    /// <summary>
    /// When each word was last written, on the clock <see cref="BgArtworkBindings"/> shares (#1350), so a binding shows only
    /// over words VRAM has not written since; null until bindings attach, as a frontend screen's VRAM never has any.
    /// </summary>
    private uint[]? _writeStamps;

    /// <summary>The write clock: the stamp of the latest write or binding.</summary>
    private uint _writeClock;

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

    /// <summary>Starts tracking when each word is written, for background artwork bindings to order against (#1350).</summary>
    internal void TrackWriteOrder() => _writeStamps ??= new uint[WordCount];

    /// <summary>Advances the write clock and returns its new stamp (#1350).</summary>
    internal uint NextWriteStamp() => ++_writeClock;

    /// <summary>When VRAM word <paramref name="wordAddress"/> was last written; zero while write order is not tracked.</summary>
    internal uint WriteStampAt(int wordAddress) => _writeStamps?[wordAddress] ?? 0;

    /// <summary>The stamp a write takes: the next on the clock while write order is tracked, otherwise none.</summary>
    private uint BeginWrite() => _writeStamps is null ? 0 : NextWriteStamp();

    /// <summary>Records that a write stamped <paramref name="stamp"/> reached <paramref name="wordCount"/> words from <paramref name="firstWord"/>.</summary>
    private void StampWords(int firstWord, int wordCount, uint stamp) => _writeStamps?.AsSpan(firstWord, wordCount).Fill(stamp);

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
    public void ExecuteWordTransfer(ReadOnlySpan<ushort> words, ushort destinationWord, int wordIncrement)
    {
        writeGeneration++;
        uint stamp = BeginWrite();
        if (wordIncrement is not (1 or 32))
            throw new ArgumentOutOfRangeException(nameof(wordIncrement), "Known tilemap DMA increments are one or 32 words.");

        int destination = destinationWord & WordAddressMask;
        foreach (ushort word in words)
        {
            int byteOffset = destination * 2;
            _bytes[byteOffset] = (byte)word;
            _bytes[byteOffset + 1] = SnesLittleEndian.HighByte(word);
            StampWords(destination, 1, stamp);
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
        if (!bytes.IsEmpty)
            StampWords(destinationByteOffset / 2, (destinationByteOffset + bytes.Length - 1) / 2 - destinationByteOffset / 2 + 1, BeginWrite());
    }

    /// <summary>Loads bytes through Mode 7's high-byte-only $2119 DMA port.</summary>
    public void LoadMode7CharacterBytes(ReadOnlySpan<byte> bytes, ushort destinationWord = 0)
    {
        writeGeneration++;
        uint stamp = BeginWrite();
        if (bytes.Length > WordCount)
            throw new ArgumentOutOfRangeException(nameof(bytes));
        int destination = destinationWord & WordAddressMask;
        foreach (byte value in bytes)
        {
            _bytes[destination * 2 + 1] = value;
            StampWords(destination, 1, stamp);
            destination = (destination + 1) & WordAddressMask;
        }
    }

    /// <summary>Loads bytes through Mode 7's low-byte-only $2118 DMA port.</summary>
    public void LoadMode7MapBytes(ReadOnlySpan<byte> bytes, ushort destinationWord = 0)
    {
        writeGeneration++;
        uint stamp = BeginWrite();
        if (bytes.Length > WordCount)
            throw new ArgumentOutOfRangeException(nameof(bytes));
        int destination = destinationWord & WordAddressMask;
        foreach (byte value in bytes)
        {
            _bytes[destination * 2] = value;
            StampWords(destination, 1, stamp);
            destination = (destination + 1) & WordAddressMask;
        }
    }

    /// <summary>Fills Mode 7 low bytes as repeated writes to $2118 do.</summary>
    public void FillMode7MapBytes(byte value, int wordCount, ushort destinationWord = 0)
    {
        writeGeneration++;
        uint stamp = BeginWrite();
        if ((uint)wordCount > WordCount)
            throw new ArgumentOutOfRangeException(nameof(wordCount));
        int destination = destinationWord & WordAddressMask;
        for (int index = 0; index < wordCount; index++)
        {
            _bytes[destination * 2] = value;
            StampWords(destination, 1, stamp);
            destination = (destination + 1) & WordAddressMask;
        }
    }
    /// <summary>Runs the same mode-1 VRAM port sequence from compiled artwork, without synthesizing a CPU address space.</summary>
    public void ExecuteQueuedAssetWrite(ReadOnlySpan<byte> bytes, ushort encodedDestination)
    {
        writeGeneration++;
        uint stamp = BeginWrite();
        if (bytes.Length is <= 0 or > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(bytes));
        VramDmaDestination destination = VramDmaDestination.FromNative(encodedDestination);
        int destinationWord = destination.Word;
        int increment = destination.WordIncrement;
        for (int i = 0; i < bytes.Length; i++)
        {
            bool high = (i & 1) != 0;
            _bytes[destinationWord * 2 + (high ? 1 : 0)] = bytes[i];
            StampWords(destinationWord, 1, stamp);
            if (high) destinationWord = (destinationWord + increment) & WordAddressMask;
        }
    }
}
