namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Which half of the cartridge's colour table a sheet the import composes names its colours from (#1392): the import's
/// knowledge while it composes. The installed image is only its colours instead (#1633); see
/// <see cref="InstalledSheetConversion"/>.
/// </summary>
public enum SpriteImageColours
{
    /// <summary>The eight sprite colour sets, colours 128 to 255: actors' sheets.</summary>
    Sprite,

    /// <summary>The eight background colour sets, colours 0 to 127: room block sheets.</summary>
    Background,

    /// <summary>
    /// The HUD layer's eight colour sets of four, colours 0 to 31: HUD and room effect sheets. Every pixel keeps its set
    /// times four plus its colour, colour zero included, so a layer that shows colour zero shows its set's (#1392); the
    /// installed image keeps such a pixel as a clear pixel with a colour of its own, its RGB under alpha zero (#1633).
    /// </summary>
    TwoBppBackground,

    /// <summary>
    /// All 256 colours, as a turning-floor tile names its colours directly: the Ceres rooms' turning-floor tiles, some of whose
    /// pixels name the sprite colour sets every room load restores (#1392).
    /// </summary>
    FullPalette,
}
