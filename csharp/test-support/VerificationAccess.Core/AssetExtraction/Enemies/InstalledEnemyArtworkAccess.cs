namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The installed enemy directory's identity, which only verification compares.</summary>
internal static class InstalledEnemyArtworkAccess
{
    extension(InstalledEnemyArtwork self)
    {
        /// <summary>
        /// The selected presentation of the whole enemy directory: an edit to any of its enemy, boss or area artwork
        /// changes it.
        /// </summary>
        internal string ContentIdentity => InstalledEnemyArtwork.DirectoryIdentity(self.Enemies, self.Bosses, self.Areas);
    }
}
