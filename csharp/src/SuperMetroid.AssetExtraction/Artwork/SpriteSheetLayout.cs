using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A sprite sheet laid out before any pixel is painted (#1392): its distinct pictures in order, each one complete
/// pre-assembled picture of a frame that is drawn as it is. Frames whose parts read the same characters at the same
/// places are composed once; each composed frame is cropped to its parts, and frames whose crops ink the same picture,
/// whatever origin each is drawn from, share one rectangle, so each picture is held and painted once however many frames
/// show it (#1513). The pictures are packed as <see cref="SheetPacking"/> packs a sheet's pictures. A frame with no parts is an empty frame. Its <see cref="Identity"/> covers everything painting reads,
/// so two layouts with one identity paint the same sheet and the second need not be painted (#1490); the pictures are
/// composed and packed only when the sheet's size or paint is first asked for.
/// </summary>
internal sealed class SpriteSheetLayout
{
    /// <summary>Colours in one sprite palette row.</summary>
    private const int RowColours = 16;

    /// <summary>What the sheet composes.</summary>
    private readonly SpriteSheetSources sources;

    /// <summary>The row of parts that do not choose their own.</summary>
    private readonly int palette;

    /// <summary>The game's priority of parts that do not choose their own, from which each frame's depth is decided.</summary>
    private readonly int priority;

    /// <summary>The distinct frame compositions in order.</summary>
    private readonly SheetFrameSource[] laid;

    /// <summary>Each frame of the sources, in order, with the laid frame composition it shows.</summary>
    private readonly NamedFrameImage[] frameImages;

    /// <summary>The laid compositions' pictures and where each is held, composed when first asked for.</summary>
    private readonly Lazy<LaidPictures> pictures;

    /// <summary>
    /// For each row shade, the colour each ink its painted pixels hold shows under it, and whether the shade's draw takes
    /// that ink's pixels from another row than their own (#1512).
    /// </summary>
    private readonly Dictionary<int, RowShadedInk>[] rowShadeInks;

    /// <summary>Whether each ink's row shading is recorded: it follows from the ink alone, so it is recorded once.</summary>
    private readonly bool[] rowShadedInks = new bool[8 * RowColours];

    /// <summary>Lays out a sheet's distinct frame images in order.</summary>
    /// <param name="sources">Each frame's installed name, parts and characters, and the eight sprite palette rows as the
    /// family is drawn with them, sixteen colours each.</param>
    /// <param name="palette">The row of parts that do not choose their own.</param>
    /// <param name="priority">The game's priority of parts that do not choose their own, from which each frame's depth is decided.</param>
    internal SpriteSheetLayout(SpriteSheetSources sources, int palette, int priority)
    {
        this.sources = Ensure.NotNull(sources);
        this.palette = palette;
        this.priority = priority;
        rowShadeInks = [.. (sources.RowShades ?? []).Select(_ => new Dictionary<int, RowShadedInk>())];
        IReadOnlyList<SheetFrameSource> frames = sources.Frames;
        ReadOnlySpan<RgbColor> spriteColours = sources.SpriteColours;
        Ensure.LengthEqual(spriteColours, 8 * RowColours);
        // Each distinct frame image: frames whose parts paint the same pixels at the same places share one image, whichever
        // characters' copy they read them from.
        var frameImageOf = new Dictionary<string, int>(StringComparer.Ordinal);
        var uniqueFrames = new List<SheetFrameSource>();
        frameImages = new NamedFrameImage[frames.Count];
        for (int index = 0; index < frames.Count; index++)
        {
            SheetFrameSource frame = frames[index];
            // The PPU draws only its screen's two object sizes, so a part of any other size cannot stand for an object.
            foreach (SpriteFramePart part in frame.Frame.Parts)
            {
                if (part.Size != sources.ObjectSizes.Small && part.Size != sources.ObjectSizes.Large)
                    throw new InvalidDataException($"Sheet frame {frame.Name} has a {part.Size.X}x{part.Size.Y} part, not an object size of its screen.");
            }
            string content = FrameContent(frame.Frame, frame.Palette ?? palette, frame.Characters);
            if (!frameImageOf.TryGetValue(content, out int laidFrame))
            {
                laidFrame = uniqueFrames.Count;
                frameImageOf.Add(content, laidFrame);
                uniqueFrames.Add(frame);
            }
            frameImages[index] = new NamedFrameImage(frame.Name, laidFrame);
        }
        laid = [.. uniqueFrames];
        pictures = new Lazy<LaidPictures>(LayOutPictures);
        Identity = IdentityOf();
    }

    /// <summary>The sheet's width and height.</summary>
    internal Extent16<ImagePixels> Size => pictures.Value.Size;

    /// <summary>
    /// Composes each laid frame's picture, keeps each distinct picture once, and packs the distinct pictures onto the
    /// sheet. Composing records which row each inked colour shows under each row shade.
    /// </summary>
    /// <exception cref="InvalidDataException">The pictures do not fit in an artwork image.</exception>
    private LaidPictures LayOutPictures()
    {
        var composed = new FramePicture[laid.Length];
        var pictureOf = new int[laid.Length];
        var pictureIndex = new Dictionary<string, int>(StringComparer.Ordinal);
        var distinct = new List<FramePicture>();
        for (int index = 0; index < laid.Length; index++)
        {
            composed[index] = Compose(index);
            // A picture is told apart by its size and the ink of each of its pixels: frames alike there show alike under
            // every colouring and shade, whatever origin each is drawn from.
            string key = composed[index].Key;
            if (!pictureIndex.TryGetValue(key, out int picture))
            {
                picture = distinct.Count;
                pictureIndex.Add(key, picture);
                distinct.Add(composed[index]);
            }
            pictureOf[index] = picture;
        }
        Extent16<ImagePixels>[] sizes = [.. distinct.Select(shown => shown.Bounds.Size)];
        var corners = new Position16<ImagePixels>[sizes.Length];
        Extent16<ImagePixels> packed = SheetPacking.Pack(sizes, corners, $"Sheet {sources.Frames[0].Name}'s {sources.Frames.Count} frames");
        return new LaidPictures(packed, composed, pictureOf, [.. distinct], corners);
    }

    /// <summary>
    /// A hash of everything painting reads: the colours, the palette bits and row rule, the default row and priority, each
    /// laid image's name, row and parts, each part by its place, size, flip, row, priority and the characters it reads, and
    /// the image each frame shows. Layouts with one identity paint the same sheet.
    /// </summary>
    internal string Identity { get; }

    /// <summary>
    /// Paints the laid-out frame images onto <paramref name="planes"/>, which are <see cref="Size"/> and clear, and gives
    /// the sheet's manifest.
    /// </summary>
    internal SpriteSheetDocument Paint(SpriteSheetPlanes planes)
    {
        LaidPictures laidPictures = pictures.Value;
        for (int picture = 0; picture < laidPictures.Distinct.Length; picture++)
            PaintPicture(planes, laidPictures.Distinct[picture], laidPictures.Corners[picture]);
        var frameDocuments = new SpriteSheetFrameDocument[laid.Length];
        for (int index = 0; index < laid.Length; index++)
        {
            FramePicture composed = laidPictures.Composed[index];
            FrameBounds bound = composed.Bounds;
            frameDocuments[index] = new SpriteSheetFrameDocument
            {
                Corner = laidPictures.Corners[laidPictures.PictureOf[index]],
                Size = bound.Size,
                Origin = new(checked((short)-bound.TopLeft.X), checked((short)-bound.TopLeft.Y)),
                ColourGroups = composed.Groups.ManifestMembers,
                Depth = composed.Depth,
            };
        }
        // Every frame names the rectangle holding its picture; frames showing the same picture name the same one, each
        // drawn from its own origin.
        var manifest = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        foreach (NamedFrameImage frame in frameImages)
            manifest.Add(frame.Name, frameDocuments[frame.Image]);
        RgbColor[] spriteColours = sources.SpriteColours;
        ushort[] colours = new ushort[spriteColours.Length];
        for (int ink = 0; ink < colours.Length; ink++)
            colours[ink] = spriteColours[DrawnColourOf(ink)].ToWord();
        return new SpriteSheetDocument
        {
            Palette = SpriteImageColours.Sprite, Colours = colours, PaletteBits = sources.PaletteBits,
            Frames = manifest,
            Shades = SheetShade.Documents([.. (sources.Shades ?? []).Select(InkShade), .. RowShades()]),
            Blank = sources.Blank is { Count: > 0 } blank ? [.. blank] : null,
        };
    }

    /// <summary>
    /// The sprite colour ink <paramref name="ink"/> shows as the family draws it: the colour of the ink's own row with the
    /// family's palette bits ORed in. A sheet's ink is its part's own row and colour (#1512), so parts whose rows the bits
    /// fold together keep separate inks, which a draw selecting another row shows apart.
    /// </summary>
    private int DrawnColourOf(int ink) => ((ink / RowColours) | sources.PaletteBits) * RowColours + ink % RowColours;

    /// <summary>
    /// <paramref name="shade"/>, which names the sprite colours it recolours, over the sheet's inks in rising order: each
    /// ink drawn in one of those colours shows that colour's shade.
    /// </summary>
    private SheetShade InkShade(SheetShade shade)
    {
        var inks = new List<int>();
        var shown = new List<RgbColor>();
        for (int ink = 0; ink < sources.SpriteColours.Length; ink++)
        {
            int index = Array.IndexOf(shade.Inks, DrawnColourOf(ink));
            if (index < 0)
                continue;
            inks.Add(ink);
            shown.Add(shade.Colours[index]);
        }
        return shade with { Inks = [.. inks], Colours = [.. shown] };
    }

    /// <summary>
    /// Each row shade as every ink whose pixels its draw takes from another row, with the colour each shows there (#1512).
    /// An ink that shows its standard colour under the shade is listed all the same: the draw reads the other row, so a
    /// shade laid before it over the ink's own row does not show through. An ink whose pixels the draw still takes from
    /// their own row is left out, as the draw shows whatever that row holds, the shades laid before it among them.
    /// </summary>
    private IEnumerable<SheetShade> RowShades()
    {
        IReadOnlyList<SheetRowShade> shades = sources.RowShades ?? [];
        for (int index = 0; index < shades.Count; index++)
        {
            KeyValuePair<int, RowShadedInk>[] moved = [.. rowShadeInks[index].Where(ink => ink.Value.Moves).OrderBy(ink => ink.Key)];
            yield return new SheetShade(shades[index].Shade, [.. moved.Select(ink => ink.Key)], [.. moved.Select(ink => ink.Value.Colour)]);
        }
    }

    /// <summary>
    /// Records that ink <paramref name="ink"/>, a part's own row and colour, shows each row shade's colour, and whether the
    /// shade's draw takes it from another row than the family's draw does. Both follow from the ink alone.
    /// </summary>
    private void ShadeRows(int ink)
    {
        if (rowShadedInks[ink])
            return;
        rowShadedInks[ink] = true;
        IReadOnlyList<SheetRowShade> shades = sources.RowShades ?? [];
        int ownRow = ink / RowColours;
        int colour = ink % RowColours;
        for (int index = 0; index < shades.Count; index++)
        {
            SheetRowShade shade = shades[index];
            int shownRow = ownRow | shade.Row;
            rowShadeInks[index][ink] = new RowShadedInk(shade.SpriteColours[shownRow * RowColours + colour],
                shownRow * RowColours + colour != DrawnColourOf(ink));
        }
    }

    /// <summary>
    /// Composes the <paramref name="index"/>th laid frame's picture over its parts' cover: the ink each pixel shows, its own
    /// row and colour, the colour groups it is coloured from, and the depth its gameplay draws sit at (#1514): the frame is
    /// drawn whole at one depth, the one the game draws most of its shown pixels at (the front one of a tie), so a frame
    /// whose parts the game drew at different depths shows wholly at its main part's. A frame showing no pixel has none.
    /// </summary>
    private FramePicture Compose(int index)
    {
        SheetFrameSource source = laid[index];
        FrameBounds bound = Bounds(source.Frame);
        int framePalette = source.Palette ?? palette;
        ImageColourGroups groups = ImageColourGroups.None;
        // Each shown pixel's ink, zero where no part shows one, in the frame's own rectangle.
        var inks = new byte[bound.Size.X * bound.Size.Y];
        // Each shown pixel's game priority, one up so that zero marks a pixel no part shows, in the frame's own rectangle.
        var shownPriorities = new int[inks.Length];
        // Back to front, so the first part ends in front.
        for (int partIndex = source.Frame.Parts.Length - 1; partIndex >= 0; partIndex--)
        {
            SpriteFramePart part = source.Frame.Parts[partIndex];
            // A part taking its draw's palette is coloured in the frame's, with the family's palette bits ORed in; its
            // pixels are inked in its own row (#1512).
            int ownRow = part.Palette ?? framePalette;
            groups = groups.With(ImageColourGroup.Numbered(ownRow | sources.PaletteBits));
            for (int row = 0; row < part.Size.Y; row++)
            {
                for (int column = 0; column < part.Size.X; column++)
                {
                    int character = part.CharacterAt(Position16<ScreenPixels>.Of(column, row), out Position16<CharacterPixels> pixel);
                    int colour = source.Characters.ColorIndex(character, pixel);
                    if (colour == 0)
                        continue;
                    int at = (part.Offset.Y - bound.TopLeft.Y + row) * bound.Size.X + part.Offset.X - bound.TopLeft.X + column;
                    int ink = ownRow * RowColours + colour;
                    inks[at] = checked((byte)ink);
                    // Parts are laid back to front, so each pixel ends with the priority of the part in front.
                    shownPriorities[at] = 1 + (part.Priority ?? priority);
                    // A part keeping its own row under a drawn row keeps its colours under every row shade.
                    if (part.Palette is null || !sources.PartsKeepOwnRows)
                        ShadeRows(ink);
                }
            }
        }
        return new FramePicture(bound, inks, groups, DepthOf(shownPriorities, source.Name));
    }

    /// <summary>
    /// Paints <paramref name="picture"/> with its top-left corner at <paramref name="corner"/>: each inked pixel in the
    /// colour its ink shows as the family draws it, and its ink.
    /// </summary>
    private void PaintPicture(SpriteSheetPlanes planes, FramePicture picture, Position16<ImagePixels> corner)
    {
        RgbColor[] spriteColours = sources.SpriteColours;
        int width = planes.Size.X;
        Extent16<ImagePixels> size = picture.Bounds.Size;
        for (int row = 0; row < size.Y; row++)
        {
            for (int column = 0; column < size.X; column++)
            {
                int ink = picture.Inks[row * size.X + column];
                if (ink == 0)
                    continue;
                int at = (corner.Y + row) * width + corner.X + column;
                planes.Pixels[at] = spriteColours[DrawnColourOf(ink)].ToRgbaPixel();
                planes.PaletteIndices[at] = checked((byte)ink);
            }
        }
    }

    /// <summary>
    /// A laid frame's composed picture: its parts' cover from its origin, the ink each of the cover's pixels shows (zero
    /// where none), the colour groups it is coloured from and the depth its draws sit at.
    /// </summary>
    /// <param name="Bounds">Its parts' cover relative to its origin.</param>
    /// <param name="Inks">Each pixel's ink, row by row from the top, zero where no part shows one.</param>
    /// <param name="Groups">The colour groups it is coloured from.</param>
    /// <param name="Depth">The depth its gameplay draws sit at; none for a frame showing no pixel.</param>
    private sealed record FramePicture(FrameBounds Bounds, byte[] Inks, ImageColourGroups Groups, ActorDepth? Depth)
    {
        /// <summary>The picture spelled for telling pictures apart: its size and every pixel's ink.</summary>
        internal string Key => $"{Bounds.Size.X.ToString(CultureInfo.InvariantCulture)}x{Bounds.Size.Y.ToString(CultureInfo.InvariantCulture)} {Convert.ToBase64String(Inks)}";
    }

    /// <summary>The laid compositions' pictures and where each distinct one is held on the sheet.</summary>
    /// <param name="Size">The sheet's width and height.</param>
    /// <param name="Composed">Each laid composition's picture.</param>
    /// <param name="PictureOf">The index among <paramref name="Distinct"/> of each laid composition's picture.</param>
    /// <param name="Distinct">The distinct pictures in order.</param>
    /// <param name="Corners">Each distinct picture's top-left corner on the sheet.</param>
    private sealed record LaidPictures(Extent16<ImagePixels> Size, FramePicture[] Composed, int[] PictureOf, FramePicture[] Distinct,
        Position16<ImagePixels>[] Corners);

    /// <summary>The game's priorities, of which priorities zero and one draw behind the whole room.</summary>
    private const int GamePriorities = 4;

    /// <summary>
    /// The depth a frame whose shown pixels hold <paramref name="shownPriorities"/> (each priority one up, zero for none) is
    /// drawn at (#1514): the priority most of its pixels show, the front one of a tie. The game's priorities zero and one both
    /// draw behind the whole room, two between its back and front tiles, and three in front of it.
    /// </summary>
    /// <exception cref="InvalidDataException">A pixel holds a priority the game does not have.</exception>
    private static ActorDepth? DepthOf(ReadOnlySpan<int> shownPriorities, string frame)
    {
        Span<int> counts = stackalloc int[GamePriorities];
        foreach (int shown in shownPriorities)
        {
            if (shown == 0)
                continue;
            if (shown > GamePriorities)
                throw new InvalidDataException($"Sheet frame {frame} shows a pixel of priority {shown - 1}, which the game does not have.");
            counts[shown - 1]++;
        }
        int main = -1;
        for (int candidate = 0; candidate < GamePriorities; candidate++)
        {
            if (counts[candidate] > 0 && (main < 0 || counts[candidate] >= counts[main]))
                main = candidate;
        }
        return main < 0 ? null : DepthOfPriority[main];
    }

    /// <summary>The depth each of the game's priorities draws at, from priority zero up.</summary>
    private static readonly ActorDepth[] DepthOfPriority =
        [ActorDepth.BehindRoom, ActorDepth.BehindRoom, ActorDepth.BehindFrontParts, ActorDepth.InFront];

    /// <summary>
    /// Hashes everything <see cref="Paint"/> reads into <see cref="Identity"/>. A part is hashed by the characters it
    /// reads rather than by where they are, so a frame painted from the same characters at another index shares the
    /// identity.
    /// </summary>
    private string IdentityOf()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(palette);
            writer.Write(priority);
            writer.Write(sources.PaletteBits);
            writer.Write(sources.PartsKeepOwnRows);
            foreach (RgbColor colour in sources.SpriteColours)
                WriteColour(writer, colour);
            // A sheet's shades are part of what it installs (#1512).
            IReadOnlyList<SheetShade> shades = sources.Shades ?? [];
            writer.Write(shades.Count);
            foreach (SheetShade shade in shades)
            {
                writer.Write(shade.Shade.Key);
                writer.Write(shade.Inks.Length);
                foreach (int ink in shade.Inks)
                    writer.Write(ink);
                foreach (RgbColor colour in shade.Colours)
                    WriteColour(writer, colour);
            }
            IReadOnlyList<SheetRowShade> rowShades = sources.RowShades ?? [];
            writer.Write(rowShades.Count);
            foreach (SheetRowShade shade in rowShades)
            {
                writer.Write(shade.Shade.Key);
                writer.Write(shade.Row);
                foreach (RgbColor colour in shade.SpriteColours)
                    WriteColour(writer, colour);
            }
            writer.Write(laid.Length);
            for (int index = 0; index < laid.Length; index++)
            {
                SheetFrameSource source = laid[index];
                writer.Write(source.Name);
                writer.Write(source.Palette ?? palette);
                writer.Write(source.Frame.Parts.Length);
                foreach (SpriteFramePart part in source.Frame.Parts)
                    WritePart(writer, part, source.Characters);
            }
            writer.Write(frameImages.Length);
            foreach (NamedFrameImage frame in frameImages)
            {
                writer.Write(frame.Name);
                writer.Write(frame.Image);
            }
            // The frames that show nothing are part of what the sheet installs (#1513).
            IReadOnlyList<string> blanks = sources.Blank ?? [];
            writer.Write(blanks.Count);
            foreach (string blank in blanks)
                writer.Write(blank);
        }
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }

    /// <summary>Writes a colour's three channels.</summary>
    private static void WriteColour(BinaryWriter writer, RgbColor colour)
    {
        writer.Write(colour.Red);
        writer.Write(colour.Green);
        writer.Write(colour.Blue);
    }

    /// <summary>
    /// What a frame paints, spelled for telling frame images apart: its row and each part's place, size, flip, row, priority
    /// and the characters it reads, wherever they lie in <paramref name="characters"/>.
    /// </summary>
    private static string FrameContent(SpriteFrame frame, int framePalette, SpriteArtwork characters)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(framePalette);
            writer.Write(frame.Parts.Length);
            foreach (SpriteFramePart part in frame.Parts)
                WritePart(writer, part, characters);
        }
        return Convert.ToBase64String(stream.GetBuffer().AsSpan(0, checked((int)stream.Length)));
    }

    /// <summary>Writes what painting reads of one part: its place, size, flip, row, priority and the characters it reads.</summary>
    private static void WritePart(BinaryWriter writer, SpriteFramePart part, SpriteArtwork characters)
    {
        writer.Write(part.Offset.X);
        writer.Write(part.Offset.Y);
        writer.Write(part.Size.X);
        writer.Write(part.Size.Y);
        writer.Write(part.Flip.ToString());
        writer.Write(part.Palette.HasValue);
        writer.Write(part.Palette.GetValueOrDefault());
        writer.Write(part.Priority.HasValue);
        writer.Write(part.Priority.GetValueOrDefault());
        // Each row of the part's characters is a run of consecutive characters in the artwork's grid.
        int rowBytes = part.Size.X / SpriteArtwork.CharacterPixels * SpriteArtwork.CharacterByteCount;
        for (int row = 0; row < part.Size.Y / SpriteArtwork.CharacterPixels; row++)
            writer.Write(characters.Characters.Slice((part.Character + row * SpriteArtwork.CharactersPerRow) * SpriteArtwork.CharacterByteCount, rowBytes));
    }

    /// <summary>The rectangle a frame's parts cover, relative to its origin; empty for a frame with no parts.</summary>
    private static FrameBounds Bounds(SpriteFrame frame)
    {
        if (frame.Parts.IsEmpty)
            return default;
        int left = int.MaxValue, top = int.MaxValue, right = int.MinValue, bottom = int.MinValue;
        foreach (SpriteFramePart part in frame.Parts)
        {
            left = Math.Min(left, part.Offset.X);
            top = Math.Min(top, part.Offset.Y);
            right = Math.Max(right, part.Offset.X + part.Size.X);
            bottom = Math.Max(bottom, part.Offset.Y + part.Size.Y);
        }
        return new FrameBounds(new(checked((short)left), checked((short)top)), new(checked((ushort)(right - left)), checked((ushort)(bottom - top))));
    }

    /// <summary>A frame of the sources and the laid frame image it shows.</summary>
    /// <param name="Name">The frame's installed name.</param>
    /// <param name="Image">The index of its image among the laid frame images.</param>
    private readonly record struct NamedFrameImage(string Name, int Image);

    /// <summary>A frame's cover relative to its origin.</summary>
    /// <param name="TopLeft">The cover's top-left corner, from the origin.</param>
    /// <param name="Size">The cover's size.</param>
    private readonly record struct FrameBounds(Offset16<ImagePixels> TopLeft, Extent16<ImagePixels> Size);
}
