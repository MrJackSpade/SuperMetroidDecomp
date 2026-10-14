using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>The opening cinematic's character atlases, tilemaps, sprite presentations and palette (#633).</summary>
public sealed record IntroCinematicArtworkParts
{
    /// <summary>Palette-indexed BG characters for the VRAM byte-$0000 upload.</summary>
    public required RoomCharacterAtlas BackgroundCharacters { get; init; }

    /// <summary>Fixed-bank intro OBJ characters for the VRAM byte-$C000 upload.</summary>
    public required RoomCharacterAtlas IntroObjectCharacters { get; init; }

    /// <summary>Cinematic OBJ characters for the VRAM byte-$DC00 upload.</summary>
    public required RoomCharacterAtlas CinematicObjectCharacters { get; init; }

    /// <summary>Exactly four ordered 32x32 tilemap pages, concatenated for the VRAM byte-$A000 upload.</summary>
    public required IReadOnlyList<RoomBackgroundTilemapAtlas> BackgroundPages { get; init; }

    /// <summary>One 32x32 Samus-head portrait page for VRAM byte $9000.</summary>
    public required RoomBackgroundTilemapAtlas PortraitTilemap { get; init; }

    /// <summary>One 32x32 pre-typewriter narration page for VRAM byte $9800.</summary>
    public required RoomBackgroundTilemapAtlas InitialNarrationTilemap { get; init; }

    /// <summary>Four BG3 rows referencing staged Japanese subtitle glyphs.</summary>
    public required IntroFinalLineTilemap FinalLine { get; init; }

    /// <summary>Samus portrait eye rectangles, independent of blink timing.</summary>
    public required IntroEyeTilemapPresentation EyeFrames { get; init; }

    /// <summary>Visible narration caret OAM artwork, independent of positioning and blink timing.</summary>
    public required IntroCaretSpritePresentation CaretSprites { get; init; }

    /// <summary>Intro Mother Brain actor OAM frames.</summary>
    public required IntroMotherBrainSpritePresentation MotherBrainSprites { get; init; }

    /// <summary>Fourth-hit Mother Brain explosion OAM frames.</summary>
    public required IntroMotherBrainExplosionSpritePresentation MotherBrainExplosionSprites { get; init; }

    /// <summary>Rinka actor frames for the Mother Brain scene.</summary>
    public required IntroRinkaSpritePresentation RinkaSprites { get; init; }

    /// <summary>SR388 egg-shell and slime visual frames.</summary>
    public required IntroEggEffectSpritePresentation EggEffectSprites { get; init; }

    /// <summary>SR388 egg and confused-baby actor frames.</summary>
    public required IntroDiscoveryActorSpritePresentation DiscoveryActorSprites { get; init; }

    /// <summary>Baby-Metroid frames for the delivery and examination scenes.</summary>
    public required IntroScientistSpritePresentation ScientistSprites { get; init; }

    /// <summary>Native-precision colors loaded before the opening narration.</summary>
    public required IntroCinematicPalette Palette { get; init; }

    /// <summary>Mode-7 and OBJ artwork used after the narration fades to Ceres.</summary>
    public required CeresFlightArtworkCatalog CeresFlight { get; init; }

    /// <summary>Ceres destruction artwork and subsequent Zebes reveal artwork.</summary>
    public required CeresDestructionArtworkCatalog CeresDestruction { get; init; }
}
