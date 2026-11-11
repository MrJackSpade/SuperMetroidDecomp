using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>
/// The menu tiles file select's slot fields and prompts are written with (#1467): only the import reads them; it pictures
/// each patch, digit, slot letter and energy tank once, and the game places those pictures.
/// </summary>
public static class FileSelectFieldTileDefinitions
{
    /// <summary>The blank menu tile.</summary>
    internal static BackgroundTile BlankTile { get; } = new(15, 0, highPriority: false);
    /// <summary>The first numeric menu tile: the digits follow it in order.</summary>
    internal static BackgroundTile DigitTileBase { get; } = new(96, 0, highPriority: true);
    /// <summary>The first letter tile used to draw a save slot's name in the Copy and Clear prompts: the slots' letters follow it.</summary>
    internal static BackgroundTile SamusLetterTileBase { get; } = new(106, 0, highPriority: true);
    /// <summary>$81:A0F5: the filled-tank tile, drawn in the installed health digit's colour variant.</summary>
    internal const int FilledTankTile = 152;
    /// <summary>$81:A0EC: the empty-tank tile, drawn in the installed health digit's colour variant.</summary>
    internal const int EmptyTankTile = 153;
}
