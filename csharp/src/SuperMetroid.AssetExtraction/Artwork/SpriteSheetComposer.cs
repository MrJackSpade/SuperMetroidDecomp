using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Pre-assembles and pre-colours a family's frames into one sprite sheet (#1392): each frame composed from its parts over
/// its characters, coloured with its standard palette rows, cropped to its parts and packed in rows no wider
/// than <see cref="MaximumWidth"/>. A frame with no parts is an empty frame.
/// </summary>
internal static class SpriteSheetComposer
{
    /// <summary>The palette rows at or past which a sprite takes part in colour blending.</summary>
    private const int FirstBlendingRow = 4;

    /// <summary>Colours in one sprite palette row.</summary>
    private const int RowColours = 16;

    /// <summary>The widest a sheet is packed; a frame that would cross it starts a new row of frames.</summary>
    internal const int MaximumWidth = 1024;

    /// <summary>Composes a sheet's frames in order onto one sheet.</summary>
    /// <param name="sources">Each frame's installed name, parts and characters, and the eight sprite palette rows as the
    /// family is drawn with them, sixteen colours each.</param>
    /// <param name="palette">The row of parts that do not choose their own.</param>
    /// <param name="priority">The priority of parts that do not choose their own.</param>
    /// <exception cref="InvalidDataException">A frame mixes blending and non-blending rows, or priorities.</exception>
    internal static ComposedSpriteSheet Compose(SpriteSheetSources sources, int palette, int priority)
    {
        Ensure.NotNull(sources);
        IReadOnlyList<SheetFrameSource> frames = sources.Frames;
        ReadOnlySpan<Bgr555> spriteColours = sources.SpriteColours;
        Ensure.LengthEqual(spriteColours, 8 * RowColours);
        var bounds = new FrameBounds[frames.Count];
        var corners = new Position16<ImagePixels>[frames.Count];
        int width = 1, x = 0, y = 0, rowHeight = 0;
        for (int index = 0; index < frames.Count; index++)
        {
            FrameBounds bound = bounds[index] = Bounds(frames[index].Frame);
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
        var pixels = new Rgba32[width * height];
        var paletteIndices = new byte[width * height];
        var manifest = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        for (int index = 0; index < frames.Count; index++)
        {
            (string name, SpriteFrame frame, SpriteArtwork artwork) = (frames[index].Name, frames[index].Frame, frames[index].Characters);
            FrameBounds bound = bounds[index];
            Position16<ImagePixels> corner = corners[index];
            bool blends = false;
            SpritePaletteRows rows = SpritePaletteRows.None;
            int framePriority = frame.Parts.IsEmpty ? 0 : frame.Parts[^1].Priority ?? priority;
            // Back to front, so the first part ends in front.
            for (int partIndex = frame.Parts.Length - 1; partIndex >= 0; partIndex--)
            {
                SpriteFramePart part = frame.Parts[partIndex];
                int paletteRow = part.Palette ?? palette;
                if (partIndex == frame.Parts.Length - 1)
                    blends = paletteRow >= FirstBlendingRow;
                else if (blends != paletteRow >= FirstBlendingRow)
                    throw new InvalidDataException($"Sprite frame {name} mixes blending and non-blending palette rows.");
                if ((part.Priority ?? priority) != framePriority)
                    throw new InvalidDataException($"Sprite frame {name} mixes priorities.");
                rows |= SpritePaletteRowSet.Rows[paletteRow];
                var placed = new SpriteScreenPart(default, part.Size, part.Character,
                    (part.Flip & SnesTileFlipFlags.Horizontal) != 0, (part.Flip & SnesTileFlipFlags.Vertical) != 0, paletteRow, 0);
                for (ushort row = 0; row < part.Size.Y; row++)
                {
                    for (ushort column = 0; column < part.Size.X; column++)
                    {
                        int character = placed.CharacterAt(new(column, row), out Position16<CharacterPixels> pixel);
                        byte colour = artwork.ColorIndex(character, pixel);
                        if (colour == 0)
                            continue;
                        int sheetX = corner.X + part.Offset.X - bound.Left + column;
                        int sheetY = corner.Y + part.Offset.Y - bound.Top + row;
                        pixels[sheetY * width + sheetX] = spriteColours[paletteRow * RowColours + colour].ToRgba32();
                        paletteIndices[sheetY * width + sheetX] = (byte)(paletteRow * RowColours + colour);
                    }
                }
            }
            manifest.Add(name, new SpriteSheetFrameDocument
            {
                Corner = corner,
                Size = new(checked((ushort)bound.Width), checked((ushort)bound.Height)),
                Origin = new(checked((short)-bound.Left), checked((short)-bound.Top)),
                Blends = blends,
                Priority = framePriority,
                Rows = rows,
            });
        }
        ushort[] colours = new ushort[spriteColours.Length];
        for (int colour = 0; colour < colours.Length; colour++)
            colours[colour] = spriteColours[colour].ToWord();
        return new ComposedSpriteSheet(new((ushort)width, (ushort)height), pixels, paletteIndices,
            new SpriteSheetDocument { Version = SpriteSheet.Version, Colours = colours, Frames = manifest });
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
        return new FrameBounds(left, top, right - left, bottom - top);
    }

    /// <summary>A frame's cover relative to its origin.</summary>
    /// <param name="Left">The leftmost column, from the origin.</param>
    /// <param name="Top">The topmost row, from the origin.</param>
    /// <param name="Width">The width of the cover.</param>
    /// <param name="Height">The height of the cover.</param>
    private readonly record struct FrameBounds(int Left, int Top, int Width, int Height);
}
