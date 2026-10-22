using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Pre-assembles and pre-colours a family's frames into one sprite sheet (#1392): each frame composed from its parts over
/// its characters, coloured with its standard palette rows, cropped to its parts and packed in rows no wider
/// than <see cref="MaximumWidth"/>. A frame with no parts is an empty frame. Each distinct part is also composed alone
/// as a part image, which the frame lists in draw order, so a screen whose object limit cuts the frame short can show
/// its first parts as the cartridge's loader does.
/// </summary>
internal static class SpriteSheetComposer
{
    /// <summary>Colours in one sprite palette row.</summary>
    private const int RowColours = 16;

    /// <summary>The widest a sheet is packed; a frame that would cross it starts a new row of frames.</summary>
    internal const int MaximumWidth = 1024;

    /// <summary>Composes a sheet's frames in order onto one sheet, then their part images.</summary>
    /// <param name="sources">Each frame's installed name, parts and characters, and the eight sprite palette rows as the
    /// family is drawn with them, sixteen colours each.</param>
    /// <param name="palette">The row of parts that do not choose their own.</param>
    /// <param name="priority">The priority of parts that do not choose their own.</param>
    internal static ComposedSpriteSheet Compose(SpriteSheetSources sources, int palette, int priority)
    {
        Ensure.NotNull(sources);
        IReadOnlyList<SheetFrameSource> frames = sources.Frames;
        ReadOnlySpan<Bgr555> spriteColours = sources.SpriteColours;
        Ensure.LengthEqual(spriteColours, 8 * RowColours);
        // Each distinct part, alone at its frame's origin, coloured as its frame colours it.
        var partNames = new Dictionary<PartKey, string>();
        var parts = new List<SheetFrameSource>();
        var frameParts = new SpriteSheetPartDocument[frames.Count][];
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
        SheetFrameSource[] laid = [.. frames, .. parts];
        var bounds = new FrameBounds[laid.Length];
        var corners = new Position16<ImagePixels>[laid.Length];
        int width = 1, x = 0, y = 0, rowHeight = 0;
        for (int index = 0; index < laid.Length; index++)
        {
            FrameBounds bound = bounds[index] = Bounds(laid[index].Frame);
            if (bound.Width == 0)
                continue;
            if (x > 0 && x + bound.Width > MaximumWidth)
            {
                y += rowHeight;
                x = 0;
                rowHeight = 0;
            }
            corners[index] = new(checked((ushort)x), checked((ushort)y));
            x += bound.Width;
            rowHeight = Math.Max(rowHeight, bound.Height);
            width = Math.Max(width, x);
        }
        int height = Math.Max(1, y + rowHeight);
        var canvas = new Canvas(width, height, spriteColours.ToArray());
        var manifest = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        var partManifest = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        for (int index = 0; index < laid.Length; index++)
        {
            SpritePaletteRows rows = canvas.Paint(laid[index], bounds[index], corners[index], sources, palette, priority);
            bool isPart = index >= frames.Count;
            var document = new SpriteSheetFrameDocument
            {
                Corner = corners[index],
                Size = new(checked((ushort)bounds[index].Width), checked((ushort)bounds[index].Height)),
                Origin = new(checked((short)-bounds[index].Left), checked((short)-bounds[index].Top)),
                Rows = rows,
                Parts = isPart ? [] : frameParts[index],
            };
            (isPart ? partManifest : manifest).Add(laid[index].Name, document);
        }
        ushort[] colours = new ushort[spriteColours.Length];
        for (int colour = 0; colour < colours.Length; colour++)
            colours[colour] = spriteColours[colour].ToWord();
        return new ComposedSpriteSheet(new((ushort)width, (ushort)height), canvas.Pixels, canvas.PaletteIndices, canvas.Priorities,
            canvas.PartRows, new SpriteSheetDocument { Version = SpriteSheet.Version, Palette = SpriteImageColours.Sprite, Colours = colours, Frames = manifest, PartImages = partManifest });
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
        return new FrameBounds(left, top, right - left, bottom - top);
    }

