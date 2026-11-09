using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Bosses.Crocomire;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Kraid;
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
public sealed record BossSourceArtwork
{
    /// <summary>Five editable Crocomire transfer images, which the import pre-colours Crocomire's sheets in (#1512); fight and death phases stay compiled.</summary>
    public CrocomireColorCatalog? CrocomireColors { get; init; }

    /// <summary>
    /// Kraid's installed colours, which the import pre-colours his sheets and room in (#1512); setting them also sets the
    /// eye colours gameplay reads back (<see cref="KraidEyes"/>). Null only for constructed fixtures.
    /// </summary>
    public KraidColorCatalog? KraidColors
    {
        get;
        init
        {
            field = value;
            KraidEyes = value?.Eyes();
        }
    }

    /// <summary>Kraid's installed eye colours, which the game binds for his eye glow (#1512); null only for constructed fixtures.</summary>
    public KraidEyeColours? KraidEyes { get; private init; }

    /// <summary>Editable Mother Brain death-fade colours, which the import pre-colours her sheets in (#1512).</summary>
    public MotherBrainDeathColorCatalog? MotherBrainDeathColors { get; init; }

    /// <summary>Editable initial and fade-to-black colours of the Mother Brain cutscene Baby, which the import pre-colours its sheet in (#1512).</summary>
    public BabyMetroidCutsceneColorCatalog? BabyMetroidCutsceneColors { get; init; }

    /// <summary>Editable live-Shitroid normal-cycle and target colours, which the import pre-colours its sheet in (#1512).</summary>
    public ShitroidColorCatalog? ShitroidColors { get; init; }

    /// <summary>Editable Phantoon health, fade-out and Wrecked Ship power-on colours, which the import pre-colours Phantoon's sheets and room in (#1512).</summary>
    public PhantoonColorCatalog? PhantoonColors { get; init; }

    /// <summary>Editable Draygon opening, normal, flash and health-band colours, which the import pre-colours Draygon's sheets in (#1512).</summary>
    public DraygonColorCatalog? DraygonColors { get; init; }

    /// <summary>Editable Tourian Zebetite two-colour pulse, which the import pre-colours the Zebetites' sheets in (#1512).</summary>
    public ZebetiteColorCatalog? ZebetiteColors { get; init; }

    /// <summary>Editable Spore Spawn spore, health and death-sequence colours, which the import pre-colours Spore Spawn's sheets in (#1512).</summary>
    public SporeSpawnColorCatalog? SporeSpawnColors { get; init; }

    /// <summary>Editable Botwoon colours for all eight health bands, which the import pre-colours Botwoon's sheets in (#1512).</summary>
    public BotwoonColorCatalog? BotwoonColors { get; init; }

    /// <summary>Editable Norfair Ridley initial and arena-reveal colours, which the import pre-colours Ridley's sheets and room in (#1512).</summary>
    public NorfairRidleyColorCatalog? NorfairRidleyColors { get; init; }

    /// <summary>Both of Crocomire's melts; optional only for constructed fixtures.</summary>
    public CrocomireMeltingArtwork? CrocomireMelting { get; init; }

    /// <summary>Editable BG2 half of Crocomire's mixed fight-body frames.</summary>
    public CrocomireBg2FrameCatalog? CrocomireBg2Frames { get; init; }

    /// <summary>Editable Phantoon BG2 tilemap frames; collision remains engine-owned.</summary>
    public PhantoonBg2FrameCatalog? PhantoonBg2Frames { get; init; }

    /// <summary>Editable Draygon BG2 tilemap frames; collision remains engine-owned.</summary>
    public DraygonBg2FrameCatalog? DraygonBg2Frames { get; init; }

    /// <summary>Kraid's ordered BG2 tile references; null only for constructed fixtures.</summary>
    public KraidBackgroundArtwork? KraidBackground { get; init; }

    /// <summary>Editable BG2 body poses paired with the installed Mother Brain OAM limbs.</summary>
    public MotherBrainBodyBg2FrameCatalog? MotherBrainBodyBg2Frames { get; init; }

