using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The opening cinematic's backdrop and Ceres artwork the game binds (#633, #1463); its tilemaps are the import's (#1467), and
/// so are its colours (#1544).
/// </summary>
public record IntroCinematicSceneParts
{
    /// <summary>The colour the opening shows where nothing is drawn, at full brightness.</summary>
    public required RgbColor Backdrop { get; init; }

    /// <summary>Mode-7 artwork and placements used after the narration fades to Ceres.</summary>
    public required CeresFlightArtwork CeresFlight { get; init; }

    /// <summary>The destruction and Zebes-reveal actors' placements.</summary>
    public required CeresDestructionArtwork CeresDestruction { get; init; }
}
