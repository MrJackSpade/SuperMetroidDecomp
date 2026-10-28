using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using System.Buffers.Binary;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Compression;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction.ReferenceRendering;

/// <summary>
/// Import/reference-only renderer for named raw assets extracted from bank $82.
/// PLMs, enemies, and runtime palette effects are intentionally separate layers.
/// </summary>
public static class RoomRenderer
{
    /// <summary>Static 9-by-5-screen Landing Site definition from RoomHeader_LandingSite ($8F:91F8), using graphics set 0 and the shared level data without state-specific actors or effects.</summary>
    /// <remarks>RoomHeader_LandingSite ($8F:91F8) declares a 9x5-screen room. Every state uses graphics set 0 and
    /// LevelData_LandingSite; music, enemies, effects, and PLMs vary.</remarks>
    public static readonly RoomRenderDefinition LandingSite = new(
        "Landing Site", new(9, 5),
        "LevelData_LandingSite.bin",
        "Tiles_0_1_UpperCrateria.bin",
        "TileTables_0_1_UpperCrateria.bin",
        "Palettes_0_UpperCrateria.bin");

    /// <summary>
    /// The BG character portion of VRAM as <c>$82:E783</c> leaves it: the CRE characters, then the
    /// area tileset over them from VMADD $0000, at their gameplay character numbers.
    /// </summary>
    private static byte[] BackgroundCharacterImage(byte[] areaTiles, byte[] commonTiles)
    {
        var vram = new byte[RoomGraphicsLayoutTooling.BackgroundCharacterVramByteCount];
        areaTiles.CopyTo(vram, RoomGraphicsLayout.AreaCharacters.Offset * sizeof(ushort));
        commonTiles.CopyTo(vram, RoomGraphicsToolingLayout.CommonRoomElementCharacters.Offset * sizeof(ushort));
        return vram;
    }

    /// <summary>
    /// Loads and composes both static block layers for a room. This mirrors the data setup in
    /// <c>LoadLevelDataAndOtherThings</c> at <c>$82:E7D3</c>, but renders to RGBA instead of PPU VRAM.
    /// </summary>
    public static RenderedRoom Render(string rawAssetDirectory, RoomRenderDefinition room)
    {
        // One SNES room screen is 16x16 blocks, and one block is 16x16 pixels.
        Extent16<RoomBlocks> blocks = RoomGridConversions.ToRoomBlocks(room.Size);
        int expectedLayerBytes = checked(blocks.Area * 2);

        // Decompressed level streams have this exact layout:
        //   u16 layerByteCount
        //   u16 foregroundBlocks[layerByteCount / 2]
        //   u8  behaviorTypeScript[layerByteCount / 2]  ("BTS")
        //   u16 backgroundBlocks[layerByteCount / 2]    (when present)
        byte[] levelStream = LoadCompressed(rawAssetDirectory, room.LevelData);
        if (levelStream.Length < 2)
            throw new InvalidDataException("Level stream has no size header.");
        int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(levelStream);
        if (layerBytes != expectedLayerBytes)
            throw new InvalidDataException($"{room.Name} declares {layerBytes} layer bytes; dimensions require {expectedLayerBytes}.");
        int btsBytes = layerBytes / 2;
        int foregroundOffset = 2;
        int backgroundOffset = foregroundOffset + layerBytes + btsBytes;
        if (levelStream.Length < backgroundOffset + layerBytes)
            throw new InvalidDataException("Level stream does not contain its complete foreground, BTS, and background layers.");

        // Model the portion of VRAM used by BG graphics. Preserving the console's original
        // tile-number layout lets unmodified tile-table entries address this byte array.
        byte[] areaTiles = LoadCompressed(rawAssetDirectory, room.AreaTiles);
        byte[] commonTiles = LoadCompressed(rawAssetDirectory, room.CommonTiles);
        byte[] vram = BackgroundCharacterImage(areaTiles, commonTiles);

        // $82:E7D3 places CRE block definitions at WRAM $7E:A000 and the current area's
        // definitions immediately after them at $7E:A800. A level block index can therefore
        // select either table without any runtime rebasing.
        byte[] commonBlocks = LoadCompressed(rawAssetDirectory, room.CommonTileTable);
        byte[] areaBlocks = LoadCompressed(rawAssetDirectory, room.AreaTileTable);
        if (commonBlocks.Length != RoomGraphicsLayoutTooling.CreBlockDefinitionsByteCount)
        {
            throw new InvalidDataException(
                $"Expected {RoomGraphicsLayoutTooling.CreBlockDefinitionCount} " +
                "common 16x16 block definitions.");
        }
        var blockDefinitions = new byte[commonBlocks.Length + areaBlocks.Length];
        commonBlocks.CopyTo(blockDefinitions, 0);
        areaBlocks.CopyTo(blockDefinitions, commonBlocks.Length);

        // Tiles do not carry colors. Each tile-table entry selects one of eight 16-color
        // background sub-palettes using bits 10-12.
        IReadOnlyList<RgbaPixel> palette = SnesGraphicsTooling.DecodeRgbColorPalette(LoadCompressed(rawAssetDirectory, room.Palette));
        if (palette.Count < 128)
            throw new InvalidDataException("Room palette must contain eight 16-color background palettes.");

        var size = new Extent16<ImagePixels>(
            checked((ushort)(blocks.X * 16)), checked((ushort)(blocks.Y * 16)));
        // CGRAM color 0 is the backdrop visible wherever every BG layer is transparent.
        var pixels = Enumerable.Repeat(palette[0], size.Area).ToArray();

        // The SNES draws BG2 behind BG1 for this room configuration. Drawing in that order
        // and skipping palette index zero reproduces the static transparency relationship.
        DrawLayer(levelStream.AsSpan(backgroundOffset, layerBytes), blocks,
            blockDefinitions, vram, palette, pixels, size.X);
        DrawLayer(levelStream.AsSpan(foregroundOffset, layerBytes), blocks,
            blockDefinitions, vram, palette, pixels, size.X);

        return new RenderedRoom(size, pixels);
    }

