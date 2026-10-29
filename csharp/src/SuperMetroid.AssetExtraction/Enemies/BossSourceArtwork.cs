using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Bosses.Crocomire;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The bosses' installed artwork as the import pipeline holds it (#1463, #1466): beside what gameplay reads, the characters
/// the bosses' pre-coloured pictures are composed from (Crocomire's skeleton, Torizo's artwork swaps, Mother Brain's escape
/// text and special pages), the frames their body sheets are composed from (Kraid's body and head tilemaps, Phantoon's,
/// Draygon's, Crocomire's and Mother Brain's body-layer frames, Crocomire's melts), and the identity of the whole selected
/// boss presentation.
/// </summary>
public sealed record BossSourceArtwork : BossArtworkCatalog
{
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

    /// <summary>Editable OBJ characters for the Mother Brain escape typewriter transfer.</summary>
    public RoomCharacterAtlas? MotherBrainEscapeText { get; init; }

    /// <summary>Editable leg, Baby, attack-restoration and exploded-door OBJ pages.</summary>
    public MotherBrainSpecialSpriteArtworkCatalog? MotherBrainSpecialSprites { get; init; }

    /// <inheritdoc/>
    /// <remarks>Hashes each boss artwork entry's identity, in a fixed order; absent entries hash as absent.</remarks>
    public override string ContentIdentity =>
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
