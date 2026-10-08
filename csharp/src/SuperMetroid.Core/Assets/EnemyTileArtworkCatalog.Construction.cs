using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Separates complete installed artwork from explicitly partial verification construction.</summary>
public sealed partial class EnemyTileArtworkCatalog
{
    /// <summary>
    /// Publishes complete ordinary enemy sheets, palettes and DMA aliases.
    /// The Ceres and Torizo transfer extensions are required; other boss attachments retain
    /// their independently validated contracts and are not certified here.
    /// </summary>
    public static EnemyTileArtworkCatalog FromInstalledArtwork(IReadOnlyDictionary<ushort, RoomCharacterAtlas> sheets,
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
        CeresEscapeOverlayTilemapCatalog? ceresEscapeOverlayTilemaps = null,
        EnemyAuxiliaryColorCatalog? auxiliaryColors = null,
        MotherBrainBodyBg2FrameCatalog? motherBrainBodyBg2Frames = null)
    {
        ArgumentNullException.ThrowIfNull(sheets);
        ArgumentNullException.ThrowIfNull(palettes);
        sheets = new Dictionary<ushort, RoomCharacterAtlas>(sheets);
        palettes = new Dictionary<ushort, EnemyPaletteSheet>(palettes);
        ValidateInstalledSheets(sheets, palettes);
        if (ceresEscapeTiles is null || ceresEscapeOverlayTilemaps is null)
            throw new InvalidDataException("Installed enemy DMA artwork requires both Ceres transfer providers.");
        if (torizoInstructionVram is null)
            throw new InvalidDataException("Installed enemy DMA artwork requires the Torizo transfer provider.");
        return new EnemyTileArtworkCatalog(
            sheets, palettes, crocomireMelting, spritemaps,
            extendedFrames, kraidBackground, kraidColors, gunshipLiftoff,
            ceresDoorVisual, InstalledDmaSources(dmaSources), projectileSpritemaps, magdollitePaletteCycle,
            workRobotPaletteCycle, crocomireColors, draygonColors, phantoonColors,
            chozoAndTubeColors, sporeSpawnColors, dachoraColors, shitroidColors,
            babyMetroidCutsceneColors, botwoonColors, motherBrainDeathColors, zebetiteColors,
            norfairRidleyColors, tourianStatueColors, phantoonBg2Frames, draygonBg2Frames,
            motherBrainCorpse, motherBrainEscapeText, motherBrainSpecialSprites, crocomireSkeleton,
            crocomireBg2Frames, torizoInstructionVram, ceresEscapeTiles, ceresEscapeOverlayTilemaps,
            auxiliaryColors, motherBrainBodyBg2Frames);
    }

    private EnemyTileArtworkCatalog(IReadOnlyDictionary<ushort, RoomCharacterAtlas> sheets,
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
        CeresEscapeOverlayTilemapCatalog? ceresEscapeOverlayTilemaps = null,
        EnemyAuxiliaryColorCatalog? auxiliaryColors = null,
        MotherBrainBodyBg2FrameCatalog? motherBrainBodyBg2Frames = null)
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
        MotherBrainBodyBg2Frames = motherBrainBodyBg2Frames;
        MotherBrainCorpse = motherBrainCorpse;
        MotherBrainEscapeText = motherBrainEscapeText;
        MotherBrainSpecialSprites = motherBrainSpecialSprites;
        CrocomireSkeleton = crocomireSkeleton;
        TorizoInstructionVram = torizoInstructionVram;
        CeresEscapeTiles = ceresEscapeTiles;
        CeresEscapeOverlayTilemaps = ceresEscapeOverlayTilemaps;
        AuxiliaryColors = auxiliaryColors;
    }

    private static void ValidateInstalledSheets(IReadOnlyDictionary<ushort, RoomCharacterAtlas> sheets,
        IReadOnlyDictionary<ushort, EnemyPaletteSheet> palettes)
    {
        ArgumentNullException.ThrowIfNull(sheets);
        ArgumentNullException.ThrowIfNull(palettes);
        if (sheets.Count != EnemyTileSourceDefinitions.All.Count ||
            palettes.Count != EnemyTileSourceDefinitions.All.Count)
            throw new InvalidDataException("Installed enemy artwork must contain every required sheet and palette.");
        foreach (EnemyTileSourceDefinition definition in EnemyTileSourceDefinitions.All)
            if (!sheets.TryGetValue(definition.DefinitionPointer, out RoomCharacterAtlas? sheet) || sheet is null ||
                sheet.Transfer.Length != definition.ByteCount ||
                !palettes.TryGetValue(definition.DefinitionPointer, out EnemyPaletteSheet? palette) || palette is null)
                throw new InvalidDataException(
                    $"Installed enemy ${definition.DefinitionPointer:X4} lacks its complete tile sheet or palette.");
    }

    private static Dictionary<ushort, int> InstalledDmaSources(IReadOnlyDictionary<ushort, int>? sources)
    {
        var expected = EnemyTileSourceDefinitions.All.ToDictionary(
            definition => definition.DefinitionPointer, definition => definition.SourceAddress);
        if (sources is not null && (sources.Count != expected.Count ||
            expected.Any(pair => !sources.TryGetValue(pair.Key, out int source) || source != pair.Value)))
            throw new InvalidDataException("Installed enemy DMA aliases must match the compiled graphics definitions.");
        return expected;
    }
}
