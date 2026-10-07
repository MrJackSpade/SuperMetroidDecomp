using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static SamusProjectileSystem CreateProjectileFixture() => RepositoryInstallation.CreateProjectileSystem();

    private static SamusBombProjectileSystem CreateBombFixture() => RepositoryInstallation.CreateBombSystem();
    private static SamusState PrepareRetailSamusFixture(SamusState samus) => RepositoryInstallation.BindSamus(samus);
    // Tests using retail gameplay must supply the host's installed presentation
    // contract explicitly. Each fixture retains its own bus and mutable runtime.
    private static SuperMetroidRuntime CreateRetailRuntimeFixture(
        ISnesAddressSpace addressSpace,
        bool playerInvincibilityEnabled = false,
        bool infiniteAmmoEnabled = false,
        MapRevealMode mapRevealMode = MapRevealMode.None,
        bool preventEscapeTimeout = false) =>
        RepositoryInstallation.CreateRuntime(addressSpace, playerInvincibilityEnabled,
            infiniteAmmoEnabled, mapRevealMode, preventEscapeTimeout);

    /// <summary>Binds the shared installation's runtime catalogs to a restored or bare runtime.</summary>
    private static void BindRetailRuntimeFixture(SuperMetroidRuntime runtime) =>
        RepositoryInstallation.BindRuntime(runtime);


    /// <summary>The shared installation's stock enemy artwork, loaded once per process.</summary>
    private static EnemyTileArtworkCatalog FixtureEnemyTileArtwork() => RepositoryInstallation.EnemyTiles;

    /// <summary>
    /// Loads a room's asset graph with the shared installation's stock catalogs, replacing only the
    /// catalogs a case supplies. The stock catalogs are the cartridge reference extracted from the ROM.
    /// </summary>
    private static CartridgeRoomAssets LoadFixtureRoomAssets(ISnesAddressSpace bus, CartridgeRoomHeader room,
        RoomCharacterAtlasCatalog? characterArt = null, RoomStaticPaletteCatalog? paletteArt = null,
        RoomMetatileCatalog? metatileArt = null, RoomVisualLayoutCatalog? visualLayouts = null) =>
        CartridgeRoomAssets.Load(bus, room, RepositoryInstallation.RoomAssets with
        {
            Characters = characterArt ?? RepositoryInstallation.RoomAssets.Characters,
            Palettes = paletteArt ?? RepositoryInstallation.RoomAssets.Palettes,
            Metatiles = metatileArt ?? RepositoryInstallation.RoomAssets.Metatiles,
            VisualLayouts = visualLayouts ?? RepositoryInstallation.RoomAssets.VisualLayouts,
        });
}
