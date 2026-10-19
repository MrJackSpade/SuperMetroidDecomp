namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The presentation boundary of Kraid's head programs (#641): the installed head image of each
/// <see cref="KraidHeadFrame"/> pose, keyed by the bank-$A7 address of the native pose's tilemap, as the head artwork
/// files are.
/// </summary>
internal static class KraidHeadFrameDefinitions
{
    /// <summary>The installed head image key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(KraidHeadFrame frame) => frame.MouthStage switch
    {
        0 => 0x97c8,
        1 => 0x9ac8,
        2 => 0x9dc8,
        KraidHeadFrame.FullyOpenMouthStage => 0xa0c8,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Kraid head pose."),
    };

    /// <summary>Every head image key, from the closed mouth to the fully open one.</summary>
    internal static IEnumerable<ushort> PresentationKeys => KraidHeadFrame.ByMouthStage.Select(PresentationKey);
}
