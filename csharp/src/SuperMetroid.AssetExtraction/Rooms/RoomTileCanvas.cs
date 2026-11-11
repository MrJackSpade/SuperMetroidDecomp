using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The pixel planes of a pre-coloured room sheet, painted one four-bpp tile at a time over a tileset's background colours
/// (#1392): each pixel's colour, palette index and colour group, and whether it was painted from a front tile, which decides
/// the part of its picture a split sheet installs it in (#1634) and is not installed itself.
/// </summary>
/// <param name="size">The sheet's size.</param>
internal sealed class RoomTileCanvas(Extent16<ImagePixels> size)
{
    /// <summary>Colours in one palette row.</summary>
    private const int RowColours = 16;

    /// <summary>Pixels along a character's side.</summary>
    internal const int CharacterPixels = 8;

    /// <summary>Bytes in one four-bpp character.</summary>
    internal const int CharacterBytes = 32;

    /// <summary>The sheet's size.</summary>
    internal Extent16<ImagePixels> Size { get; } = size;

    /// <summary>The sheet's width: the pixels from one row to the next.</summary>
    internal int Width => Size.X;

    /// <summary>Each pixel's colour.</summary>
    internal RgbaPixel[] Pixels { get; } = new RgbaPixel[size.X * size.Y];

    /// <summary>Each pixel's colour group's place times sixteen plus its colour; zero where transparent.</summary>
    internal byte[] PaletteIndices { get; } = new byte[size.X * size.Y];

    /// <summary>
    /// One where the pixel was painted from one of its layer's front tiles, zero elsewhere: the part of its picture
    /// <see cref="ToSplitRoomSheet"/> installs it in (#1634). No sheet installs it, and a sheet installed whole ignores it.
    /// </summary>
    internal byte[] FrontTiles { get; } = new byte[size.X * size.Y];

    /// <summary>
    /// Paints <paramref name="tile"/> over <paramref name="character"/>'s 32 bytes with its top-left pixel at
    /// (<paramref name="left"/>, <paramref name="top"/>), and returns the colour groups it was coloured from.
    /// </summary>
    internal ImageColourGroups PaintTile(int left, int top, BackgroundTile tile, ReadOnlySpan<byte> character, RgbColor[] colours) =>
        PaintTile(left, top, new SnesBgTilemapWord(BackgroundTileWords.ToWord(tile)), character, colours);

    /// <summary>
    /// Paints tile word <paramref name="word"/> over <paramref name="character"/>'s 32 bytes with its top-left pixel at
    /// (<paramref name="left"/>, <paramref name="top"/>), and returns the colour groups it was coloured from.
    /// </summary>
    internal ImageColourGroups PaintTile(int left, int top, SnesBgTilemapWord word, ReadOnlySpan<byte> character, RgbColor[] colours)
    {
        Ensure.LengthEqual(character, CharacterBytes);
        ImageColourGroups groups = ImageColourGroups.None;
        for (int y = 0; y < CharacterPixels; y++)
        {
            int row = word.FlipVertically ? CharacterPixels - 1 - y : y;
            for (int x = 0; x < CharacterPixels; x++)
            {
                int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
                int colour = (character[row * 2] >> bit & 1) | (character[row * 2 + 1] >> bit & 1) << 1
                    | (character[16 + row * 2] >> bit & 1) << 2 | (character[17 + row * 2] >> bit & 1) << 3;
                if (colour == 0)
                    continue;
                int pixel = (top + y) * Width + left + x;
                int index = word.PaletteIndex * RowColours + colour;
                PaletteIndices[pixel] = (byte)index;
                Pixels[pixel] = colours[index].ToRgbaPixel();
                FrontTiles[pixel] = word.HasPriority ? (byte)1 : (byte)0;
                ImageColourGroup group = ImageColourGroup.Numbered(word.PaletteIndex);
                groups = groups.With(group);
            }
        }
        return groups;
    }

    /// <summary>
    /// Paints two-bit <paramref name="tile"/> over <paramref name="character"/>'s 16 bytes with its top-left pixel at
    /// (<paramref name="left"/>, <paramref name="top"/>), as <see cref="PaintTwoBppTile(int, int, SnesBgTilemapWord, ReadOnlySpan{byte}, RgbColor[])"/>
    /// paints its word.
    /// </summary>
    internal ImageColourGroups PaintTwoBppTile(int left, int top, BackgroundTile tile, ReadOnlySpan<byte> character, RgbColor[] colours) =>
        PaintTwoBppTile(left, top, new SnesBgTilemapWord(BackgroundTileWords.ToWord(tile)), character, colours);

