namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Editable front/rear map slices from <c>Gunship_Ceres_Tilemap</c> at <c>$96:FE69</c>, preserving native transfer order.</summary>
/// <remarks>The 32-by-24 dimensions describe the host editing arrays, not a replacement for the hardware's full Mode-7 map geometry. Arrays are caller-owned mutable document data and are copied into byte transfers when loaded.</remarks>
[SpatialComponents("External format: the flight-maps JSON stores its declared grid dimensions as width/height integers.")]
public sealed record CeresFlightMapDocument
{
    /// <summary>Schema revision; loading requires <see cref="CeresFlightArtworkFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Host editing-array width in tile cells; must be 32 for each view.</summary>
    public required int Width { get; init; }
    /// <summary>Host editing-array height in tile cells; must be 24 for each view.</summary>
    public required int Height { get; init; }
    /// <summary>Front-view tile indices $00-$FF in ordered native upload-byte sequence; exactly 768 entries, preceding the rear slice.</summary>
    public required int[] FrontTiles { get; init; }
    /// <summary>Rear-view tile indices $00-$FF in ordered native upload-byte sequence; exactly 768 entries, selected after the initial camera approach.</summary>
    public required int[] RearTiles { get; init; }
}
