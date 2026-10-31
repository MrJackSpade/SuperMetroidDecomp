using SuperMetroid.Core;


namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the gameplay base palettes (#1420): the import pipeline's private knowledge, decoded once
/// into the installed base-palette document.
/// </summary>
[EncodingOwner("cartridge addresses of the gameplay base palettes")]
internal static class GameplayBasePaletteSourceDefinitions
{
    /// <summary>$9A:8000, the complete starting 256-colour image.</summary>
    internal const int InitialPalette = 0x9a8000;

    /// <summary>$9A:FC00, the sixteen common room-sprite colours.</summary>
    internal const int CommonSpritePalette = 0x9afc00;
}
