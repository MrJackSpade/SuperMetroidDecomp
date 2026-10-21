using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The presentation boundary of the Ceres door's programs (#641): the bank-$A6 spritemap each
/// <see cref="CeresDoorFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class CeresDoorFrameDefinitions
{

    /// <summary>The Ridley scene's private door overlay frame.</summary>
    internal const EnemyFrameId RidleyPrivateOverlayFrame = EnemyFrameId.CeresDoorRidleyPrivateOverlay;

    /// <summary>The installed frame each <see cref="CeresDoorFrame"/> shows.</summary>
    internal static EnemyFrameTable<CeresDoorFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.CeresDoorRotatingOverlay },
        { new(Pose: 1), EnemyFrameId.CeresDoorLeftHold },
        { new(Pose: 10), EnemyFrameId.CeresDoorRightTransition3 },
        { new(Pose: 11), EnemyFrameId.CeresDoorMode7LeftWall },
        { new(Pose: 12), EnemyFrameId.CeresDoorMode7RightWall },
        { new(Pose: 2), EnemyFrameId.CeresDoorLeftTransition0 },
        { new(Pose: 3), EnemyFrameId.CeresDoorLeftTransition1 },
        { new(Pose: 4), EnemyFrameId.CeresDoorLeftTransition2 },
        { new(Pose: 5), EnemyFrameId.CeresDoorLeftTransition3 },
        { new(Pose: 6), EnemyFrameId.CeresDoorRightHold },
        { new(Pose: 7), EnemyFrameId.CeresDoorRightTransition0 },
        { new(Pose: 8), EnemyFrameId.CeresDoorRightTransition1 },
        { new(Pose: 9), EnemyFrameId.CeresDoorRightTransition2 },
    };
}