    /// <summary>Six editable skeleton character uploads; death timing and VRAM positions stay fixed.</summary>
    public CrocomireSkeletonArtwork? CrocomireSkeleton { get; init; }

    /// <summary>Editable tile pages for Bomb and Golden Torizo's instruction-time VRAM uploads.</summary>
    public TorizoSwapArtwork? TorizoSwaps { get; init; }

    /// <summary>Editable source tile sheet for Mother Brain's corpse-rotting staging.</summary>
    public RoomCharacterAtlas? MotherBrainCorpse { get; init; }

    /// <summary>Editable OBJ characters for the Mother Brain escape typewriter transfer.</summary>
    public RoomCharacterAtlas? MotherBrainEscapeText { get; init; }

    /// <summary>Editable leg, Baby, attack-restoration and exploded-door OBJ pages.</summary>
    public MotherBrainSpecialSpriteArtworkCatalog? MotherBrainSpecialSprites { get; init; }

    /// <summary>The selected boss presentation, computed from the installed content rather than kept with the game state.</summary>
    /// <remarks>Hashes each boss artwork entry's identity, in a fixed order; absent entries hash as absent.</remarks>
    public string ContentIdentity =>
        SelectedPresentationHash.Create("boss-bundle-v1", content =>
        {
            content.AppendIdentity("CrocomireMelting", CrocomireMelting?.ContentIdentity);
            content.AppendIdentity("CrocomireSkeleton", CrocomireSkeleton?.ContentIdentity);
            content.AppendIdentity("CrocomireBg2Frames", CrocomireBg2Frames?.ContentIdentity);
            content.AppendIdentity("CrocomireColors", CrocomireColors?.ContentIdentity);
            content.AppendIdentity("TorizoInstructionVram", TorizoSwaps?.ContentIdentity);
            content.AppendIdentity("PhantoonBg2Frames", PhantoonBg2Frames?.ContentIdentity);
            content.AppendIdentity("PhantoonColors", PhantoonColors?.ContentIdentity);
            content.AppendIdentity("DraygonBg2Frames", DraygonBg2Frames?.ContentIdentity);
            content.AppendIdentity("DraygonColors", DraygonColors?.ContentIdentity);
            content.AppendIdentity("KraidBackground", KraidBackground?.ContentIdentity);
            content.AppendIdentity("KraidColors", KraidColors?.ContentIdentity);
            content.AppendIdentity("MotherBrainBodyBg2Frames", MotherBrainBodyBg2Frames?.ContentIdentity);
            content.AppendIdentity("MotherBrainCorpse", MotherBrainCorpse?.ContentIdentity);
            content.AppendIdentity("MotherBrainEscapeText", MotherBrainEscapeText?.ContentIdentity);
            content.AppendIdentity("MotherBrainSpecialSprites", MotherBrainSpecialSprites?.ContentIdentity);
            content.AppendIdentity("MotherBrainDeathColors", MotherBrainDeathColors?.ContentIdentity);
            content.AppendIdentity("BabyMetroidCutsceneColors", BabyMetroidCutsceneColors?.ContentIdentity);
            content.AppendIdentity("ShitroidColors", ShitroidColors?.ContentIdentity);
            content.AppendIdentity("ZebetiteColors", ZebetiteColors?.ContentIdentity);
            content.AppendIdentity("SporeSpawnColors", SporeSpawnColors?.ContentIdentity);
            content.AppendIdentity("BotwoonColors", BotwoonColors?.ContentIdentity);
            content.AppendIdentity("NorfairRidleyColors", NorfairRidleyColors?.ContentIdentity);
        });

    /// <summary>Returns installed boss artwork, which must carry the Torizo artwork swaps its sheets compose from.</summary>
    /// <exception cref="InvalidDataException">The Torizo artwork swaps are absent.</exception>
    public static BossSourceArtwork FromInstalledArtwork(BossSourceArtwork artwork)
    {
        Ensure.NotNull(artwork);
        if (artwork.TorizoSwaps is null)
            throw new InvalidDataException("Installed boss artwork requires the Torizo transfer provider.");
        return artwork;
    }
}
