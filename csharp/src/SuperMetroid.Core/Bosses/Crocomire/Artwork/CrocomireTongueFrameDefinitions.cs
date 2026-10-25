using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>The presentation boundary of Crocomire's tongue frames: the installed extended frame each one shows.</summary>
internal static class CrocomireTongueFrameDefinitions
{
    /// <summary>The installed frame each <see cref="CrocomireTongueFrame"/> shows.</summary>
    internal static EnemyFrameTable<CrocomireTongueFrame> ShownFrames { get; } = new()
    {
        { CrocomireTongueFrame.Fight0, EnemyFrameId.CrocomireTongueFight0 },
        { CrocomireTongueFrame.Fight1, EnemyFrameId.CrocomireTongueFight1 },
        { CrocomireTongueFrame.Fight2, EnemyFrameId.CrocomireTongueFight2 },
        { CrocomireTongueFrame.Fight3, EnemyFrameId.CrocomireTongueFight3 },
        { CrocomireTongueFrame.MeltingPose0, EnemyFrameId.CrocomireTongueMeltingPose0 },
        { CrocomireTongueFrame.MeltingPose1, EnemyFrameId.CrocomireTongueMeltingPose1 },
        { CrocomireTongueFrame.MeltingPose2, EnemyFrameId.CrocomireTongueMeltingPose2 },
        { CrocomireTongueFrame.MeltingPose3, EnemyFrameId.CrocomireTongueMeltingPose3 },
        { CrocomireTongueFrame.MeltingPose4, EnemyFrameId.CrocomireTongueMeltingPose4 },
    };
}
