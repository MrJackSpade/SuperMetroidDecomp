using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// The installed opening backdrops (#1544): the solid colour the opening narration and the Ceres scenes show where nothing
/// is drawn, as a player edits them in place.
/// </summary>
public sealed record IntroBackdropDocument
{
    /// <summary>Schema revision; loading requires <see cref="IntroBackdropFiles.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>The opening narration's backdrop.</summary>
    public required PaletteRgb5 Opening { get; init; }

    /// <summary>The backdrop of the Ceres flight and the Ceres destruction, once they load the flight's colours.</summary>
    public required PaletteRgb5 CeresFlight { get; init; }
}
