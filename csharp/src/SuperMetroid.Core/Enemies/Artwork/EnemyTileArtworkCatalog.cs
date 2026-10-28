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

    /// <summary>
    /// The installed artwork: ordinary enemy tile sheets keyed by enemy definition, the palette paired with each sheet,
    /// and the special catalogs.
    /// </summary>
    internal EnemyTileArtworkParts installed;

    /// <summary>Editable face-block, corpse-sidehopper, and Golden Torizo health colors.</summary>
    public EnemyAuxiliaryColorCatalog? AuxiliaryColors => installed.AuxiliaryColors;

    /// <summary>Installed visual-only OAM frames; null for constructed legacy fixtures.</summary>
    public EnemySpritemapCatalog? Spritemaps
    {
        get => installed.Spritemaps;
        internal set => installed = installed with { Spritemaps = value };
    }

    /// <summary>Installed extended visual frames; hitbox and AI data stay engine-owned.</summary>
    public EnemyExtendedFrameCatalog? ExtendedFrames
    {
        get => installed.ExtendedFrames;
        internal set => installed = installed with { ExtendedFrames = value };
    }

    /// <summary>Five editable gunship takeoff character uploads; null for constructed fixtures.</summary>
    public GunshipLiftoffArtworkCatalog? GunshipLiftoff => installed.GunshipLiftoff;

    /// <summary>Installed bank-$8D projectile compositions; null for constructed fixtures.</summary>
    public EnemyProjectileSpritemapCatalog? ProjectileSpritemaps => installed.ProjectileSpritemaps;

    /// <summary>Four editable color frames; the Magdollite draw hook retains timing.</summary>
    public MagdollitePaletteCycle? MagdollitePaletteCycle => installed.MagdollitePaletteCycle;

    /// <summary>Six editable Work Robot color records; timer/terminator stay compiled.</summary>
    public WorkRobotPaletteCycle? WorkRobotPaletteCycle => installed.WorkRobotPaletteCycle;

    /// <summary>Editable Chozo statue and n00b-tube crack sprite-palette pairs.</summary>
    public ChozoAndTubeColorCatalog? ChozoAndTubeColors => installed.ChozoAndTubeColors;

    /// <summary>Editable Dachora default, speed and shine sprite colors.</summary>
    public DachoraColorCatalog? DachoraColors => installed.DachoraColors;

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
        installed = EnemyTileArtwork.Paired(parts) with
        {
            Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(parts.Sheets),
            Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(parts.Palettes),
        };
}