    /// <summary>
    /// Paints two-bit tile word <paramref name="word"/> over <paramref name="character"/>'s 16 bytes with its top-left pixel
    /// at (<paramref name="left"/>, <paramref name="top"/>), keeping each pixel's row times four plus its colour, colour
    /// zero included, as BG3's sheets do; returns the colour groups it was coloured from.
    /// </summary>
    internal ImageColourGroups PaintTwoBppTile(int left, int top, SnesBgTilemapWord word, ReadOnlySpan<byte> character, RgbColor[] colours)
    {
        Ensure.LengthEqual(character, TwoBppCharacterBytes);
        ImageColourGroups groups = ImageColourGroups.None;
        for (int y = 0; y < CharacterPixels; y++)
        {
            int row = word.FlipVertically ? CharacterPixels - 1 - y : y;
            for (int x = 0; x < CharacterPixels; x++)
            {
                int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
                int colour = (character[row * 2] >> bit & 1) | (character[row * 2 + 1] >> bit & 1) << 1;
                int pixel = (top + y) * Width + left + x;
                int index = word.PaletteIndex * TwoBppRowColours + colour;
                PaletteIndices[pixel] = (byte)index;
                FrontTiles[pixel] = word.HasPriority ? (byte)1 : (byte)0;
                if (colour == 0)
                {
                    Pixels[pixel] = default;
                    continue;
                }
                Pixels[pixel] = colours[index].ToRgbaPixel();
                ImageColourGroup group = ImageColourGroup.OfInk(index);
                groups = groups.With(group);
            }
        }
        return groups;
    }

    /// <summary>
    /// Paints one pixel of colour <paramref name="index"/> at <paramref name="at"/>, as a Mode 7 tile
    /// names its colours directly from all 256; colour zero is transparent.
    /// </summary>
    internal void PaintIndex(Position16<ImagePixels> at, int index, RgbColor[] colours)
    {
        int pixel = at.Y * Width + at.X;
        PaletteIndices[pixel] = checked((byte)index);
        if (index != 0)
            Pixels[pixel] = colours[index].ToRgbaPixel();
    }

    /// <summary>Colours in one two-bit palette row.</summary>
    private const int TwoBppRowColours = 4;

    /// <summary>Bytes in one two-bit character.</summary>
    internal const int TwoBppCharacterBytes = 16;

    /// <summary>
    /// The painted planes as a frontend screen's background sheet coloured as <paramref name="kind"/> with
    /// <paramref name="frames"/> cut from it; its colour groups show the screen's sets of the same identity.
    /// </summary>
    internal ComposedSpriteSheet ToSheet(RgbColor[] colours, Dictionary<string, SpriteSheetFrameDocument> frames,
        SpriteImageColours kind = SpriteImageColours.Background) => Sheet(colours, frames, kind, null);

    /// <summary>
    /// The painted planes as a gameplay background sheet coloured as <paramref name="kind"/> with <paramref name="frames"/>
    /// cut from it, painted from the cartridge's background colours, so each colour group draws in the gameplay
    /// background set of its order (#1464).
    /// </summary>
    internal ComposedSpriteSheet ToRoomSheet(RgbColor[] colours, Dictionary<string, SpriteSheetFrameDocument> frames,
        SpriteImageColours kind = SpriteImageColours.Background) =>
        Sheet(colours, frames, kind, [.. InstalledPictureColours.CartridgeOrderSets]);

    /// <summary>
    /// The painted planes as a gameplay background sheet, as <see cref="ToRoomSheet"/> gives it, with each of
    /// <paramref name="frames"/> that <paramref name="split"/> names, or every frame without it, installed as its two parts
    /// (#1634, <see cref="PictureParts"/>): the pixels painted from back tiles under the frame's own name and those painted
    /// from front tiles under <see cref="PictureParts.InFront"/>'s. A part that inks no pixel is installed as a frame that
    /// shows nothing. A frame whose pixels all lie in one part keeps its rectangle; the in-front part of a frame with pixels
    /// in both is laid in a rectangle of its own below the painted planes. Every other frame is installed whole.
    /// </summary>
    /// <param name="colours">The colours the planes were painted from.</param>
    /// <param name="frames">The frames cut from the planes.</param>
    /// <param name="kind">How the sheet is coloured.</param>
    /// <param name="split">The frames installed as two parts, or null for every frame.</param>
    /// <param name="keepsEmpty">
    /// Whether a frame that inks no opaque pixel is installed whole rather than as two parts that show nothing, as a picture
    /// written over a room's layer is, which covers the layer wherever it lies.
    /// </param>
    /// <exception cref="InvalidDataException">A frame leaves the painted planes, or the in-front parts need more rows than a sheet holds.</exception>
    internal ComposedSpriteSheet ToSplitRoomSheet(RgbColor[] colours, Dictionary<string, SpriteSheetFrameDocument> frames,
        SpriteImageColours kind = SpriteImageColours.Background, IReadOnlySet<string>? split = null, bool keepsEmpty = false) =>
        SplitSheet(colours, frames, kind, [.. InstalledPictureColours.CartridgeOrderSets], split, keepsEmpty);

