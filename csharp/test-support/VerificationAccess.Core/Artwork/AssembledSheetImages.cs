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
    /// The image the loader assembles for <paramref name="converted"/>, a sheet of <paramref name="size"/> pixels: every pixel
    /// clear black, in recolour region zero and not blending, except each frame rectangle holding a pixel that is not clear,
    /// which shows the converted pixels, regions and blending there. A frame whose every pixel is clear shows nothing, is no
    /// picture, and the installation stores nothing for it.
    /// </summary>
    /// <exception cref="ArgumentException">The converted planes are not one entry per pixel of <paramref name="size"/>.</exception>
    internal static SpriteImagePixels Assemble(InstalledSheet converted, Extent16<ImagePixels> size)
    {
        Ensure.NotNull(converted);
        SpriteImagePixels source = Ensure.NotNull(converted.Pixels);
        int area = size.X * size.Y;
        Ensure.LengthEqual(source.Pixels, area);
        if (source.RecolourRegions is { } sourceRegions)
            Ensure.LengthEqual(sourceRegions, area);
        if (source.Blending is { } sourceBlending)
            Ensure.LengthEqual(sourceBlending, area);
        var pixels = new RgbaPixel[area];
        int[]? regions = source.RecolourRegions is null ? null : new int[area];
        bool[]? blending = source.Blending is null ? null : new bool[area];
        foreach (InstalledSheetFrameDocument frame in converted.Manifest.Frames.Values)
        {
            if (frame.Size.X == 0 || frame.Size.Y == 0 || !Shows(source.Pixels, size, frame))
                continue;
            for (int row = 0; row < frame.Size.Y; row++)
            {
                int line = (frame.Corner.Y + row) * size.X + frame.Corner.X;
                Array.Copy(source.Pixels, line, pixels, line, frame.Size.X);
                if (regions is not null && source.RecolourRegions is { } fromRegions)
                    Array.Copy(fromRegions, line, regions, line, frame.Size.X);
                if (blending is not null && source.Blending is { } fromBlending)
                    Array.Copy(fromBlending, line, blending, line, frame.Size.X);
            }
        }
        return new SpriteImagePixels(pixels, regions, blending);
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
    /// gives a sheet a map when a store page it reads has one, which another sheet's picture may be the reason for.
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

    /// <summary>Whether any pixel of <paramref name="frame"/>'s rectangle of <paramref name="pixels"/> is not clear.</summary>
    private static bool Shows(RgbaPixel[] pixels, Extent16<ImagePixels> size, InstalledSheetFrameDocument frame)
    {
        for (int row = 0; row < frame.Size.Y; row++)
        {
            int line = (frame.Corner.Y + row) * size.X + frame.Corner.X;
            for (int column = 0; column < frame.Size.X; column++)
            {
                if (pixels[line + column].A != 0)
                    return true;
            }
        }
        return false;
    }
}
