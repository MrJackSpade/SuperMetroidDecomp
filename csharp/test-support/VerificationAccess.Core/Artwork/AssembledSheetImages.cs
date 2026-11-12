using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The image a converted sheet shows once installed (#1635). The installation holds each picture once, in the picture store
/// every sheet shares, and a sheet's manifest names where in the store each frame's picture lies; the loader assembles the
/// sheet's image from the store, clear everywhere except the frames' rectangles. A check that compares an installed image
/// with a sheet it composed and converted (<see cref="InstalledSheetConversion"/>) compares it with this assembly of the
/// conversion: what the conversion holds outside its pictures, the installation never holds.
/// </summary>
internal static class AssembledSheetImages
{
    /// <summary>
    /// The image the loader assembles for <paramref name="converted"/>, of the size its manifest names: every pixel clear
    /// black, in recolour region zero and not blending, except each frame's rectangle, which shows the converted pixels,
    /// regions and blending there, copied whole (<see cref="InstalledImagePlanes.CopyTo"/>). The installation stores only the
    /// box of what each rectangle holds (<see cref="InstalledImagePlanes.ContentBox"/>, #1646), and the rest of the rectangle
    /// is as the image starts, so copying the whole rectangle leaves the image as the installation's copy of the box does. A
    /// rectangle that holds only clear black pixels in region zero, blending nowhere, is stored nowhere and stays as the
    /// image starts, which copying it leaves alike.
    /// </summary>
    /// <exception cref="ArgumentException">The converted pixels are not one per pixel of the manifest's size, or a frame leaves the image.</exception>
    internal static SpriteImagePixels Assemble(InstalledSheet converted)
    {
        Ensure.NotNull(converted);
        InstalledImagePlanes source = converted.Image;
        int area = source.Size.X * source.Size.Y;
        Ensure.LengthEqual(Ensure.NotNull(source.Planes).Pixels, area);
        var assembled = new InstalledImagePlanes(new SpriteImagePixels(new RgbaPixel[area],
            source.Planes.RecolourRegions is null ? null : new int[area],
            source.Planes.Blending is null ? null : new bool[area]), source.Size);
        foreach (InstalledSheetFrameDocument frame in converted.Manifest.Frames.Values)
        {
            var rectangle = new FrameRectangle(frame.Corner, frame.Size);
            if (!rectangle.IsEmpty)
                source.CopyTo(rectangle, assembled, frame.Corner);
        }
        return assembled.Planes;
    }

    /// <summary>Whether <paramref name="installed"/> holds exactly the pixels of <paramref name="assembled"/>.</summary>
    internal static bool HoldsPixels(SpriteImage installed, SpriteImagePixels assembled)
    {
        Ensure.NotNull(installed);
        Ensure.NotNull(assembled);
        return installed.Pixels.SequenceEqual(assembled.Pixels);
    }

    /// <summary>
    /// Whether every pixel of <paramref name="installed"/> lies in the recolour region it lies in on
    /// <paramref name="assembled"/>. An image without a recolour-region map holds every pixel in region zero: the loader
    /// gives a sheet a map when one of its frames names one (#1644), and the conversion keeps a sheet-wide map whatever its
    /// frames hold.
    /// </summary>
    internal static bool HoldsRecolourRegions(SpriteImage installed, SpriteImagePixels assembled)
    {
        Ensure.NotNull(installed);
        Ensure.NotNull(assembled);
        ReadOnlySpan<int> held = installed.RecolourRegions;
        int[]? expected = assembled.RecolourRegions;
        int area = assembled.Pixels.Length;
        if (!held.IsEmpty && held.Length != area)
            return false;
        for (int pixel = 0; pixel < area; pixel++)
        {
            if ((held.IsEmpty ? 0 : held[pixel]) != (expected is null ? 0 : expected[pixel]))
                return false;
        }
        return true;
    }
}
