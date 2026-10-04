using SuperMetroid.Core.Assets;

internal static partial class Program
{
    // Evidence assertions belong to the existing two font proofs, not a second
    // oracle or a separate test that searches for other artwork differences.
    private static byte MenuEvidencePixel(IndexedPngImage image, int tile, int x, int y) =>
        image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];

    private static void AssertMenuInkRegionEqual(IndexedPngImage image,
        int firstTile, int firstX, int firstY, int secondTile, int secondX, int secondY,
        int width, int height, string context)
    {
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            AssertEqual(MenuEvidencePixel(image, firstTile, firstX + x, firstY + y) == 14,
                MenuEvidencePixel(image, secondTile, secondX + x, secondY + y) == 14,
                $"{context} ink at {x},{y}");
    }
}
