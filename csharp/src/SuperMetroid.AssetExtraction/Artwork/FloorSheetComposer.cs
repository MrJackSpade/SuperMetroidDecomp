using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Composes turning floors' pictures once (#1467): each picture is a rectangle of the cartridge's 128 by 128 floor cells,
/// each cell showing one of 256 eight-by-eight tiles whose pixels name their colours directly from all 256. The floor's
/// image is the whole floor, or the part of it that shows anything (#1649); a smaller picture is a state drawn over part of
/// it. Each is placed by its frame's origin at the floor point its top-left cell lies on. Gameplay and the screens never see
/// the cells: they select the pictures.
/// </summary>
internal static class FloorSheetComposer
{
    /// <summary>Cells along each side of the floor.</summary>
    internal const int Side = 128;

    /// <summary>Cells in the whole floor.</summary>
    internal const int CellCount = Side * Side;

    /// <summary>The tiles a cell can show.</summary>
    internal const int Tiles = 256;

    /// <summary>Pixels along a tile's side.</summary>
    internal const int TilePixels = 8;

    /// <summary>Pixels of one tile, one colour index each.</summary>
    internal const int TileBytes = TilePixels * TilePixels;

    /// <summary>The pixels of every tile a cell can show, one colour index each, tile after tile.</summary>
    internal const int TileSetBytes = Tiles * TileBytes;

    /// <summary>
    /// One picture of a floor: the frame it is installed as, its cells row by row, its width in cells, the floor cell its
    /// top-left cell lies on, and the pixels of the tiles its cells show, one colour index each, tile after tile; a tile
    /// past the pixels given shows colour zero, as characters never loaded hold zeros.
    /// </summary>
    /// <param name="Name">The frame's name.</param>
    /// <param name="Cells">The tile each of its cells shows, row by row.</param>
    /// <param name="Columns">Its width in cells.</param>
    /// <param name="Column">The floor column its left cells lie in.</param>
    /// <param name="Row">The floor row its top cells lie in.</param>
    /// <param name="TilePixels">The pixels of the tiles its cells show.</param>
    internal sealed record FloorPicture(string Name, byte[] Cells, int Columns, int Column, int Row, byte[] TilePixels)
    {
        /// <summary>Its height in cells.</summary>
        internal int Rows => Cells.Length / Columns;
    }

    /// <summary>The picture of the whole floor whose cells, row by row, are <paramref name="map"/>, showing <paramref name="tilePixels"/>' tiles.</summary>
    /// <exception cref="InvalidDataException">The map is not every cell of the floor.</exception>
    internal static FloorPicture Whole(string name, ReadOnlySpan<byte> map, ReadOnlySpan<byte> tilePixels) => map.Length == CellCount
        ? new FloorPicture(name, map.ToArray(), Side, 0, 0, tilePixels.ToArray())
        : throw new InvalidDataException($"Floor {name}'s map holds {map.Length} cells, not {CellCount}.");

    /// <summary>
    /// The picture of the part of the floor that shows anything (#1649): the smallest rectangle of the floor whose cells,
    /// row by row, are <paramref name="map"/>, holding every cell whose tile shows a pixel, showing
    /// <paramref name="tilePixels"/>' tiles, and placed at the floor cell its top-left cell lies on. A tile pixel of colour
    /// zero shows nothing, so the floor beyond the rectangle shows nothing whichever way the floor is turned, and its
    /// pixels within it land where they lie on the whole floor.
    /// </summary>
    /// <exception cref="InvalidDataException">The map is not every cell of the floor, or no cell of it shows anything.</exception>
    internal static FloorPicture ShownPart(string name, ReadOnlySpan<byte> map, ReadOnlySpan<byte> tilePixels)
    {
        if (map.Length != CellCount)
            throw new InvalidDataException($"Floor {name}'s map holds {map.Length} cells, not {CellCount}.");
        var showing = new bool[Tiles];
        for (int tile = 0; tile < Tiles; tile++)
        {
            for (int pixel = 0; pixel < TileBytes; pixel++)
            {
                int source = tile * TileBytes + pixel;
                if (source < tilePixels.Length && tilePixels[source] != 0)
                {
                    showing[tile] = true;
                    break;
                }
            }
        }
        int left = Side, top = Side, right = -1, bottom = -1;
        for (int cell = 0; cell < CellCount; cell++)
        {
            if (!showing[map[cell]])
                continue;
            left = Math.Min(left, cell % Side);
            right = Math.Max(right, cell % Side);
            top = Math.Min(top, cell / Side);
            bottom = Math.Max(bottom, cell / Side);
        }
        if (right < 0)
            throw new InvalidDataException($"No cell of floor {name} shows anything.");
        int columns = right - left + 1;
        var cells = new byte[columns * (bottom - top + 1)];
        for (int index = 0; index < cells.Length; index++)
            cells[index] = map[(top + index / columns) * Side + left + index % columns];
        return new FloorPicture(name, cells, columns, left, top, tilePixels.ToArray());
    }

