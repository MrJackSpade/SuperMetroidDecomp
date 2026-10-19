using System.Collections.Immutable;
using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Constructed room backgrounds for render tests (#1346): a blank room, or a seeded one over the screen.</summary>
internal static class RoomBackgroundsFixtures
{
    /// <summary>Blocks along each side of a fixture room.</summary>
    private const int SideBlocks = 32;

    /// <summary>Block definitions a fixture room loads: every ten-bit visual index.</summary>
    private const int Definitions = 1024;

    /// <summary>A room whose BG1 shows tile word zero over the whole unscrolled screen, no BG2 plane, and a zero BG3 plane.</summary>
    internal static RoomBackgrounds Blank() => Uniform(default);

    /// <summary>A BG3 plane of zero words.</summary>
    internal static Layer3Plane BlankBg3() => new(new ushort[Layer3Plane.Rows * Layer3Plane.Columns]);

    /// <summary>
    /// A room whose BG1 shows <paramref name="tile"/> in every cell over the whole unscrolled screen, as a VRAM BG1
    /// tilemap filled with that word did, except the listed eight-pixel <paramref name="cells"/>; no BG2 plane; and a zero
    /// BG3 plane.
    /// </summary>
    internal static RoomBackgrounds Uniform(SnesBgTilemapWord tile,
        params ReadOnlySpan<(int Column, int Row, SnesBgTilemapWord Tile)> cells)
    {
        // Each block's four cells, top left, top right, bottom left, bottom right, as its definition stores them.
        var blockCells = new ushort[SideBlocks * SideBlocks][];
        for (int block = 0; block < blockCells.Length; block++)
            blockCells[block] = [tile.Raw, tile.Raw, tile.Raw, tile.Raw];
        foreach (var (column, row, cell) in cells)
            blockCells[row / 2 * SideBlocks + column / 2][row % 2 * 2 + column % 2] = cell.Raw;
        var definitionIndices = new Dictionary<(ushort, ushort, ushort, ushort), ushort>();
        var definitions = new byte[Definitions * RoomLevelPlane.DefinitionBytes];
        var blocks = new ushort[blockCells.Length];
        for (int block = 0; block < blocks.Length; block++)
        {
            ushort[] words = blockCells[block];
            var key = (words[0], words[1], words[2], words[3]);
            if (!definitionIndices.TryGetValue(key, out ushort index))
            {
                index = (ushort)definitionIndices.Count;
                definitionIndices.Add(key, index);
                for (int corner = 0; corner < words.Length; corner++)
                    BinaryPrimitives.WriteUInt16LittleEndian(
                        definitions.AsSpan(index * RoomLevelPlane.DefinitionBytes + corner * 2), words[corner]);
            }
            blocks[block] = index;
        }
        return new(new RoomBackgroundLayer(new RoomLevelPlane(new Extent16<RoomBlocks>(SideBlocks, SideBlocks),
            blocks, [.. definitions]), default), null, BlankBg3());
    }

    /// <summary>
    /// A room of random blocks over random definitions, with a random BG3 plane. Each plane's offset places a random point of the room at the
    /// given scroll, so the screen it is scrolled to shows mostly room and some edge. Optionally adds a door's source
    /// screen and words written over BG1.
    /// </summary>
    /// <param name="random">The seeded source.</param>
    /// <param name="bg1Scroll">The BG1 scroll registers the room is shown through.</param>
    /// <param name="bg2Scroll">The BG2 scroll registers, or null for no BG2 plane.</param>
    /// <param name="door">Whether a door's source screen covers part of BG1.</param>
    /// <param name="overlay">Whether words are written over part of BG1.</param>
    internal static RoomBackgrounds Seeded(Random random, Position16<BgPlanePixels> bg1Scroll,
        Position16<BgPlanePixels>? bg2Scroll = null, bool door = false, bool overlay = false)
    {
        var bytes = new byte[Definitions * RoomLevelPlane.DefinitionBytes];
        random.NextBytes(bytes);
        ImmutableArray<byte> definitions = [.. bytes];
        RoomBackgroundLayer Layer(Position16<BgPlanePixels> scroll)
        {
            var blocks = new ushort[SideBlocks * SideBlocks];
            for (int index = 0; index < blocks.Length; index++)
                blocks[index] = (ushort)random.Next(ushort.MaxValue + 1);
            var inside = new Position16<BgPlanePixels>((ushort)random.Next(SideBlocks * RoomLevelPlane.BlockPixels / 2),
                (ushort)random.Next(SideBlocks * RoomLevelPlane.BlockPixels / 2));
            return new RoomBackgroundLayer(new RoomLevelPlane(new Extent16<RoomBlocks>(SideBlocks, SideBlocks), blocks, definitions),
                scroll - inside);
        }
        RoomBackgroundLayer bg1 = Layer(bg1Scroll);
        RoomBackgroundLayer? bg2 = bg2Scroll is { } scroll2 ? Layer(scroll2) : null;
        RoomDoorBackground? source = door
            ? RoomDoorBackground.Begin(Layer(bg1Scroll), bg1Scroll + new Offset16<BgPlanePixels>((short)random.Next(-128, 128), 0))
            : null;
        RoomTilemapOverlay? written = null;
        if (overlay)
        {
            var words = new ushort[random.Next(8, 24)];
            for (int index = 0; index < words.Length; index++)
                words[index] = (ushort)random.Next(ushort.MaxValue + 1);
            written = RoomTilemapOverlay.Empty.With(random.Next(RoomTilemapOverlay.WordCount - words.Length), words);
        }
        var bg3 = new ushort[Layer3Plane.Rows * Layer3Plane.Columns];
        for (int index = 0; index < bg3.Length; index++)
            bg3[index] = (ushort)random.Next(ushort.MaxValue + 1);
        return new RoomBackgrounds(bg1, bg2, new Layer3Plane(bg3), source, written);
    }
}
