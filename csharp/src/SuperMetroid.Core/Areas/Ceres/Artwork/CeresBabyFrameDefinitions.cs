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

    /// <summary>The spritemap <paramref name="pose"/> shows.</summary>
    internal static ushort Spritemap(CeresBabyPose pose) => pose switch
    {
        CeresBabyPose.Horizontal => HorizontalFrame,
        CeresBabyPose.Round => RoundFrame,
        CeresBabyPose.Vertical => VerticalFrame,
        _ => throw new ArgumentOutOfRangeException(nameof(pose), pose, "Undefined Ceres Baby pose."),
    };
}
