using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private ProjectileSpriteCatalog? projectileCompositions;
    [NonSerialized] private ProjectileFrameBindingCatalog? projectileFrameBindings;

    /// <summary>Supplies current visual frame bindings to live, restored and future runtimes.</summary>
    public void BindProjectileFrameBindings(ProjectileFrameBindingCatalog? catalog)
    {
        projectileFrameBindings = catalog;
        if (runtime is not null) runtime.ProjectileFrameBindings = catalog;
    }
    [NonSerialized] private BeamTileCatalog? beamArtwork;
    [NonSerialized] private EnemyTileArtworkCatalog? enemyTileArtwork;

    /// <summary>Supplies editable ordinary enemy character sheets across room and state loads.</summary>
    public void BindEnemyTileArtwork(EnemyTileArtworkCatalog? catalog)
    {
        enemyTileArtwork = catalog;
        if (runtime is not null) runtime.Enemies.TileArtwork = catalog;
    }
    [NonSerialized] private ProjectileTrailCatalog? trailArtwork;
    [NonSerialized] private ChargeFlarePlacementCatalog? chargeFlarePlacement;
    [NonSerialized] private ChargeFlareSpriteCatalog? chargeFlareCompositions;
    [NonSerialized] private GrappleTileAtlas? grappleArtwork;

    /// <summary>Supplies current endpoint/rope PNG content to live and future runtimes.</summary>
    public void BindGrappleArtwork(GrappleTileAtlas? atlas)
    {
        grappleArtwork = atlas;
        if (runtime is not null) runtime.GrappleArtwork = atlas;
    }

    /// <summary>Rebinds current flare parts without replacing saved animation state.</summary>
    public void BindChargeFlareCompositions(ChargeFlareSpriteCatalog? catalog)
    {
        chargeFlareCompositions = catalog;
        if (runtime is not null) runtime.ChargeFlareCompositions = catalog;
    }

    /// <summary>Current visual flare offsets survive runtime replacement but are not embedded in saves.</summary>
    public void BindChargeFlarePlacement(ChargeFlarePlacementCatalog? catalog)
    {
        chargeFlarePlacement = catalog;
        if (runtime is not null) runtime.ChargeFlarePlacement = catalog;
    }

    /// <summary>Rebinds current trail appearance without changing live animation state.</summary>
    public void BindTrailArtwork(ProjectileTrailCatalog? catalog)
    {
        trailArtwork = catalog;
        if (runtime is not null) runtime.TrailArtwork = catalog;
        if (intro is not null) intro.TrailArtwork = catalog;
    }

    /// <summary>Supplies current beam PNG content across runtime creation and state restoration.</summary>
    public void BindBeamArtwork(BeamTileCatalog? catalog)
    {
        beamArtwork = catalog;
        if (runtime is not null) runtime.BeamArtwork = catalog;
        intro?.BindBeamArtwork(catalog);
    }

    /// <summary>Attaches current host content to an existing runtime and future game/demo runtimes.</summary>
    public void BindProjectileCompositions(ProjectileSpriteCatalog? catalog)
    {
        projectileCompositions = catalog;
        if (runtime is not null) runtime.ProjectileCompositions = catalog;
        if (intro is not null) intro.ProjectileCompositions = catalog;
    }
}
