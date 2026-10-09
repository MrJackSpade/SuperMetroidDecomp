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

    /// <summary>Stores required enemy sheets and palettes alongside optional specialized providers for resolving visuals.</summary>
    /// <param name="sheets">Complete room-character atlases keyed by native enemy definition pointer.</param>
    /// <param name="palettes">The matching palette sheets keyed by the same native pointers.</param>
    /// <param name="crocomireMelting">Optional tilemap artwork for Crocomire's melting sequence.</param>
    /// <param name="spritemaps">Optional compiled enemy spritemap catalog.</param>
    /// <param name="extendedFrames">Optional extended-frame selector catalog.</param>
    /// <param name="kraidBackground">Optional Kraid background artwork.</param>
    /// <param name="kraidColors">Optional Kraid color definitions.</param>
    /// <param name="gunshipLiftoff">Optional Gunship liftoff artwork.</param>
    /// <param name="ceresDoorVisual">Optional Ceres door presentation data.</param>
    /// <param name="dmaSources">Optional native DMA source aliases, checked against the compiled graphics definitions.</param>
    /// <param name="projectileSpritemaps">Optional projectile spritemap catalog.</param>
    /// <param name="magdollitePaletteCycle">Optional Magdollite palette-cycle data.</param>
    /// <param name="workRobotPaletteCycle">Optional Work Robot palette-cycle data.</param>
    /// <param name="crocomireColors">Optional Crocomire color definitions.</param>
    /// <param name="draygonColors">Optional Draygon color definitions.</param>
    /// <param name="phantoonColors">Optional Phantoon color definitions.</param>
    /// <param name="chozoAndTubeColors">Optional Chozo statue and tube-enemy color definitions.</param>
    /// <param name="sporeSpawnColors">Optional Spore Spawn color definitions.</param>
    /// <param name="dachoraColors">Optional Dachora color definitions.</param>
    /// <param name="shitroidColors">Optional Shitroid color definitions.</param>
    /// <param name="babyMetroidCutsceneColors">Optional Baby Metroid cutscene color definitions.</param>
    /// <param name="botwoonColors">Optional Botwoon color definitions.</param>
    /// <param name="motherBrainDeathColors">Optional Mother Brain death-sequence color definitions.</param>
    /// <param name="zebetiteColors">Optional Zebetite color definitions.</param>
    /// <param name="norfairRidleyColors">Optional Norfair Ridley color definitions.</param>
    /// <param name="tourianStatueColors">Optional Tourian statue color definitions.</param>
    /// <param name="phantoonBg2Frames">Optional Phantoon BG2 animation frames.</param>
    /// <param name="draygonBg2Frames">Optional Draygon BG2 animation frames.</param>
    /// <param name="motherBrainCorpse">Optional Mother Brain corpse character artwork.</param>
    /// <param name="motherBrainEscapeText">Optional Mother Brain escape-text character artwork.</param>
    /// <param name="motherBrainSpecialSprites">Optional special sprites used by Mother Brain sequences.</param>
    /// <param name="crocomireSkeleton">Optional Crocomire skeleton artwork.</param>
    /// <param name="crocomireBg2Frames">Optional Crocomire BG2 animation frames.</param>
    /// <param name="torizoInstructionVram">Optional Torizo instruction-list VRAM artwork.</param>
    /// <param name="ceresEscapeTiles">Optional Ceres escape tile transfer data.</param>
    /// <param name="ceresEscapeOverlayTilemaps">Optional Ceres escape overlay tilemaps.</param>
    /// <param name="auxiliaryColors">Optional auxiliary enemy color definitions.</param>
    /// <param name="motherBrainBodyBg2Frames">Optional Mother Brain body BG2 animation frames.</param>
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

    /// <summary>Ensures installed atlases and palettes cover every compiled enemy tile source at its required size.</summary>
    /// <param name="sheets">Atlases keyed by native enemy definition pointer.</param>
    /// <param name="palettes">Palette sheets expected for the same set of pointers.</param>
    /// <exception cref="InvalidDataException">The maps do not provide a correctly sized atlas and palette for every required source.</exception>
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

    /// <summary>Validates supplied DMA aliases and returns the canonical source map from compiled enemy definitions.</summary>
    /// <param name="sources">Optional installed aliases to compare with the compiled pointer-to-source mapping.</param>
    /// <returns>The canonical DMA source map used to associate transfers with installed atlases.</returns>
    /// <exception cref="InvalidDataException">The supplied aliases differ from the compiled graphics definitions.</exception>
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
