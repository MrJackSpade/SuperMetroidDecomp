namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// The presentation boundary of the Tourian entrance statue's programs (#641): the bank-$AA spritemap each
/// <see cref="TourianEntranceStatueFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class TourianEntranceStatueFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xaa;

    /// <summary>The poses.</summary>
    internal const int PoseCount = 0;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(TourianEntranceStatueFrame frame) => frame.Pose switch
    {

        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Tourian entrance statue pose."),
    };
}
