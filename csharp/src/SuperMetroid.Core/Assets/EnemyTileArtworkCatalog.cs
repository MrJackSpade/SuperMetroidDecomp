using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Installed, palette-indexed ordinary enemy characters. Definition pointers select art;
/// enemy health, hitboxes, AI, and native VRAM destinations remain engine-owned.
/// </summary>
public sealed partial class EnemyTileArtworkCatalog
{
    /// <summary>
    /// Complete selected enemy presentation, including legacy-override merges, display
    /// bindings, DMA aliases, special uploads and colors. Gameplay definitions are excluded.
    /// This is computed rather than persisted in an exact debugger snapshot.
    /// </summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-bundle-v1", content =>
        {
            foreach ((ushort definition, RoomCharacterAtlas sheet) in sheets.OrderBy(pair => pair.Key))
            {
                content.Append("definition", definition);
                content.Append("tiles", sheet.Transfer.Span);
                content.AppendIdentity("palette", palettes[definition].ContentIdentity);
            }
            foreach (var pair in byDmaSource.OrderBy(pair => pair.Key.Source).ThenBy(pair => pair.Key.ByteCount))
            {
                content.Append("dma-source", pair.Key.Source);
                content.Append("dma-length", pair.Key.ByteCount);
                content.Append("dma-tiles", pair.Value.Transfer.Span);
            }
            content.AppendIdentity("CrocomireMelting", CrocomireMelting?.ContentIdentity);
            content.AppendIdentity("Spritemaps", Spritemaps?.ContentIdentity);
            content.AppendIdentity("ExtendedFrames", ExtendedFrames?.ContentIdentity);
            content.AppendIdentity("KraidBackground", KraidBackground?.ContentIdentity);
            content.AppendIdentity("KraidColors", KraidColors?.ContentIdentity);
            content.AppendIdentity("GunshipLiftoff", GunshipLiftoff?.ContentIdentity);
            content.AppendIdentity("CeresDoorVisual", CeresDoorVisual?.ContentIdentity);
            content.AppendIdentity("ProjectileSpritemaps", ProjectileSpritemaps?.ContentIdentity);
            content.AppendIdentity("MagdollitePaletteCycle", MagdollitePaletteCycle?.ContentIdentity);
            content.AppendIdentity("WorkRobotPaletteCycle", WorkRobotPaletteCycle?.ContentIdentity);
            content.AppendIdentity("CrocomireColors", CrocomireColors?.ContentIdentity);
            content.AppendIdentity("DraygonColors", DraygonColors?.ContentIdentity);
            content.AppendIdentity("PhantoonColors", PhantoonColors?.ContentIdentity);
            content.AppendIdentity("ChozoAndTubeColors", ChozoAndTubeColors?.ContentIdentity);
            content.AppendIdentity("SporeSpawnColors", SporeSpawnColors?.ContentIdentity);
            content.AppendIdentity("DachoraColors", DachoraColors?.ContentIdentity);
            content.AppendIdentity("ShitroidColors", ShitroidColors?.ContentIdentity);
            content.AppendIdentity("BabyMetroidCutsceneColors", BabyMetroidCutsceneColors?.ContentIdentity);
            content.AppendIdentity("BotwoonColors", BotwoonColors?.ContentIdentity);
            content.AppendIdentity("MotherBrainDeathColors", MotherBrainDeathColors?.ContentIdentity);
            content.AppendIdentity("ZebetiteColors", ZebetiteColors?.ContentIdentity);
            content.AppendIdentity("NorfairRidleyColors", NorfairRidleyColors?.ContentIdentity);
            content.AppendIdentity("TourianStatueColors", TourianStatueColors?.ContentIdentity);
            content.AppendIdentity("PhantoonBg2Frames", PhantoonBg2Frames?.ContentIdentity);
            content.AppendIdentity("DraygonBg2Frames", DraygonBg2Frames?.ContentIdentity);
            content.AppendIdentity("MotherBrainCorpse", MotherBrainCorpse?.ContentIdentity);
            content.AppendIdentity("MotherBrainEscapeText", MotherBrainEscapeText?.ContentIdentity);
            content.AppendIdentity("MotherBrainSpecialSprites", MotherBrainSpecialSprites?.ContentIdentity);
            content.AppendIdentity("CrocomireSkeleton", CrocomireSkeleton?.ContentIdentity);
            content.AppendIdentity("CrocomireBg2Frames", CrocomireBg2Frames?.ContentIdentity);
            content.AppendIdentity("MotherBrainBodyBg2Frames", MotherBrainBodyBg2Frames?.ContentIdentity);
            content.AppendIdentity("TorizoInstructionVram", TorizoInstructionVram?.ContentIdentity);
            content.AppendIdentity("CeresEscapeTiles", CeresEscapeTiles?.ContentIdentity);
            content.AppendIdentity("CeresEscapeOverlayTilemaps", CeresEscapeOverlayTilemaps?.ContentIdentity);
            content.AppendIdentity("AuxiliaryColors", AuxiliaryColors?.ContentIdentity);
        });

    private readonly Dictionary<ushort, RoomCharacterAtlas> sheets;
    private readonly Dictionary<ushort, EnemyPaletteSheet> palettes;
    private readonly Dictionary<(int Source, int ByteCount), RoomCharacterAtlas> byDmaSource;


    /// <summary>Optional only for constructed fixtures; installed retail catalogs include both melts.</summary>
    public CrocomireMeltingArtwork? CrocomireMelting { get; }

    /// <summary>Six editable skeleton character uploads; death timing and VRAM positions stay fixed.</summary>
    public CrocomireSkeletonArtwork? CrocomireSkeleton { get; }

    /// <summary>Editable tile pages for Bomb/Golden Torizo instruction-time VRAM uploads.</summary>
    public TorizoInstructionVramArtwork? TorizoInstructionVram { get; }

    /// <summary>Editable face-block, corpse-sidehopper, and Golden Torizo health colors.</summary>
    public EnemyAuxiliaryColorCatalog? AuxiliaryColors { get; }

    /// <summary>Editable warning-text and door character pages for Ceres escape.</summary>
    public CeresEscapeTileArtwork? CeresEscapeTiles { get; }

    /// <summary>Editable English/Japanese visual tile words for the Ceres warning overlay.</summary>
    public CeresEscapeOverlayTilemapCatalog? CeresEscapeOverlayTilemaps { get; }

    /// <summary>Installed visual-only OAM frames; null for constructed legacy fixtures.</summary>
    public EnemySpritemapCatalog? Spritemaps { get; }

    /// <summary>Installed extended visual frames; hitbox and AI data stay engine-owned.</summary>
    public EnemyExtendedFrameCatalog? ExtendedFrames { get; }

    /// <summary>Editable Phantoon BG2 tilemap frames; collision remains engine-owned.</summary>
    public PhantoonBg2FrameCatalog? PhantoonBg2Frames { get; }

    /// <summary>Editable Draygon BG2 tilemap frames; collision remains engine-owned.</summary>
    public DraygonBg2FrameCatalog? DraygonBg2Frames { get; }

    /// <summary>Editable BG2 half of Crocomire's mixed fight-body frames.</summary>
    public CrocomireBg2FrameCatalog? CrocomireBg2Frames { get; }

    /// <summary>Editable BG2 body poses paired with the installed Mother Brain OAM limbs.</summary>
    public MotherBrainBodyBg2FrameCatalog? MotherBrainBodyBg2Frames { get; }

    /// <summary>Editable source tile sheet for Mother Brain's corpse-rotting WRAM staging.</summary>
    public RoomCharacterAtlas? MotherBrainCorpse { get; }

    /// <summary>Editable OBJ characters for the Mother Brain escape typewriter transfer.</summary>
    public RoomCharacterAtlas? MotherBrainEscapeText { get; }

    /// <summary>Editable leg, Baby, attack-restoration and exploded-door OBJ pages.</summary>
    public MotherBrainSpecialSpriteArtworkCatalog? MotherBrainSpecialSprites { get; }

    /// <summary>Kraid's ordered BG2 tile references; null only for constructed fixtures.</summary>
    public KraidBackgroundArtwork? KraidBackground { get; }

    /// <summary>Installed Kraid RGB5 artwork; null only for constructed fixtures.</summary>
    public KraidColorCatalog? KraidColors { get; }

    /// <summary>Five editable gunship takeoff character uploads; null for constructed fixtures.</summary>
    public GunshipLiftoffArtworkCatalog? GunshipLiftoff { get; }

    /// <summary>Ceres-door actor's special tile transfer and RGB5 rows.</summary>
    public CeresDoorVisualCatalog? CeresDoorVisual { get; }

    /// <summary>Installed bank-$8D projectile compositions; null for constructed fixtures.</summary>
    public EnemyProjectileSpritemapCatalog? ProjectileSpritemaps { get; }

    /// <summary>Four editable color frames; the Magdollite draw hook retains timing.</summary>
    public MagdollitePaletteCycle? MagdollitePaletteCycle { get; }

    /// <summary>Six editable Work Robot color records; timer/terminator stay compiled.</summary>
    public WorkRobotPaletteCycle? WorkRobotPaletteCycle { get; }

    /// <summary>Five editable Crocomire RGB5 transfer images; fight/death phases stay compiled.</summary>
    public CrocomireColorCatalog? CrocomireColors { get; }

    /// <summary>Editable Draygon opening, normal, flash and health-band colors.</summary>
    public DraygonColorCatalog? DraygonColors { get; }

    /// <summary>Editable Phantoon health, fade-out and Wrecked Ship power-on colors.</summary>
    public PhantoonColorCatalog? PhantoonColors { get; }

    /// <summary>Editable Chozo statue and n00b-tube crack sprite-palette pairs.</summary>
    public ChozoAndTubeColorCatalog? ChozoAndTubeColors { get; }

    /// <summary>Editable Spore Spawn spore, health and death-sequence colors.</summary>
    public SporeSpawnColorCatalog? SporeSpawnColors { get; }

    /// <summary>Editable Dachora default, speed and shine sprite colors.</summary>
    public DachoraColorCatalog? DachoraColors { get; }

    /// <summary>Editable live-Shitroid normal-cycle and target sprite colors.</summary>
    public ShitroidColorCatalog? ShitroidColors { get; }

    /// <summary>Editable initial and fade-to-black colors for the Mother Brain cutscene Baby.</summary>
    public BabyMetroidCutsceneColorCatalog? BabyMetroidCutsceneColors { get; }

    /// <summary>Editable Botwoon sprite colors for all eight health bands.</summary>
    public BotwoonColorCatalog? BotwoonColors { get; }

    /// <summary>Editable Mother Brain death-fade and exploded-door colors.</summary>
    public MotherBrainDeathColorCatalog? MotherBrainDeathColors { get; }

    /// <summary>Editable Tourian Zebetite two-color pulse; actor selection stays in code.</summary>
    public ZebetiteColorCatalog? ZebetiteColors { get; }

    /// <summary>Editable initial and arena-reveal colors; Ridley's fade cadence stays in code.</summary>
    public NorfairRidleyColorCatalog? NorfairRidleyColors { get; }

    /// <summary>Editable entrance, eye, and grey colors; statue unlocking stays compiled.</summary>
    public TourianStatueColorCatalog? TourianStatueColors { get; }

    /// <summary>Resolves the native room-entry enemy VRAM queue against the same indexed PNGs.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        if (byDmaSource.TryGetValue((sourceAddress, byteCount), out RoomCharacterAtlas? atlas))
        {
            data = atlas.Transfer;
            return true;
        }
        if (CeresEscapeTiles?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (CeresEscapeOverlayTilemaps?.TryResolve(sourceAddress, byteCount,
                out data) == true)
            return true;
        // The hand PLM queues its fragment sheet through NMI, unlike the enemy
        // instruction uploads that apply directly. Both use current installed art.
        if (TorizoInstructionVram?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        data = default;
        return false;
    }

    /// <summary>Uploads the complete sheet selected by a room graphics-set record.</summary>
    public void LoadTo(ushort definitionPointer, int byteCount, SnesVram vram, int destinationByteAddress)
    {
        if (!sheets.TryGetValue(definitionPointer, out RoomCharacterAtlas? atlas))
            throw new InvalidDataException($"Enemy ${definitionPointer:X4} has no installed tile sheet.");
        if (atlas.Transfer.Length != byteCount)
            throw new InvalidDataException(
                $"Enemy ${definitionPointer:X4} requires {byteCount} tile bytes, installed sheet has {atlas.Transfer.Length}.");
        atlas.LoadTo(vram, destinationByteAddress);
    }

    /// <summary>Loads the sixteen indexed colors selected by a room graphics-set record.</summary>
    public void LoadPaletteTo(ushort definitionPointer, SnesCgram cgram, int destinationColor)
    {
        if (!palettes.TryGetValue(definitionPointer, out EnemyPaletteSheet? palette))
            throw new InvalidDataException($"Enemy ${definitionPointer:X4} has no installed palette.");
        palette.LoadTo(cgram, destinationColor);
    }
}

