using System.Buffers.Binary;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Names the ROM-derived pieces required to render one static room state. Keeping filenames
/// here, rather than hiding them in the renderer, makes the eventual room-state table explicit.
/// </summary>
public sealed record RoomRenderDefinition(
    string Name,
    int WidthInScreens,
    int HeightInScreens,
    string LevelData,
    string AreaTiles,
    string AreaTileTable,
    string Palette,
    string CommonTiles = "CRE_Tiles_Compressed.bin",
    string CommonTileTable = "CRE_TileTable_Compressed.bin");

/// <summary>A fully composed, top-left-origin room image in row-major RGBA order.</summary>
public sealed record RenderedRoom(int Width, int Height, Rgba32[] Pixels);

/// <summary>
/// Composes the same room assets loaded by bank $82 into a convenient RGBA image.
/// PLMs, enemies, and runtime palette effects are intentionally separate layers.
/// </summary>
public static class RoomRenderer
{
    // RoomHeader_LandingSite ($8F:91F8) declares a 9x5-screen room. Every state uses
    // graphics set 0 and LevelData_LandingSite; music, enemies, effects, and PLMs vary.
    public static readonly RoomRenderDefinition LandingSite = new(
        "Landing Site", 9, 5,
        "LevelData_LandingSite.bin",
        "Tiles_0_1_UpperCrateria.bin",
        "TileTables_0_1_UpperCrateria.bin",
        "Palettes_0_UpperCrateria.bin");

    // One 8x8 four-bit SNES tile has four 8-byte bitplanes.
    private const int BytesPer4BppTile = 32;

    // LoadCRETilesTilesetTilesAndPalette ($82:E783) decompresses the area tiles at VRAM
    // byte address $0000 and CRE (Common Room Elements) tiles at byte address $5000.
    // $5000 / 32 means CRE tile numbers begin at tile $280.
    private const int CommonTilesVramByteOffset = 0x5000;

    // CRE's decompressed block table is $800 bytes. Each 16x16 block uses four two-byte
    // tilemap entries, so $800 / 8 gives 256 common blocks at indexes $000-$0FF.
    private const int CommonBlockCount = 256;

    /// <summary>
    /// Loads and composes both static block layers for a room. This mirrors the data setup in
    /// <c>LoadLevelDataAndOtherThings</c> at <c>$82:E7D3</c>, but renders to RGBA instead of PPU VRAM.
    /// </summary>
    public static RenderedRoom Render(string rawAssetDirectory, RoomRenderDefinition room)
    {
        // One SNES room screen is 16x16 blocks, and one block is 16x16 pixels.
        int widthInBlocks = room.WidthInScreens * 16;
        int heightInBlocks = room.HeightInScreens * 16;
        int expectedLayerBytes = checked(widthInBlocks * heightInBlocks * 2);

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
        var vram = new byte[0x8000];
        areaTiles.CopyTo(vram, 0);
        commonTiles.CopyTo(vram, CommonTilesVramByteOffset);

        // $82:E7D3 places CRE block definitions at WRAM $7E:A000 and the current area's
        // definitions immediately after them at $7E:A800. A level block index can therefore
        // select either table without any runtime rebasing.
        byte[] commonBlocks = LoadCompressed(rawAssetDirectory, room.CommonTileTable);
        byte[] areaBlocks = LoadCompressed(rawAssetDirectory, room.AreaTileTable);
        if (commonBlocks.Length != CommonBlockCount * 8)
            throw new InvalidDataException($"Expected {CommonBlockCount} common 16x16 block definitions.");
        var blockDefinitions = new byte[commonBlocks.Length + areaBlocks.Length];
        commonBlocks.CopyTo(blockDefinitions, 0);
        areaBlocks.CopyTo(blockDefinitions, commonBlocks.Length);

        // Tiles do not carry colors. Each tile-table entry selects one of eight 16-color
        // background sub-palettes using bits 10-12.
        IReadOnlyList<Rgba32> palette = SnesGraphics.DecodeBgr555Palette(LoadCompressed(rawAssetDirectory, room.Palette));
        if (palette.Count < 128)
            throw new InvalidDataException("Room palette must contain eight 16-color background palettes.");

        int width = widthInBlocks * 16;
        int height = heightInBlocks * 16;
        // CGRAM color 0 is the backdrop visible wherever every BG layer is transparent.
        var pixels = Enumerable.Repeat(palette[0], checked(width * height)).ToArray();

        // The SNES draws BG2 behind BG1 for this room configuration. Drawing in that order
        // and skipping palette index zero reproduces the static transparency relationship.
        DrawLayer(levelStream.AsSpan(backgroundOffset, layerBytes), widthInBlocks, heightInBlocks,
            blockDefinitions, vram, palette, pixels, width);
        DrawLayer(levelStream.AsSpan(foregroundOffset, layerBytes), widthInBlocks, heightInBlocks,
            blockDefinitions, vram, palette, pixels, width);

        return new RenderedRoom(width, height, pixels);
    }

