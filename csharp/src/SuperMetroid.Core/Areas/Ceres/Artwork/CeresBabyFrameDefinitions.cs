using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>The presentation boundary of the Ceres Baby's poses (#641): the bank-$A6 spritemap each pose shows.</summary>
internal static class CeresBabyFrameDefinitions
{
    /// <summary>Bank $A6 owns the Baby's OAM frames.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>The horizontal-squish Baby OAM frame at $A6:BFFD.</summary>
    internal const ushort HorizontalFrame = 0xbffd;

    /// <summary>The round Baby OAM frame at $A6:C018.</summary>
    internal const ushort RoundFrame = 0xc018;

    /// <summary>The vertical-squish Baby OAM frame at $A6:C033.</summary>
    internal const ushort VerticalFrame = 0xc033;

    /// <summary>The installed frame each <see cref="CeresBabyPose"/> shows.</summary>
    internal static EnemyFrameTable<CeresBabyPose> ShownFrames { get; } = new()
    {
        { CeresBabyPose.Horizontal, EnemyFrameId.CeresBabyHorizontal },
        { CeresBabyPose.Round, EnemyFrameId.CeresBabyRound },
        { CeresBabyPose.Vertical, EnemyFrameId.CeresBabyVertical },
    };
}
