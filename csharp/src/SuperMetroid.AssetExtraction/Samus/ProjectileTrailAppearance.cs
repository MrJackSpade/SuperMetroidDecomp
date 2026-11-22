namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>One small eight-by-eight OBJ's selected character and attributes, using the full gameplay projectile tile layout rather than coordinates in the compact trail PNG.</summary>
public sealed partial record ProjectileTrailAppearance
{
    /// <summary>Zero-based gameplay OBJ tile column 0..15; with the row this produces character number row * 16 + column.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based gameplay OBJ tile row 0..31, spanning all 512 character numbers including the tile-page bit.</summary>
    public required int TileRow { get; init; }
    /// <summary>OBJ palette selector 0..7 encoded in attribute bits 9..11; stock ice uses 6 and wave/missile trails use 5.</summary>
    public required int Palette { get; init; }
    /// <summary>Where the trail draws among the room's layers; the stock trails draw between its back and front tiles.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(SuperMetroid.Core.Assets.ActorDepthJsonConverter))]
    public required SuperMetroid.Core.Assets.ActorDepth Depth { get; init; }
    /// <summary>Whether the selected eight-pixel tile is reflected horizontally via OBJ attribute bit 14.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether the selected eight-pixel tile is reflected vertically via OBJ attribute bit 15.</summary>
    public required bool FlipY { get; init; }
}
