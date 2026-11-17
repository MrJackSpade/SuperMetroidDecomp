using static SuperMetroid.Core.Frontend.Ending.EndingShootingStarDefinitions;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Names and draws the ending shooting star's sheet frames (<see cref="EndingShootingStarDefinitions"/>).
/// </summary>
internal static class EndingShootingStarFrames
{
    /// <summary>The sheet frame showing <paramref name="look"/> (#1392).</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="look"/> is the hidden lead-in, which shows no picture.</exception>
    internal static string SheetFrameName(EndingShootingStarLook look) => Ensure.Listed(SheetFrameNames, look);

    /// <summary>The pictures a visible star shows, in the order its flight shows them.</summary>
    internal static IReadOnlyList<EndingShootingStarLook> Visible { get; } =
        [EndingShootingStarLook.FaintSpeck, EndingShootingStarLook.BrightSpeck, EndingShootingStarLook.DimStar, EndingShootingStarLook.FullStar];
}
