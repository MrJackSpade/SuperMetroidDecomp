namespace SuperMetroid.Core.Enemies.Skree.Artwork;

/// <summary>
/// The presentation boundary of the Skree's and Metaree's death particles programs (#641): the installed artwork key of each
/// <see cref="SkreeMetareeParticleFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class SkreeMetareeParticleFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 2;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(SkreeMetareeParticleFrame frame) => frame.Pose switch
    {
        0 => 0x8abf,
        1 => 0x8ac7,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Skree/Metaree particle pose."),
    };
}
