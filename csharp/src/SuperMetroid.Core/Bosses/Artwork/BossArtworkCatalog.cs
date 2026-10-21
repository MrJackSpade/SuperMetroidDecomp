using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>
/// The bosses' installed artwork beside the shared enemy sheets (#1395): melts, skeletons, BG2 frames, Mother Brain's
/// special pages and each boss's colors. Each boss reads only its own entries.
/// </summary>
[ArchitectureReview("context-bag", "A record of installed boss artwork catalogs; its members are data, not services.")]
public sealed record BossArtworkCatalog
{
    /// <summary>Both of Crocomire's melts; optional only for constructed fixtures.</summary>
    public CrocomireMeltingArtwork? CrocomireMelting { get; init; }

    /// <summary>Six editable skeleton character uploads; death timing and VRAM positions stay fixed.</summary>
    public CrocomireSkeletonArtwork? CrocomireSkeleton { get; init; }

    /// <summary>Editable BG2 half of Crocomire's mixed fight-body frames.</summary>
    public CrocomireBg2FrameCatalog? CrocomireBg2Frames { get; init; }

    /// <summary>Five editable Crocomire RGB5 transfer images; fight and death phases stay compiled.</summary>
    public CrocomireColorCatalog? CrocomireColors { get; init; }

    /// <summary>Editable tile pages for Bomb and Golden Torizo's instruction-time VRAM uploads.</summary>
    public TorizoInstructionVramArtwork? TorizoInstructionVram { get; init; }

    /// <summary>Editable Phantoon BG2 tilemap frames; collision remains engine-owned.</summary>
    public PhantoonBg2FrameCatalog? PhantoonBg2Frames { get; init; }

    /// <summary>Editable Phantoon health, fade-out and Wrecked Ship power-on colors.</summary>
    public PhantoonColorCatalog? PhantoonColors { get; init; }

    /// <summary>Editable Draygon BG2 tilemap frames; collision remains engine-owned.</summary>
    public DraygonBg2FrameCatalog? DraygonBg2Frames { get; init; }

    /// <summary>Editable Draygon opening, normal, flash and health-band colors.</summary>
    public DraygonColorCatalog? DraygonColors { get; init; }

    /// <summary>Kraid's ordered BG2 tile references; null only for constructed fixtures.</summary>
    public KraidBackgroundArtwork? KraidBackground { get; init; }

    /// <summary>Installed Kraid RGB5 artwork; null only for constructed fixtures.</summary>
    public KraidColorCatalog? KraidColors { get; init; }

    /// <summary>Editable BG2 body poses paired with the installed Mother Brain OAM limbs.</summary>
    public MotherBrainBodyBg2FrameCatalog? MotherBrainBodyBg2Frames { get; init; }

    /// <summary>Editable source tile sheet for Mother Brain's corpse-rotting WRAM staging.</summary>
    public RoomCharacterAtlas? MotherBrainCorpse { get; init; }

    /// <summary>Editable OBJ characters for the Mother Brain escape typewriter transfer.</summary>
    public RoomCharacterAtlas? MotherBrainEscapeText { get; init; }

    /// <summary>Editable leg, Baby, attack-restoration and exploded-door OBJ pages.</summary>
    public MotherBrainSpecialSpriteArtworkCatalog? MotherBrainSpecialSprites { get; init; }

    /// <summary>Editable Mother Brain death-fade and exploded-door colors.</summary>
    public MotherBrainDeathColorCatalog? MotherBrainDeathColors { get; init; }

    /// <summary>Editable initial and fade-to-black colors for the Mother Brain cutscene Baby.</summary>
    public BabyMetroidCutsceneColorCatalog? BabyMetroidCutsceneColors { get; init; }

    /// <summary>Editable live-Shitroid normal-cycle and target sprite colors.</summary>
    public ShitroidColorCatalog? ShitroidColors { get; init; }

    /// <summary>Editable Tourian Zebetite two-color pulse; actor selection stays in code.</summary>
    public ZebetiteColorCatalog? ZebetiteColors { get; init; }

    /// <summary>Editable Spore Spawn spore, health and death-sequence colors.</summary>
    public SporeSpawnColorCatalog? SporeSpawnColors { get; init; }

    /// <summary>Editable Botwoon sprite colors for all eight health bands.</summary>
    public BotwoonColorCatalog? BotwoonColors { get; init; }

    /// <summary>Editable initial and arena-reveal colors; Ridley's fade cadence stays in code.</summary>
    public NorfairRidleyColorCatalog? NorfairRidleyColors { get; init; }

    /// <summary>The selected boss presentation, computed rather than persisted in an exact debugger snapshot.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("boss-bundle-v1", content =>
    {
        content.AppendIdentity("CrocomireMelting", CrocomireMelting?.ContentIdentity);
        content.AppendIdentity("CrocomireSkeleton", CrocomireSkeleton?.ContentIdentity);
        content.AppendIdentity("CrocomireBg2Frames", CrocomireBg2Frames?.ContentIdentity);
        content.AppendIdentity("CrocomireColors", CrocomireColors?.ContentIdentity);
        content.AppendIdentity("TorizoInstructionVram", TorizoInstructionVram?.ContentIdentity);
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

    /// <summary>Returns installed boss artwork, which must carry the Torizo transfer provider the room-entry queue resolves.</summary>
    public static BossArtworkCatalog FromInstalledArtwork(BossArtworkCatalog artwork)
    {
        Ensure.NotNull(artwork);
        if (artwork.TorizoInstructionVram is null)
            throw new InvalidDataException("Installed boss artwork requires the Torizo transfer provider.");
        return artwork;
    }
}
