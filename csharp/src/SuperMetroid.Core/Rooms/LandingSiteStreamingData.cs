using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge-backed loader for Landing Site's native BG stream inputs.</summary>
public static class LandingSiteStreamingData
{
    // These addresses and compressed lengths are labels/boundaries in bank_B9..CE.asm.
    // Keeping the lengths beside the addresses makes decompression reject a wrong ROM
    // revision or accidental walk into the following compressed asset.
    private const int CreBlockTableAddress = 0xb9a09d;
    private const int CreBlockTableCompressedBytes = 0x0597;
    private const int AreaBlockTableAddress = 0xc1b6f6;
    private const int AreaBlockTableCompressedBytes = 0x07f8;
    private const int LevelDataAddress = 0xc2c2bb;
    private const int LevelDataCompressedBytes = 0x142d;
    private const int CreTilesAddress = 0xb98000;
    private const int CreTilesCompressedBytes = 0x209d;
    private const int AreaTilesAddress = 0xbac629;
    private const int AreaTilesCompressedBytes = 0x32e8;

    private const int WidthInBlocks = 9 * 16;
    private const int HeightInBlocks = 5 * 16;
    private const int LayerEntryCount = WidthInBlocks * HeightInBlocks;
    private const int LayerByteCount = LayerEntryCount * 2;

    /// <summary>
    /// Decompresses the same three inputs that bank $82 installs in WRAM and constructs the
    /// verified bank-$80 row/column producer over them.
    /// </summary>
    public static BackgroundTilemapStreamer CreateStreamer(ISnesAddressSpace bus)
    {
        return LoadLevel(bus).CreateBackgroundStreamer(sizeOfBg2: 0);
    }

    /// <summary>
    /// Decompresses Landing Site's unified BG1/BTS/BG2 allocation exactly once so visual
    /// streaming and the forthcoming bank-$94 collision port share identical source data.
    /// </summary>
    public static RoomLevelData LoadLevel(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);

        byte[] creDefinitions = DecompressExact(bus, CreBlockTableAddress, CreBlockTableCompressedBytes);
        byte[] areaDefinitions = DecompressExact(bus, AreaBlockTableAddress, AreaBlockTableCompressedBytes);
        if (creDefinitions.Length != 0x0800)
            throw new InvalidDataException($"CRE block table expanded to ${creDefinitions.Length:X}, expected $800.");

        var combinedDefinitions = new byte[creDefinitions.Length + areaDefinitions.Length];
        creDefinitions.CopyTo(combinedDefinitions, 0);
        areaDefinitions.CopyTo(combinedDefinitions, creDefinitions.Length);

        byte[] levelStream = DecompressExact(bus, LevelDataAddress, LevelDataCompressedBytes);
        if (levelStream.Length < 2)
            throw new InvalidDataException("Landing Site level stream has no layer-size word.");
        int declaredLayerBytes = BinaryPrimitives.ReadUInt16LittleEndian(levelStream);
        if (declaredLayerBytes != LayerByteCount)
        {
            throw new InvalidDataException(
                $"Landing Site declares ${declaredLayerBytes:X} bytes per layer, expected ${LayerByteCount:X}.");
        }

        // Native decompressed layout is: size word, BG1 words, one BTS byte per block,
        // then BG2 words. We retain entries as complete words so collision/type bits and
        // the two parent flip flags arrive unchanged at the bank-$80 expander.
        int foregroundOffset = 2;
        int btsByteCount = LayerEntryCount;
        int backgroundOffset = foregroundOffset + LayerByteCount + btsByteCount;
        if (levelStream.Length < backgroundOffset + LayerByteCount)
            throw new InvalidDataException("Landing Site stream is missing its complete BG2 layer.");

        // Preserve native WRAM adjacency after BG1. At a bottom-aligned +$1F camera, the
        // 16-block column loop can read one block row past the logical level and therefore
        // interprets the first neighboring BTS bytes as tile words. It is offscreen garbage,
        // but clamping it would make DMA staging diverge from the cartridge. BG1's source
        // view consequently includes every complete word to the end of this allocation.
        ushort[] streamingForegroundAllocation = ReadWords(levelStream.AsSpan(foregroundOffset));
        ushort[] foreground = ReadWords(levelStream.AsSpan(foregroundOffset, LayerByteCount));
        byte[] behavior = levelStream.AsSpan(foregroundOffset + LayerByteCount, btsByteCount).ToArray();
        ushort[] background = ReadWords(levelStream.AsSpan(backgroundOffset, LayerByteCount));

        // Landing Site's $81/1 layer-2 modes suppress ordinary BG2 streaming, so SizeOfBG2
        // is not consumed here. It remains zero until BG1SC/BG2SC register initialization is
        // modeled for rooms that use ordinary even-mode BG2 updates.
        return new RoomLevelData(
            WidthInBlocks,
            HeightInBlocks,
            foreground,
            behavior,
            background,
            combinedDefinitions,
            streamingForegroundAllocation);
    }

    /// <summary>
    /// Reproduces the two direct-to-VRAM decompressions at <c>$82:E795-$82:E7BB</c> for
    /// graphics set zero: CRE characters at byte $5000, then area characters at byte $0000.
    /// </summary>
    public static void LoadCharacterGraphics(
        ISnesAddressSpace bus,
        SnesVram vram,
        LandingSiteEntryState entry)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(entry);
        byte[] creTiles = DecompressExact(bus, CreTilesAddress, CreTilesCompressedBytes);
        byte[] areaTiles = DecompressExact(bus, AreaTilesAddress, AreaTilesCompressedBytes);
        vram.LoadBytes(0x5000, creTiles);
        vram.LoadBytes(0x0000, areaTiles);

        // $82:E9E7's door-dependent library-background command has already been resolved
        // by LandingSiteEntryState. Copy its literal ROM slice to its literal VRAM word;
        // unlike the CRE/area inputs above, scrolling-sky tilemaps are not compressed.
        var skyTilemap = new byte[entry.SkyByteCount];
        for (int index = 0; index < skyTilemap.Length; index++)
            skyTilemap[index] = bus.ReadByte(entry.SkySourceAddress + index);
        vram.LoadBytes(entry.SkyVramDestination * 2, skyTilemap);
    }

    private static byte[] DecompressExact(ISnesAddressSpace bus, int address, int compressedBytes)
    {
        var stored = new byte[compressedBytes];
        for (int index = 0; index < stored.Length; index++)
            stored[index] = bus.ReadByte(address + index);
        return SmCompression.Decompress(stored);
    }

    private static ushort[] ReadWords(ReadOnlySpan<byte> bytes)
    {
        var words = new ushort[bytes.Length / 2];
        for (int index = 0; index < words.Length; index++)
            words[index] = BinaryPrimitives.ReadUInt16LittleEndian(bytes[(index * 2)..]);
        return words;
    }
}
