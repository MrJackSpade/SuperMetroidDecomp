namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed enemy artwork gameplay reads (#1463): the extended frame placements its draw selects and the auxiliary
/// colours the Golden Torizo still reads. The pictures enemies draw are pre-coloured sheets with their shades (#1512);
/// the palettes, colour cycles, characters and compositions they are composed from belong to the import pipeline's
/// subclass.
/// </summary>
public abstract class EnemyTileArtworkCatalog
{
    /// <summary>The installed extended frames and colour catalogs.</summary>
    private EnemyGameplayArtworkParts installed;

    /// <summary>Copies the installed catalogs.</summary>
    /// <param name="parts">The installed artwork gameplay reads.</param>
    protected EnemyTileArtworkCatalog(EnemyGameplayArtworkParts parts) => installed = Ensure.NotNull(parts);

    /// <summary>Editable face-block, corpse-sidehopper, and Golden Torizo health colors.</summary>
    public EnemyAuxiliaryColorCatalog? AuxiliaryColors => installed.AuxiliaryColors;

    /// <summary>Installed extended frames' component placements and display bindings; hitbox and AI data stay engine-owned.</summary>
    public EnemyExtendedFramePlacementCatalog? ExtendedFramePlacements
    {
        get => installed.ExtendedFramePlacements;
        internal set => installed = installed with { ExtendedFramePlacements = value };
    }
}
