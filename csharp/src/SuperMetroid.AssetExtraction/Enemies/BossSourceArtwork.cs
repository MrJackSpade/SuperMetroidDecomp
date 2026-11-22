using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Bosses.Crocomire;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Botwoon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The bosses' installed artwork as the import pipeline holds it (#1463, #1466, #1583): beside Kraid's eye colours, the one
/// part the game binds, the characters the bosses' pre-coloured pictures are composed from (Crocomire's skeleton, Torizo's
/// artwork swaps, Mother Brain's escape text and special pages), the frames their body sheets are composed from (Kraid's body and head tilemaps, Phantoon's,
/// Draygon's, Crocomire's and Mother Brain's body-layer frames, Crocomire's melts), and the identity of the whole selected
/// boss presentation.
/// </summary>
public sealed partial record BossSourceArtwork
{
    /// <summary>Five editable Crocomire transfer images, which the import pre-colours Crocomire's sheets in (#1512); fight and death phases stay compiled.</summary>
    public required CrocomireColorCatalog CrocomireColors { get; init; }

    /// <summary>
    /// Kraid's installed colours, which the import pre-colours his sheets and room in (#1512), and from which the game's
    /// eye colours are read (<see cref="KraidColorCatalog.Eyes"/>). Null only for constructed fixtures.
    /// </summary>
    public required KraidColorCatalog KraidColors { get; init; }

    /// <summary>Editable Mother Brain death-fade colours, which the import pre-colours her sheets in (#1512).</summary>
    public required MotherBrainDeathColorCatalog MotherBrainDeathColors { get; init; }

    /// <summary>Editable initial and fade-to-black colours of the Mother Brain cutscene Baby, which the import pre-colours its sheet in (#1512).</summary>
    public required BabyMetroidCutsceneColorCatalog BabyMetroidCutsceneColors { get; init; }

    /// <summary>Editable live-Baby Metroid normal-cycle and target colours, which the import pre-colours its sheet in (#1512).</summary>
    public required BabyMetroidColorCatalog BabyMetroidColors { get; init; }

    /// <summary>Editable Phantoon health, fade-out and Wrecked Ship power-on colours, which the import pre-colours Phantoon's sheets and room in (#1512).</summary>
    public required PhantoonColorCatalog PhantoonColors { get; init; }

    /// <summary>Editable Draygon opening, normal, flash and health-band colours, which the import pre-colours Draygon's sheets in (#1512).</summary>
    public required DraygonColorCatalog DraygonColors { get; init; }

    /// <summary>Editable Tourian Zebetite two-colour pulse, which the import pre-colours the Zebetites' sheets in (#1512).</summary>
    public required ZebetiteColorCatalog ZebetiteColors { get; init; }

    /// <summary>Editable Spore Spawn spore, health and death-sequence colours, which the import pre-colours Spore Spawn's sheets in (#1512).</summary>
    public required SporeSpawnColorCatalog SporeSpawnColors { get; init; }

    /// <summary>Editable Botwoon colours for all eight health bands, which the import pre-colours Botwoon's sheets in (#1512).</summary>
    public required BotwoonColorCatalog BotwoonColors { get; init; }

    /// <summary>Editable Norfair Ridley initial and arena-reveal colours, which the import pre-colours Ridley's sheets and room in (#1512).</summary>
    public required NorfairRidleyColorCatalog NorfairRidleyColors { get; init; }

    /// <summary>Both of Crocomire's melts; optional only for constructed fixtures.</summary>
    public required CrocomireMeltingArtwork CrocomireMelting { get; init; }

    /// <summary>Editable BG2 half of Crocomire's mixed fight-body frames.</summary>
    public required CrocomireBg2FrameCatalog CrocomireBg2Frames { get; init; }

    /// <summary>Editable Phantoon BG2 tilemap frames; collision remains engine-owned.</summary>
    public required PhantoonBg2FrameCatalog PhantoonBg2Frames { get; init; }

    /// <summary>Editable Draygon BG2 tilemap frames; collision remains engine-owned.</summary>
    public required DraygonBg2FrameCatalog DraygonBg2Frames { get; init; }

    /// <summary>Kraid's ordered BG2 tile references; null only for constructed fixtures.</summary>
    public required KraidBackgroundArtwork KraidBackground { get; init; }

    /// <summary>Editable BG2 body poses paired with the installed Mother Brain OAM limbs.</summary>
    public required MotherBrainBodyBg2FrameCatalog MotherBrainBodyBg2Frames { get; init; }

    /// <summary>Six editable skeleton character uploads; death timing and VRAM positions stay fixed.</summary>
    public required CrocomireSkeletonArtwork CrocomireSkeleton { get; init; }

    /// <summary>Editable tile pages for Bomb and Golden Torizo's instruction-time VRAM uploads.</summary>
    public required TorizoSwapArtwork TorizoSwaps { get; init; }

    /// <summary>Editable source tile sheet for Mother Brain's corpse-rotting staging.</summary>
    public required RoomCharacterAtlas MotherBrainCorpse { get; init; }

    /// <summary>Editable OBJ characters for the Mother Brain escape typewriter transfer.</summary>
    public required RoomCharacterAtlas MotherBrainEscapeText { get; init; }

    /// <summary>Editable leg, Baby, attack-restoration and exploded-door OBJ pages.</summary>
    public required MotherBrainSpecialSpriteArtworkCatalog MotherBrainSpecialSprites { get; init; }
}