    /// <summary>A part as composed alone: the characters it reads, the part at the origin and its frame's palette row.</summary>
    /// <param name="Characters">The characters the part reads.</param>
    /// <param name="Part">The part, placed at the origin.</param>
    /// <param name="FramePalette">The row its frame colours palette-inheriting parts in.</param>
    private readonly record struct PartKey(SpriteArtwork Characters, SpriteFramePart Part, int FramePalette);

    /// <summary>A frame's cover relative to its origin.</summary>
    /// <param name="Left">The leftmost column, from the origin.</param>
    /// <param name="Top">The topmost row, from the origin.</param>
    /// <param name="Width">The width of the cover.</param>
    /// <param name="Height">The height of the cover.</param>
    private readonly record struct FrameBounds(int Left, int Top, int Width, int Height);

    /// <summary>The sheet's pixel planes, painted one frame at a time.</summary>
    /// <param name="width">The sheet's width.</param>
    /// <param name="height">The sheet's height.</param>
    /// <param name="spriteColours">The eight sprite palette rows the frames are coloured with.</param>
    private sealed class Canvas(int width, int height, Bgr555[] spriteColours)
    {
        /// <summary>Each pixel's colour.</summary>
        internal Rgba32[] Pixels { get; } = new Rgba32[width * height];

        /// <summary>Each pixel's palette row times sixteen plus its colour.</summary>
        internal byte[] PaletteIndices { get; } = new byte[width * height];

        /// <summary>Each pixel's priority against the backgrounds.</summary>
        internal byte[] Priorities { get; } = new byte[width * height];

        /// <summary>Each pixel's part row, with the fixed-row flag where its part keeps its own.</summary>
        internal byte[] PartRows { get; } = new byte[width * height];

        /// <summary>Paints <paramref name="source"/> at <paramref name="corner"/> and returns the rows it was coloured from.</summary>
        internal SpritePaletteRows Paint(SheetFrameSource source, FrameBounds bound, Position16<ImagePixels> corner, SpriteSheetSources sources,
            int palette, int priority)
        {
            int framePalette = source.Palette ?? palette;
            SpritePaletteRows rows = SpritePaletteRows.None;
            // Back to front, so the first part ends in front.
            for (int partIndex = source.Frame.Parts.Length - 1; partIndex >= 0; partIndex--)
            {
                SpriteFramePart part = source.Frame.Parts[partIndex];
                // A part taking its draw's palette is coloured in the frame's and keeps no row of its own, so an
                // override replaces it; a part naming its own keeps it under an override when the family says so.
                int paletteRow = (part.Palette ?? framePalette) | sources.PaletteBits;
                int partRow = part.Palette is not int own ? 0
                    : sources.PartsKeepOwnRows ? own | SpriteImage.FixedPartRow : own;
                rows |= SpritePaletteRowSet.Rows[paletteRow];
                var placed = new SpriteScreenPart(default, part.Size, part.Character,
                    (part.Flip & SnesTileFlipFlags.Horizontal) != 0, (part.Flip & SnesTileFlipFlags.Vertical) != 0, paletteRow, 0);
                for (ushort row = 0; row < part.Size.Y; row++)
                {
                    for (ushort column = 0; column < part.Size.X; column++)
                    {
                        int character = placed.CharacterAt(new(column, row), out Position16<CharacterPixels> pixel);
                        byte colour = source.Characters.ColorIndex(character, pixel);
                        if (colour == 0)
                            continue;
                        int at = (corner.Y + part.Offset.Y - bound.Top + row) * width + corner.X + part.Offset.X - bound.Left + column;
                        Pixels[at] = spriteColours[paletteRow * RowColours + colour].ToRgba32();
                        PaletteIndices[at] = (byte)(paletteRow * RowColours + colour);
                        // Parts are laid back to front, so each pixel ends with the priority of the part in front.
                        Priorities[at] = (byte)(part.Priority ?? priority);
                        PartRows[at] = (byte)partRow;
                    }
                }
            }
            return rows;
        }
    }
}
