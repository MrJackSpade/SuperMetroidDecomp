using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Pre-assembles and pre-colours a family's frames into one sprite sheet (#1392): each frame composed from its parts over
/// the family's characters, coloured with its standard palette rows, cropped to its parts and laid left to right.
/// </summary>
internal static class SpriteSheetComposer
{
    /// <summary>The palette rows at or past which a sprite takes part in colour blending.</summary>
    private const int FirstBlendingRow = 4;

    /// <summary>Colours in one sprite palette row.</summary>
    private const int RowColours = 16;

    /// <summary>Composes <paramref name="frames"/> in order onto one sheet.</summary>
    /// <param name="frames">Each frame's installed name and its parts.</param>
    /// <param name="artwork">The characters the parts index.</param>
    /// <param name="spriteColours">The eight sprite palette rows as the family is drawn with them, sixteen colours each.</param>
    /// <param name="palette">The row of parts that do not choose their own.</param>
    /// <exception cref="InvalidDataException">A frame has no parts or mixes blending and non-blending rows.</exception>
    internal static ComposedSpriteSheet Compose(IReadOnlyList<KeyValuePair<string, SpriteFrame>> frames, SpriteArtwork artwork,
        ReadOnlySpan<Bgr555> spriteColours, int palette)
    {
        Ensure.NotNull(frames);
        Ensure.NotNull(artwork);
        Ensure.LengthEqual(spriteColours, 8 * RowColours);
        var bounds = new FrameBounds[frames.Count];
        int width = 0, height = 0;
        for (int index = 0; index < frames.Count; index++)
        {
            bounds[index] = Bounds(frames[index].Key, frames[index].Value);
            width += bounds[index].Width;
            height = Math.Max(height, bounds[index].Height);
        }
        var pixels = new Rgba32[width * height];
        var manifest = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        int left = 0;
        for (int index = 0; index < frames.Count; index++)
        {
            (string name, SpriteFrame frame) = (frames[index].Key, frames[index].Value);
            FrameBounds bound = bounds[index];
            bool blends = false;
            // Back to front, so the first part ends in front.
            for (int partIndex = frame.Parts.Length - 1; partIndex >= 0; partIndex--)
            {
                SpriteFramePart part = frame.Parts[partIndex];
                int row = part.Palette ?? palette;
                if (partIndex == frame.Parts.Length - 1)
                    blends = row >= FirstBlendingRow;
                else if (blends != row >= FirstBlendingRow)
                    throw new InvalidDataException($"Sprite frame {name} mixes blending and non-blending palette rows.");
                var placed = new SpriteScreenPart(default, part.Size, part.Character,
                    (part.Flip & SnesTileFlipFlags.Horizontal) != 0, (part.Flip & SnesTileFlipFlags.Vertical) != 0, row, 0);
                for (ushort y = 0; y < part.Size.Y; y++)
                {
                    for (ushort x = 0; x < part.Size.X; x++)
                    {
                        int character = placed.CharacterAt(new(x, y), out Position16<CharacterPixels> pixel);
                        byte colour = artwork.ColorIndex(character, pixel);
                        if (colour == 0)
                            continue;
                        int sheetX = left + part.Offset.X - bound.Left + x;
                        int sheetY = part.Offset.Y - bound.Top + y;
                        pixels[sheetY * width + sheetX] = spriteColours[row * RowColours + colour].ToRgba32();
                    }
                }
            }
            manifest.Add(name, new SpriteSheetFrameDocument
            {
                Corner = new(checked((ushort)left), 0),
                Size = new(checked((ushort)bound.Width), checked((ushort)bound.Height)),
                Origin = new(checked((short)-bound.Left), checked((short)-bound.Top)),
                Blends = blends,
            });
            left += bound.Width;
        }
        return new ComposedSpriteSheet(new((ushort)width, (ushort)height), pixels,
            new SpriteSheetDocument { Version = SpriteSheet.Version, Frames = manifest });
    }

    /// <summary>The rectangle a frame's parts cover, relative to its origin.</summary>
    private static FrameBounds Bounds(string name, SpriteFrame frame)
    {
        if (frame.Parts.IsEmpty)
            throw new InvalidDataException($"Sprite frame {name} has no parts.");
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
