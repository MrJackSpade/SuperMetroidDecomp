using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>The installed manifest of a sprite sheet: each frame's rectangle on the sheet image, origin and blending.</summary>
public sealed record SpriteSheetDocument
{
    /// <summary>The manifest's schema revision, <see cref="SpriteSheetFile.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Which half of the colour table the sheet's palette indices and colours name.</summary>
    public required SpriteImageColours Palette { get; init; }

    /// <summary>The 128 colours of that half the sheet was coloured with, as packed colour words.</summary>
    public required ushort[] Colours { get; init; }

    /// <summary>
    /// The palette bits a sprite sheet's draws OR into each ink's own row (#1512): an ink is its part's own row and colour,
    /// coloured from that row with these bits ORed in; zero for a sheet whose parts are drawn in their own rows.
    /// </summary>
    public int PaletteBits { get; init; }

    /// <summary>
    /// The gameplay background set each of a gameplay background sheet's colour groups draws in, group after group, which
    /// the import pipeline decides as it installs the sheet (#1464); absent for an actor's sheet and a frontend screen's.
    /// </summary>
    public InstalledBackgroundSet[]? BackgroundSets { get; init; }

    /// <summary>The shades the sheet was pre-coloured in besides its standard colours, by <see cref="PictureShade.Key"/> (#1512); absent for none.</summary>
    public Dictionary<string, SpriteSheetShadeDocument>? Shades { get; init; }

    /// <summary>The frames by name.</summary>
    public required Dictionary<string, SpriteSheetFrameDocument> Frames { get; init; }

    /// <summary>
    /// The names of the frames the sheet's family draws that show nothing (#1513): each inks no pixel, so the sheet holds
    /// no picture for it and its draw draws nothing; absent for none. No name is also a frame of <see cref="Frames"/>.
    /// </summary>
    public string[]? Blank { get; init; }
}
