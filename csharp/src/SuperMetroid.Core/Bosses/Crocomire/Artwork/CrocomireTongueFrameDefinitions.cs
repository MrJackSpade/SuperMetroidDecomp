using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>The presentation boundary of Crocomire's tongue frames: the installed frame each one shows.</summary>
internal static class CrocomireTongueFrameDefinitions
{
    /// <summary>The installed frame each <see cref="CrocomireTongueFrame"/> shows.</summary>
    internal static EnemyFrameTable<CrocomireTongueFrame> ShownFrames { get; } = new()
    {
        { CrocomireTongueFrame.Fight0, EnemyFrameId.CrocomireTongueFightFirst },
        { CrocomireTongueFrame.Fight1, EnemyFrameId.CrocomireTongueFightSecond },
        { CrocomireTongueFrame.Fight2, EnemyFrameId.CrocomireTongueFightThird },
        { CrocomireTongueFrame.Fight3, EnemyFrameId.CrocomireTongueFightFourth },
        { CrocomireTongueFrame.MeltingPose0, EnemyFrameId.CrocomireTongueMeltingPoseFirst },
        { CrocomireTongueFrame.MeltingPose1, EnemyFrameId.CrocomireTongueMeltingPoseSecond },
        { CrocomireTongueFrame.MeltingPose2, EnemyFrameId.CrocomireTongueMeltingPoseThird },
        { CrocomireTongueFrame.MeltingPose3, EnemyFrameId.CrocomireTongueMeltingPoseFourth },
        { CrocomireTongueFrame.MeltingPose4, EnemyFrameId.CrocomireTongueMeltingPoseFifth },
    };
}