    /// <summary>The painted planes as a sheet whose colour groups draw in <paramref name="backgroundSets"/>, or in none named.</summary>
    private ComposedSpriteSheet Sheet(RgbColor[] colours, Dictionary<string, SpriteSheetFrameDocument> frames,
        SpriteImageColours kind, InstalledBackgroundSet[]? backgroundSets) =>
        new(Size, Pixels, PaletteIndices, Manifest(colours, frames, null, kind, backgroundSets));

    /// <summary>The composed manifest of a sheet coloured as <paramref name="kind"/> from <paramref name="colours"/>.</summary>
    private static SpriteSheetDocument Manifest(RgbColor[] colours, Dictionary<string, SpriteSheetFrameDocument> frames, string[]? blank,
        SpriteImageColours kind, InstalledBackgroundSet[]? backgroundSets) => new()
        {
            Palette = kind,
            Colours = [.. colours.Select(colour => colour.ToWord())],
            BackgroundSets = backgroundSets,
            Frames = frames,
            Blank = blank,
        };

    /// <summary>
    /// The painted planes with each of <paramref name="frames"/> that <paramref name="split"/> names, or every frame, installed
    /// as its two parts, as <see cref="ToSplitRoomSheet"/> describes, on a sheet coloured as <paramref name="kind"/> whose
    /// colour groups draw in <paramref name="backgroundSets"/>; a frame that inks no opaque pixel is installed whole when
    /// <paramref name="keepsEmpty"/> says so.
    /// </summary>
    /// <remarks>
    /// A part shows nothing at the pixels of the other part's tiles. On a four-bit or full-palette sheet such a pixel is
    /// transparent. A two-bit sheet keeps a colour at every pixel, its clear pixels showing their group's first colour where
    /// their layer is drawn opaque, so there each part keeps every clear pixel of the picture and shows the other part's
    /// opaque pixels as clear pixels of their own group: the parts differ only where one of them is opaque. A two-bit frame
    /// that inks no opaque pixel is installed whole, as its clear pixels still show where its layer is drawn opaque.
    /// </remarks>
    /// <exception cref="InvalidDataException">A frame leaves the painted planes, or the in-front parts need more rows than a sheet holds.</exception>
    private ComposedSpriteSheet SplitSheet(RgbColor[] colours, Dictionary<string, SpriteSheetFrameDocument> frames,
        SpriteImageColours kind, InstalledBackgroundSet[]? backgroundSets, IReadOnlySet<string>? split, bool keepsEmpty)
    {
        Ensure.NotNull(frames);
        bool twoBit = kind == SpriteImageColours.TwoBppBackground;
        var parts = new List<FramePart>(frames.Count);
        // The in-front parts of frames with pixels in both parts are laid in rows below the painted planes, each row as tall
        // as its tallest part.
        int shelfLeft = 0, shelfTop = Size.Y, shelfHeight = 0;
        foreach ((string name, SpriteSheetFrameDocument frame) in frames)
        {
            if (frame.Corner.X + frame.Size.X > Size.X || frame.Corner.Y + frame.Size.Y > Size.Y)
                throw new InvalidDataException($"Frame {name} at ({frame.Corner.X}, {frame.Corner.Y}) leaves its {Size.X}x{Size.Y} sheet.");
            if (split is not null && !split.Contains(name))
            {
                parts.Add(new FramePart(name, frame, null, null));
                continue;
            }
            InkedParts inked = InkedPartsOf(frame);
            // A frame kept whole though it inks nothing opaque is its part behind, and has no part in front.
            if ((twoBit || keepsEmpty) && inked == InkedParts.None)
                inked = InkedParts.Behind;
            Position16<ImagePixels>? moved = null;
            if (inked == InkedParts.Both)
            {
                if (shelfLeft + frame.Size.X > Size.X)
                {
                    shelfTop += shelfHeight;
                    shelfLeft = 0;
                    shelfHeight = 0;
                }
                moved = new Position16<ImagePixels>(checked((ushort)shelfLeft), checked((ushort)shelfTop));
                shelfLeft += frame.Size.X;
                shelfHeight = Math.Max(shelfHeight, frame.Size.Y);
            }
            parts.Add(new FramePart(name, frame, inked, moved));
        }
        if (shelfTop + shelfHeight > RgbaPng.MaximumDimension)
            throw new InvalidDataException($"A {Size.X}x{Size.Y} sheet's in-front parts need {shelfTop + shelfHeight} rows, past the {RgbaPng.MaximumDimension} a sheet holds.");
        var size = new Extent16<ImagePixels>(Size.X, checked((ushort)(shelfTop + shelfHeight)));
        var pixels = new RgbaPixel[size.X * size.Y];
        var paletteIndices = new byte[size.X * size.Y];
        Pixels.CopyTo(pixels, 0);
        PaletteIndices.CopyTo(paletteIndices, 0);
        var installed = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        var blank = new List<string>();
        foreach (FramePart part in parts)
        {
            if (part.Inked is not { } inked)
            {
                installed.Add(part.Name, part.Frame);
                continue;
            }
            string inFront = PictureParts.InFront(part.Name);
            switch (inked)
            {
                case InkedParts.None:
                    blank.Add(part.Name);
                    blank.Add(inFront);
                    break;
                case InkedParts.Behind:
                    installed.Add(part.Name, part.Frame);
                    blank.Add(inFront);
                    break;
                case InkedParts.InFront:
                    blank.Add(part.Name);
                    installed.Add(inFront, part.Frame);
                    break;
                case InkedParts.Both:
                    Position16<ImagePixels> corner = part.Moved ?? throw new InvalidOperationException($"Frame {part.Name}'s in-front part was given no rectangle.");
                    SeparateParts(part.Frame, corner, size.X, twoBit, pixels, paletteIndices);
                    installed.Add(part.Name, part.Frame);
                    installed.Add(inFront, part.Frame with { Corner = corner });
                    break;
                default:
                    throw new InvalidOperationException($"Undefined inked parts {inked}.");
            }
        }
        return new ComposedSpriteSheet(size, pixels, paletteIndices,
            Manifest(colours, installed, blank.Count == 0 ? null : [.. blank], kind, backgroundSets));
    }

