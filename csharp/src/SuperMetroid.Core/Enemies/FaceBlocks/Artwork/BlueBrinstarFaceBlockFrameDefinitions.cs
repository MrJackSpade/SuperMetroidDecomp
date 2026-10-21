using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.FaceBlocks.Artwork;

/// <summary>
/// The presentation boundary of the Blue Brinstar face block's programs (#641): the bank-$A8 spritemap each
/// <see cref="BlueBrinstarFaceBlockFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class BlueBrinstarFaceBlockFrameDefinitions
{

    /// <summary>The installed frame each <see cref="BlueBrinstarFaceBlockFrame"/> shows.</summary>
    internal static EnemyFrameTable<BlueBrinstarFaceBlockFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.FaceBlockNeutral },
        { new(Pose: 1), EnemyFrameId.FaceBlockSamusLeft1 },
        { new(Pose: 2), EnemyFrameId.FaceBlockSamusLeft2 },
        { new(Pose: 3), EnemyFrameId.FaceBlockSamusRight1 },
        { new(Pose: 4), EnemyFrameId.FaceBlockSamusRight2 },
    };
}
