using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>Verification's installation of artwork into a room enemy system it builds without a runtime (#1395).</summary>
internal static class RoomEnemySystemArtworkAccess
{
    /// <summary>The artwork a fixture installs.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>The session's installed enemy presentation, and the enemy sheets drawn with it.</summary>
        /// <remarks>Fixtures install the import pipeline's artwork, whose compositions the sheets are composed from.</remarks>
        internal EnemyTileSourceArtwork? TileArtwork
        {
            get => (EnemyTileSourceArtwork?)self.Room.Session.TileArtwork;
            set
            {
                self.Room.Session.TileArtwork = value;
                self.Room.Session.Sheets = value is null ? null : SuperMetroid.TestSupport.RepositoryInstallation.Verification.EnemySheetFixtures.For(value);
            }
        }

        /// <summary>The bosses' installed artwork, through the session's binding.</summary>
        internal BossArtworkCatalog? BossArtwork
        {
            get => self.BossArtworkBinding.Artwork;
            set => self.BossArtworkBinding.Artwork = value;
        }
    }
}
