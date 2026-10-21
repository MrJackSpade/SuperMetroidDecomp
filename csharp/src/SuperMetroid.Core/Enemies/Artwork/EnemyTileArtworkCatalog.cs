using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

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
            content.AppendIdentity("Spritemaps", Spritemaps?.ContentIdentity);
            content.AppendIdentity("ExtendedFrames", ExtendedFrames?.ContentIdentity);
            content.AppendIdentity("GunshipLiftoff", GunshipLiftoff?.ContentIdentity);
            content.AppendIdentity("ProjectileSpritemaps", ProjectileSpritemaps?.ContentIdentity);
            content.AppendIdentity("MagdollitePaletteCycle", MagdollitePaletteCycle?.ContentIdentity);
            content.AppendIdentity("WorkRobotPaletteCycle", WorkRobotPaletteCycle?.ContentIdentity);
            content.AppendIdentity("ChozoAndTubeColors", ChozoAndTubeColors?.ContentIdentity);
            content.AppendIdentity("DachoraColors", DachoraColors?.ContentIdentity);
            content.AppendIdentity("AuxiliaryColors", AuxiliaryColors?.ContentIdentity);
        });

    /// <summary>Ordinary enemy tile sheets keyed by the native enemy definition pointer.</summary>
    internal readonly Dictionary<EnemyDefinitionId, RoomCharacterAtlas> sheets;
    /// <summary>Palette paired with each installed ordinary enemy tile sheet.</summary>
    private readonly Dictionary<EnemyDefinitionId, EnemyPaletteSheet> palettes;
    /// <summary>Tile sheets addressable by the native source and byte count of a DMA request.</summary>
    private readonly Dictionary<EnemyDmaSourceKey, RoomCharacterAtlas> byDmaSource;

    /// <summary>Editable face-block, corpse-sidehopper, and Golden Torizo health colors.</summary>
    public EnemyAuxiliaryColorCatalog? AuxiliaryColors { get; }

    /// <summary>Installed visual-only OAM frames; null for constructed legacy fixtures.</summary>
    public EnemySpritemapCatalog? Spritemaps { get; internal set; }

    /// <summary>Installed extended visual frames; hitbox and AI data stay engine-owned.</summary>
    public EnemyExtendedFrameCatalog? ExtendedFrames { get; internal set; }

    /// <summary>Five editable gunship takeoff character uploads; null for constructed fixtures.</summary>
    public GunshipLiftoffArtworkCatalog? GunshipLiftoff { get; }

    /// <summary>Installed bank-$8D projectile compositions; null for constructed fixtures.</summary>
    public EnemyProjectileSpritemapCatalog? ProjectileSpritemaps { get; }

    /// <summary>Four editable color frames; the Magdollite draw hook retains timing.</summary>
    public MagdollitePaletteCycle? MagdollitePaletteCycle { get; }

    /// <summary>Six editable Work Robot color records; timer/terminator stay compiled.</summary>
    public WorkRobotPaletteCycle? WorkRobotPaletteCycle { get; }

    /// <summary>Editable Chozo statue and n00b-tube crack sprite-palette pairs.</summary>
    public ChozoAndTubeColorCatalog? ChozoAndTubeColors { get; }

    /// <summary>Editable Dachora default, speed and shine sprite colors.</summary>
    public DachoraColorCatalog? DachoraColors { get; }

    /// <summary>Resolves the native room-entry enemy VRAM queue against the same indexed PNGs.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        if (byDmaSource.TryGetValue(new EnemyDmaSourceKey(sourceAddress, byteCount), out RoomCharacterAtlas? atlas))
        {
            data = atlas.Transfer;
            return true;
        }
        data = default;
        return false;
    }

    /// <summary>Loads the sixteen indexed colors selected by a room graphics-set record.</summary>
    public void LoadPaletteTo(EnemyDefinitionId definitionId, CgramPalette destination)
    {
        if (!palettes.TryGetValue(definitionId, out EnemyPaletteSheet? palette))
            throw new InvalidDataException($"Enemy ${(int)definitionId:X4} has no installed palette.");
        palette.LoadTo(destination);
    }

    /// <summary>Publishes complete ordinary enemy sheets, palettes and DMA aliases.</summary>
    public static EnemyTileArtworkCatalog FromInstalledArtwork(EnemyTileArtworkParts parts)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(parts.Sheets);
        Ensure.NotNull(parts.Palettes);
        var installedSheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(parts.Sheets);
        var installedPalettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(parts.Palettes);
        ValidateInstalledSheets(installedSheets, installedPalettes);
        return new EnemyTileArtworkCatalog(parts with
        {
            Sheets = installedSheets,
            Palettes = installedPalettes,
            DmaSources = InstalledDmaSources(parts.DmaSources),
        });
    }

    /// <summary>Copies installed artwork and builds DMA aliases, rejecting missing or conflicting sheets.</summary>
    internal EnemyTileArtworkCatalog(EnemyTileArtworkParts parts)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(parts.Sheets);
        Ensure.NotNull(parts.Palettes);
        if (parts.Sheets.Count != parts.Palettes.Count || parts.Sheets.Keys.Any(pointer => !parts.Palettes.ContainsKey(pointer)))
            throw new InvalidDataException("Enemy artwork requires one color sheet per tile sheet.");
        this.sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(parts.Sheets);
        this.palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(parts.Palettes);
        byDmaSource = new Dictionary<EnemyDmaSourceKey, RoomCharacterAtlas>();
        if (parts.DmaSources is not null)
        {
            foreach ((EnemyDefinitionId pointer, int sourceAddress) in parts.DmaSources)
            {
                if (!this.sheets.TryGetValue(pointer, out RoomCharacterAtlas? atlas))
                    throw new InvalidDataException(
                        $"Enemy ${(int)pointer:X4} DMA source has no installed sheet.");
                var key = new EnemyDmaSourceKey(sourceAddress, atlas.Transfer.Length);
                if (byDmaSource.TryGetValue(key, out RoomCharacterAtlas? existing))
                {
                    if (!existing.Transfer.Span.SequenceEqual(atlas.Transfer.Span))
                        throw new InvalidDataException(
                            $"Enemy DMA source ${sourceAddress:X6} has conflicting installed sheets.");
                }
                else byDmaSource.Add(key, atlas);
            }
        }
        Spritemaps = parts.Spritemaps;
        ExtendedFrames = parts.ExtendedFrames;
        GunshipLiftoff = parts.GunshipLiftoff;
        ProjectileSpritemaps = parts.ProjectileSpritemaps;
        MagdollitePaletteCycle = parts.MagdollitePaletteCycle;
        WorkRobotPaletteCycle = parts.WorkRobotPaletteCycle;
        ChozoAndTubeColors = parts.ChozoAndTubeColors;
        DachoraColors = parts.DachoraColors;
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
            if (!sheets.TryGetValue(definition.DefinitionId, out RoomCharacterAtlas? sheet) || sheet is null ||
                sheet.Transfer.Length != definition.ByteCount ||
                !palettes.TryGetValue(definition.DefinitionId, out EnemyPaletteSheet? palette) || palette is null)
                throw new InvalidDataException(
                    $"Installed enemy ${(int)definition.DefinitionId:X4} lacks its complete tile sheet or palette.");
    }

    /// <summary>Checks supplied DMA aliases against compiled graphics sources and returns the complete mapping.</summary>
    private static Dictionary<EnemyDefinitionId, int> InstalledDmaSources(IReadOnlyDictionary<EnemyDefinitionId, int>? sources)
    {
        var expected = EnemyTileSourceDefinitions.All.ToDictionary(
            definition => definition.DefinitionId, definition => definition.SourceAddress);
        if (sources is not null && (sources.Count != expected.Count ||
            expected.Any(pair => !sources.TryGetValue(pair.Key, out int source) || source != pair.Value)))
            throw new InvalidDataException("Installed enemy DMA aliases must match the compiled graphics definitions.");
        return expected;
    }
}
