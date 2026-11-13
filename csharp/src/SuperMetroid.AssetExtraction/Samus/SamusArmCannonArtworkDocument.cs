using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The extracted arm-cannon document, import data as <c>samus-arm-cannon-artwork.json</c> (#1638): placement,
/// cover-appearance, and tile-selection tables; pixels are supplied separately by <c>samus-arm-cannon-tiles.png</c>, and
/// opening/closing mechanics remain runtime-owned. The installation holds only its layout (<see cref="SamusArmCannonLayoutDocument"/>).
/// </summary>
public sealed record SamusArmCannonArtworkDocument
{
    /// <summary>Document schema revision; loading requires <see cref="SamusArmCannonArtworkFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>
    /// Where each pose with a record's descriptor begins within <see cref="DrawingData"/>, keyed by the pose's name, for
    /// exactly those poses; loading requires room for a descriptor.
    /// </summary>
    public required Dictionary<SamusPoseId, int> PoseDescriptors { get; init; }
    /// <summary>Exactly 608 byte values for $90:C9D9–$CC38: direction and drawing-mode bytes, optional alternate direction/mode, then signed X/Y pixel-offset pairs indexed by body animation frame.</summary>
    public required int[] DrawingData { get; init; }
    /// <summary>Ten cover appearances in native direction order, each one small piece at the origin: character, palette, priority and mirroring.</summary>
    public required SpriteVisualPart[] CoverAppearances { get; init; }
    /// <summary>Ten direction rows of the installed cover tile, 0 through 11, each of the three opening frames shows; the closed cover shows none.</summary>
    public required int[][] CoverTiles { get; init; }
}
