using SuperMetroid.AssetExtraction.Artwork;
using static SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork.FileSelectFieldTileDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>
/// Spells file select's digits and slot letters in menu tiles (<see cref="FileSelectFieldTileDefinitions"/>, #1467), which
/// the import writes into the installed presentation.
/// </summary>
public static class FileSelectFieldTiles
{
    /// <summary>The menu tile of decimal digit <paramref name="digit"/>.</summary>
    internal static BackgroundTile Digit(int digit) => DigitTileBase with { Tile = DigitTileBase.Tile + digit };

    /// <summary>The menu tile of save slot <paramref name="slot"/>'s letter.</summary>
    internal static BackgroundTile SlotLetter(int slot) => SamusLetterTileBase with { Tile = SamusLetterTileBase.Tile + slot };
}
