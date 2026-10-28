using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The opening cinematic as the import pipeline reads it (#633, #1463): the scene artwork the game binds, with the
/// character atlases and sprite compositions its sheets are composed from.
/// </summary>
public sealed record IntroCinematicArtworkParts : IntroCinematicSceneParts
{
    /// <summary>Palette-indexed BG characters of the illustrated scenes.</summary>
    public required RoomCharacterAtlas BackgroundCharacters { get; init; }

    /// <summary>The intro's fixed-bank OBJ characters.</summary>
    public required RoomCharacterAtlas IntroObjectCharacters { get; init; }

    /// <summary>The intro's cinematic OBJ characters.</summary>
    public required RoomCharacterAtlas CinematicObjectCharacters { get; init; }

    /// <summary>Visible narration caret compositions, independent of positioning and blink timing.</summary>
    public required IntroCaretSpritePresentation CaretSprites { get; init; }

    /// <summary>Intro Mother Brain actor compositions.</summary>
    public required IntroMotherBrainSpritePresentation MotherBrainSprites { get; init; }

    /// <summary>Fourth-hit Mother Brain explosion compositions.</summary>
    public required IntroMotherBrainExplosionSpritePresentation MotherBrainExplosionSprites { get; init; }

    /// <summary>Rinka actor compositions for the Mother Brain scene.</summary>
    public required IntroRinkaSpritePresentation RinkaSprites { get; init; }

    /// <summary>SR388 egg-shell and slime compositions.</summary>
    public required IntroEggEffectSpritePresentation EggEffectSprites { get; init; }

    /// <summary>SR388 egg and confused-baby actor compositions.</summary>
    public required IntroDiscoveryActorSpritePresentation DiscoveryActorSprites { get; init; }

    /// <summary>Baby-Metroid compositions for the delivery and examination scenes.</summary>
    public required IntroScientistSpritePresentation ScientistSprites { get; init; }

    /// <summary>Station-blast and Zebes-reveal compositions of the Ceres destruction scene.</summary>
    public required CeresDestructionSpritePresentation CeresDestructionSprites { get; init; }
}
