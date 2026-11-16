using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification reads of the room's Kraid (#2194).</summary>
internal static class KraidRoomAccess
{
    /// <summary>The room's Kraid, as the body layer he attached to the room shows him.</summary>
    extension(RoomEnemySystem enemies)
    {
        /// <summary>
        /// The encounter whose body the room shows, the one his body last began as it set up and attached his body layer
        /// with; none in a room he has not set up in.
        /// </summary>
        internal KraidEnemyState? Kraid => (enemies.Presentation.BossBodyDisplay as KraidBodyLayer)?.Kraid;
    }
}
