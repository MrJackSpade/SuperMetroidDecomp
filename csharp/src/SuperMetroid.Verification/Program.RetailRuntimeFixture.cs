using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static readonly Lazy<GameInstallation> runtimeFixtureInstallation = new(() =>
    {
        string root = Path.GetFullPath("csharp/test-temp/verification-installed-content");
        return GameAssetInstaller.EnsureInstalled(root) ??
            GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root);
    });

    private static readonly Lazy<InstalledProjectilePresentation> projectileFixtureArt =
        new(() => runtimeFixtureInstallation.Value.LoadProjectiles());

    private static SamusProjectileSystem CreateProjectileFixture() =>
        new() { FrameBindings = projectileFixtureArt.Value.FrameBindings };

    private static SamusBombProjectileSystem CreateBombFixture() =>
        new() { FrameBindings = projectileFixtureArt.Value.FrameBindings, PowerBombExplosion = { PresentationColors = RetailPresentationFixture().PowerBombFixedColors } };
    private static readonly Lazy<Action<SamusState>> samusFixtureBindings = new(() =>
    {
        var maps = RetailPresentationFixture();
        var body = runtimeFixtureInstallation.Value.LoadSamusBodyArt();
        var grapple = projectileFixtureArt.Value.GrappleTiles;
        return samus =>
        {
            samus.SuitColors = maps.SamusSuitColors;
            samus.FullBodyCycleColors = maps.SamusFullBodyCycleColors;
            samus.ChargeColors = maps.SamusChargeColors;
            samus.VisorPalette.PresentationColors = maps.SamusVisorColors;
            samus.Xray.PresentationColors = maps.SamusVisorColors;
            samus.Drained.PresentationColors = maps.SamusHyperBeamColors;
            samus.TileTransfers.BindArtwork(body);
            samus.ArmCannon.Artwork = body.ArmCannon;
            samus.Grapple.FlarePlacement = grapple.FlarePlacement;
            samus.Grapple.SwingFrames = grapple.SwingFrames;
        };
    });

    private static SamusState PrepareRetailSamusFixture(SamusState samus)
    {
        samusFixtureBindings.Value(samus);
        return samus;
    }
    private static readonly Lazy<Action<SuperMetroidRuntime>> runtimeFixtureBindings =
        new(() => InstalledRuntimeBindings.Create(runtimeFixtureInstallation.Value));

    // Tests using retail gameplay must supply the host's installed presentation
    // contract explicitly. Each fixture retains its own bus and mutable runtime.
    private static SuperMetroidRuntime CreateRetailRuntimeFixture(
        ISnesAddressSpace addressSpace,
        bool playerInvincibilityEnabled = false,
        bool infiniteAmmoEnabled = false,
        MapRevealMode mapRevealMode = MapRevealMode.None,
        bool preventEscapeTimeout = false)
    {
        var runtime = new SuperMetroidRuntime(addressSpace, playerInvincibilityEnabled,
            infiniteAmmoEnabled, mapRevealMode, preventEscapeTimeout,
            runtimeFixtureInstallation.Value.LoadGameplayBasePalettes());
        runtimeFixtureBindings.Value(runtime);
        return runtime;
    }

    private static readonly Lazy<EnemyTileArtworkCatalog> fixtureEnemyTileArtwork = new(() => runtimeFixtureInstallation.Value.LoadEnemyTiles());

    /// <summary>The shared installation's stock enemy artwork, loaded once per process.</summary>
    private static EnemyTileArtworkCatalog FixtureEnemyTileArtwork() => fixtureEnemyTileArtwork.Value;

    private static readonly Lazy<RoomCharacterAtlasCatalog> fixtureRoomCharacters = new(() => runtimeFixtureInstallation.Value.LoadRoomCharacters());
    private static readonly Lazy<RoomStaticPaletteCatalog> fixtureRoomPalettes = new(() => runtimeFixtureInstallation.Value.LoadRoomPalettes());
    private static readonly Lazy<RoomMetatileCatalog> fixtureRoomMetatiles = new(() => runtimeFixtureInstallation.Value.LoadRoomMetatiles());
    private static readonly Lazy<RoomVisualLayoutCatalog> fixtureRoomVisualLayouts = new(() => runtimeFixtureInstallation.Value.LoadRoomVisualLayouts());

    /// <summary>
    /// Loads a room's asset graph with the shared installation's stock catalogs, replacing only the
    /// catalogs a case supplies. The stock catalogs are the cartridge reference extracted from the ROM.
    /// </summary>
    private static CartridgeRoomAssets LoadFixtureRoomAssets(ISnesAddressSpace bus, CartridgeRoomHeader room,
        RoomCharacterAtlasCatalog? characterArt = null, RoomStaticPaletteCatalog? paletteArt = null,
        RoomMetatileCatalog? metatileArt = null, RoomVisualLayoutCatalog? visualLayouts = null) =>
        CartridgeRoomAssets.Load(bus, room, characterArt ?? fixtureRoomCharacters.Value,
            paletteArt ?? fixtureRoomPalettes.Value, metatileArt ?? fixtureRoomMetatiles.Value,
            visualLayouts ?? fixtureRoomVisualLayouts.Value);
}
