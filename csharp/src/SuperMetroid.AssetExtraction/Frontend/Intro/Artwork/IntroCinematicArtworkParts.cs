using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The opening cinematic's installed documents (#633, #1463, #1636): the scene artwork the game binds, with the
/// destruction's maps the Ceres floor is composed from (#1467). Its characters, layouts and sprite compositions are import
/// data the installation never holds (<see cref="IntroCinematicCharacters"/>, <see cref="IntroCinematicLayouts"/>,
/// <see cref="IntroCinematicSprites"/>, #1638).
/// </summary>
public sealed record IntroCinematicArtworkParts : IntroCinematicSceneParts
{
    /// <summary>The destruction's Mode 7 maps, which the Ceres floor's shots are composed from.</summary>
    public required CeresDestructionMaps CeresDestructionMaps { get; init; }
}
