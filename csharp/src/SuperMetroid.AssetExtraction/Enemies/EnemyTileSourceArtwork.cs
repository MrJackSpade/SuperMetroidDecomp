using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed enemy artwork as the import pipeline holds it (#1463, #1583): each enemy's palette and colour cycles,
/// which the sheets and their shades are pre-coloured in (#1512), each enemy's characters, the enemy, extended-frame and
/// enemy-projectile compositions and the gunship's takeoff pages that the sheet composers compose the pre-coloured enemy
/// sheets from, and the identity of the whole selected presentation. The runtime receives none of it: gameplay is given
/// only every installed frame's display (<see cref="FrameDisplays"/>) and the composed sheets.
/// </summary>
public sealed class EnemyTileSourceArtwork
{
    /// <summary>Each enemy's installed characters, by definition.</summary>
    internal readonly Dictionary<EnemyDefinitionId, RoomCharacterAtlas> sheets;

    /// <summary>Each enemy's installed palette, by definition.</summary>
    private readonly Dictionary<EnemyDefinitionId, EnemyPaletteSheet> palettes;

    /// <summary>Copies installed artwork, rejecting a sheet without its palette.</summary>
    internal EnemyTileSourceArtwork(EnemyTileArtworkParts parts)
    {
        RejectUnpaired(parts);
        sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(parts.Sheets);
        palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(parts.Palettes);
        MagdollitePaletteCycle = parts.MagdollitePaletteCycle;
        WorkRobotPaletteCycle = parts.WorkRobotPaletteCycle;
        ChozoAndTubeColors = parts.ChozoAndTubeColors;
        DachoraColors = parts.DachoraColors;
        AuxiliaryColors = parts.AuxiliaryColors;
        _spritemaps = parts.Spritemaps;
        _extendedFrames = parts.ExtendedFrames;
        BindFrameDisplays();
        GunshipLiftoff = parts.GunshipLiftoff;
        ProjectileSpritemaps = parts.ProjectileSpritemaps;
    }

    /// <summary>Finds the installed palette of <paramref name="enemy"/>.</summary>
    internal bool TryGetPalette(EnemyDefinitionId enemy, [NotNullWhen(true)] out EnemyPaletteSheet? palette) =>
        palettes.TryGetValue(enemy, out palette);

    /// <summary>The installed palette of <paramref name="enemy"/>.</summary>
    /// <exception cref="InvalidDataException">The enemy has no installed palette.</exception>
    public EnemyPaletteSheet PaletteOf(EnemyDefinitionId enemy) => TryGetPalette(enemy, out EnemyPaletteSheet? palette) ? palette
        : throw new InvalidDataException($"Enemy {enemy} has no installed palette.");

    /// <summary>Four editable Magdollite glow frames.</summary>
    public MagdollitePaletteCycle? MagdollitePaletteCycle { get; }

    /// <summary>Six editable Work Robot glow records.</summary>
    public WorkRobotPaletteCycle? WorkRobotPaletteCycle { get; }

    /// <summary>Editable Chozo statue and n00b-tube crack colours.</summary>
    public ChozoAndTubeColorCatalog? ChozoAndTubeColors { get; }

    /// <summary>Editable Dachora normal, speed and shine colours.</summary>
    public DachoraColorCatalog? DachoraColors { get; }

    /// <summary>Editable face-block glow, dead Sidehopper drain and Golden Torizo health colours.</summary>
    public EnemyAuxiliaryColorCatalog? AuxiliaryColors { get; }

    /// <summary>Finds the installed tile sheet of <paramref name="enemy"/>.</summary>
    internal bool TryGetSheet(EnemyDefinitionId enemy, [NotNullWhen(true)] out RoomCharacterAtlas? sheet) =>
        sheets.TryGetValue(enemy, out sheet);

    /// <summary>The installed ordinary enemy compositions.</summary>
    private EnemySpritemapCatalog? _spritemaps;

    /// <summary>The installed extended frame compositions.</summary>
    private EnemyExtendedFrameCatalog? _extendedFrames;

    /// <summary>Installed visual-only OAM frames; null for constructed legacy fixtures. Replacing them rebuilds <see cref="FrameDisplays"/>.</summary>
    public EnemySpritemapCatalog? Spritemaps
    {
        get => _spritemaps;
        internal set
        {
            _spritemaps = value;
            BindFrameDisplays();
        }
    }

    /// <summary>
    /// Installed extended frame compositions the sheet composer composes the frames' whole pictures from. Replacing them
    /// rebuilds <see cref="FrameDisplays"/>.
    /// </summary>
    public EnemyExtendedFrameCatalog? ExtendedFrames
    {
        get => _extendedFrames;
        internal set
        {
            _extendedFrames = value;
            BindFrameDisplays();
        }
    }

    /// <summary>
    /// Every installed enemy frame as gameplay draws it, built from both enemy frame documents; null for constructed
    /// fixtures that install only one of them.
    /// </summary>
    public SuperMetroid.Core.Enemies.Artwork.EnemyFrameDisplays? FrameDisplays { get; private set; }

    /// <summary>Builds <see cref="FrameDisplays"/> from the two installed enemy frame documents, none while either is missing.</summary>
    private void BindFrameDisplays() =>
        FrameDisplays = _spritemaps is { } spritemaps && _extendedFrames is { } extended
            ? InstalledEnemyFrameDisplays.Of(spritemaps, extended).Displays() : null;

    /// <summary>Five editable gunship takeoff character uploads; null for constructed fixtures.</summary>
    public GunshipLiftoffArtworkCatalog? GunshipLiftoff { get; }

    /// <summary>Installed bank-$8D projectile compositions; null for constructed fixtures.</summary>
    public EnemyProjectileSpritemapCatalog? ProjectileSpritemaps { get; }

    /// <summary>The installed tiles of <paramref name="enemy"/>, as its room's graphics set shows them (#1406).</summary>
    /// <exception cref="InvalidDataException">The installed artwork holds no tile set for the enemy.</exception>
    public ReadOnlyMemory<byte> TilesOf(EnemyDefinitionId enemy) =>
        TryGetSheet(enemy, out RoomCharacterAtlas? atlas) ? atlas.Characters
            : throw new InvalidDataException($"Installed enemy artwork holds no {enemy} tile set.");

    /// <summary>Publishes complete ordinary enemy sheets and palettes.</summary>
    /// <exception cref="InvalidDataException">A retail definition lacks its complete tile sheet or palette.</exception>
    public static EnemyTileSourceArtwork FromInstalledArtwork(EnemyTileArtworkParts parts)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(parts.Sheets);
        Ensure.NotNull(parts.Palettes);
        var installedSheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(parts.Sheets);
        var installedPalettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(parts.Palettes);
        ValidateInstalledSheets(installedSheets, installedPalettes);
        return new EnemyTileSourceArtwork(parts with
        {
            Sheets = installedSheets,
            Palettes = installedPalettes,
        });
    }

    /// <summary>Rejects a sheet without its palette.</summary>
    /// <exception cref="InvalidDataException">A sheet has no colour sheet, or the counts differ.</exception>
    private static void RejectUnpaired(EnemyTileArtworkParts parts)
    {
        Ensure.NotNull(parts);
        Ensure.NotNull(parts.Sheets);
        Ensure.NotNull(parts.Palettes);
        if (parts.Sheets.Count != parts.Palettes.Count || parts.Sheets.Keys.Any(definition => !parts.Palettes.ContainsKey(definition)))
            throw new InvalidDataException("Enemy artwork requires one color sheet per tile sheet.");
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
                sheet.Characters.Length != definition.ByteCount ||
                !palettes.TryGetValue(definition.DefinitionId, out EnemyPaletteSheet? palette) || palette is null)
                throw new InvalidDataException(
                    $"Installed enemy {definition.DefinitionId} lacks its complete tile sheet or palette.");
    }
}
