using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>
/// The descriptors the getaway's floor updates queue, as the cartridge's floor update lists name them (#1415, #1467):
/// where each run takes its tile numbers from, how many and to which floor cell.
/// </summary>
public sealed record CeresRidleyFloorUpdateDocument
{
    /// <summary>Schema revision required to equal <see cref="CeresRidleyFloorUpdateFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>The Baby capsule's three frames, each two run descriptors.</summary>
    public required CeresRidleyFloorUpdateRunDocument[][] CapsuleFrames { get; init; }
    /// <summary>Ridley's wings' two frames, each six run descriptors.</summary>
    public required CeresRidleyFloorUpdateRunDocument[][] WingFrames { get; init; }
}
