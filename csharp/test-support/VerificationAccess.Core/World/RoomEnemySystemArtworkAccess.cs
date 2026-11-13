using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// Verification's installation of artwork into a room enemy system it builds without a runtime (#1395). The enemy sheets
/// and frame displays are the session's, given when it is built (#1616).
/// </summary>
internal static class RoomEnemySystemArtworkAccess
{
    /// <summary>The artwork a fixture installs.</summary>
    extension(RoomEnemySystem self)
    {
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
