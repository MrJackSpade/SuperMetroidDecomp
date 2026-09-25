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

    public EnemyTileArtworkCatalog(IReadOnlyDictionary<ushort, RoomCharacterAtlas> sheets,
        IReadOnlyDictionary<ushort, EnemyPaletteSheet> palettes,
        CrocomireMeltingArtwork? crocomireMelting = null,
        EnemySpritemapCatalog? spritemaps = null,
        EnemyExtendedFrameCatalog? extendedFrames = null,
        KraidBackgroundArtwork? kraidBackground = null,
        KraidColorCatalog? kraidColors = null)
    {
        ArgumentNullException.ThrowIfNull(sheets);
        ArgumentNullException.ThrowIfNull(palettes);
        if (sheets.Count != palettes.Count || sheets.Keys.Any(pointer => !palettes.ContainsKey(pointer)))
            throw new InvalidDataException("Enemy artwork requires one color sheet per tile sheet.");
        this.sheets = new Dictionary<ushort, RoomCharacterAtlas>(sheets);
        this.palettes = new Dictionary<ushort, EnemyPaletteSheet>(palettes);
        CrocomireMelting = crocomireMelting;
        Spritemaps = spritemaps;
        ExtendedFrames = extendedFrames;
        KraidBackground = kraidBackground;
        KraidColors = kraidColors;
    }

    /// <summary>Optional only for constructed fixtures; installed retail catalogs include both melts.</summary>
    public CrocomireMeltingArtwork? CrocomireMelting { get; }

    /// <summary>Installed visual-only OAM frames; null for constructed legacy fixtures.</summary>
    public EnemySpritemapCatalog? Spritemaps { get; }

    /// <summary>Installed extended visual frames; hitbox and AI data stay engine-owned.</summary>
    public EnemyExtendedFrameCatalog? ExtendedFrames { get; }

    /// <summary>Kraid's ordered BG2 tile references; null only for constructed fixtures.</summary>
    public KraidBackgroundArtwork? KraidBackground { get; }

    /// <summary>Installed Kraid RGB5 artwork; null only for constructed fixtures.</summary>
    public KraidColorCatalog? KraidColors { get; }

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
    public const int Version = 16;
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
