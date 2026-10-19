using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Shaktool.Artwork;

/// <summary>
/// The presentation boundary of the Shaktool's programs (#641): the bank-$AA spritemap each
/// <see cref="ShaktoolFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ShaktoolFrameDefinitions
{

    /// <summary>The installed frame each <see cref="ShaktoolFrame"/> shows.</summary>
    internal static EnemyFrameTable<ShaktoolFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.ShaktoolFinalSaw0 },
        { new(Pose: 1), EnemyFrameId.ShaktoolFinalSaw1 },
        { new(Pose: 10), EnemyFrameId.ShaktoolHeadDown },
        { new(Pose: 11), EnemyFrameId.ShaktoolHeadDownLeft },
        { new(Pose: 12), EnemyFrameId.ShaktoolPrimarySaw0 },
        { new(Pose: 13), EnemyFrameId.ShaktoolPrimarySaw1 },
        { new(Pose: 14), EnemyFrameId.ShaktoolPrimarySaw2 },
        { new(Pose: 2), EnemyFrameId.ShaktoolFinalSaw2 },
        { new(Pose: 3), EnemyFrameId.ShaktoolArm },
        { new(Pose: 4), EnemyFrameId.ShaktoolHeadLeft },
        { new(Pose: 5), EnemyFrameId.ShaktoolHeadUpLeft },
        { new(Pose: 6), EnemyFrameId.ShaktoolHeadUp },
        { new(Pose: 7), EnemyFrameId.ShaktoolHeadUpRight },
        { new(Pose: 8), EnemyFrameId.ShaktoolHeadRight },
        { new(Pose: 9), EnemyFrameId.ShaktoolHeadDownRight },
    };
}