    /// <summary>
    /// The frontend screen sheet of <paramref name="pictures"/>, each a frame under its name, laid out in rows, every cell
    /// showing its tile's pixels in <paramref name="palette"/>'s colours.
    /// </summary>
    /// <exception cref="InvalidDataException">A picture is not whole rows of cells, leaves the floor, or two share a name.</exception>
    internal static ComposedSpriteSheet Compose(IReadOnlyList<FloorPicture> pictures, ReadOnlySpan<RgbColor> palette)
    {
        PaintedFloors painted = Paint(pictures, palette);
        return painted.Canvas.ToSheet(painted.Colours, painted.Frames, SpriteImageColours.FullPalette);
    }

    /// <summary>
    /// The gameplay room sheet of <paramref name="pictures"/>, laid out as <see cref="Compose"/> lays them, coloured so each
    /// colour group draws in the room's background set of its order.
    /// </summary>
    /// <exception cref="InvalidDataException">A picture is not whole rows of cells, leaves the floor, or two share a name.</exception>
    internal static ComposedSpriteSheet ComposeRoom(IReadOnlyList<FloorPicture> pictures, ReadOnlySpan<RgbColor> palette)
    {
        PaintedFloors painted = Paint(pictures, palette);
        return painted.Canvas.ToRoomSheet(painted.Colours, painted.Frames, SpriteImageColours.FullPalette);
    }

    /// <summary>The painted canvas, its colours and its frames.</summary>
    /// <param name="Canvas">The canvas every picture is painted on.</param>
    /// <param name="Colours">The colours the pixels name.</param>
    /// <param name="Frames">Each picture's frame by its name.</param>
    private sealed record PaintedFloors(RoomTileCanvas Canvas, RgbColor[] Colours, Dictionary<string, SpriteSheetFrameDocument> Frames);

