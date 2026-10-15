namespace SuperMetroid.Core.Enemies.Stoke.Artwork;

/// <summary>
/// The presentation boundary of the Stoke's projectile programs (#641): the installed artwork key of each
/// <see cref="StokeProjectileFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class StokeProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 2;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(StokeProjectileFrame frame) => frame.Pose switch
    {
        0 => 0xdb0e,
        1 => 0xdb12,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Stoke projectile pose."),
    };
}
