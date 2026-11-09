using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>Verification's installation of artwork into a room enemy system it builds without a runtime (#1395).</summary>
internal static class RoomEnemySystemArtworkAccess
{
    /// <summary>The artwork a fixture installs.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>
        /// Installs what the installation gives the session from the import pipeline's enemy artwork (#1583): its extended
        /// frames' display bindings and the enemy sheets composed from it. The session keeps none of the import's artwork,
        /// so a fixture that reads it again keeps its own reference.
        /// </summary>
        internal EnemyTileSourceArtwork? TileArtwork
        {
            set
            {
                self.Room.Session.FrameDisplays = value?.ExtendedFrames?.Displays;
                self.Room.Session.Sheets = value is null ? null : SuperMetroid.TestSupport.RepositoryInstallation.Verification.EnemySheetFixtures.For(value);
            }
        }

        /// <summary>
        /// Installs the bosses' installed artwork as the game binds it (#1583): of it, the session's binding takes Kraid's eye
        /// colours, the one part the room's bosses read.
        /// </summary>
        internal BossSourceArtwork? BossArtwork
        {
            set => self.BossArtworkBinding.KraidEyes = value?.KraidColors?.Eyes();
        }
    }
}
