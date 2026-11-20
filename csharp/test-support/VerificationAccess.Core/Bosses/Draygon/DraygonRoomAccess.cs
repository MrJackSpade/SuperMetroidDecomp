using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Verification reads of the room's Draygon (#2194).</summary>
internal static class DraygonRoomAccess
{
    /// <summary>The room's Draygon, as the place his body occupies holds him.</summary>
    extension(RoomEnemySystem enemies)
    {
        /// <summary>
        /// The encounter the room's Draygon body holds, the one his body's setup began; none in a room whose places hold no
        /// Draygon body, as a defeated Draygon's room does.
        /// </summary>
        internal DraygonEnemyState? Draygon =>
            enemies.Places.InUpdateOrder.Select(place => SuperMetroid.Core.Enemies.EnemyPartners.Listed<DraygonBodyEntity>(place).State).FirstOrDefault(state => state is not null);
    }
}
