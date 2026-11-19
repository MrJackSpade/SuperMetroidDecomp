namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed scene colours (#1544): the solid colour the title and each menu screen shows where nothing is drawn, and
/// the Ceres haze's two tints, as a player edits them in place, each written as <c>#rrggbb</c> (#2564).
/// </summary>
public sealed record SceneColourDocument
{
    /// <summary>Schema revision; loading requires <see cref="SceneColourFiles.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>The title's backdrop.</summary>
    public required string TitleBackdrop { get; init; }

    /// <summary>The pause screen's backdrop.</summary>
    public required string PauseBackdrop { get; init; }

    /// <summary>The backdrop file select, its room maps, the options screens and the game-over screen share.</summary>
    public required string MenusBackdrop { get; init; }

    /// <summary>The world map's backdrop while each Zebes area is selected, keyed by area name.</summary>
    public required Dictionary<string, string> WorldMapBackdrops { get; init; }

    /// <summary>The Ceres haze's tint while Ceres Ridley lives.</summary>
    public required string CeresHazeWhileRidleyLives { get; init; }

    /// <summary>The Ceres haze's tint once Ceres Ridley is dead.</summary>
    public required string CeresHazeAfterRidleyDies { get; init; }
}
