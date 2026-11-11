namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The editable document that holds the Ceres escape warning pages (#1466), and where the warning's characters begin
/// among the room's characters. Extraction composes the pages' pictures from the installed pages; gameplay writes the
/// pictures by page.
/// </summary>
internal static class CeresWarningGlyphDefinitions
{
    /// <summary>Supported editable warning-overlay schema revision.</summary>
    internal const int Version = 2;
    /// <summary>Installed JSON file containing all five Ceres warning tilemap pages.</summary>
    internal const string FileName = "ceres-escape-overlay-tilemaps.json";
    /// <summary>$A6:C4D9: the warning text's characters begin at character $182, where its Latin letters start.</summary>
    internal const int WarningFirstCharacter = 386;
}
