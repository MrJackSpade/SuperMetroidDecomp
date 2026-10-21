namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Cartridge values and helpers of <see cref="CeresBabyFrameDefinitions"/> that only verification reads.</summary>
internal static class CeresBabyFrameDefinitionsConstants
{
    /// <summary>The spritemap <paramref name="pose"/> shows.</summary>
    internal static ushort Spritemap(CeresBabyPose pose) => pose switch
    {
        CeresBabyPose.Horizontal => CeresBabyFrameDefinitionsConstants.HorizontalFrame,
        CeresBabyPose.Round => CeresBabyFrameDefinitionsConstants.RoundFrame,
        CeresBabyPose.Vertical => CeresBabyFrameDefinitionsConstants.VerticalFrame,
        _ => throw new ArgumentOutOfRangeException(nameof(pose), pose, "Undefined Ceres Baby pose."),
    };
    /// <summary>Bank $A6 owns the Baby's OAM frames.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>The horizontal-squish Baby OAM frame at $A6:BFFD.</summary>
    internal const ushort HorizontalFrame = 0xbffd;
    /// <summary>The round Baby OAM frame at $A6:C018.</summary>
    internal const ushort RoundFrame = 0xc018;
    /// <summary>The vertical-squish Baby OAM frame at $A6:C033.</summary>
    internal const ushort VerticalFrame = 0xc033;
}
