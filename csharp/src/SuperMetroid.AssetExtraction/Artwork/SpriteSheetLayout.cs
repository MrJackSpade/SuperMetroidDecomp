using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A sprite sheet laid out before any pixel is painted (#1392): its frames in order, then each distinct part composed
/// alone as a part image, which the frame lists in draw order, so a screen whose object limit cuts the frame short can
/// show its first parts as the cartridge's loader does. Each is cropped to its parts and packed in rows no wider than
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

    /// <summary>The priority of parts that do not choose their own.</summary>
    private readonly int priority;

    /// <summary>The frames in order, then the part images.</summary>
    private readonly SheetFrameSource[] laid;

    /// <summary>How many of <see cref="laid"/> are frames rather than part images.</summary>
    private readonly int frameCount;

    /// <summary>Each frame's part images in draw order, at their offsets from its origin.</summary>
    private readonly SpriteSheetPartDocument[][] frameParts;

    /// <summary>Each laid frame's cover relative to its origin.</summary>
    private readonly FrameBounds[] bounds;

    /// <summary>Each laid frame's top-left corner on the sheet.</summary>
    private readonly Position16<ImagePixels>[] corners;

    /// <summary>For each row shade, the colour each ink its painted pixels hold shows under it (#1512).</summary>
    private readonly Dictionary<int, RgbColor>[] rowShadeInks;

    /// <summary>Lays out a sheet's frames in order, then their part images.</summary>
    /// <param name="sources">Each frame's installed name, parts and characters, and the eight sprite palette rows as the
    /// family is drawn with them, sixteen colours each.</param>
    /// <param name="palette">The row of parts that do not choose their own.</param>
    /// <param name="priority">The priority of parts that do not choose their own.</param>
    internal SpriteSheetLayout(SpriteSheetSources sources, int palette, int priority)
    {
        this.sources = Ensure.NotNull(sources);
        this.palette = palette;
        this.priority = priority;
        rowShadeInks = [.. (sources.RowShades ?? []).Select(_ => new Dictionary<int, RgbColor>())];
        IReadOnlyList<SheetFrameSource> frames = sources.Frames;
        ReadOnlySpan<RgbColor> spriteColours = sources.SpriteColours;
        Ensure.LengthEqual(spriteColours, 8 * RowColours);
        // Each distinct part, alone at its frame's origin, coloured as its frame colours it.
        var partNames = new Dictionary<PartKey, string>();
        var parts = new List<SheetFrameSource>();
        frameParts = new SpriteSheetPartDocument[frames.Count][];
        for (int index = 0; index < frames.Count; index++)
        {
            SheetFrameSource frame = frames[index];
            int framePalette = frame.Palette ?? palette;
            frameParts[index] = [.. frame.Frame.Parts.Select(part =>
            {
                // The PPU draws only its screen's two object sizes, so a part of any other size cannot stand for an object.
                if (part.Size != sources.ObjectSizes.Small && part.Size != sources.ObjectSizes.Large)
                    throw new InvalidDataException($"Sheet frame {frame.Name} has a {part.Size.X}x{part.Size.Y} part, not an object size of its screen.");
                SpriteFramePart alone = part with { Offset = default };
                var key = new PartKey(frame.Characters, alone, framePalette);
                if (!partNames.TryGetValue(key, out string? name))
                {
                    name = PartImageName(parts.Count);
                    partNames.Add(key, name);
                    parts.Add(new SheetFrameSource(name, new SpriteFrame(name, [alone]), frame.Characters, framePalette));
                }
                return new SpriteSheetPartDocument
                {
                    Image = name,
                    Offset = new(part.Offset.X, part.Offset.Y),
                };
            })];
        }
        frameCount = frames.Count;
        laid = [.. frames, .. parts];
        bounds = [.. laid.Select(frame => Bounds(frame.Frame))];
        corners = new Position16<ImagePixels>[laid.Length];
        Extent16<ImagePixels> size;
        for (int rowWidth = MaximumWidth; ; rowWidth *= 2)
        {
            if (rowWidth > RgbaPng.MaximumDimension)
                throw new InvalidDataException($"Sheet {frames[0].Name}'s frames do not fit in a {RgbaPng.MaximumDimension}-pixel square image.");
            size = Pack(bounds, corners, rowWidth);
            if (size.Y <= RgbaPng.MaximumDimension)
                break;
        }
        Size = size;
        Identity = IdentityOf();
    }

    /// <summary>The sheet's width and height.</summary>
    internal Extent16<ImagePixels> Size { get; }

    /// <summary>
    /// A hash of everything painting reads: the colours, the palette bits and row rule, the default row and priority, and
    /// each laid frame's name, row, part images and parts, each part by its place, size, flip, row, priority and the
    /// characters it reads. Layouts with one identity paint the same sheet.
    /// </summary>
    internal string Identity { get; }

    /// <summary>
    /// Paints the laid-out frames and part images onto <paramref name="planes"/>, which are <see cref="Size"/> and clear,
    /// and gives the sheet's manifest.
    /// </summary>
    internal SpriteSheetDocument Paint(SpriteSheetPlanes planes)
    {
        var manifest = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        var partManifest = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        for (int index = 0; index < laid.Length; index++)
        {
            ImageColourGroups groups = PaintFrame(planes, index);
            bool isPart = index >= frameCount;
            var document = new SpriteSheetFrameDocument
            {
                Corner = corners[index],
                Size = bounds[index].Size,
                Origin = new(checked((short)-bounds[index].TopLeft.X), checked((short)-bounds[index].TopLeft.Y)),
                ColourGroups = groups.ManifestMembers,
                Parts = isPart ? [] : frameParts[index],
            };
            (isPart ? partManifest : manifest).Add(laid[index].Name, document);
        }
        RgbColor[] spriteColours = sources.SpriteColours;
        ushort[] colours = new ushort[spriteColours.Length];
        for (int colour = 0; colour < colours.Length; colour++)
            colours[colour] = spriteColours[colour].ToWord();
        return new SpriteSheetDocument
        {
            Version = SpriteSheetFile.Version, Palette = SpriteImageColours.Sprite, Colours = colours, Frames = manifest, PartImages = partManifest,
            Shades = SheetShade.Documents([.. sources.Shades ?? [], .. RowShades()]),
        };
    }

    /// <summary>
    /// Each row shade as the inks the painted pixels hold and the colour each shows under it, leaving out inks that show
    /// their standard colour (#1512).
    /// </summary>
    private IEnumerable<SheetShade> RowShades()
    {
        IReadOnlyList<SheetRowShade> shades = sources.RowShades ?? [];
        for (int index = 0; index < shades.Count; index++)
        {
            KeyValuePair<int, RgbColor>[] changed = [.. rowShadeInks[index].Where(ink => ink.Value != sources.SpriteColours[ink.Key]).OrderBy(ink => ink.Key)];
            yield return new SheetShade(shades[index].Shade, [.. changed.Select(ink => ink.Key)], [.. changed.Select(ink => ink.Value)]);
        }
    }

    /// <summary>Records that a pixel of colour <paramref name="colour"/>, inked <paramref name="ink"/> in a part taking its draw's row from <paramref name="partRow"/>, shows each row shade's colour.</summary>
    /// <exception cref="InvalidDataException">The ink already shows another colour under a row shade.</exception>
    private void ShadeRows(int ink, int partRow, int colour)
    {
        IReadOnlyList<SheetRowShade> shades = sources.RowShades ?? [];
        for (int index = 0; index < shades.Count; index++)
        {
            SheetRowShade shade = shades[index];
            RgbColor shown = shade.SpriteColours[(partRow | shade.Row) * RowColours + colour];
            if (rowShadeInks[index].TryGetValue(ink, out RgbColor known) && known != shown)
                throw new InvalidDataException($"Sheet ink {ink} shows two colours under shade {shade.Shade.Key}: its parts take different rows.");
            rowShadeInks[index][ink] = shown;
        }
    }

    /// <summary>Paints the <paramref name="index"/>th laid frame at its corner and returns the colour groups it was coloured from.</summary>
    private ImageColourGroups PaintFrame(SpriteSheetPlanes planes, int index)
    {
        SheetFrameSource source = laid[index];
        FrameBounds bound = bounds[index];
        Position16<ImagePixels> corner = corners[index];
        RgbColor[] spriteColours = sources.SpriteColours;
        int width = planes.Size.X;
        int framePalette = source.Palette ?? palette;
        ImageColourGroups groups = ImageColourGroups.None;
        // Back to front, so the first part ends in front.
        for (int partIndex = source.Frame.Parts.Length - 1; partIndex >= 0; partIndex--)
        {
            SpriteFramePart part = source.Frame.Parts[partIndex];
            // A part taking its draw's palette is coloured in the frame's and has no group bits of its own, so a drawn
            // colouring replaces it; a part naming its own keeps it under a colouring when the family says so.
            int paletteRow = (part.Palette ?? framePalette) | sources.PaletteBits;
            PartColourGroup partGroup = part.Palette is not int own ? PartColourGroup.Following(ImageColourGroup.Every[0])
                : sources.PartsKeepOwnRows ? PartColourGroup.Keeping(ImageColourGroup.Numbered(own))
                : PartColourGroup.Following(ImageColourGroup.Numbered(own));
            groups = groups.With(ImageColourGroup.Numbered(paletteRow));
            for (int row = 0; row < part.Size.Y; row++)
            {
                for (int column = 0; column < part.Size.X; column++)
                {
                    int character = part.CharacterAt(Position16<ScreenPixels>.Of(column, row), out Position16<CharacterPixels> pixel);
                    int colour = source.Characters.ColorIndex(character, pixel);
                    if (colour == 0)
                        continue;
                    int at = (corner.Y + part.Offset.Y - bound.TopLeft.Y + row) * width + corner.X + part.Offset.X - bound.TopLeft.X + column;
                    planes.Pixels[at] = spriteColours[paletteRow * RowColours + colour].ToRgbaPixel();
                    planes.PaletteIndices[at] = (byte)(paletteRow * RowColours + colour);
                    // Parts are laid back to front, so each pixel ends with the priority of the part in front.
                    planes.Priorities[at] = (byte)(part.Priority ?? priority);
                    planes.PartGroups[at] = (byte)partGroup.Code;
                    // A part keeping its own row under a drawn row keeps its colours under every row shade.
                    if (part.Palette is null || !sources.PartsKeepOwnRows)
                        ShadeRows(paletteRow * RowColours + colour, part.Palette ?? framePalette, colour);
                }
            }
        }
        return groups;
    }

    /// <summary>
    /// Hashes everything <see cref="Paint"/> reads into <see cref="Identity"/>. A part is hashed by the characters it
    /// reads rather than by where they are, so a frame painted from the same characters at another index shares the
    /// identity; which frames share a part image is hashed through each frame's part image names.
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
            foreach (SheetShade shade in sources.Shades ?? [])
            {
                writer.Write(shade.Shade.Key);
                foreach (int ink in shade.Inks)
                    writer.Write(ink);
                foreach (RgbColor colour in shade.Colours)
                    WriteColour(writer, colour);
            }
            foreach (SheetRowShade shade in sources.RowShades ?? [])
            {
                writer.Write(shade.Shade.Key);
                writer.Write(shade.Row);
                foreach (RgbColor colour in shade.SpriteColours)
                    WriteColour(writer, colour);
            }
            writer.Write(frameCount);
            writer.Write(laid.Length);
            for (int index = 0; index < laid.Length; index++)
            {
                SheetFrameSource source = laid[index];
                writer.Write(source.Name);
                writer.Write(source.Palette ?? palette);
                SpriteSheetPartDocument[] images = index < frameCount ? frameParts[index] : [];
                writer.Write(images.Length);
                foreach (SpriteSheetPartDocument image in images)
                {
                    writer.Write(image.Image);
                    writer.Write(image.Offset.X);
                    writer.Write(image.Offset.Y);
                }
                writer.Write(source.Frame.Parts.Length);
                foreach (SpriteFramePart part in source.Frame.Parts)
                    WritePart(writer, part, source.Characters);
            }
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
    /// starting a new row, writing each one's corner, and gives the sheet's size.
    /// </summary>
    private static Extent16<ImagePixels> Pack(FrameBounds[] bounds, Position16<ImagePixels>[] corners, int rowWidth)
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
            corners[index] = new(checked((ushort)x), checked((ushort)y));
            x += bound.Size.X;
            rowHeight = Math.Max(rowHeight, bound.Size.Y);
            width = Math.Max(width, x);
        }
        return new(checked((ushort)width), checked((ushort)Math.Max(1, y + rowHeight)));
    }

    /// <summary>The name of the part image composed <paramref name="ordinal"/>th.</summary>
    private static string PartImageName(int ordinal) => $"part.{ordinal}";

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

    /// <summary>A part as composed alone: the characters it reads, the part at the origin and its frame's palette row.</summary>
    /// <param name="Characters">The characters the part reads.</param>
    /// <param name="Part">The part, placed at the origin.</param>
    /// <param name="FramePalette">The row its frame colours palette-inheriting parts in.</param>
    private readonly record struct PartKey(SpriteArtwork Characters, SpriteFramePart Part, int FramePalette);

    /// <summary>A frame's cover relative to its origin.</summary>
    /// <param name="TopLeft">The cover's top-left corner, from the origin.</param>
    /// <param name="Size">The cover's size.</param>
    private readonly record struct FrameBounds(Offset16<ImagePixels> TopLeft, Extent16<ImagePixels> Size);
}