    /// <summary>Paints <paramref name="pictures"/> laid out in rows in <paramref name="palette"/>'s colours.</summary>
    /// <exception cref="InvalidDataException">A picture is not whole rows of cells, leaves the floor, or two share a name.</exception>
    private static PaintedFloors Paint(IReadOnlyList<FloorPicture> pictures, ReadOnlySpan<RgbColor> palette)
    {
        Ensure.NotNull(pictures);
        if (pictures.Count == 0)
            throw new InvalidDataException("A floor sheet holds at least one picture.");
        int width = Math.Max(AffineFloor.SidePixels, pictures.Max(picture => picture.Columns * TilePixels));
        var places = new List<Position16<ImagePixels>>(pictures.Count);
        // A picture showing the same cells as one laid before it is held where that one is, under its own name and origin,
        // so the sheet holds each picture once (#1513).
        var placeOf = new Dictionary<string, int>(StringComparer.Ordinal);
        var firstOf = new int[pictures.Count];
        int left = 0, top = 0, rowHeight = 0;
        for (int index = 0; index < pictures.Count; index++)
        {
            FloorPicture picture = pictures[index];
            if (picture.Columns <= 0 || picture.Cells.Length == 0 || picture.Cells.Length % picture.Columns != 0)
                throw new InvalidDataException($"Floor picture {picture.Name} is not whole rows of cells.");
            if (picture.Column < 0 || picture.Row < 0 || picture.Column + picture.Columns > Side || picture.Row + picture.Rows > Side)
                throw new InvalidDataException($"Floor picture {picture.Name} leaves the floor.");
            string shown = Shown(picture);
            if (placeOf.TryGetValue(shown, out int first))
            {
                firstOf[index] = first;
                places.Add(places[first]);
                continue;
            }
            placeOf.Add(shown, index);
            firstOf[index] = index;
            int pictureWidth = picture.Columns * TilePixels;
            if (left + pictureWidth > width)
            {
                left = 0;
                top += rowHeight;
                rowHeight = 0;
            }
            places.Add(new Position16<ImagePixels>(checked((ushort)left), checked((ushort)top)));
            left += pictureWidth;
            rowHeight = Math.Max(rowHeight, picture.Rows * TilePixels);
        }
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(checked((ushort)width), checked((ushort)(top + rowHeight))));
        RgbColor[] colours = palette[..ImageColourGroup.ColourCountOf(SpriteImageColours.FullPalette)].ToArray();
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        for (int index = 0; index < pictures.Count; index++)
        {
            FloorPicture picture = pictures[index];
            Position16<ImagePixels> corner = places[index];
            ImageColourGroups coloured = ImageColourGroups.None;
            for (int cell = 0; cell < picture.Cells.Length; cell++)
            {
                int cellLeft = corner.X + cell % picture.Columns * TilePixels, cellTop = corner.Y + cell / picture.Columns * TilePixels;
                // A picture held where an earlier one is paints the same pixels; painting them again changes nothing.
                if (firstOf[index] == index)
                    coloured = coloured.Union(PaintTile(canvas, cellLeft, cellTop, picture.Cells[cell], picture.TilePixels, colours));
                else
                    coloured = coloured.Union(TileGroups(picture.Cells[cell], picture.TilePixels));
            }
            var frame = new SpriteSheetFrameDocument
            {
                Corner = corner,
                Size = new Extent16<ImagePixels>(checked((ushort)(picture.Columns * TilePixels)), checked((ushort)(picture.Rows * TilePixels))),
                Origin = Offset16<ImagePixels>.Of(-picture.Column * TilePixels, -picture.Row * TilePixels),
                ColourGroups = coloured.ManifestMembers,
            };
            if (!frames.TryAdd(picture.Name, frame))
                throw new InvalidDataException($"Two floor pictures are named {picture.Name}.");
        }
        return new PaintedFloors(canvas, colours, frames);
    }

    /// <summary>
    /// Checks that every two of <paramref name="pictures"/>, drawn over one floor, cover either the same cells or none in
    /// common, so a picture drawn where another lies replaces it whichever was drawn first.
    /// </summary>
    /// <exception cref="InvalidDataException">Two pictures partly overlap.</exception>
    internal static void RequireSeparateParts(IReadOnlyList<FloorPicture> pictures)
    {
        Ensure.NotNull(pictures);
        foreach (FloorPicture first in pictures)
        {
            foreach (FloorPicture second in pictures)
            {
                bool same = first.Column == second.Column && first.Row == second.Row && first.Columns == second.Columns && first.Rows == second.Rows;
                bool apart = first.Column + first.Columns <= second.Column || second.Column + second.Columns <= first.Column ||
                    first.Row + first.Rows <= second.Row || second.Row + second.Rows <= first.Row;
                if (!same && !apart)
                    throw new InvalidDataException($"Floor pictures {first.Name} and {second.Name} partly overlap.");
            }
        }
    }

    /// <summary>Paints tile <paramref name="tile"/>'s pixels with their top-left at (<paramref name="left"/>, <paramref name="top"/>), returning the colour groups they name.</summary>
    private static ImageColourGroups PaintTile(RoomTileCanvas canvas, int left, int top, int tile, ReadOnlySpan<byte> tilePixels, RgbColor[] colours)
    {
        for (int pixel = 0; pixel < TileBytes; pixel++)
        {
            int source = tile * TileBytes + pixel;
            int index = source < tilePixels.Length ? tilePixels[source] : 0;
            canvas.PaintIndex(new(checked((ushort)(left + pixel % TilePixels)), checked((ushort)(top + pixel / TilePixels))), index, colours);
        }
        return TileGroups(tile, tilePixels);
    }

    /// <summary>What <paramref name="picture"/> shows, spelled for telling pictures apart: its width and each cell's tile's pixels.</summary>
    private static string Shown(FloorPicture picture)
    {
        var shown = new System.Text.StringBuilder().Append(picture.Columns).Append(':');
        foreach (int tile in picture.Cells)
        {
            for (int pixel = 0; pixel < TileBytes; pixel++)
            {
                int source = tile * TileBytes + pixel;
                shown.Append((char)(source < picture.TilePixels.Length ? picture.TilePixels[source] : 0));
            }
        }
        return shown.ToString();
    }

    /// <summary>The colour groups tile <paramref name="tile"/>'s pixels name.</summary>
    private static ImageColourGroups TileGroups(int tile, ReadOnlySpan<byte> tilePixels)
    {
        ImageColourGroups coloured = ImageColourGroups.None;
        for (int pixel = 0; pixel < TileBytes; pixel++)
        {
            int source = tile * TileBytes + pixel;
            int index = source < tilePixels.Length ? tilePixels[source] : 0;
            if (index % ColourGroupSize != 0 && index < ImageColourGroup.HalfColours)
                coloured = coloured.With(ImageColourGroup.OfInk(index));
        }
        return coloured;
    }

    /// <summary>The colours of one colour group, whose first is transparent.</summary>
    private const int ColourGroupSize = 16;
}
