using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>Where the getaway's floor runs take their tile numbers from, as the cartridge's floor update lists name them (#1415).</summary>
public sealed record CeresRidleyFloorUpdateDocument
{
    /// <summary>Schema revision required to equal <see cref="CeresRidleyFloorUpdateFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>The Baby capsule's three frames, each two run sources, 0 through 65535.</summary>
    public required int[][] CapsuleFrames { get; init; }
    /// <summary>Ridley's wings' two frames, each six run sources, 0 through 65535.</summary>
    public required int[][] WingFrames { get; init; }
}
