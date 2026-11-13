using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The opening cinematic's installed documents (#633, #1463, #1636): its backdrop and Ceres artwork, of which the game binds
/// the backdrops and placements (#1616). Its tilemaps and Mode 7 maps are the import's (#1467, #1693), and so are its
/// colours (#1544), its characters, layouts and sprite compositions (<see cref="IntroCinematicCharacters"/>,
/// <see cref="IntroCinematicLayouts"/>, <see cref="IntroCinematicSprites"/>, #1638).
/// </summary>
public sealed record IntroCinematicSceneParts
{
    /// <summary>The colour the opening shows where nothing is drawn, at full brightness.</summary>
    public required RgbColor Backdrop { get; init; }

    /// <summary>The backdrop and placements used after the narration fades to Ceres.</summary>
    public required CeresFlightSourceArtwork CeresFlight { get; init; }

    /// <summary>The destruction and Zebes-reveal actors' placements.</summary>
    public required CeresDestructionArtwork CeresDestruction { get; init; }
}
