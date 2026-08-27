namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The SNES PPU's 64 KiB of video RAM, exposed as bytes while retaining word-addressed
/// DMA behavior.
/// </summary>
public sealed class SnesVram
{
    /// <summary>The PPU contains 32,768 16-bit VRAM words.</summary>
    public const int WordCount = 0x8000;

    /// <summary>Two bytes per word gives the physical 64 KiB storage size.</summary>
    public const int ByteCount = WordCount * 2;

    private readonly byte[] _bytes = new byte[ByteCount];

    /// <summary>
    /// Provides a non-writable view for renderers, assertions, and debugger inspection.
    /// Writes should pass through DMA-aware methods so word stepping remains visible.
    /// </summary>
    public ReadOnlySpan<byte> Bytes => _bytes;

    /// <summary>
    /// Reads a byte by physical VRAM byte offset.
    /// </summary>
    public byte ReadByte(int byteOffset)
    {
        if ((uint)byteOffset >= ByteCount)
            throw new ArgumentOutOfRangeException(nameof(byteOffset));

        return _bytes[byteOffset];
    }

    /// <summary>Reads one little-endian PPU word for debugger inspection and verification.</summary>
    public ushort ReadWord(int wordAddress)
    {
        if ((uint)wordAddress >= WordCount)
            throw new ArgumentOutOfRangeException(nameof(wordAddress));
        int byteOffset = wordAddress * 2;
        return (ushort)(_bytes[byteOffset] | (_bytes[byteOffset + 1] << 8));
    }

    /// <summary>
    /// Executes a word-oriented DMA staging slice with the VMAIN increment already decoded.
    /// </summary>
    /// <param name="words">Complete source-word slice, in DMA order.</param>
    /// <param name="destinationWord">Initial 15-bit VMADD word address.</param>
    /// <param name="wordIncrement">One for a row (VMAIN=$80), 32 for a column (VMAIN=$81).</param>
    public void ExecuteWordTransfer(ReadOnlySpan<ushort> words, ushort destinationWord, int wordIncrement)
    {
        if (wordIncrement is not (1 or 32))
            throw new ArgumentOutOfRangeException(nameof(wordIncrement), "Known tilemap DMA increments are one or 32 words.");

        int destination = destinationWord & 0x7fff;
        foreach (ushort word in words)
        {
            int byteOffset = destination * 2;
            _bytes[byteOffset] = (byte)word;
            _bytes[byteOffset + 1] = (byte)(word >> 8);
            destination = (destination + wordIncrement) & 0x7fff;
        }
    }

    /// <summary>
    /// Loads a consecutive decompressed byte range as the VRAM-targeting decompressor does.
    /// This is used for room character graphics, whose command stream is expanded directly
    /// to the PPU rather than represented by the ordinary seven-byte write queue.
    /// </summary>
    public void LoadBytes(int destinationByteOffset, ReadOnlySpan<byte> bytes)
    {
        if (destinationByteOffset < 0 || destinationByteOffset + bytes.Length > ByteCount)
            throw new ArgumentOutOfRangeException(nameof(destinationByteOffset));
        bytes.CopyTo(_bytes.AsSpan(destinationByteOffset));
    }

    /// <summary>Loads bytes through Mode 7's high-byte-only $2119 DMA port.</summary>
    public void LoadMode7CharacterBytes(ReadOnlySpan<byte> bytes, ushort destinationWord = 0)
    {
        if (bytes.Length > WordCount)
            throw new ArgumentOutOfRangeException(nameof(bytes));
        int destination = destinationWord & 0x7fff;
        foreach (byte value in bytes)
        {
            _bytes[destination * 2 + 1] = value;
            destination = (destination + 1) & 0x7fff;
        }
    }

    /// <summary>Loads bytes through Mode 7's low-byte-only $2118 DMA port.</summary>
    public void LoadMode7MapBytes(ReadOnlySpan<byte> bytes, ushort destinationWord = 0)
    {
        if (bytes.Length > WordCount)
            throw new ArgumentOutOfRangeException(nameof(bytes));
        int destination = destinationWord & 0x7fff;
        foreach (byte value in bytes)
        {
            _bytes[destination * 2] = value;
            destination = (destination + 1) & 0x7fff;
        }
    }

    /// <summary>Fills Mode 7 low bytes as repeated writes to $2118 do.</summary>
    public void FillMode7MapBytes(byte value, int wordCount, ushort destinationWord = 0)
    {
        if ((uint)wordCount > WordCount)
            throw new ArgumentOutOfRangeException(nameof(wordCount));
        int destination = destinationWord & 0x7fff;
        for (int index = 0; index < wordCount; index++)
        {
            _bytes[destination * 2] = value;
            destination = (destination + 1) & 0x7fff;
        }
    }

    /// <summary>
    /// Performs the transfer produced by <c>$80:8C83</c>'s DMA channel 1 setup.
    /// </summary>
    /// <param name="bus">CPU address space from which DMA channel 1 reads.</param>
    /// <param name="sourceAddress">Fixed source bank plus initial 16-bit offset.</param>
    /// <param name="sizeInBytes">Number of source bytes copied.</param>
    /// <param name="encodedDestination">
    /// VRAM word destination. Bit 15 doubles as Super Metroid's private queue marker:
    /// clear selects consecutive words (<c>VMAIN=$80</c>), set selects columns in steps of
    /// 32 words (<c>VMAIN=$81</c>).
    /// </param>
    public void ExecuteQueuedWrite(
        ISnesAddressSpace bus,
        int sourceAddress,
        ushort sizeInBytes,
        ushort encodedDestination)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if ((uint)sourceAddress > 0x00ff_ffff)
            throw new ArgumentOutOfRangeException(nameof(sourceAddress), sourceAddress, "DMA source must be a 24-bit CPU address.");
        if (sizeInBytes == 0)
            throw new ArgumentOutOfRangeException(nameof(sizeInBytes), "A zero size is the original queue terminator, not a transfer.");

        // $80:8CAA-$80:8CB2 uses the destination sign bit to choose VMAIN. The bit is
        // harmless when written to VMADD because VRAM contains only 15 address bits.
        int destinationWord = encodedDestination & 0x7fff;
        int wordIncrement = (encodedDestination & 0x8000) == 0 ? 1 : 32;

        int sourceBank = sourceAddress & 0x00ff_0000;
        int sourceOffset = sourceAddress & 0xffff;

        for (int byteIndex = 0; byteIndex < sizeInBytes; byteIndex++)
        {
            // DMA mode $01 alternates writes between $2118 (word low byte) and $2119
            // (word high byte). VMAIN=$8x requests increment after the high-byte port.
            bool writesHighByte = (byteIndex & 1) != 0;
            int vramByteOffset = destinationWord * 2 + (writesHighByte ? 1 : 0);

            // A-bus DMA increments its 16-bit source offset but leaves the bank register
            // fixed. Explicit wrapping here matters for a transfer beginning at xx:FFFF.
            int currentSource = sourceBank | ((sourceOffset + byteIndex) & 0xffff);
            _bytes[vramByteOffset] = bus.ReadByte(currentSource);

            if (writesHighByte)
            {
                // VMADD wraps at 15 bits because physical VRAM has $8000 words.
                destinationWord = (destinationWord + wordIncrement) & 0x7fff;
            }
        }
    }

    /// <summary>
    /// Clears all VRAM. This is a host-side convenience for resets and isolated tests;
    /// the original game normally clears memory through explicit PPU/DMA operations.
    /// </summary>
    public void Clear() => Array.Clear(_bytes);
}
