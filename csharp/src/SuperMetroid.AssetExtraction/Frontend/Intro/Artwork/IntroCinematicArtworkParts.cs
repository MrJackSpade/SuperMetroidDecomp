using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The opening cinematic's installed documents (#633, #1463, #1636): the scene artwork the game binds, with the sprite
/// compositions its sheets are composed from (#1467) and named by. Its characters and layouts are import data the
/// installation never holds (<see cref="IntroCinematicCharacters"/>, <see cref="IntroCinematicLayouts"/>).
/// </summary>
[ArchitectureReview("context-bag", "The opening cinematic's decoded artwork documents the import composes its sheets from; every member is immutable artwork data, not a service.")]
public sealed record IntroCinematicArtworkParts : IntroCinematicSceneParts
{
    /// <summary>The destruction's Mode 7 maps, which the Ceres floor's shots are composed from.</summary>
    public required CeresDestructionMaps CeresDestructionMaps { get; init; }

    /// <summary>Visible narration caret compositions, independent of positioning and blink timing.</summary>
    public required IntroCaretSpritePresentation CaretSprites { get; init; }

    /// <summary>The Japanese text's next-page arrow compositions, independent of its position and cadence (#1655).</summary>
    public required IntroNextPageArrowSpritePresentation NextPageArrowSprites { get; init; }

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
