using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>Verification's installation of artwork into a room enemy system it builds without a runtime (#1395).</summary>
internal static class RoomEnemySystemArtworkAccess
{
    /// <summary>The artwork a fixture installs.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>The session's installed enemy presentation.</summary>
        internal EnemyTileArtworkCatalog? TileArtwork
        {
            get => self.Room.Session.TileArtwork;
            set => self.Room.Session.TileArtwork = value;
        }

        /// <summary>The bosses' installed artwork, through the session's binding.</summary>
        internal BossArtworkCatalog? BossArtwork
        {
            get => self.BossArtworkBinding.Artwork;
            set => self.BossArtworkBinding.Artwork = value;
        }
    }
}
