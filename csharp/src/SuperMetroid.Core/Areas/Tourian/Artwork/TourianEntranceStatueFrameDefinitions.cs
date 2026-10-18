using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// The presentation boundary of the Tourian entrance statue's programs (#641): the installed frame each
/// <see cref="TourianEntranceStatueFrame"/> pose shows. The statue's programs show none.
/// </summary>
internal static class TourianEntranceStatueFrameDefinitions
{
    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(TourianEntranceStatueFrame frame) =>
        throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Tourian entrance statue pose.");
}