/// <summary>Host-file geometry for native four-bit enemy tile DMA sheets.</summary>
public static class EnemyTileArtworkFormat
{
    public const string ManifestFileName = "enemy-tiles.json";
    public const int Version = 69;
    /// <summary>Stable, source-address-free name for a gunship takeoff character chunk.</summary>
    public static string GunshipLiftoffFileName(int index) =>
        $"gunship-liftoff-{index + 1}-tiles.png";
    /// <summary>All distinct ordinary graphics-set definitions in the pinned retail room states.</summary>
    public const int RetailDefinitionCount = 122;
    /// <summary>Bank-$A0 enemy-header word zero: bit 15 selects staging; bits 0..14 are tile byte length.</summary>
    public const int TileByteCountMask = 0x7fff;
    /// <summary>
    /// SHA-256 of the 122 sorted four-digit definition IDs joined with commas, independently
    /// enumerated from every bank-$B4 enemy graphics set referenced by retail room states.
    /// This makes a missing or substituted sheet fail during installation validation.
    /// </summary>
    public const string RetailDefinitionIdsSha256 =
        "8B665DEC36A4AA649CDF2327E4B7F60197D42B84534CD354347F4581D1442DD1";

    public static string FileName(ushort definitionPointer) => $"enemy-{definitionPointer:X4}-tiles.png";
    public static string PaletteFileName(ushort definitionPointer) => $"enemy-{definitionPointer:X4}-colors.json";
}
