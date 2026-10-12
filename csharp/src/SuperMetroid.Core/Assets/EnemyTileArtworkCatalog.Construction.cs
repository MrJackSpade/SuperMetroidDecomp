using SuperMetroid.Core.Game;
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
    public static EnemyTileArtworkCatalog FromInstalledArtwork(EnemyTileArtworkParts parts)
    {
        Ensure.NotNull(parts);
        ArgumentNullException.ThrowIfNull(parts.Sheets);
        ArgumentNullException.ThrowIfNull(parts.Palettes);
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

    private EnemyTileArtworkCatalog(EnemyTileArtworkParts parts)
    {
        Ensure.NotNull(parts);
        ArgumentNullException.ThrowIfNull(parts.Sheets);
        ArgumentNullException.ThrowIfNull(parts.Palettes);
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

    private static void ValidateInstalledSheets(Dictionary<EnemyDefinitionId, RoomCharacterAtlas> sheets,
        Dictionary<EnemyDefinitionId, EnemyPaletteSheet> palettes)
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
                    $"Installed enemy ${(int)definition.DefinitionPointer:X4} lacks its complete tile sheet or palette.");
    }

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
