using SuperMetroid.Core.Assets;

namespace SuperMetroid.Desktop;

/// <summary>Result of updating one persistent GDI frame surface twice.</summary>
public readonly record struct RgbaBitmapSmokeTestResult(int Width, int Height, Color FinalPixel);

/// <summary>
/// Verifies channel order and in-place replacement without opening a WinForms window. This
/// guards the host optimization separately from the software-PPU pixel-parity benchmark.
/// </summary>
public static class RgbaBitmapSmokeTest
{
    public static RgbaBitmapSmokeTestResult Run()
    {
        Rgba32[] first =
        [
            new(255, 0, 0), new(0, 255, 0),
            new(0, 0, 255), new(255, 255, 255),
        ];
        using Bitmap bitmap = RgbaBitmap.Create(width: 2, height: 2, first);
        AssertPixel(bitmap, 0, 0, Color.FromArgb(255, 255, 0, 0), "initial red");
        AssertPixel(bitmap, 1, 0, Color.FromArgb(255, 0, 255, 0), "initial green");

        Rgba32[] second =
        [
            new(1, 2, 3), new(4, 5, 6),
            new(7, 8, 9), new(10, 11, 12),
        ];
        RgbaBitmap.CopyTo(bitmap, width: 2, height: 2, second);
        Color final = bitmap.GetPixel(1, 1);
        AssertPixel(bitmap, 1, 1, Color.FromArgb(255, 10, 11, 12), "replacement");
        return new RgbaBitmapSmokeTestResult(bitmap.Width, bitmap.Height, final);
    }

    private static void AssertPixel(Bitmap bitmap, int x, int y, Color expected, string subject)
    {
        Color actual = bitmap.GetPixel(x, y);
        if (actual.ToArgb() != expected.ToArgb())
        {
            throw new InvalidDataException(
                $"RGBA bitmap {subject} pixel was {actual}, expected {expected}.");
        }
    }
}
