using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Pre-assembles and pre-colours a family's frames into one sprite sheet (#1392): each frame composed from its parts over
/// its characters, coloured with its standard palette rows, at the place its <see cref="SpriteSheetLayout"/> gives it:
/// one complete image per distinct frame, drawn as it is (#1513).
/// </summary>
internal static class SpriteSheetComposer
{
    /// <summary>Composes a sheet's distinct frame images in order onto one sheet.</summary>
    /// <param name="sources">Each frame's installed name, parts and characters, and the eight sprite palette rows as the
    /// family is drawn with them, sixteen colours each.</param>
    /// <param name="palette">The row of parts that do not choose their own.</param>
    /// <param name="priority">The priority of parts that do not choose their own.</param>
    internal static ComposedSpriteSheet Compose(SpriteSheetSources sources, int palette, int priority)
    {
        var layout = new SpriteSheetLayout(sources, palette, priority);
        Extent16<ImagePixels> size = layout.Size;
        int area = size.X * size.Y;
        var pixels = new RgbaPixel[area];
        var paletteIndices = new byte[area];
        var priorities = new byte[area];
        SpriteSheetDocument manifest = layout.Paint(new SpriteSheetPlanes(size, pixels, paletteIndices, priorities));
        return new ComposedSpriteSheet(size, pixels, paletteIndices, priorities, manifest);
    }
}
