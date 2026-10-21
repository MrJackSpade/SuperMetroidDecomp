using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>The presentation boundary of the Ceres Baby's poses (#641): the bank-$A6 spritemap each pose shows.</summary>
internal static class CeresBabyFrameDefinitions
{

    /// <summary>The installed frame each <see cref="CeresBabyPose"/> shows.</summary>
    internal static EnemyFrameTable<CeresBabyPose> ShownFrames { get; } = new()
    {
        { CeresBabyPose.Horizontal, EnemyFrameId.CeresBabyHorizontal },
        { CeresBabyPose.Round, EnemyFrameId.CeresBabyRound },
        { CeresBabyPose.Vertical, EnemyFrameId.CeresBabyVertical },
    };
}
