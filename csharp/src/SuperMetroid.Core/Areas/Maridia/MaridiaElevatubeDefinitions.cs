using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Areas.Maridia;

/// <summary>
/// The Maridia elevatube's ride (#1415): where it holds Samus, where each entry starts her, how fast she starts and
/// accelerates, and the fastest she may travel.
/// </summary>
internal static class MaridiaElevatubeDefinitions
{
    /// <summary>The room X, in whole pixels, the tube holds Samus at on every update.</summary>
    internal const int SamusCenterX = 128;

    /// <summary>A ride entered from Oasis to the south: from the tube's bottom, moving upward and gaining speed upward each update.</summary>
    private static readonly MaridiaElevatubeRideStart FromSouth = new(
        StartingY: 2496,
        StartingVelocity: SignedEightEight.FromPixels(-1),
        Acceleration: SignedEightEight.FromPixelFraction(-1, 8));

    /// <summary>A ride entered from Plasma Spark to the north: from the tube's top, moving downward and gaining speed downward each update.</summary>
    private static readonly MaridiaElevatubeRideStart FromNorth = new(
        StartingY: 64,
        StartingVelocity: SignedEightEight.FromPixels(1),
        Acceleration: SignedEightEight.FromPixelFraction(1, 8));

    /// <summary>How the ride starts from each end of the tube, which the entry door names (#2485).</summary>
    internal static IReadOnlyDictionary<ElevatubeEntry, MaridiaElevatubeRideStart> RideStarts { get; } =
        new Dictionary<ElevatubeEntry, MaridiaElevatubeRideStart>
        {
            [ElevatubeEntry.FromSouth] = FromSouth,
            [ElevatubeEntry.FromNorth] = FromNorth,
        };

    /// <summary>
    /// The fastest the tube carries Samus either way: an acceleration that
    /// would take her past it is not applied, and her speed stays as it was.
    /// </summary>
    internal static readonly SignedEightEight MaximumSpeed = SignedEightEight.FromPixelFraction(113, 8);

    /// <summary>The block the room spawns the otherwise invisible elevatube room object at: column one of the top row.</summary>
    internal static readonly Position16<RoomBlocks> RoomObjectBlock = new(1, 0);
}
