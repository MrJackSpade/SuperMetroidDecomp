using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Builds the cartridge's copied-BG1/revealed-BG2 screens in native write order.</summary>
public static class XrayRevealTilemap
{
    /// <summary>
    /// Ports $91:CB8E-CFD9's base-copy and terrain-reveal passes. Item and special-room
    /// overlays are applied afterwards by their respective owners, as on the cartridge.
    /// </summary>
    /// <param name="level">The room whose blocks the reveal resolves.</param>
    /// <param name="bg1">
    /// The BG1 plane the base copy reads (#1351): each of its 64x32 tilemap cells shows the plane at the scrolled position
    /// nearest the screen, where the cartridge read the BG1 tilemap back out of VRAM.
    /// </param>
    /// <param name="bg1Scroll">BG1's scroll register.</param>
    /// <param name="layer1Position">Layer one's position, which selects the revealed blocks.</param>
    /// <param name="area">The room's area; only Brinstar shows Brinstar-only reveals.</param>
    /// <param name="visuals">Installed reveal art, or null for the stock table.</param>
    public static BackgroundTile[] Build(RoomLevelData level, IBgTileSource bg1,
        Position16<RoomPixels> bg1Scroll, Position16<RoomPixels> layer1Position,
        AreaId area, XrayRevealVisualCatalog? visuals = null)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(bg1);
        Position16<BgPlanePixels> scroll = bg1Scroll.AsBgPlaneScroll();
        var result = new BackgroundTile[XrayTilemapLayout.BufferWords];
        for (int row = 0; row < XrayTilemapLayout.MetatileRows; row++)
        for (int col = 0; col < XrayTilemapLayout.MetatileColumns; col++)
        {
            int sourceX = ((bg1Scroll.X >> 4) + col) * 2;
            int sourceY = ((bg1Scroll.Y >> 4) + row) * 2;
            int destination = FirstCellOf(col, row);
            Write(destination, ReadBg1(sourceX, sourceY));
            Write(destination + 1, ReadBg1(sourceX + 1, sourceY));
            Write(destination + 32, ReadBg1(sourceX, sourceY + 1));
            Write(destination + 33, ReadBg1(sourceX + 1, sourceY + 1));
        }
        int firstBlock = (byte)(layer1Position.Y >> 4) * level.Size.X + (layer1Position.X >> 4);
        for (int row = 0; row < XrayTilemapLayout.MetatileRows; row++)
        {
            int first = firstBlock + row * level.Size.X;
            // The prior column may own a 2-wide reveal crossing the left edge. Its
            // right half is copied before this row's own commands overwrite it.
            if (first != 0 && Lookup(first - 1) is { } predecessor)
            {
                if (predecessor.Command == XrayRevealCommand.CopySquare)
                    CopyMetatile(FirstCellOf(0, row) + XrayTilemapLayout.MetatileRowStride, predecessor.BottomRight);
                if (predecessor.Command is XrayRevealCommand.CopyWide or XrayRevealCommand.CopySquare)
                    CopyMetatile(FirstCellOf(0, row), predecessor.TopRight);
            }
            for (int col = 0; col < XrayTilemapLayout.MetatileColumns; col++)
            {
                int index = first + col;
                if (Lookup(index) is not { } reveal) continue;
                int destination = FirstCellOf(col, row);
                if (reveal.Command is XrayRevealCommand.HorizontalExtension or XrayRevealCommand.VerticalExtension)
                {
                    if (XrayRevealExtensions.Resolve(level, index, visuals) is { } replacement)
                        CopyMetatile(destination, replacement);
                    continue;
                }
                if (reveal.Command == XrayRevealCommand.CopyBrinstar && area != AreaId.Brinstar)
                    continue;
                CopyMetatile(destination, reveal.TopLeft);
                // The last column of screen one suppresses right-hand writes. Screen
                // two is visited separately with native remaining-count zero, not one.
                bool right = col != XrayTilemapLayout.MetatileColumns - 2;
                if (right && reveal.Command is XrayRevealCommand.CopyWide or XrayRevealCommand.CopySquare)
                    CopyMetatile(destination + 2, reveal.TopRight);
                if (reveal.Command is XrayRevealCommand.CopyTall or XrayRevealCommand.CopySquare)
                    CopyMetatile(destination + XrayTilemapLayout.MetatileRowStride, reveal.BottomLeft);
                if (right && reveal.Command == XrayRevealCommand.CopySquare)
                    CopyMetatile(destination + XrayTilemapLayout.MetatileRowStride + 2, reveal.BottomRight);
            }
        }
        return result;

        XrayRevealDefinition? Lookup(int index)
        {
            RoomCollisionBlock block = level.GetPlmCollisionBlockByIndex(index);
            return visuals is not null
                ? visuals.Apply(block.CollisionType, block.Bts.Value)
                : XrayRevealTable.Find(block.CollisionType, block.Bts.Value);
        }
        BackgroundTile ReadBg1(int x, int y)
        {
            var cell = new Position16<BgPlanePixels>(
                unchecked((ushort)(scroll.X + (((x & 63) * ScreenGeometry.BackgroundTileSizePixels - scroll.X) & (XrayTilemapLayout.TilemapPixelWidth - 1)))),
                unchecked((ushort)(scroll.Y + (((y & 31) * ScreenGeometry.BackgroundTileSizePixels - scroll.Y) & (XrayTilemapLayout.TilemapPixelHeight - 1)))));
            return bg1.TryTileAt(cell, out BackgroundTile tile) ? tile : default;
        }
        void CopyMetatile(int destination, int metatile)
        {
            ReadOnlySpan<BackgroundTile> definitions = level.BlockDefinitions.Span;
            int first = metatile * RoomLevelPlane.DefinitionTiles;
            if (first + RoomLevelPlane.DefinitionTiles > definitions.Length)
                throw new InvalidDataException($"X-ray metatile ${metatile:X4} is outside the room definition table.");
            Write(destination, definitions[first]); Write(destination + 1, definitions[first + 1]);
            Write(destination + 32, definitions[first + 2]); Write(destination + 33, definitions[first + 3]);
        }
        void Write(int index, BackgroundTile tile)
        {
            // Bottom-edge multi-block commands can write into subsequent native scratch
            // RAM. Only these two transferred screens belong to this display projection.
            if (index < result.Length) result[index] = tile;
        }
    }

    /// <summary>The first cell of the reveal's block at <paramref name="column"/>, <paramref name="row"/>: the final column maps into the second copied BG screen rather than past screen one's edge.</summary>
    internal static int FirstCellOf(int column, int row) => row * XrayTilemapLayout.MetatileRowStride +
        (column == XrayTilemapLayout.MetatileColumns - 1 ? XrayTilemapLayout.ScreenWords : column * 2);
}
