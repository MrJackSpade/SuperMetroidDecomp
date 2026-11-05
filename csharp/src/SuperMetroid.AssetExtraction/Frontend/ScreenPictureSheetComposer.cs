using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Pre-assembles and pre-colours a frontend screen's pictures (#1467): each subject's tiles painted once from the screen's
/// characters under the screen's standard palette, in each tile's colour variant, mirroring and priority tier, packed onto
/// shelves across one sheet. Each frame's origin is the page's corner, so the game places a page's pictures at the page's
/// corner and each lands where the cartridge's page shows it.
/// </summary>
internal static class ScreenPictureSheetComposer
{
    /// <summary>Pixels along a tile's side.</summary>
    private const int TilePixels = RoomTileCanvas.CharacterPixels;

    /// <summary>The tiles across the widest sheet the installation holds.</summary>
    private const int PackedTiles = RgbaPng.MaximumDimension / TilePixels;

    /// <summary>Composes the sheet picturing <paramref name="subjects"/> over <paramref name="characters"/> under <paramref name="palette"/>.</summary>
    /// <exception cref="InvalidDataException">Two subjects share a name, or the pictures need more than one sheet.</exception>
    internal static ComposedSpriteSheet Compose(IReadOnlyList<ScreenPictureSubject> subjects, ScreenCharacters characters, ReadOnlySpan<RgbColor> palette)
    {
        PaintedScreen painted = Paint(subjects, characters, palette);
        return painted.Canvas.ToSheet(painted.Colours, painted.Frames, characters.Colours);
    }

    /// <summary>
    /// Composes the sheet picturing <paramref name="subjects"/> as <see cref="Compose"/> does, except that a subject painting
    /// no pixel over <paramref name="characters"/> shows nothing: the sheet holds it without a picture, and the screen draws
    /// nothing for it (#1513). Whether a subject shows anything follows from the installed page data and characters, so
    /// the import decides it here.
    /// </summary>
    /// <exception cref="InvalidDataException">Two subjects share a name, or the pictures need more than one sheet.</exception>
    internal static ComposedSpriteSheet ComposeHoldingBlanks(IReadOnlyList<ScreenPictureSubject> subjects, ScreenCharacters characters,
        ReadOnlySpan<RgbColor> palette)
    {
        Ensure.NotNull(subjects);
        Ensure.NotNull(characters);
        var pictured = new List<ScreenPictureSubject>(subjects.Count);
        var blank = new List<string>();
        foreach (ScreenPictureSubject subject in subjects)
        {
            if (ShowsAnything(subject, characters))
                pictured.Add(subject);
            else
                blank.Add(subject.Name);
        }
        ComposedSpriteSheet sheet = Compose(pictured, characters, palette);
        return blank.Count == 0 ? sheet : sheet with { Manifest = sheet.Manifest with { Blank = [.. blank] } };
    }

    /// <summary>Whether any cell of <paramref name="subject"/> paints a pixel other than colour zero over <paramref name="characters"/>.</summary>
    private static bool ShowsAnything(ScreenPictureSubject subject, ScreenCharacters characters)
    {
        foreach (BackgroundTile? cell in subject.Tiles)
        {
            if (cell is { } tile && characters.TryCharacter(tile.Tile, out ReadOnlySpan<byte> character) && character.IndexOfAnyExcept((byte)0) >= 0)
                return true;
        }
        return false;
    }

    /// <summary>A screen's painted pictures: the canvas they are painted on, the colours its pixels name and each picture's frame.</summary>
    /// <param name="Canvas">The canvas every picture is painted on.</param>
    /// <param name="Colours">The colours the pixels name.</param>
    /// <param name="Frames">Each picture's frame by its name.</param>
    internal sealed record PaintedScreen(RoomTileCanvas Canvas, RgbColor[] Colours, Dictionary<string, SpriteSheetFrameDocument> Frames);

    /// <summary>Paints <paramref name="subjects"/> over <paramref name="characters"/> in <paramref name="palette"/>'s colours, packed onto shelves.</summary>
    /// <exception cref="InvalidDataException">Two subjects share a name, or the pictures need more than one sheet.</exception>
    internal static PaintedScreen Paint(IReadOnlyList<ScreenPictureSubject> subjects, ScreenCharacters characters, ReadOnlySpan<RgbColor> palette)
    {
        Ensure.NotNull(subjects);
        Ensure.NotNull(characters);
        // Pictures are packed onto shelves across the sheet, each shelf as tall as its tallest picture.
        var corners = new Position16<TilemapCells>[subjects.Count];
        int x = 0, y = 0, shelfRows = 0, widest = 1;
        for (int index = 0; index < subjects.Count; index++)
        {
            Extent16<TilemapCells> size = subjects[index].Size;
            if (x + size.X > PackedTiles)
            {
                y += shelfRows;
                x = 0;
                shelfRows = 0;
            }
            corners[index] = new((ushort)x, (ushort)y);
            x += size.X;
            widest = Math.Max(widest, x);
            shelfRows = Math.Max(shelfRows, size.Y);
        }
        int rows = Math.Max(1, y + shelfRows);
        if (rows > PackedTiles)
            throw new InvalidDataException($"A frontend screen's {subjects.Count} pictures need more than one {RgbaPng.MaximumDimension}-pixel sheet.");
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(checked((ushort)(widest * TilePixels)), checked((ushort)(rows * TilePixels))));
        RgbColor[] colours = palette[..SpriteImage.SpriteColourCount].ToArray();
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        WrittenPictureLayer depth = characters.Depth == TileInkDepth.SixteenColours ? WrittenPictureLayer.WrittenOverRoom : WrittenPictureLayer.FxOverlayAndHud;
        for (int index = 0; index < subjects.Count; index++)
        {
            ScreenPictureSubject subject = subjects[index];
            Position16<TilemapCells> corner = corners[index];
            for (int cell = 0; cell < subject.Tiles.Length; cell++)
            {
                if (subject.Tiles[cell] is not { } tile || !characters.TryCharacter(tile.Tile, out ReadOnlySpan<byte> character))
                    continue;
                int left = (corner.X + cell % subject.Size.X) * TilePixels, top = (corner.Y + cell / subject.Size.X) * TilePixels;
                if (characters.Depth == TileInkDepth.SixteenColours)
                    canvas.PaintTile(left, top, tile, character, colours);
                else
                    canvas.PaintTwoBppTile(left, top, tile, character, colours);
            }
            SpriteSheetFrameDocument frame = WrittenPictureSheetComposer.Frame(canvas,
                new Position16<ImagePixels>(checked((ushort)(corner.X * TilePixels)), checked((ushort)(corner.Y * TilePixels))),
                new Extent16<ImagePixels>(checked((ushort)(subject.Size.X * TilePixels)), checked((ushort)(subject.Size.Y * TilePixels))), depth);
            frame = frame with
            {
                Origin = new Offset16<ImagePixels>(checked((short)-(subject.Offset.X * TilePixels)), checked((short)-(subject.Offset.Y * TilePixels))),
            };
            if (!frames.TryAdd(subject.Name, frame))
                throw new InvalidDataException($"A frontend screen pictures {subject.Name} twice.");
        }
        return new PaintedScreen(canvas, colours, frames);
    }
}
