using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MorphBallEye.Artwork;

/// <summary>
/// The presentation boundary of the Morph Ball eye's programs (#641): the bank-$A8 spritemap each
/// <see cref="MorphBallEyeFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MorphBallEyeFrameDefinitions
{

    /// <summary>The installed frame each <see cref="MorphBallEyeFrame"/> shows.</summary>
    internal static EnemyFrameTable<MorphBallEyeFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.MorphEyeActive13 },
        { new(Pose: 1), EnemyFrameId.MorphEyeActive12 },
        { new(Pose: 10), EnemyFrameId.MorphEyeActive3 },
        { new(Pose: 11), EnemyFrameId.MorphEyeActive4 },
        { new(Pose: 12), EnemyFrameId.MorphEyeActive5 },
        { new(Pose: 13), EnemyFrameId.MorphEyeActive6 },
        { new(Pose: 14), EnemyFrameId.MorphEyeRightClosed },
        { new(Pose: 15), EnemyFrameId.MorphEyeRightTransition },
        { new(Pose: 16), EnemyFrameId.MorphEyeLeftClosed },
        { new(Pose: 17), EnemyFrameId.MorphEyeLeftTransition },
        { new(Pose: 18), EnemyFrameId.MorphEyeMountRight },
        { new(Pose: 19), EnemyFrameId.MorphEyeMountDown },
        { new(Pose: 2), EnemyFrameId.MorphEyeActive11 },
        { new(Pose: 20), EnemyFrameId.MorphEyeMountLeft },
        { new(Pose: 21), EnemyFrameId.MorphEyeMountUp },
        { new(Pose: 3), EnemyFrameId.MorphEyeActive10 },
        { new(Pose: 4), EnemyFrameId.MorphEyeActive9 },
        { new(Pose: 5), EnemyFrameId.MorphEyeActive8 },
        { new(Pose: 6), EnemyFrameId.MorphEyeActive7 },
        { new(Pose: 7), EnemyFrameId.MorphEyeActive0 },
        { new(Pose: 8), EnemyFrameId.MorphEyeActive1 },
        { new(Pose: 9), EnemyFrameId.MorphEyeActive2 },
    };
}
