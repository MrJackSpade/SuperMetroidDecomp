using System.Security.Cryptography;
using System.Text;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A sprite sheet laid out before any pixel is painted (#1392): its distinct frame images in order, each one complete
/// pre-assembled picture of its frame that is drawn as it is. Frames are told apart by the pixels their parts paint, so
/// each image is laid and painted once however many frames show it (#1513). Each is cropped to its parts and packed in
/// rows no wider than
/// <see cref="MaximumWidth"/>, or, for a sheet that would then be taller than an artwork image can be, the narrowest
/// doubling of it within that bound that keeps the sheet within it too. A frame with no parts is an empty frame. Its
/// <see cref="Identity"/> covers everything painting reads, so two layouts with one identity paint the same sheet and
/// the second need not be painted (#1490).
/// </summary>
internal sealed class SpriteSheetLayout
{
    /// <summary>Colours in one sprite palette row.</summary>
    private const int RowColours = 16;

    /// <summary>The widest a sheet that fits is packed; a frame that would cross it starts a new row of frames.</summary>
    private const int MaximumWidth = 1024;

    /// <summary>What the sheet composes.</summary>
    private readonly SpriteSheetSources sources;

    /// <summary>The row of parts that do not choose their own.</summary>
    private readonly int palette;

    /// <summary>The game's priority of parts that do not choose their own, from which each frame's depth is decided.</summary>
    private readonly int priority;

    /// <summary>The distinct frame images in order.</summary>
    private readonly SheetFrameSource[] laid;

    /// <summary>Each frame of the sources, in order, with the laid frame image it shows.</summary>
    private readonly NamedFrameImage[] frameImages;

    /// <summary>Each laid frame's cover relative to its origin.</summary>
    private readonly FrameBounds[] bounds;

    /// <summary>Each laid frame's top-left corner on the sheet.</summary>
    private readonly Position16<ImagePixels>[] corners;

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
        bounds = [.. laid.Select(frame => Bounds(frame.Frame))];
        corners = new Position16<ImagePixels>[laid.Length];
        Extent16<ImagePixels> size;
        for (int rowWidth = MaximumWidth; ; rowWidth *= 2)
        {
            if (rowWidth > RgbaPng.MaximumDimension)
                throw new InvalidDataException(
                    $"Sheet {frames[0].Name}'s {frames.Count} frames do not fit in a {RgbaPng.MaximumDimension}-pixel square image.");
            if (Pack(bounds, corners, rowWidth) is { } packed)
            {
                size = packed;
                break;
            }
        }
        Size = size;
        Identity = IdentityOf();
    }

    /// <summary>The sheet's width and height.</summary>
    internal Extent16<ImagePixels> Size { get; }

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
        var frameDocuments = new SpriteSheetFrameDocument[laid.Length];
        for (int index = 0; index < laid.Length; index++)
        {
            PaintedFrame painted = PaintFrame(planes, index);
            ImageColourGroups groups = painted.Groups;
            ActorDepth? depth = painted.Depth;
            frameDocuments[index] = new SpriteSheetFrameDocument
            {
                Corner = corners[index],
                Size = bounds[index].Size,
                Origin = new(checked((short)-bounds[index].TopLeft.X), checked((short)-bounds[index].TopLeft.Y)),
                ColourGroups = groups.ManifestMembers,
                Depth = depth,
            };
        }
        // Every frame names the image it shows; frames painting the same pixels name the same one.
        var manifest = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        foreach (NamedFrameImage frame in frameImages)
            manifest.Add(frame.Name, frameDocuments[frame.Image]);
        RgbColor[] spriteColours = sources.SpriteColours;
        ushort[] colours = new ushort[spriteColours.Length];
        for (int ink = 0; ink < colours.Length; ink++)
            colours[ink] = spriteColours[DrawnColourOf(ink)].ToWord();
        return new SpriteSheetDocument
        {
            Version = SpriteSheetFile.Version, Palette = SpriteImageColours.Sprite, Colours = colours, PaletteBits = sources.PaletteBits,
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
    /// Paints the <paramref name="index"/>th laid frame at its corner and returns the colour groups it was coloured from and
    /// the depth its gameplay draws sit at (#1514): the frame is drawn whole at one depth, the one the game draws most of its
    /// shown pixels at (the front one of a tie), so a frame whose parts the game drew at different depths shows wholly at
    /// its main part's. A frame showing no pixel has none.
    /// </summary>
    private PaintedFrame PaintFrame(SpriteSheetPlanes planes, int index)
    {
        SheetFrameSource source = laid[index];
        FrameBounds bound = bounds[index];
        Position16<ImagePixels> corner = corners[index];
        RgbColor[] spriteColours = sources.SpriteColours;
        int width = planes.Size.X;
        int framePalette = source.Palette ?? palette;
        ImageColourGroups groups = ImageColourGroups.None;
        // Each shown pixel's game priority, one up so that zero marks a pixel no part shows, in the frame's own rectangle.
        var shownPriorities = new int[bound.Size.X * bound.Size.Y];
        // Back to front, so the first part ends in front.
        for (int partIndex = source.Frame.Parts.Length - 1; partIndex >= 0; partIndex--)
        {
            SpriteFramePart part = source.Frame.Parts[partIndex];
            // A part taking its draw's palette is coloured in the frame's, with the family's palette bits ORed in; its
            // pixels are inked in its own row (#1512).
            int ownRow = part.Palette ?? framePalette;
            int paletteRow = ownRow | sources.PaletteBits;
            groups = groups.With(ImageColourGroup.Numbered(paletteRow));
            for (int row = 0; row < part.Size.Y; row++)
            {
                for (int column = 0; column < part.Size.X; column++)
                {
                    int character = part.CharacterAt(Position16<ScreenPixels>.Of(column, row), out Position16<CharacterPixels> pixel);
                    int colour = source.Characters.ColorIndex(character, pixel);
                    if (colour == 0)
                        continue;
                    int frameX = part.Offset.X - bound.TopLeft.X + column, frameY = part.Offset.Y - bound.TopLeft.Y + row;
                    int at = (corner.Y + frameY) * width + corner.X + frameX;
                    planes.Pixels[at] = spriteColours[paletteRow * RowColours + colour].ToRgbaPixel();
                    planes.PaletteIndices[at] = (byte)(ownRow * RowColours + colour);
                    // Parts are laid back to front, so each pixel ends with the priority of the part in front.
                    shownPriorities[frameY * bound.Size.X + frameX] = 1 + (part.Priority ?? priority);
                    // A part keeping its own row under a drawn row keeps its colours under every row shade.
                    if (part.Palette is null || !sources.PartsKeepOwnRows)
                        ShadeRows(ownRow * RowColours + colour);
                }
            }
        }
        return new PaintedFrame(groups, DepthOf(shownPriorities, source.Name));
    }

    /// <summary>What painting a laid frame found: the colour groups it was coloured from, and the depth its draws sit at.</summary>
    /// <param name="Groups">The colour groups the frame was coloured from.</param>
    /// <param name="Depth">The depth its gameplay draws sit at; none for a frame showing no pixel.</param>
    private readonly record struct PaintedFrame(ImageColourGroups Groups, ActorDepth? Depth);

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
        [ActorDepth.BehindRoom, ActorDepth.BehindRoom, ActorDepth.BehindFrontTiles, ActorDepth.InFront];

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
        writer.Write((byte)colour.Red);
        writer.Write((byte)colour.Green);
        writer.Write((byte)colour.Blue);
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

    /// <summary>
    /// Packs the frames' covers in order in rows no wider than <paramref name="rowWidth"/>, a cover that would cross it
    /// starting a new row, writing each one's corner, and gives the sheet's size, or none when the rows would be taller
    /// than an artwork image can be.
    /// </summary>
    private static Extent16<ImagePixels>? Pack(FrameBounds[] bounds, Position16<ImagePixels>[] corners, int rowWidth)
    {
        int width = 1, x = 0, y = 0, rowHeight = 0;
        for (int index = 0; index < bounds.Length; index++)
        {
            FrameBounds bound = bounds[index];
            if (bound.Size.X == 0)
                continue;
            if (x > 0 && x + bound.Size.X > rowWidth)
            {
                y += rowHeight;
                x = 0;
                rowHeight = 0;
            }
            if (y + bound.Size.Y > RgbaPng.MaximumDimension)
                return null;
            corners[index] = new(checked((ushort)x), checked((ushort)y));
            x += bound.Size.X;
            rowHeight = Math.Max(rowHeight, bound.Size.Y);
            width = Math.Max(width, x);
        }
        return new Extent16<ImagePixels>(checked((ushort)width), checked((ushort)Math.Max(1, y + rowHeight)));
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
