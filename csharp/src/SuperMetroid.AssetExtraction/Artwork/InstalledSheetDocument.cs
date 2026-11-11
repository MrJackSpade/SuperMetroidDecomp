using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The installed manifest of a sprite sheet (#1544, #1633, #1635): the size of the image the loader assembles from the
/// picture store, the shades the image is pre-coloured in, each naming the pixels it recolours by colour key, its scene's
/// backdrop as a colour key, each frame's rectangle, origin, depth, blending and where the store holds its picture, and the
/// frames that show nothing. Every colour is an RGB colour written as <c>#rrggbb</c>.
/// </summary>
public sealed record InstalledSheetDocument
{
    /// <summary>The manifest's schema revision, <see cref="SpriteSheetFile.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>
    /// The width and height of the sheet's image (#1635), which the loader assembles from the picture store: clear outside
    /// its frames, each frame's rectangle holding the store's copy of its picture.
    /// </summary>
    public required Extent16<ImagePixels> Size { get; init; }

    /// <summary>
    /// The colour key of what the image's scene shows where nothing is drawn: a room tileset's backdrop; absent for every
    /// other sheet.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InstalledColourKeyDocument? Backdrop { get; init; }

    /// <summary>The shades the sheet was pre-coloured in besides its own colours, by <see cref="PictureShade.Key"/> (#1512); absent for none.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, InstalledSheetShadeDocument>? Shades { get; init; }

    /// <summary>The frames by name.</summary>
    public required Dictionary<string, InstalledSheetFrameDocument> Frames { get; init; }

    /// <summary>
    /// The names of the frames the sheet's family draws that show nothing (#1513): each holds no pixel, so the sheet holds
    /// no picture for it and its draw draws nothing; absent for none. No name is also a frame of <see cref="Frames"/>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Blank { get; init; }
}
