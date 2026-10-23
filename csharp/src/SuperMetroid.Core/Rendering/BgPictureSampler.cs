using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Samples a background every pixel of which is a pre-coloured picture (#1403), decoding each against the frame's CGRAM;
/// no character is read, so no memory image is needed.
/// </summary>
/// <param name="colors">The frame's 256 CGRAM colours as RGBA.</param>
/// <param name="cgram">The frame's CGRAM.</param>
/// <param name="background">The background, which shows only pictures.</param>
internal sealed class BgPictureSampler(Rgba32[] colors, IPaletteColors cgram, IBgSource background)
{
    /// <summary>Decodes the pictures against the frame's CGRAM.</summary>
    private readonly BgPicturePixels pictures = new(cgram);

    /// <summary>Samples the plane pixel at a scrolled position; transparent where the background shows nothing.</summary>
    /// <exception cref="InvalidOperationException">The background shows a tile drawn from characters.</exception>
    internal BackgroundPixelSample Sample(Position16<BgPlanePixels> pixel, ColorZero colorZero = ColorZero.Transparent)
    {
        if (!background.TrySample(pixel, out BgSample shown))
            return new BackgroundPixelSample(default, false);
        if (shown.Picture is null)
            throw new InvalidOperationException("A pictured background shows a tile drawn from characters.");
        return pictures.TryDecode(shown, colors, colorZero, out Rgba32 pictured, out bool high)
            ? new BackgroundPixelSample(pictured, high) : new BackgroundPixelSample(default, false);
    }
}
