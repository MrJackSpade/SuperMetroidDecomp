using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Installed, palette-indexed ordinary enemy characters. Definition pointers select art;
/// enemy health, hitboxes, AI, and native VRAM destinations remain engine-owned.
/// </summary>
public sealed class EnemyTileArtworkCatalog
{
    private readonly Dictionary<ushort, RoomCharacterAtlas> sheets;
    private readonly Dictionary<ushort, EnemyPaletteSheet> palettes;
    private readonly Dictionary<(int Source, int ByteCount), RoomCharacterAtlas> byDmaSource;

    public EnemyTileArtworkCatalog(IReadOnlyDictionary<ushort, RoomCharacterAtlas> sheets,
        IReadOnlyDictionary<ushort, EnemyPaletteSheet> palettes,
        CrocomireMeltingArtwork? crocomireMelting = null,
        EnemySpritemapCatalog? spritemaps = null,
        EnemyExtendedFrameCatalog? extendedFrames = null,
        KraidBackgroundArtwork? kraidBackground = null,
        KraidColorCatalog? kraidColors = null,
        GunshipLiftoffArtworkCatalog? gunshipLiftoff = null,
        CeresDoorVisualCatalog? ceresDoorVisual = null,
        IReadOnlyDictionary<ushort, int>? dmaSources = null,
        EnemyProjectileSpritemapCatalog? projectileSpritemaps = null,
        MagdollitePaletteCycle? magdollitePaletteCycle = null,
        WorkRobotPaletteCycle? workRobotPaletteCycle = null,
        CrocomireColorCatalog? crocomireColors = null,
        DraygonColorCatalog? draygonColors = null,
        PhantoonColorCatalog? phantoonColors = null,
        ChozoAndTubeColorCatalog? chozoAndTubeColors = null,
        SporeSpawnColorCatalog? sporeSpawnColors = null,
        DachoraColorCatalog? dachoraColors = null,
        ShitroidColorCatalog? shitroidColors = null,
        BabyMetroidCutsceneColorCatalog? babyMetroidCutsceneColors = null,
        BotwoonColorCatalog? botwoonColors = null,
        MotherBrainDeathColorCatalog? motherBrainDeathColors = null,
        ZebetiteColorCatalog? zebetiteColors = null,
        NorfairRidleyColorCatalog? norfairRidleyColors = null,
        TourianStatueColorCatalog? tourianStatueColors = null,
        PhantoonBg2FrameCatalog? phantoonBg2Frames = null,
        DraygonBg2FrameCatalog? draygonBg2Frames = null,
        RoomCharacterAtlas? motherBrainCorpse = null,
        RoomCharacterAtlas? motherBrainEscapeText = null,
        MotherBrainSpecialSpriteArtworkCatalog? motherBrainSpecialSprites = null,
        CrocomireSkeletonArtwork? crocomireSkeleton = null,
        CrocomireBg2FrameCatalog? crocomireBg2Frames = null,
        TorizoInstructionVramArtwork? torizoInstructionVram = null,
        CeresEscapeTileArtwork? ceresEscapeTiles = null,
        CeresEscapeOverlayTilemapCatalog? ceresEscapeOverlayTilemaps = null)
    {
        ArgumentNullException.ThrowIfNull(sheets);
        ArgumentNullException.ThrowIfNull(palettes);
        if (sheets.Count != palettes.Count || sheets.Keys.Any(pointer => !palettes.ContainsKey(pointer)))
            throw new InvalidDataException("Enemy artwork requires one color sheet per tile sheet.");
        this.sheets = new Dictionary<ushort, RoomCharacterAtlas>(sheets);
        this.palettes = new Dictionary<ushort, EnemyPaletteSheet>(palettes);
        byDmaSource = new Dictionary<(int, int), RoomCharacterAtlas>();
        if (dmaSources is not null)
        {
            foreach ((ushort pointer, int sourceAddress) in dmaSources)
            {
                if (!this.sheets.TryGetValue(pointer, out RoomCharacterAtlas? atlas))
                    throw new InvalidDataException(
                        $"Enemy ${pointer:X4} DMA source has no installed sheet.");
                var key = (sourceAddress, atlas.Transfer.Length);
                if (byDmaSource.TryGetValue(key, out RoomCharacterAtlas? existing))
                {
                    if (!existing.Transfer.Span.SequenceEqual(atlas.Transfer.Span))
                        throw new InvalidDataException(
                            $"Enemy DMA source ${sourceAddress:X6} has conflicting installed sheets.");
                }
                else byDmaSource.Add(key, atlas);
            }
        }
        CrocomireMelting = crocomireMelting;
        Spritemaps = spritemaps;
        ExtendedFrames = extendedFrames;
        KraidBackground = kraidBackground;
        KraidColors = kraidColors;
        GunshipLiftoff = gunshipLiftoff;
        CeresDoorVisual = ceresDoorVisual;
        ProjectileSpritemaps = projectileSpritemaps;
        MagdollitePaletteCycle = magdollitePaletteCycle;
        WorkRobotPaletteCycle = workRobotPaletteCycle;
        CrocomireColors = crocomireColors;
        DraygonColors = draygonColors;
        PhantoonColors = phantoonColors;
        ChozoAndTubeColors = chozoAndTubeColors;
        SporeSpawnColors = sporeSpawnColors;
        DachoraColors = dachoraColors;
        ShitroidColors = shitroidColors;
        BabyMetroidCutsceneColors = babyMetroidCutsceneColors;
        BotwoonColors = botwoonColors;
        MotherBrainDeathColors = motherBrainDeathColors;
        ZebetiteColors = zebetiteColors;
        NorfairRidleyColors = norfairRidleyColors;
        TourianStatueColors = tourianStatueColors;
        PhantoonBg2Frames = phantoonBg2Frames;
        DraygonBg2Frames = draygonBg2Frames;
        CrocomireBg2Frames = crocomireBg2Frames;
        MotherBrainCorpse = motherBrainCorpse;
        MotherBrainEscapeText = motherBrainEscapeText;
        MotherBrainSpecialSprites = motherBrainSpecialSprites;
        CrocomireSkeleton = crocomireSkeleton;
        TorizoInstructionVram = torizoInstructionVram;
        CeresEscapeTiles = ceresEscapeTiles;
        CeresEscapeOverlayTilemaps = ceresEscapeOverlayTilemaps;
    }

    /// <summary>Optional only for constructed fixtures; installed retail catalogs include both melts.</summary>
    public CrocomireMeltingArtwork? CrocomireMelting { get; }

    /// <summary>Six editable skeleton character uploads; death timing and VRAM positions stay fixed.</summary>
    public CrocomireSkeletonArtwork? CrocomireSkeleton { get; }

    /// <summary>Editable tile pages for Bomb/Golden Torizo instruction-time VRAM uploads.</summary>
    public TorizoInstructionVramArtwork? TorizoInstructionVram { get; }

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
    public const int Version = 62;
    /// <summary>Stable, source-address-free name for a gunship takeoff character chunk.</summary>
    public static string GunshipLiftoffFileName(int index) =>
        $"gunship-liftoff-{index + 1}-tiles.png";
    /// <summary>All distinct ordinary graphics-set definitions in the pinned retail room states.</summary>
    public const int RetailDefinitionCount = 122;
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
