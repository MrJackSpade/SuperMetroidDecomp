using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>The Chozo statue hand's response to Samus touching it (#1395).</summary>
internal interface IChozoStatueHand
{
    /// <summary>Starts the statue's grab when Samus's collision reaches the hand block.</summary>
    void NotifyChozoStatueHandCollision(RoomLevelData level, RoomCollisionBlock block, SamusState samus,
        SamusPoseId collisionPose, bool movingDown);
}
