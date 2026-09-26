using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime : IVramAssetProvider, IRomArtworkSource
{
    bool IRomArtworkSource.TryResolve(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> data)
    {
        if (MapPresentation is not null &&
            sourceAddress == GameplayHudDefinitions.TopRowAddress &&
            byteCount == GameplayHudDefinitions.TopRowByteCount)
        {
            data = MapPresentation.GameplayHud.TopRowTransfer;
            return true;
        }
        if (StandardObjectArt is not null &&
            sourceAddress == StandardObjectArtworkFormat.SourceAddress &&
            byteCount == StandardObjectArtworkFormat.TransferByteCount)
        {
            data = StandardObjectArt.Transfer;
            return true;
        }
        if (SamusBodyArt?.DeathTiles.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (Enemies.TileArtwork?.GunshipLiftoff?.TryResolve(
                sourceAddress, byteCount, out data) == true)
            return true;
        if (MapPresentation?.RoomFxAnimatedTiles.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (RoomSkyTilemapArt is not null)
            return RoomSkyTilemapArt.TryResolve(sourceAddress, byteCount, out data);
        data = default;
        return false;
    }
    // Host content is rebound after restoring a graph; saved state must not freeze
    // an old user override into the simulation. Only composition emission uses this.
    [NonSerialized] private ProjectileSpriteCatalog? projectileCompositions;
    /// <summary>Complete installed standard OBJ sheet used by the queued gameplay DMA.</summary>
    [field: NonSerialized]
    public RoomCharacterAtlas? StandardObjectArt { get; set; }
    [NonSerialized] private ProjectileFrameBindingCatalog? projectileFrameBindings;

    /// <summary>Rebinds authored visual frame choices to both native projectile slot owners.</summary>
    public ProjectileFrameBindingCatalog? ProjectileFrameBindings
    {
        get => projectileFrameBindings;
        set
        {
            projectileFrameBindings = value;
            Projectiles.FrameBindings = value;
            BombProjectiles.FrameBindings = value;
        }
    }
    [field: NonSerialized]
    public ChargeFlarePlacementCatalog? ChargeFlarePlacement { get; set; }
    [field: NonSerialized]
    public ChargeFlareSpriteCatalog? ChargeFlareCompositions { get; set; }
    [NonSerialized] private GrappleTileAtlas? grappleArtwork;
    /// <summary>Current Grapple artwork and visual definitions. Rebinding legacy pending transfers preserves their NMI order and destination.</summary>
    public GrappleTileAtlas? GrappleArtwork
    {
        get => grappleArtwork;
        set { grappleArtwork = value; value?.RebindPendingWrites(VramWrites); BindGrapplePresentation(); }
    }
    private void BindGrapplePresentation()
    {
        if (Samus is not null)
        {
            Samus.Grapple.FlarePlacement = grappleArtwork?.FlarePlacement;
            Samus.Grapple.SwingFrames = grappleArtwork?.SwingFrames;
        }
    }
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
        asset is VramAssetId.GunshipLiftoffFirstTiles or
            VramAssetId.GunshipLiftoffSecondTiles or
            VramAssetId.GunshipLiftoffThirdTiles or
            VramAssetId.GunshipLiftoffFourthTiles or
            VramAssetId.GunshipLiftoffFifthTiles
            ? (Enemies.TileArtwork?.GunshipLiftoff ??
                throw new InvalidOperationException("Gunship takeoff artwork is not bound.")).Resolve(asset)
            : asset is VramAssetId.GrapplePointFirstTiles or VramAssetId.GrapplePointSecondTiles or
            VramAssetId.GrapplePointThirdTiles or VramAssetId.GrapplePointFourthTiles or
            VramAssetId.GrappleHorizontalSegmentTiles or VramAssetId.GrappleDiagonalSegmentTiles or VramAssetId.GrappleVerticalSegmentTiles
            ? (grappleArtwork ?? throw new InvalidOperationException("Grapple artwork is not bound.")).Resolve(asset)
            : asset is VramAssetId.ProjectileIceWaveTrailTiles or VramAssetId.ProjectileMissileTrailTiles
            ? (trailArtwork?.Tiles ?? throw new InvalidOperationException("Trail artwork is not bound.")).Resolve(asset)
            : asset is VramAssetId.StandardHudTiles or
                VramAssetId.KraidBg3RestoreQuarter0 or VramAssetId.KraidBg3RestoreQuarter1 or
                VramAssetId.KraidBg3RestoreQuarter2 or VramAssetId.KraidBg3RestoreQuarter3 or
                VramAssetId.EscapeTimerFirstTiles or VramAssetId.EscapeTimerSecondTiles
            ? (MapPresentation ?? throw new InvalidOperationException("Map presentation artwork is not bound.")).Resolve(asset)
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
