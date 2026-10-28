namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>Verification access to <see cref="GameInstallation"/> members production does not use.</summary>
internal static class GameInstallationAccess
{
    /// <summary>Exposes <see cref="GameInstallation"/> state that only verification reads.</summary>
    extension(GameInstallation self)
    {
        /// <summary>
        /// Stock room block compositions under the replaceable content tree, which extraction composes the block sheets from
        /// (#1431); a room's blocks are changed only through their pictures in the block sheet.
        /// </summary>
        public string RoomMetatileDirectory => Path.Combine(self.ContentDirectory, GameInstallationLayout.RoomMetatileDirectoryName);
    }
}
