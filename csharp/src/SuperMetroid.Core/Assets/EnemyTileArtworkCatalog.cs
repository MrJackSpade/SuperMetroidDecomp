using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Installed, palette-indexed ordinary enemy characters. Definition pointers select art;
/// enemy health, hitboxes, AI, and native VRAM destinations remain engine-owned.
/// </summary>
public sealed class EnemyTileArtworkCatalog
{
    /// <summary>
    /// Complete selected enemy presentation, including legacy-override merges, display
    /// bindings, DMA aliases, special uploads and colors. Gameplay definitions are excluded.
    /// This is computed rather than persisted in an exact debugger snapshot.
    /// </summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-bundle-v1", content =>
        {
            foreach ((EnemyDefinitionId definition, RoomCharacterAtlas sheet) in sheets.OrderBy(pair => pair.Key))
            {
                content.Append("definition", (ushort)definition);
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

    /// <summary>Ordinary enemy tile sheets keyed by the native enemy definition pointer.</summary>
    private readonly Dictionary<EnemyDefinitionId, RoomCharacterAtlas> sheets;
    /// <summary>Palette paired with each installed ordinary enemy tile sheet.</summary>
    private readonly Dictionary<EnemyDefinitionId, EnemyPaletteSheet> palettes;
    /// <summary>Tile sheets addressable by the native source and byte count of a DMA request.</summary>
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
    public EnemySpritemapCatalog? Spritemaps { get; private set; }

    /// <summary>Installed extended visual frames; hitbox and AI data stay engine-owned.</summary>
    public EnemyExtendedFrameCatalog? ExtendedFrames { get; private set; }

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
    public void LoadTo(EnemyDefinitionId definitionPointer, int byteCount, VramRegion destination, int byteOffset)
    {
        if (!sheets.TryGetValue(definitionPointer, out RoomCharacterAtlas? atlas))
            throw new InvalidDataException($"Enemy ${(int)definitionPointer:X4} has no installed tile sheet.");
        if (atlas.Transfer.Length != byteCount)
            throw new InvalidDataException(
                $"Enemy ${(int)definitionPointer:X4} requires {byteCount} tile bytes, installed sheet has {atlas.Transfer.Length}.");
        destination.Load(atlas.Transfer.Span, byteOffset);
    }

    /// <summary>Loads the sixteen indexed colors selected by a room graphics-set record.</summary>
    public void LoadPaletteTo(EnemyDefinitionId definitionPointer, CgramPalette destination)
    {
        if (!palettes.TryGetValue(definitionPointer, out EnemyPaletteSheet? palette))
            throw new InvalidDataException($"Enemy ${(int)definitionPointer:X4} has no installed palette.");
        palette.LoadTo(destination);
    }

    /// <summary>
    /// Publishes complete ordinary enemy sheets, palettes and DMA aliases.
    /// The Ceres and Torizo transfer extensions are required; other boss attachments retain
    /// their independently validated contracts and are not certified here.
    /// </summary>
    public static EnemyTileArtworkCatalog FromInstalledArtwork(EnemyTileArtworkParts parts)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(parts.Sheets);
        Ensure.NotNull(parts.Palettes);
        var installedSheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(parts.Sheets);
        var installedPalettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(parts.Palettes);
        ValidateInstalledSheets(installedSheets, installedPalettes);
        if (parts.CeresEscapeTiles is null || parts.CeresEscapeOverlayTilemaps is null)
            throw new InvalidDataException("Installed enemy DMA artwork requires both Ceres transfer providers.");
        if (parts.TorizoInstructionVram is null)
            throw new InvalidDataException("Installed enemy DMA artwork requires the Torizo transfer provider.");
        return new EnemyTileArtworkCatalog(parts with
        {
            Sheets = installedSheets,
            Palettes = installedPalettes,
            DmaSources = InstalledDmaSources(parts.DmaSources),
        });
    }

    /// <summary>Copies installed artwork and builds DMA aliases, rejecting missing or conflicting sheets.</summary>
    private EnemyTileArtworkCatalog(EnemyTileArtworkParts parts)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(parts.Sheets);
        Ensure.NotNull(parts.Palettes);
        if (parts.Sheets.Count != parts.Palettes.Count || parts.Sheets.Keys.Any(pointer => !parts.Palettes.ContainsKey(pointer)))
            throw new InvalidDataException("Enemy artwork requires one color sheet per tile sheet.");
        this.sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(parts.Sheets);
        this.palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(parts.Palettes);
        byDmaSource = new Dictionary<(int, int), RoomCharacterAtlas>();
        if (parts.DmaSources is not null)
        {
            foreach ((EnemyDefinitionId pointer, int sourceAddress) in parts.DmaSources)
            {
                if (!this.sheets.TryGetValue(pointer, out RoomCharacterAtlas? atlas))
                    throw new InvalidDataException(
                        $"Enemy ${(int)pointer:X4} DMA source has no installed sheet.");
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
        CrocomireMelting = parts.CrocomireMelting;
        Spritemaps = parts.Spritemaps;
        ExtendedFrames = parts.ExtendedFrames;
        KraidBackground = parts.KraidBackground;
        KraidColors = parts.KraidColors;
        GunshipLiftoff = parts.GunshipLiftoff;
        CeresDoorVisual = parts.CeresDoorVisual;
        ProjectileSpritemaps = parts.ProjectileSpritemaps;
        MagdollitePaletteCycle = parts.MagdollitePaletteCycle;
        WorkRobotPaletteCycle = parts.WorkRobotPaletteCycle;
        CrocomireColors = parts.CrocomireColors;
        DraygonColors = parts.DraygonColors;
        PhantoonColors = parts.PhantoonColors;
        ChozoAndTubeColors = parts.ChozoAndTubeColors;
        SporeSpawnColors = parts.SporeSpawnColors;
        DachoraColors = parts.DachoraColors;
        ShitroidColors = parts.ShitroidColors;
        BabyMetroidCutsceneColors = parts.BabyMetroidCutsceneColors;
        BotwoonColors = parts.BotwoonColors;
        MotherBrainDeathColors = parts.MotherBrainDeathColors;
        ZebetiteColors = parts.ZebetiteColors;
        NorfairRidleyColors = parts.NorfairRidleyColors;
        TourianStatueColors = parts.TourianStatueColors;
        PhantoonBg2Frames = parts.PhantoonBg2Frames;
        DraygonBg2Frames = parts.DraygonBg2Frames;
        CrocomireBg2Frames = parts.CrocomireBg2Frames;
        MotherBrainBodyBg2Frames = parts.MotherBrainBodyBg2Frames;
        MotherBrainCorpse = parts.MotherBrainCorpse;
        MotherBrainEscapeText = parts.MotherBrainEscapeText;
        MotherBrainSpecialSprites = parts.MotherBrainSpecialSprites;
        CrocomireSkeleton = parts.CrocomireSkeleton;
        TorizoInstructionVram = parts.TorizoInstructionVram;
        CeresEscapeTiles = parts.CeresEscapeTiles;
        CeresEscapeOverlayTilemaps = parts.CeresEscapeOverlayTilemaps;
        AuxiliaryColors = parts.AuxiliaryColors;
    }

    /// <summary>Requires a complete, correctly sized tile sheet and palette for every retail definition.</summary>
    private static void ValidateInstalledSheets(Dictionary<EnemyDefinitionId, RoomCharacterAtlas> sheets,
        Dictionary<EnemyDefinitionId, EnemyPaletteSheet> palettes)
    {
        Ensure.NotNull(sheets);
        Ensure.NotNull(palettes);
        if (sheets.Count != EnemyTileSourceDefinitions.All.Count ||
            palettes.Count != EnemyTileSourceDefinitions.All.Count)
            throw new InvalidDataException("Installed enemy artwork must contain every required sheet and palette.");
        foreach (EnemyTileSourceDefinition definition in EnemyTileSourceDefinitions.All)
            if (!sheets.TryGetValue(definition.DefinitionPointer, out RoomCharacterAtlas? sheet) || sheet is null ||
                sheet.Transfer.Length != definition.ByteCount ||
                !palettes.TryGetValue(definition.DefinitionPointer, out EnemyPaletteSheet? palette) || palette is null)
                throw new InvalidDataException(
                    $"Installed enemy ${(int)definition.DefinitionPointer:X4} lacks its complete tile sheet or palette.");
    }

    /// <summary>Checks supplied DMA aliases against compiled graphics sources and returns the complete mapping.</summary>
    private static Dictionary<EnemyDefinitionId, int> InstalledDmaSources(IReadOnlyDictionary<EnemyDefinitionId, int>? sources)
    {
        var expected = EnemyTileSourceDefinitions.All.ToDictionary(
            definition => definition.DefinitionPointer, definition => definition.SourceAddress);
        if (sources is not null && (sources.Count != expected.Count ||
            expected.Any(pair => !sources.TryGetValue(pair.Key, out int source) || source != pair.Value)))
            throw new InvalidDataException("Installed enemy DMA aliases must match the compiled graphics definitions.");
        return expected;
    }
}

/// <summary>Host-file geometry for native four-bit enemy tile DMA sheets.</summary>
public static class EnemyTileArtworkFormat
{
    /// <summary>File name of the versioned manifest binding native enemy-definition identities to artwork sheets.</summary>
    public const string ManifestFileName = "enemy-tiles.json";
    /// <summary>Current enemy-tile installation schema revision.</summary>
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

    /// <summary>Creates the indexed-PNG filename for a bank-$A0 enemy-definition pointer.</summary>
    public static string FileName(EnemyDefinitionId definitionPointer) => $"enemy-{(ushort)definitionPointer:X4}-tiles.png";
    /// <summary>Creates the RGB5 palette JSON filename for a bank-$A0 enemy-definition pointer.</summary>
    public static string PaletteFileName(EnemyDefinitionId definitionPointer) => $"enemy-{(ushort)definitionPointer:X4}-colors.json";
}
