using SuperMetroid.AssetExtraction.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// The installed ending Mode 7 artwork, for checks of the floors the import pictures from it (#1555); the game never
/// loads it.
/// </summary>
internal static class GameInstallationEndingFloorAccess
{
    extension(GameInstallation self)
    {
        /// <summary>The installed ending Mode 7 scene artwork's directory, which the player edits in place.</summary>
        internal string EndingMode7Directory => Path.Combine(self.ContentDirectory, GameInstallationLayout.EndingMode7DirectoryName);

        /// <summary>Loads the installed ending Mode 7 artwork.</summary>
        internal EndingFloorCharacters LoadEndingMode7Art() => EndingMode7ArtworkFiles.Load(self.EndingMode7Directory);
    }
}
