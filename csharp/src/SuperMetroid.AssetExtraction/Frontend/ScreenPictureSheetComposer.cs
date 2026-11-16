using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Pre-assembles and pre-colours a frontend screen's pictures (#1467): each subject's tiles painted once from the screen's
/// characters under the screen's standard palette, in each tile's colour variant and mirroring, packed onto shelves across
/// one sheet. Each picture is installed whole: a screen draws every tile of it in one pass, so no tile lies in front of
/// another (#1634). Each frame's origin is the page's corner, so the game places a page's pictures at the page's
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
        // Each picture is held once (#1513): subjects painting the same cells over the same characters share one
        // rectangle, each frame placing it at its own offset, as the reserve digits share one picture per digit.
        var held = new Dictionary<string, int>(StringComparer.Ordinal);
        var pictureOf = new int[subjects.Count];
        var pictured = new List<ScreenPictureSubject>();
        for (int index = 0; index < subjects.Count; index++)
        {
            string content = Content(subjects[index], characters);
            if (!held.TryGetValue(content, out int picture))
            {
                picture = pictured.Count;
                held.Add(content, picture);
                pictured.Add(subjects[index]);
            }
            pictureOf[index] = picture;
        }
        // Pictures are packed onto shelves across the sheet, each shelf as tall as its tallest picture.
        var corners = new Position16<CharacterCells>[pictured.Count];
        int x = 0, y = 0, shelfRows = 0, widest = 1;
        for (int index = 0; index < pictured.Count; index++)
        {
            Extent16<CharacterCells> size = pictured[index].Size;
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
        RgbColor[] colours = palette[..ImageColourGroup.HalfColours].ToArray();
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        var paintedFrames = new SpriteSheetFrameDocument[pictured.Count];
        for (int index = 0; index < pictured.Count; index++)
        {
            ScreenPictureSubject subject = pictured[index];
            Position16<CharacterCells> corner = corners[index];
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
            paintedFrames[index] = WrittenPictureSheetComposer.Frame(canvas,
                new Position16<ImagePixels>(checked((ushort)(corner.X * TilePixels)), checked((ushort)(corner.Y * TilePixels))),
                new Extent16<ImagePixels>(checked((ushort)(subject.Size.X * TilePixels)), checked((ushort)(subject.Size.Y * TilePixels))), characters.Colours);
        }
        // Every subject names its picture's rectangle, placed from its own offset.
        for (int index = 0; index < subjects.Count; index++)
        {
            ScreenPictureSubject subject = subjects[index];
            SpriteSheetFrameDocument frame = paintedFrames[pictureOf[index]] with
            {
                Origin = new Offset16<ImagePixels>(checked((short)-(subject.Offset.X * TilePixels)), checked((short)-(subject.Offset.Y * TilePixels))),
            };
            if (!frames.TryAdd(subject.Name, frame))
                throw new InvalidDataException($"A frontend screen pictures {subject.Name} twice.");
        }
        return new PaintedScreen(canvas, colours, frames);
    }

    /// <summary>
    /// What <paramref name="subject"/> paints, spelled for telling pictures apart: its size and each cell's colour variant,
    /// mirroring and the pixels of the character it shows over <paramref name="characters"/>, wherever that character lies,
    /// whichever tier the cell is written in, since a whole picture keeps none (#1634); a cell painting nothing is spelled
    /// empty.
    /// </summary>
    private static string Content(ScreenPictureSubject subject, ScreenCharacters characters)
    {
        var content = new System.Text.StringBuilder();
        content.Append(subject.Size.X).Append('x').Append(subject.Size.Y);
        foreach (BackgroundTile? cell in subject.Tiles)
        {
            content.Append('|');
            if (cell is not { } tile || !characters.TryCharacter(tile.Tile, out ReadOnlySpan<byte> character))
                continue;
            content.Append(tile.ColourVariant).Append(tile.FlipHorizontally ? 'h' : '-').Append(tile.FlipVertically ? 'v' : '-')
                .Append(Convert.ToBase64String(character));
        }
        return content.ToString();
    }
}
