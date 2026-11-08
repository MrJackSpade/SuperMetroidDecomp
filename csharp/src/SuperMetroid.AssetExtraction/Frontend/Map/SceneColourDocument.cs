using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed scene colours (#1544): the solid colour the title and each menu screen shows where nothing is drawn, and
/// the Ceres haze's two tints, as a player edits them in place.
/// </summary>
public sealed record SceneColourDocument
{
    /// <summary>Schema revision; loading requires <see cref="SceneColourFiles.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>The title's backdrop.</summary>
    public required PaletteRgb5 TitleBackdrop { get; init; }

    /// <summary>The pause screen's backdrop.</summary>
    public required PaletteRgb5 PauseBackdrop { get; init; }

    /// <summary>The backdrop of file select, its room maps, the options screens and the game-over screen.</summary>
    public required PaletteRgb5 FileSelectBackdrop { get; init; }

    /// <summary>The world map's backdrop while each Zebes area is selected, keyed by area name.</summary>
    public required Dictionary<string, PaletteRgb5> WorldMapBackdrops { get; init; }

    /// <summary>The Ceres haze's tint while Ceres Ridley lives.</summary>
    public required PaletteRgb5 CeresHazeWhileRidleyLives { get; init; }

    /// <summary>The Ceres haze's tint once Ceres Ridley is dead.</summary>
    public required PaletteRgb5 CeresHazeAfterRidleyDies { get; init; }
}
