using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Desktop;

/// <summary>Copies platform-neutral core RGBA pixels into a Windows GDI bitmap.</summary>
public static class RgbaBitmap
{
    /// <summary>
    /// Converts without <see cref="Bitmap.SetPixel(int, int, Color)"/>, whose per-pixel
    /// managed/native transition makes multi-megapixel rooms painfully slow to inspect.
    /// </summary>
    public static unsafe Bitmap Create(int width, int height, ReadOnlySpan<Rgba32> pixels)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
        if (pixels.Length != checked(width * height))
            throw new ArgumentException("Pixel count does not match dimensions.", nameof(pixels));

        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        try
        {
            CopyTo(bitmap, width, height, pixels);
            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Updates an existing GDI bitmap in place. The playable canvas owns one bitmap for its
    /// lifetime instead of allocating and disposing a native GDI object on every video frame.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe void CopyTo(
        Bitmap bitmap,
        int width,
        int height,
        ReadOnlySpan<Rgba32> pixels)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
        if (bitmap.Width != width || bitmap.Height != height ||
            bitmap.PixelFormat != PixelFormat.Format32bppArgb)
        {
            throw new ArgumentException(
                "Destination bitmap must be a matching 32-bpp ARGB raster.",
                nameof(bitmap));
        }
        if (pixels.Length != checked(width * height))
            throw new ArgumentException("Pixel count does not match dimensions.", nameof(pixels));

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
