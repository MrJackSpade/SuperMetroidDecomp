using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>The presentation boundary of Crocomire's tongue frames: the installed extended frame each one shows.</summary>
internal static class CrocomireTongueFrameDefinitions
{
    /// <summary>The installed frame each <see cref="CrocomireTongueFrame"/> shows.</summary>
    internal static EnemyFrameTable<CrocomireTongueFrame> ShownFrames { get; } = new()
    {
        { CrocomireTongueFrame.Fight0, EnemyFrameId.CrocomireOamC65E },
        { CrocomireTongueFrame.Fight1, EnemyFrameId.CrocomireOamC668 },
        { CrocomireTongueFrame.Fight2, EnemyFrameId.CrocomireOamC672 },
        { CrocomireTongueFrame.Fight3, EnemyFrameId.CrocomireOamC67C },
        { CrocomireTongueFrame.MeltingPose0, EnemyFrameId.CrocomireOamCACE },
        { CrocomireTongueFrame.MeltingPose1, EnemyFrameId.CrocomireOamCAD8 },
        { CrocomireTongueFrame.MeltingPose2, EnemyFrameId.CrocomireOamCAE2 },
        { CrocomireTongueFrame.MeltingPose3, EnemyFrameId.CrocomireOamCAEC },
        { CrocomireTongueFrame.MeltingPose4, EnemyFrameId.CrocomireOamCAF6 },
    };
}
