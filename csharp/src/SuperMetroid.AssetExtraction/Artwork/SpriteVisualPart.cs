using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One ordered region in an indexed tile sheet. Null palette inherits its drawing owner's current palette.</summary>
[SpatialComponents("External format: sprite composition JSON stores each OAM part displacement as offsetX/offsetY.")]
public sealed partial record SpriteVisualPart
{
    /// <summary>Signed horizontal pixel displacement from the composition origin.</summary>
    public required int OffsetX { get; init; }
    /// <summary>Signed vertical pixel displacement from the composition origin.</summary>
    public required int OffsetY { get; init; }
    /// <summary>Zero-based tile column in the presentation's indexed artwork atlas.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based tile row in the presentation's indexed artwork atlas.</summary>
    public required int TileRow { get; init; }
    /// <summary>Square OBJ size in pixels, either 8 or 16.</summary>
    [IntegerQuantity("sprite edge length in pixels, eight or sixteen")]
    public required int Size { get; init; }
    /// <summary>Where the part draws among the room's layers.</summary>
    [JsonConverter(typeof(ActorDepthJsonConverter))]
    public required ActorDepth Depth { get; init; }
    /// <summary>OBJ palette selector, or <see langword="null"/> to inherit the drawing owner's palette.</summary>
    public required int? Palette { get; init; }
    /// <summary>Horizontal reflection applied to the selected artwork region.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Vertical reflection applied to the selected artwork region.</summary>
    public required bool FlipY { get; init; }

    /// <summary>The document form of <paramref name="part"/> on a sheet <paramref name="tileColumns"/> characters wide.</summary>
    /// <param name="part">The piece.</param>
    /// <param name="tileColumns">Characters across the sheet.</param>
    /// <param name="inheritPalette">Whether the document records the palette as inherited instead of the piece's row.</param>
    public static SpriteVisualPart FromSpritePart(SpritePart part, int tileColumns, bool inheritPalette = false) => new()
    {
        OffsetX = part.Offset.X,
        OffsetY = part.Offset.Y,
        TileColumn = part.Character % tileColumns,
        TileRow = part.Character / tileColumns,
        Size = part.Large ? 16 : 8,
        Depth = part.Depth,
        Palette = inheritPalette ? null : part.Palette,
        FlipX = part.FlipsHorizontally,
        FlipY = part.FlipsVertically,
    };
}