    /// <summary>Expands a row-major layer of 16x16 level blocks into pixels.</summary>
    private static void DrawLayer(
        ReadOnlySpan<byte> blocks,
        Extent16<RoomBlocks> size,
        ReadOnlySpan<byte> blockDefinitions,
        ReadOnlySpan<byte> vram,
        IReadOnlyList<RgbaPixel> palette,
        Span<RgbaPixel> destination,
        int destinationWidth)
    {
        BackgroundTile[] definitionTiles = BackgroundTileWords.ToTiles(blockDefinitions);
        for (int blockY = 0; blockY < size.Y; blockY++)
        {
            for (int blockX = 0; blockX < size.X; blockX++)
            {
                int levelOffset = size.RowMajorIndex(new(checked((ushort)blockX), checked((ushort)blockY))) * 2;
                // Level entry layout:
                //   bits  0-9  index into the combined 16x16 block-definition table
                //   bit     10 horizontal block flip
                //   bit     11 vertical block flip
                //   bits 12-15 collision/block type (rendering does not consume these)
                ushort levelEntry = BinaryPrimitives.ReadUInt16LittleEndian(blocks[levelOffset..]);
                ExpandedBlockTiles expanded = LevelBlockTilemapExpander.Expand(levelEntry, definitionTiles);

                for (int outputTileY = 0; outputTileY < 2; outputTileY++)
                {
                    for (int outputTileX = 0; outputTileX < 2; outputTileX++)
                    {
                        // The shared bank-$80 expansion has already swapped children and
                        // toggled their own PPU flip flags, so DrawTile consumes final words.
                        ushort tileEntry = BackgroundTileWords.ToWord(new OutputTileCorner(outputTileX, outputTileY) switch
                        {
                            (0, 0) => expanded.TopLeft,
                            (1, 0) => expanded.TopRight,
                            (0, 1) => expanded.BottomLeft,
                            _ => expanded.BottomRight,
                        });
                        DrawTile(tileEntry, vram, palette, destination, destinationWidth,
                            new(checked((ushort)(blockX * 16 + outputTileX * 8)), checked((ushort)(blockY * 16 + outputTileY * 8))));
                    }
                }
            }
        }
    }

    /// <summary>Draws one standard SNES BG tilemap entry into the destination image.</summary>
    private static void DrawTile(
        ushort tileEntry,
        ReadOnlySpan<byte> vram,
        IReadOnlyList<RgbaPixel> palette,
        Span<RgbaPixel> destination,
        int destinationWidth,
        Position16<ImagePixels> origin)
    {
        // Standard SNES BG tilemap entry layout:
        //   bits  0-9  VRAM tile number
        //   bits 10-12 sub-palette
        //   bit     13 priority
        //   bit     14 horizontal flip
        //   bit     15 vertical flip
        SnesBgTilemapWord packedEntry = tileEntry;
        int tileIndex = packedEntry.CharacterIndex;
        int tileOffset = tileIndex * RoomGraphicsLayoutTooling.BytesPer4BppCharacter;
        if (tileOffset + RoomGraphicsLayoutTooling.BytesPer4BppCharacter > vram.Length)
            throw new InvalidDataException($"Tile index ${tileIndex:X3} exceeds modeled VRAM.");
        int paletteBase = packedEntry.PaletteIndex * 16;

        for (int y = 0; y < 8; y++)
        {
            int sourceY = packedEntry.FlipVertically ? 7 - y : y;
            for (int x = 0; x < 8; x++)
            {
                int sourceX = packedEntry.FlipHorizontally ? 7 - x : x;
                int colorIndex = Read4BppPixel(vram[tileOffset..], new(checked((ushort)sourceX), checked((ushort)sourceY)));
                // Index zero is transparent for BG tiles. It reveals the lower BG or the
                // backdrop; it is not necessarily the RGB value stored at paletteBase + 0.
                if (colorIndex == 0)
                    continue;
                destination[(origin.Y + y) * destinationWidth + origin.X + x] = palette[paletteBase + colorIndex];
            }
        }
    }

    /// <summary>Reassembles one four-bit palette index from the tile's four bitplanes.</summary>
    private static int Read4BppPixel(ReadOnlySpan<byte> tile, Position16<CharacterPixels> pixel)
    {
        int mask = 1 << (7 - pixel.X);
        int row = pixel.Y * 2;
        return ((tile[row] & mask) != 0 ? 1 : 0)
             | ((tile[row + 1] & mask) != 0 ? 2 : 0)
             | ((tile[16 + row] & mask) != 0 ? 4 : 0)
             | ((tile[16 + row + 1] & mask) != 0 ? 8 : 0);
    }

    /// <summary>Reads one named ROM slice and requires it to be exactly one compressed stream.</summary>
    private static byte[] LoadCompressed(string directory, string name)
    {
        byte[] stored = File.ReadAllBytes(Path.Combine(directory, name));
        return SmCompression.Decompress(stored);
    }
}