    /// <summary>
    /// Which parts of <paramref name="frame"/>'s rectangle ink an opaque pixel: those painted from back tiles, from front
    /// tiles, both or neither.
    /// </summary>
    private InkedParts InkedPartsOf(SpriteSheetFrameDocument frame)
    {
        bool behind = false, inFront = false;
        for (int y = 0; y < frame.Size.Y; y++)
        {
            int row = (frame.Corner.Y + y) * Width + frame.Corner.X;
            for (int x = 0; x < frame.Size.X; x++)
            {
                int pixel = row + x;
                if (Pixels[pixel].A == 0)
                    continue;
                if (FrontTiles[pixel] != 0)
                    inFront = true;
                else
                    behind = true;
            }
        }
        return behind ? inFront ? InkedParts.Both : InkedParts.Behind
            : inFront ? InkedParts.InFront : InkedParts.None;
    }

    /// <summary>
    /// Separates <paramref name="frame"/>'s two parts on the split sheet's planes, <paramref name="width"/> pixels across:
    /// its rectangle keeps the part behind and the rectangle at <paramref name="corner"/> takes the part in front, each
    /// showing nothing at the other part's opaque pixels, as <see cref="SplitSheet"/> describes for a two-bit sheet.
    /// </summary>
    private void SeparateParts(SpriteSheetFrameDocument frame, Position16<ImagePixels> corner, int width, bool twoBit,
        RgbaPixel[] pixels, byte[] paletteIndices)
    {
        for (int y = 0; y < frame.Size.Y; y++)
        {
            for (int x = 0; x < frame.Size.X; x++)
            {
                int from = (frame.Corner.Y + y) * Width + frame.Corner.X + x;
                int to = (corner.Y + y) * width + corner.X + x;
                bool opaque = Pixels[from].A != 0;
                byte nothing = twoBit ? (byte)(PaletteIndices[from] - PaletteIndices[from] % TwoBppRowColours) : (byte)0;
                if (opaque && FrontTiles[from] != 0)
                {
                    pixels[to] = Pixels[from];
                    paletteIndices[to] = PaletteIndices[from];
                    pixels[from] = default;
                    paletteIndices[from] = nothing;
                }
                else if (opaque)
                    paletteIndices[to] = nothing;
                else
                    paletteIndices[to] = PaletteIndices[from];
            }
        }
    }

    /// <summary>Which parts of a frame ink an opaque pixel.</summary>
    private enum InkedParts
    {
        /// <summary>Neither: the frame shows nothing.</summary>
        None,

        /// <summary>Only the pixels painted from back tiles.</summary>
        Behind,

        /// <summary>Only the pixels painted from front tiles.</summary>
        InFront,

        /// <summary>Pixels painted from both.</summary>
        Both,
    }

    /// <summary>One composed frame as the split sheet installs it.</summary>
    /// <param name="Name">The frame's composed name, which its behind part keeps.</param>
    /// <param name="Frame">The frame's composed document.</param>
    /// <param name="Inked">Which of its parts ink an opaque pixel; null for a frame installed whole.</param>
    /// <param name="Moved">The corner its in-front part is laid at when both parts ink a pixel; null otherwise.</param>
    private sealed record FramePart(string Name, SpriteSheetFrameDocument Frame, InkedParts? Inked, Position16<ImagePixels>? Moved);
}
