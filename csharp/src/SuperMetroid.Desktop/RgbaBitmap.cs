using SuperMetroid.Core;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Desktop;

/// <summary>Copies platform-neutral core RGBA pixels into a Windows GDI bitmap.</summary>
public static class RgbaBitmap
{
    /// <summary>
    /// Updates an existing GDI bitmap in place. The playable canvas owns one bitmap for its
    /// lifetime instead of allocating and disposing a native GDI object on every video frame.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe void CopyTo(
        Bitmap bitmap,
        Extent16<ScreenPixels> size,
        ReadOnlySpan<Rgba32> pixels)
    {
        Ensure.NotNull(bitmap);
        int width = size.X, height = size.Y;
        ArgumentOutOfRangeException.ThrowIfZero(width, nameof(size));
        ArgumentOutOfRangeException.ThrowIfZero(height, nameof(size));
        if (bitmap.Width != width || bitmap.Height != height ||
            bitmap.PixelFormat != PixelFormat.Format32bppArgb)
        {
            throw new ArgumentException(
                "Destination bitmap must be a matching 32-bpp ARGB raster.",
                nameof(bitmap));
        }
        Ensure.LengthEqual(pixels, width * height);

        var bounds = new Rectangle(0, 0, width, height);
        BitmapData data = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            for (int y = 0; y < height; y++)
            {
                // GDI's Format32bppArgb bytes are physically BGRA on this little-endian
                // host. A freshly allocated bitmap has a positive stride, which may still
                // contain padding beyond width*4 and therefore must be honored per row.
                byte* row = (byte*)data.Scan0 + y * data.Stride;
                for (int x = 0; x < width; x++)
                {
                    Rgba32 color = pixels[y * width + x];
                    int destination = x * 4;
                    row[destination] = color.B;
                    row[destination + 1] = color.G;
                    row[destination + 2] = color.R;
                    row[destination + 3] = color.A;
                }
            }
        }
        finally
        {
            // Always release the native lock if a malformed debugger edit throws halfway
            // through conversion; otherwise the Bitmap remains unusable and undisposable.
            bitmap.UnlockBits(data);
        }

    }
}
