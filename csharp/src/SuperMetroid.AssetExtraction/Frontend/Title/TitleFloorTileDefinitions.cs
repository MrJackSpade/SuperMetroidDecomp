namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>
/// The title floor's extents and where its artwork sits among its tile numbers (#1404, #1467): the import composes the
/// floor's pictures from them.
/// </summary>
internal static class TitleFloorTileDefinitions
{
    /// <summary>The tile number of the first floor character.</summary>
    internal const int FloorFirstTile = 0;

    /// <summary>The first of the four floor tiles ($E0-$E3) the Baby Metroid animation replaces a page at a time.</summary>
    internal const int BabyFirstTile = 224;

    /// <summary>The Baby Metroid's floor characters: four pages of four characters.</summary>
    public const int BabyCharacterByteCount = 4 * BabyCharacterPageByteCount;

    /// <summary>The title floor's 256 eight-bit characters.</summary>
    public const int FloorCharacterByteCount = 256 * 64;

    /// <summary>The cells of the title floor's map that the scenes show.</summary>
    public const int FloorMapByteCount = 64 * 64;

    /// <summary>The floor character every cell outside the shown map holds.</summary>
    public const byte FloorInitialMapByte = 255;

    /// <summary>One Baby Metroid animation page: four eight-bit characters.</summary>
    public const int BabyCharacterPageByteCount = 4 * 64;

    /// <summary>The Baby Metroid animation pages the floor shows: pages 0, 1 and 2.</summary>
    public const int BabyPageCount = 3;
}
