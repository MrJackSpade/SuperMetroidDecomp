namespace SuperMetroid.Core.Assets;

/// <summary>Versioned <c>samus-arm-cannon.json</c> placement, OBJ-attribute, and tile-selection tables; pixels are supplied separately by <c>samus-arm-cannon-tiles.png</c>, and opening/closing mechanics remain runtime-owned.</summary>
public sealed record SamusArmCannonArtworkDocument
{
    /// <summary>Placement schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 253 unsigned 16-bit bank-$90 descriptor pointers in pose order; loading requires room for a descriptor within $C9D9–$CC38.</summary>
    public required int[] PosePointers { get; init; }
    /// <summary>Exactly 608 byte values for $90:C9D9–$CC38: direction and drawing-mode bytes, optional alternate direction/mode, then signed X/Y pixel-offset pairs indexed by body animation frame.</summary>
    public required int[] DrawingData { get; init; }
    /// <summary>Ten unsigned 16-bit OAM attribute words in native direction order, retaining character index, OBJ palette, priority, and mirror bits.</summary>
    public required int[] SpriteAttributes { get; init; }
    /// <summary>Ten direction rows of four unsigned 16-bit bank-$9A source identities; each row begins with zero for the closed cover, followed by three entries from the installed twelve-tile source set.</summary>
    public required int[][] TileSources { get; init; }
}
