using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The opening cinematic's characters, from the component's import data (#1636): the illustrated scenes', the intro's and
/// the cinematic's OBJ characters, the Zebes reveal's characters and the Ceres approach's floor and OBJ characters. The
/// import composes the opening's, the Ceres scenes' and the ending flyaway's pictures from them; the installation never
/// holds them.
/// </summary>
/// <param name="BackgroundCharacters">Palette-indexed BG characters of the illustrated scenes.</param>
/// <param name="IntroObjectCharacters">The intro's fixed-bank OBJ characters.</param>
/// <param name="CinematicObjectCharacters">The intro's cinematic OBJ characters.</param>
/// <param name="ZebesRevealCharacters">The Zebes reveal's characters, $4000 bytes.</param>
/// <param name="CeresFlight">The Ceres approach's floor and OBJ characters, bound to its installed documents.</param>
public sealed record IntroCinematicCharacters(RoomCharacterAtlas BackgroundCharacters, RoomCharacterAtlas IntroObjectCharacters,
    RoomCharacterAtlas CinematicObjectCharacters, RoomCharacterAtlas ZebesRevealCharacters, CeresFlightCharacters CeresFlight);
