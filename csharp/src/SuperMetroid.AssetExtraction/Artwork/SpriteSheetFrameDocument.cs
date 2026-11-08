using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One frame in a sprite sheet manifest as the import composes it; the installed manifest keeps it as an <see cref="InstalledSheetFrameDocument"/> (#1544).</summary>
public sealed record SpriteSheetFrameDocument
{
    /// <summary>The frame rectangle's top-left pixel on the sheet.</summary>
    public required Position16<ImagePixels> Corner { get; init; }

    /// <summary>The frame rectangle's width and height.</summary>
    public required Extent16<ImagePixels> Size { get; init; }

    /// <summary>The frame's origin, from the rectangle's top-left pixel.</summary>
    public required Offset16<ImagePixels> Origin { get; init; }

    /// <summary>The colour groups the frame was composed from, as the composed manifest stores them: a bit per group's place.</summary>
    public required int ColourGroups { get; init; }

    /// <summary>
    /// Where a gameplay draw of an actor's frame sits among the room's layers (#1514), by name, as the import decides it from
    /// the game's own drawing of the frame; absent for a frame that is not an actor's, and for an empty frame.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ActorDepth>))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ActorDepth? Depth { get; init; }
}
