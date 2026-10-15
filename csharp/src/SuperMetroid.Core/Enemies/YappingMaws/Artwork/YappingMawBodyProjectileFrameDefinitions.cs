namespace SuperMetroid.Core.Enemies.YappingMaws.Artwork;

/// <summary>
/// The presentation boundary of the Yapping Maw's body links programs (#641): the installed artwork key of each
/// <see cref="YappingMawBodyProjectileFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class YappingMawBodyProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 2;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(YappingMawBodyProjectileFrame frame) => frame.Pose switch
    {
        0 => 0xec58,
        1 => 0xec5e,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Yapping Maw body link pose."),
    };
}
