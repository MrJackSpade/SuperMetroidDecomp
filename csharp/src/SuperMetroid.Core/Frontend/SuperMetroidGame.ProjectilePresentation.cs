using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>Current projectile sprite compositions retained for runtime and intro rebinding.</summary>
    [NonSerialized] private ProjectileSpriteCatalog? projectileCompositions;
    /// <summary>Current animation-frame mappings used to resolve projectile artwork in active and future scenes.</summary>
    [NonSerialized] private ProjectileFrameBindingCatalog? projectileFrameBindings;

    /// <summary>Supplies current visual frame bindings to live, restored and future runtimes.</summary>
    public void BindProjectileFrameBindings(ProjectileFrameBindingCatalog? catalog)
    {
        projectileFrameBindings = catalog;
        if (runtime is not null) runtime.ProjectileFrameBindings = catalog;
        if (intro is not null) intro.ProjectileFrameBindings = catalog;
    }
    /// <summary>Beam tile artwork supplied to the active intro or subsequent runtime instances.</summary>
    [NonSerialized] private BeamTileCatalog? beamArtwork;
    /// <summary>Editable enemy character sheets retained while rooms and runtimes are replaced.</summary>
    [NonSerialized] private EnemyTileArtworkCatalog? enemyTileArtwork;

    /// <summary>Supplies editable ordinary enemy character sheets across room and state loads.</summary>
    public void BindEnemyTileArtwork(EnemyTileArtworkCatalog? catalog)
    {
        enemyTileArtwork = catalog;
        if (runtime is not null)
        {
            runtime.Enemies.TileArtwork = catalog;
            runtime.CeresElevatorArrival?.BindProjectileSpritemaps(
                catalog?.ProjectileSpritemaps);
        }
    }
    /// <summary>Projectile trail appearance retained for active and future runtime or intro scenes.</summary>
    [NonSerialized] private ProjectileTrailCatalog? trailArtwork;
    /// <summary>Placement offsets for charge-flare parts, preserved separately from animation state.</summary>
    [NonSerialized] private ChargeFlarePlacementCatalog? chargeFlarePlacement;
    /// <summary>Charge-flare sprite parts rebound into the active and future runtime instances.</summary>
    [NonSerialized] private ChargeFlareSpriteCatalog? chargeFlareCompositions;
    /// <summary>Grapple endpoint and rope tile artwork supplied to the active or next runtime.</summary>
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
        // A completed intro remains in a restored object graph, but its VRAM
        // and beam DMA are no longer displayed or advanced during gameplay.
        // Rebinding that dormant scene would replay an unrelated cartridge
        // read while loading a gameplay debugger state.
        if (GameState == SuperMetroidGameState.IntroCinematic)
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
