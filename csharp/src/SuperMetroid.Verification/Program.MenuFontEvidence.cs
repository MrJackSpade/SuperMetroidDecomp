using SuperMetroid.Core.Assets;

internal static class MenuFontEvidenceVerification
{
    // Evidence assertions belong to the existing two font proofs, not a second
    // oracle or a separate test that searches for other artwork differences.
    internal static byte MenuEvidencePixel(IndexedPngImage image, int tile, Position16<CharacterPixels> pixel) =>
        image.Pixels[(tile / 16 * 8 + pixel.Y) * image.Size.X + tile % 16 * 8 + pixel.X];

    internal static void AssertMenuInkRegionEqual(IndexedPngImage image,
        int firstTile, Position16<CharacterPixels> first, int secondTile, Position16<CharacterPixels> second,
        Extent16<CharacterPixels> size, string context)
    {
        for (int y = 0; y < size.Y; y++)
        for (int x = 0; x < size.X; x++)
        {
            var step = new Offset16<CharacterPixels>((short)x, (short)y);
            AssertEqual(MenuEvidencePixel(image, firstTile, first + step) == 14,
                MenuEvidencePixel(image, secondTile, second + step) == 14,
                $"{context} ink at {x},{y}");
        }
    }
}
