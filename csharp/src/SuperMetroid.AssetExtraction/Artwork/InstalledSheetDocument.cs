using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The installed manifest of a sprite sheet (#1544): the image's own colours, which its palette map's pixels name by place,
/// the shades it is pre-coloured in, each frame's rectangle, origin and depth, and the frames that show nothing.
/// </summary>
public sealed record InstalledSheetDocument
{
    /// <summary>The manifest's schema revision, <see cref="SpriteSheetFile.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>The image's own colours, as packed colour words, each named by its place in the list.</summary>
    public required ushort[] Colours { get; init; }

    /// <summary>
    /// The place among <see cref="Colours"/> of the colour the image's scene shows where nothing is drawn: a room tileset's
    /// backdrop; absent for every other sheet.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? Backdrop { get; init; }

    /// <summary>
    /// The shades the sheet was pre-coloured in besides its standard colours, by <see cref="PictureShade.Key"/> (#1512), each
    /// naming the image's colours it recolours by place; absent for none.
    /// </summary>
    public Dictionary<string, SpriteSheetShadeDocument>? Shades { get; init; }

    /// <summary>The frames by name.</summary>
    public required Dictionary<string, InstalledSheetFrameDocument> Frames { get; init; }

    /// <summary>
    /// The names of the frames the sheet's family draws that show nothing (#1513): each inks no pixel, so the sheet holds
    /// no picture for it and its draw draws nothing; absent for none. No name is also a frame of <see cref="Frames"/>.
    /// </summary>
    public string[]? Blank { get; init; }

    /// <summary>Where among the cartridge's colours the import coloured each of the image's colours from.</summary>
    public required InstalledSheetSource Source { get; init; }
}
