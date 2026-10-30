using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>The opening cinematic's palette and Ceres artwork the game binds (#633, #1463); its tilemaps are the import's (#1467).</summary>
public record IntroCinematicSceneParts
{
    /// <summary>Native-precision colors loaded before the opening narration.</summary>
    public required IntroCinematicPalette Palette { get; init; }

    /// <summary>Mode-7 artwork and placements used after the narration fades to Ceres.</summary>
    public required CeresFlightArtwork CeresFlight { get; init; }

    /// <summary>Ceres destruction maps and the destruction and Zebes-reveal actors' placements.</summary>
    public required CeresDestructionArtwork CeresDestruction { get; init; }
}
