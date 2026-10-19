using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>The presentation boundary of Ridley's body frames: the installed extended frame each one shows.</summary>
internal static class RidleyBodyFrameDefinitions
{
    /// <summary>The installed frame each <see cref="RidleyCollisionDefinitions.BodyFrame"/> shows.</summary>
    internal static EnemyFrameTable<RidleyCollisionDefinitions.BodyFrame> ShownFrames { get; } = new()
    {
        { RidleyCollisionDefinitions.BodyFrame.Forward, EnemyFrameId.RidleyBodyEAD7 },
        { RidleyCollisionDefinitions.BodyFrame.Left, EnemyFrameId.RidleyBodyE983 },
        { RidleyCollisionDefinitions.BodyFrame.LeftLegsExtended, EnemyFrameId.RidleyBodyEA71 },
        { RidleyCollisionDefinitions.BodyFrame.LeftLegsHalfExtended, EnemyFrameId.RidleyBodyEA4F },
        { RidleyCollisionDefinitions.BodyFrame.LeftMouthHalfOpen, EnemyFrameId.RidleyBodyE9C7 },
        { RidleyCollisionDefinitions.BodyFrame.LeftMouthOpen, EnemyFrameId.RidleyBodyE9E9 },
        { RidleyCollisionDefinitions.BodyFrame.Right, EnemyFrameId.RidleyBodyE9A5 },
        { RidleyCollisionDefinitions.BodyFrame.RightLegsExtended, EnemyFrameId.RidleyBodyEAB5 },
        { RidleyCollisionDefinitions.BodyFrame.RightLegsHalfExtended, EnemyFrameId.RidleyBodyEA93 },
        { RidleyCollisionDefinitions.BodyFrame.RightMouthHalfOpen, EnemyFrameId.RidleyBodyEA0B },
        { RidleyCollisionDefinitions.BodyFrame.RightMouthOpen, EnemyFrameId.RidleyBodyEA2D },
    };
}
