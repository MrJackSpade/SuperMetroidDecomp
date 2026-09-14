using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime : IVramAssetProvider
{
    // Host content is rebound after restoring a graph; saved state must not freeze
    // an old user override into the simulation. Only composition emission uses this.
    [NonSerialized] private ProjectileSpriteCatalog? projectileCompositions;
    [field: NonSerialized]
    public ChargeFlarePlacementCatalog? ChargeFlarePlacement { get; set; }
    [NonSerialized] private ProjectileTrailCatalog? trailArtwork;
    [NonSerialized] private bool trailArtworkRefreshPending;
    public ProjectileTrailCatalog? TrailArtwork
    {
        get => trailArtwork;
        set { trailArtwork = value; trailArtworkRefreshPending = value?.Tiles is not null; }
    }
    [NonSerialized] private BeamTileCatalog? beamArtwork;
    [NonSerialized] private bool beamArtworkRefreshPending;

    /// <summary>Current beam tiles. Rebinding changes the next accepted display, not the retained frame.</summary>
    public BeamTileCatalog? BeamArtwork
    {
        get => beamArtwork;
        set { beamArtwork = value; beamArtworkRefreshPending = value is not null; }
    }

    ReadOnlyMemory<byte> IVramAssetProvider.Resolve(VramAssetId asset) =>
        asset is VramAssetId.ProjectileIceWaveTrailTiles or VramAssetId.ProjectileMissileTrailTiles
            ? (trailArtwork?.Tiles ?? throw new InvalidOperationException("Trail artwork is not bound.")).Resolve(asset)
            : asset == VramAssetId.StandardHudTiles
            ? (MapPresentation ?? throw new InvalidOperationException("HUD artwork is not bound.")).Resolve(asset)
            : (beamArtwork ?? throw new InvalidOperationException("Beam artwork is not bound.")).Resolve(asset);

    private void PublishReboundTrailArtwork()
    {
        if (!trailArtworkRefreshPending) return;
        // A restored legacy queue can still contain the standard OBJ upload. Publish
        // current host art only after that queue drains, and only on accepted NMIs.
        trailArtwork!.Tiles!.LoadTo(Vram);
        trailArtworkRefreshPending = false;
    }

    private void PublishReboundBeamArtwork()
    {
        if (!beamArtworkRefreshPending || Samus is null) return;
        int selection = Samus.EquippedBeams & 0x0fff;
        // Do not reinterpret invalid combination overreads as a legal replacement.
        if (selection < BeamTileAtlasDefinitions.SelectionCount)
        {
            Vram.ExecuteQueuedAssetWrite(beamArtwork!.Resolve(BeamTileCatalog.AssetFor(selection)).Span,
                BeamTileAtlasDefinitions.DestinationWord);
            // An in-flight effect owns these colors across frames. Its own completion
            // restores the selected normal palette; host rebind must not erase its phase.
            if (Samus.CrystalFlash.SpecialPaletteKind != Game.SamusSpecialPaletteType.CrystalFlash &&
                !Samus.Drained.HyperBeamPaletteFx.IsActive)
                beamArtwork.Palettes?.LoadTo(Cgram, selection);
        }
        beamArtworkRefreshPending = false;
    }

    /// <summary>Current external timed-projectile composition catalog, independent of mechanics.</summary>
    public ProjectileSpriteCatalog? ProjectileCompositions
    {
        get => projectileCompositions;
        set => projectileCompositions = value;
    }
}
