namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Only the appearance of a native eight-pixel OBJ; no simulation fields.</summary>
public sealed partial record GrappleSpriteStyle
{
    /// <summary>Zero-based column 0..15 in the gameplay OBJ tile layout, not a position in the grapple atlas or on screen.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based row 0..31 in the gameplay OBJ tile layout; row * 16 + column selects the complete nine-bit character number.</summary>
    public required int TileRow { get; init; }
    /// <summary>OBJ palette selector 0..7 packed into bits 9..11; stock endpoint and rope styles use 5.</summary>
    public required int Palette { get; init; }
    /// <summary>Where the piece draws among the room's layers; the stock endpoint and rope draw in front of it.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(SuperMetroid.Core.Assets.ActorDepthJsonConverter))]
    public required SuperMetroid.Core.Assets.ActorDepth Depth { get; init; }
    /// <summary>Authored horizontal reflection in OBJ bit 14; rope drawing can additionally set this bit from its angle.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Authored vertical reflection in OBJ bit 15; rope drawing can additionally set this bit from its angle.</summary>
    public required bool FlipY { get; init; }
}
