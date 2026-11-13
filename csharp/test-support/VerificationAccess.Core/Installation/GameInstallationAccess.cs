namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>Verification access to <see cref="GameInstallation"/> members production does not use.</summary>
internal static class GameInstallationAccess
{
    /// <summary>Exposes <see cref="GameInstallation"/> state that only verification reads.</summary>
    extension(GameInstallation self)
    {
        /// <summary>The room characters' provenance manifest; the characters are import data no installation holds (#1636).</summary>
        public string RoomCharacterDirectory => Path.Combine(self.ContentDirectory, GameInstallationLayout.RoomCharacterDirectoryName);
        /// <summary>
        /// The installed picture store (#1635): every picture the sheets show, each written once, on pages the player edits in
        /// place, an edit changing the picture for every sheet that shows it.
        /// </summary>
        public string PictureStoreDirectory => Path.Combine(self.ContentDirectory, GameInstallationLayoutConstants.PictureStoreDirectoryName);
    }
}
