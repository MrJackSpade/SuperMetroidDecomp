namespace SuperMetroid.Core.Assets;

/// <summary>The installed manifest of a sprite sheet: each frame's rectangle on the sheet image, origin and blending.</summary>
public sealed record SpriteSheetDocument
{
    /// <summary>The manifest's schema revision, <see cref="SpriteSheet.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Which half of CGRAM the sheet's palette indices and colours name.</summary>
    public required SpriteImageColours Palette { get; init; }

    /// <summary>The 128 colours of that half the sheet was coloured with, as CGRAM words.</summary>
    public required ushort[] Colours { get; init; }

    /// <summary>
    /// The gameplay background set each of a gameplay background sheet's colour groups draws in, group after group, which
    /// the import pipeline decides as it installs the sheet (#1464); absent for an actor's sheet and a frontend screen's.
    /// </summary>
    public BackgroundColourSet[]? BackgroundSets { get; init; }

    /// <summary>The frames by name.</summary>
    public required Dictionary<string, SpriteSheetFrameDocument> Frames { get; init; }

    /// <summary>The pre-coloured part images the frames' parts show, by name; they have no parts of their own.</summary>
    public required Dictionary<string, SpriteSheetFrameDocument> PartImages { get; init; }
}
