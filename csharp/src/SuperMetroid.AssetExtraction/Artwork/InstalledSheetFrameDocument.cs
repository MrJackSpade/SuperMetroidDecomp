using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One frame in an installed sprite sheet manifest (#1544, #1633, #1635, #1644).</summary>
public sealed record InstalledSheetFrameDocument
{
    /// <summary>The frame rectangle's top-left pixel on the sheet.</summary>
    public required Position16<ImagePixels> Corner { get; init; }

    /// <summary>The frame rectangle's width and height.</summary>
    public required Extent16<ImagePixels> Size { get; init; }

    /// <summary>The frame's origin, from the rectangle's top-left pixel.</summary>
    public required Offset16<ImagePixels> Origin { get; init; }

    /// <summary>
    /// Where a gameplay draw of an actor's frame sits among the room's layers (#1514), by name, as the import decides it from
    /// the game's own drawing of the frame; absent for a frame that is not an actor's, and for an empty frame.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ActorDepth>))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ActorDepth? Depth { get; init; }

    /// <summary>
    /// Whether every pixel of the frame blends with what lies behind it when drawn as a sprite (#1633), as
    /// <see cref="SpriteSheetFrame.Blends"/> keeps it; absent for a frame that does not, and for every frame of a sheet not
    /// drawn as sprites.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Blends { get; init; }

    /// <summary>
    /// Where the installation's picture store holds the frame's picture (#1635): a rectangle on one of its pages, the box of
    /// what the frame shows, which the loader copies to the place in the frame's rectangle the reference names, the rest of
    /// the rectangle staying clear (#1646), and the maps the frame shows it with (#1644). Frames showing the same pixels, on
    /// this sheet or any other, name one rectangle, each with its own place and maps. Absent for a frame whose rectangle shows
    /// nothing at all, which stays clear.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InstalledPictureReferenceDocument? Picture { get; init; }
}
