using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>The Chozo statue hand's response to Samus touching it (#1395).</summary>
internal interface IChozoStatueHand
{
    /// <summary>Starts the statue's grab when Samus's collision reaches the special block at <paramref name="block"/>.</summary>
    /// <param name="level">The room's level, which holds the block.</param>
    /// <param name="block">The position of the special block Samus's collision reached.</param>
    /// <param name="samus">Samus, whom the hand may grab.</param>
    /// <param name="collisionPose">The pose Samus collided in.</param>
    /// <param name="movingDown">Whether Samus reached the block moving down.</param>
    void NotifyChozoStatueHandCollision(RoomLevelData level, Position16<RoomBlocks> block, SamusState samus,
        SamusPoseId collisionPose, bool movingDown);
}
