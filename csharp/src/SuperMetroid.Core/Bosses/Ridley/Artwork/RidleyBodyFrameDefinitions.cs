using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>The presentation boundary of Ridley's body frames: the installed extended frame each one shows.</summary>
internal static class RidleyBodyFrameDefinitions
{
    /// <summary>The installed frame each <see cref="RidleyCollisionDefinitions.BodyFrame"/> shows.</summary>
    internal static EnemyFrameTable<RidleyCollisionDefinitions.BodyFrame> ShownFrames { get; } = new()
    {
        { RidleyCollisionDefinitions.BodyFrame.Forward, EnemyFrameId.RidleyBodyForward },
        { RidleyCollisionDefinitions.BodyFrame.Left, EnemyFrameId.RidleyBodyLeft },
        { RidleyCollisionDefinitions.BodyFrame.LeftLegsExtended, EnemyFrameId.RidleyBodyLeftLegsExtended },
        { RidleyCollisionDefinitions.BodyFrame.LeftLegsHalfExtended, EnemyFrameId.RidleyBodyLeftLegsHalfExtended },
        { RidleyCollisionDefinitions.BodyFrame.LeftMouthHalfOpen, EnemyFrameId.RidleyBodyLeftMouthHalfOpen },
        { RidleyCollisionDefinitions.BodyFrame.LeftMouthOpen, EnemyFrameId.RidleyBodyLeftMouthOpen },
        { RidleyCollisionDefinitions.BodyFrame.Right, EnemyFrameId.RidleyBodyRight },
        { RidleyCollisionDefinitions.BodyFrame.RightLegsExtended, EnemyFrameId.RidleyBodyRightLegsExtended },
        { RidleyCollisionDefinitions.BodyFrame.RightLegsHalfExtended, EnemyFrameId.RidleyBodyRightLegsHalfExtended },
        { RidleyCollisionDefinitions.BodyFrame.RightMouthHalfOpen, EnemyFrameId.RidleyBodyRightMouthHalfOpen },
        { RidleyCollisionDefinitions.BodyFrame.RightMouthOpen, EnemyFrameId.RidleyBodyRightMouthOpen },
    };
}
