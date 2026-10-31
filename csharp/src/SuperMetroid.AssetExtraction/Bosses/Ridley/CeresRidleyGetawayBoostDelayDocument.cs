using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>
/// The Speed Booster delays each of the getaway's wing frames leaves behind for the unauthored cadence (#1338, #1479).
/// </summary>
public sealed record CeresRidleyGetawayBoostDelayDocument
{
    /// <summary>Schema revision required to equal <see cref="CeresRidleyGetawayBoostDelayFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Ridley's wings' two frames, each ten delays by running frame.</summary>
    public required int[][] WingFrames { get; init; }
}
