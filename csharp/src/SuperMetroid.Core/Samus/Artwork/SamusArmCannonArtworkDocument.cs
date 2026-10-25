using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Versioned <c>samus-arm-cannon.json</c> placement, cover-appearance, and tile-selection tables; pixels are supplied separately by <c>samus-arm-cannon-tiles.png</c>, and opening/closing mechanics remain runtime-owned.</summary>
public sealed record SamusArmCannonArtworkDocument
{
    /// <summary>Placement schema revision; loading currently requires version 3.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 253 offsets in pose order, each where the pose's descriptor begins within <see cref="DrawingData"/>; loading requires room for a descriptor.</summary>
    public required int[] PoseDescriptors { get; init; }
    /// <summary>Exactly 608 byte values for $90:C9D9–$CC38: direction and drawing-mode bytes, optional alternate direction/mode, then signed X/Y pixel-offset pairs indexed by body animation frame.</summary>
    public required int[] DrawingData { get; init; }
    /// <summary>Ten cover appearances in native direction order, each one small piece at the origin: character, palette, priority and mirroring.</summary>
    public required SpriteVisualPart[] CoverAppearances { get; init; }
    /// <summary>Ten direction rows of the installed cover tile, 0 through 11, each of the three opening frames shows; the closed cover shows none.</summary>
    public required int[][] CoverTiles { get; init; }
}
