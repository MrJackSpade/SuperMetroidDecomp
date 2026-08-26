using System.Drawing.Imaging;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.RoomViewer;

/// <summary>Copies platform-neutral core RGBA pixels into a Windows GDI bitmap.</summary>
internal static class RgbaBitmap
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

        return bitmap;
    }
}