    /// <summary>Expands a row-major layer of 16x16 level blocks into pixels.</summary>
    private static void DrawLayer(
        ReadOnlySpan<byte> blocks,
        int widthInBlocks,
        int heightInBlocks,
        ReadOnlySpan<byte> blockDefinitions,
        ReadOnlySpan<byte> vram,
        IReadOnlyList<Rgba32> palette,
        Span<Rgba32> destination,
        int destinationWidth)
    {
        for (int blockY = 0; blockY < heightInBlocks; blockY++)
        {
            for (int blockX = 0; blockX < widthInBlocks; blockX++)
            {
                int levelOffset = (blockY * widthInBlocks + blockX) * 2;
                // Level entry layout:
                //   bits  0-9  index into the combined 16x16 block-definition table
                //   bit     10 horizontal block flip
                //   bit     11 vertical block flip
                //   bits 12-15 collision/block type (rendering does not consume these)
                ushort levelEntry = BinaryPrimitives.ReadUInt16LittleEndian(blocks[levelOffset..]);
                ExpandedBlockTiles expanded = LevelBlockTilemapExpander.Expand(levelEntry, blockDefinitions);

                for (int outputTileY = 0; outputTileY < 2; outputTileY++)
                {
                    for (int outputTileX = 0; outputTileX < 2; outputTileX++)
                    {
                        // The shared bank-$80 expansion has already swapped children and
                        // toggled their own PPU flip flags, so DrawTile consumes final words.
                        ushort tileEntry = (outputTileX, outputTileY) switch
                        {
                            (0, 0) => expanded.TopLeft,
                            (1, 0) => expanded.TopRight,
                            (0, 1) => expanded.BottomLeft,
                            _ => expanded.BottomRight,
                        };
                        DrawTile(tileEntry, vram, palette, destination, destinationWidth,
                            blockX * 16 + outputTileX * 8,
                            blockY * 16 + outputTileY * 8);
                    }
                }
            }
        }
    }

    /// <summary>Draws one standard SNES BG tilemap entry into the destination image.</summary>
    private static void DrawTile(
        ushort tileEntry,
        ReadOnlySpan<byte> vram,
        IReadOnlyList<Rgba32> palette,
        Span<Rgba32> destination,
        int destinationWidth,
        int destinationX,
        int destinationY)
    {
        // Standard SNES BG tilemap entry layout:
        //   bits  0-9  VRAM tile number
        //   bits 10-12 sub-palette
        //   bit     13 priority
        //   bit     14 horizontal flip
        //   bit     15 vertical flip
        int tileIndex = tileEntry & 0x03ff;
        int tileOffset = tileIndex * BytesPer4BppTile;
        if (tileOffset + BytesPer4BppTile > vram.Length)
            throw new InvalidDataException($"Tile index ${tileIndex:X3} exceeds modeled VRAM.");
        int paletteBase = ((tileEntry >> 10) & 7) * 16;
        bool flipX = (tileEntry & 0x4000) != 0;
        bool flipY = (tileEntry & 0x8000) != 0;

        for (int y = 0; y < 8; y++)
        {
            int sourceY = flipY ? 7 - y : y;
            for (int x = 0; x < 8; x++)
            {
                int sourceX = flipX ? 7 - x : x;
                int colorIndex = Read4BppPixel(vram[tileOffset..], sourceX, sourceY);
                // Index zero is transparent for BG tiles. It reveals the lower BG or the
                // backdrop; it is not necessarily the RGB value stored at paletteBase + 0.
                if (colorIndex == 0)
                    continue;
                destination[(destinationY + y) * destinationWidth + destinationX + x] = palette[paletteBase + colorIndex];
            }
        }
    }

    /// <summary>Reassembles one four-bit palette index from the tile's four bitplanes.</summary>
    private static int Read4BppPixel(ReadOnlySpan<byte> tile, int x, int y)
    {
        int mask = 1 << (7 - x);
        return ((tile[y * 2] & mask) != 0 ? 1 : 0)
             | ((tile[y * 2 + 1] & mask) != 0 ? 2 : 0)
             | ((tile[16 + y * 2] & mask) != 0 ? 4 : 0)
             | ((tile[16 + y * 2 + 1] & mask) != 0 ? 8 : 0);
    }

    /// <summary>Reads one named ROM slice and requires it to be exactly one compressed stream.</summary>
    private static byte[] LoadCompressed(string directory, string name)
    {
        byte[] stored = File.ReadAllBytes(Path.Combine(directory, name));
        return SmCompression.Decompress(stored);
    }
}
