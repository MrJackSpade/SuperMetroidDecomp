using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's turning floor (#1404, #1467, #1515): the installed picture of the shot the ending's phase shows.
/// </summary>
internal static class EndingScreenFloor
{
    /// <summary>The floor <paramref name="shot"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installed floor sheet has no picture of the shot.</exception>
    internal static AffineFloor Of(InstalledSpriteSheets sheets, EndingFloorShot shot) =>
        FloorShots.Of(sheets, EndingFloorDefinitions.Sheets[shot], EndingFloorDefinitions.FrameName);
}
