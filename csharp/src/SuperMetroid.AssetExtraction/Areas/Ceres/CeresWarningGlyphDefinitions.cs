namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The document that holds the Ceres escape warning pages (#1466), and where the warning's characters begin among the
/// room's characters. The pages' entries are the original's packed tile placements, so the document is import data (#1638):
/// extraction composes the pages' pictures from it, the installation holds those pictures, and gameplay writes them by page.
/// </summary>
internal static class CeresWarningGlyphDefinitions
{
    /// <summary>Supported warning-overlay schema revision.</summary>
    internal const int Version = 2;
    /// <summary>Import-data JSON file containing all five Ceres warning tilemap pages.</summary>
    internal const string FileName = "ceres-escape-overlay-tilemaps.json";
    /// <summary>$A6:C4D9: the warning text's characters begin at character $182, where its Latin letters start.</summary>
    internal const int WarningFirstCharacter = 386;
}
