namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Cartridge values and helpers of <see cref="CeresBabyFrameDefinitions"/> that only verification reads.</summary>
internal static class CeresBabyFrameDefinitionsConstants
{
    /// <summary>The spritemap <paramref name="pose"/> shows.</summary>
    internal static ushort Spritemap(CeresBabyPose pose) => pose switch
    {
        CeresBabyPose.Horizontal => CeresBabyFrameDefinitions.HorizontalFrame,
        CeresBabyPose.Round => CeresBabyFrameDefinitions.RoundFrame,
        CeresBabyPose.Vertical => CeresBabyFrameDefinitions.VerticalFrame,
        _ => throw new ArgumentOutOfRangeException(nameof(pose), pose, "Undefined Ceres Baby pose."),
    };
}
