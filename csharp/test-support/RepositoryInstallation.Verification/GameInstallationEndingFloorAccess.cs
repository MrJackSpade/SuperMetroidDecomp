namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// The installed ending Mode 7 maps' directory, for checks of the floors the import pictures from it (#1555); the game
/// never loads it, and its characters are import data no installation holds (#1636).
/// </summary>
internal static class GameInstallationEndingFloorAccess
{
    extension(GameInstallation self)
    {
        /// <summary>The installed ending Mode 7 scene maps' directory, which the player edits in place.</summary>
        internal string EndingMode7Directory => Path.Combine(self.ContentDirectory, GameInstallationLayout.EndingMode7DirectoryName);
    }
}
