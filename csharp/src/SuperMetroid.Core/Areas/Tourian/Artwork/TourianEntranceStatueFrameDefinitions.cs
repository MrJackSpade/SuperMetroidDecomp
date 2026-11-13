using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// The presentation boundary of the Tourian entrance statue's animations (#641): the installed frame each
/// <see cref="TourianEntranceStatueFrame"/> pose shows. The statue's animations show none.
/// </summary>
internal static class TourianEntranceStatueFrameDefinitions
{
    /// <summary>The installed frame each <see cref="TourianEntranceStatueFrame"/> shows: none.</summary>
    internal static EnemyFrameTable<TourianEntranceStatueFrame> ShownFrames { get; } = new();
}
