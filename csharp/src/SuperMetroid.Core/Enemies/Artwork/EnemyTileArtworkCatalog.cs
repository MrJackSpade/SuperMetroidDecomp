using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Installed, palette-indexed ordinary enemy characters, one tile set and palette per enemy definition; enemy health,
/// hitboxes, AI and where the tiles are shown remain engine-owned.
/// </summary>
public sealed class EnemyTileArtworkCatalog
{
    /// <summary>
    /// Complete selected enemy presentation, including legacy-override merges, display
    /// bindings, special uploads and colors. Gameplay definitions are excluded.
    /// This is computed rather than persisted in an exact debugger snapshot.
    /// </summary>
    public string ContentIdentity => EnemyTileArtwork.ContentIdentity(this);

    /// <summary>Ordinary enemy tile sheets keyed by enemy definition.</summary>
    internal readonly Dictionary<EnemyDefinitionId, RoomCharacterAtlas> sheets;
    /// <summary>Palette paired with each installed ordinary enemy tile sheet.</summary>
    internal readonly Dictionary<EnemyDefinitionId, EnemyPaletteSheet> palettes;

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

    /// <summary>The installed tiles of <paramref name="enemy"/>, as its room's graphics set shows them (#1406).</summary>
    /// <exception cref="InvalidDataException">The installed artwork holds no tile set for the enemy.</exception>
    public ReadOnlyMemory<byte> TilesOf(EnemyDefinitionId enemy) => EnemyTileArtwork.TilesOf(this, enemy);

    /// <summary>Loads the sixteen indexed colors selected by a room graphics-set record.</summary>
    public void LoadPaletteTo(EnemyDefinitionId definitionId, ColourRun destination) =>
        EnemyTileArtwork.LoadPaletteTo(this, definitionId, destination);

    /// <summary>Publishes complete ordinary enemy sheets and palettes.</summary>
    public static EnemyTileArtworkCatalog FromInstalledArtwork(EnemyTileArtworkParts parts) => EnemyTileArtwork.FromInstalledArtwork(parts);

    /// <summary>Copies installed artwork, rejecting a sheet without its palette.</summary>
    internal EnemyTileArtworkCatalog(EnemyTileArtworkParts parts) =>
        (sheets, palettes, Spritemaps, ExtendedFrames, GunshipLiftoff, ProjectileSpritemaps, MagdollitePaletteCycle,
            WorkRobotPaletteCycle, ChozoAndTubeColors, DachoraColors, AuxiliaryColors) =
            (new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(EnemyTileArtwork.Paired(parts).Sheets),
                new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(parts.Palettes), parts.Spritemaps, parts.ExtendedFrames,
                parts.GunshipLiftoff, parts.ProjectileSpritemaps, parts.MagdollitePaletteCycle, parts.WorkRobotPaletteCycle,
                parts.ChozoAndTubeColors, parts.DachoraColors, parts.AuxiliaryColors);
}
