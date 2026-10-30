using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The opening cinematic as the import pipeline reads it (#633, #1463): the scene artwork the game binds, with the
/// tilemaps, character atlases and sprite compositions its sheets are composed from (#1467).
/// </summary>
public sealed record IntroCinematicArtworkParts : IntroCinematicSceneParts
{
    /// <summary>Exactly four ordered 32x32 tilemap pages of the illustrated scenes.</summary>
    public required IReadOnlyList<RoomBackgroundTilemapAtlas> BackgroundPages { get; init; }

    /// <summary>One 32x32 Samus-head portrait page.</summary>
    public required RoomBackgroundTilemapAtlas PortraitTilemap { get; init; }

    /// <summary>One 32x32 pre-typewriter narration page.</summary>
    public required RoomBackgroundTilemapAtlas InitialNarrationTilemap { get; init; }

    /// <summary>Four rows referencing the Japanese subtitle glyphs.</summary>
    public required IntroFinalLineTilemap FinalLine { get; init; }

    /// <summary>Samus portrait eye rectangles, independent of blink timing.</summary>
    public required IntroEyeTilemapPresentation EyeFrames { get; init; }

    /// <summary>The Zebes reveal's tilemap and characters, which the planet's picture is composed from.</summary>
    public required ZebesRevealArtwork ZebesReveal { get; init; }

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
