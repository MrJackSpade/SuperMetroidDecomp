using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One ordered region of an indexed tile sheet that names its own palette row (#2554): the stored piece of a document whose
/// pieces never take their draw's palette, as Samus's body, arm-cannon cover and atmospheric pieces do. Its document form is
/// a <see cref="SpriteVisualPart"/>'s, but its palette is always present, so a document piece with no row is rejected where
/// the document is read rather than where the piece is first drawn.
/// </summary>
[SpatialComponents("External format: sprite composition JSON stores each OAM part displacement as offsetX/offsetY.")]
public sealed partial record SpriteVisualPiece
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
    /// <summary>The OBJ palette row the piece is drawn in.</summary>
    public required int Palette { get; init; }
    /// <summary>Horizontal reflection applied to the selected artwork region.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Vertical reflection applied to the selected artwork region.</summary>
    public required bool FlipY { get; init; }

    /// <summary>
    /// The piece this document part describes on a sheet <paramref name="tileColumns"/> characters wide, in the palette row
    /// it names.
    /// </summary>
    /// <exception cref="InvalidDataException">The size is not eight or sixteen, or a field is outside its range.</exception>
    public SpritePart ToSpritePart(int tileColumns)
    {
        if (Size is not (8 or 16))
            throw new InvalidDataException($"A sprite part is {Size} pixels square; parts are 8 or 16.");
        try
        {
            return SpritePart.Create(Offset16<ScreenPixels>.Of(OffsetX, OffsetY), Size == 16, TileRow * tileColumns + TileColumn, Palette, Depth,
                (FlipX ? PictureFlip.Horizontal : PictureFlip.None) | (FlipY ? PictureFlip.Vertical : PictureFlip.None));
        }
        catch (ArgumentOutOfRangeException error)
        {
            throw new InvalidDataException("A sprite part has an offset, character, palette or depth outside its range.", error);
        }
    }

    /// <summary>The document form of <paramref name="part"/> on a sheet <paramref name="tileColumns"/> characters wide.</summary>
    /// <param name="part">The piece.</param>
    /// <param name="tileColumns">Characters across the sheet.</param>
    public static SpriteVisualPiece FromSpritePart(SpritePart part, int tileColumns) => new()
    {
        OffsetX = part.Offset.X,
        OffsetY = part.Offset.Y,
        TileColumn = part.Character % tileColumns,
        TileRow = part.Character / tileColumns,
        Size = part.Large ? 16 : 8,
        Depth = part.Depth,
        Palette = part.Palette,
        FlipX = part.FlipsHorizontally,
        FlipY = part.FlipsVertically,
    };
}
