using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The pixel planes one thread reuses for every sheet it paints and converts before painting the next (#1490), grown to
/// the largest sheet painted so far, so a sheet that is converted and dropped costs no planes of its own. Not shared
/// between threads.
/// </summary>
internal sealed class SpriteSheetCanvas
{
    /// <summary>Each pixel's colour.</summary>
    private RgbaPixel[] pixels = [];

    /// <summary>Each pixel's colour group's place times sixteen plus its colour.</summary>
    private byte[] paletteIndices = [];

    /// <summary>
    /// The planes of a sheet of <paramref name="size"/>, every pixel transparent as a new sheet's are; they are valid
    /// until the next call.
    /// </summary>
    internal SpriteSheetPlanes Cleared(Extent16<ImagePixels> size)
    {
        int area = size.X * size.Y;
        if (pixels.Length < area)
        {
            pixels = new RgbaPixel[area];
            paletteIndices = new byte[area];
        }
        var planes = new SpriteSheetPlanes(size, pixels.AsSpan(0, area), paletteIndices.AsSpan(0, area));
        // The previous sheet's pixels are cleared, so the new sheet starts as transparent as freshly allocated planes.
        planes.Pixels.Clear();
        planes.PaletteIndices.Clear();
        return planes;
    }
}
