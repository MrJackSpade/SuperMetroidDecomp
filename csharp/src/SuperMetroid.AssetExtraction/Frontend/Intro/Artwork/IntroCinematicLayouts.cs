using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The opening cinematic's layouts, from the component's import data (#1638, #1693): the illustrated scene pages, the
/// portrait, the first narration card, the Japanese subtitle line, the portrait's eye rectangles, the Zebes reveal's map and
/// the Ceres destruction's Mode 7 maps. Each entry is one of the original's packed tile placements, so the installation
/// never holds them; the import composes the opening's scene, narration, Zebes reveal and Ceres floor pictures from them,
/// and the installation holds those pictures.
/// </summary>
/// <param name="BackgroundPages">Exactly four ordered 32x32 tilemap pages of the illustrated scenes.</param>
/// <param name="PortraitTilemap">One 32x32 Samus-head portrait page.</param>
/// <param name="InitialNarrationTilemap">One 32x32 pre-typewriter narration page.</param>
/// <param name="FinalLine">Four rows referencing the Japanese subtitle glyphs.</param>
/// <param name="EyeFrames">Samus portrait eye rectangles, independent of blink timing.</param>
/// <param name="ZebesRevealMap">The Zebes reveal's tilemap, which the planet's picture is composed from with the reveal's characters.</param>
/// <param name="CeresDestructionMaps">The destruction's Mode 7 maps, which the Ceres floor's shots are composed from.</param>
public sealed record IntroCinematicLayouts(IReadOnlyList<RoomBackgroundTilemapAtlas> BackgroundPages,
    RoomBackgroundTilemapAtlas PortraitTilemap, RoomBackgroundTilemapAtlas InitialNarrationTilemap, IntroFinalLineTilemap FinalLine,
    IntroEyeTilemapPresentation EyeFrames, RoomBackgroundTilemapAtlas ZebesRevealMap, CeresDestructionMaps CeresDestructionMaps);
