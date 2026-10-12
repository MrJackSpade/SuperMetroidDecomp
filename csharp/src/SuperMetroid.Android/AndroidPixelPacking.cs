using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Android;

/// <summary>Android <c>Bitmap.Config.Argb8888</c> pixels as <c>SetPixels</c> takes them (#223): alpha, red, green, blue from the top byte down.</summary>
[SnesEncodingOwner]
internal static class AndroidPixelPacking
{
    /// <summary>The packed ARGB integer for <paramref name="color"/>.</summary>
    public static int Argb(Rgba32 color) =>
        unchecked((int)((uint)color.A << 24 | (uint)color.R << 16 | (uint)color.G << 8 | color.B));
}
