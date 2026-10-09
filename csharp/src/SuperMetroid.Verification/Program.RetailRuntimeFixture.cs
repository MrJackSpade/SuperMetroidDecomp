using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>Creates a projectile system backed by the repository installation's stock gameplay catalogs.</summary>
    private static SamusProjectileSystem CreateProjectileFixture() => RepositoryInstallation.CreateProjectileSystem();

    /// <summary>Creates a bomb projectile system using the repository installation's stock gameplay catalogs.</summary>
    private static SamusBombProjectileSystem CreateBombFixture() => RepositoryInstallation.CreateBombSystem();

    /// <summary>Binds a Samus state to the installed retail presentation and gameplay catalogs.</summary>
    /// <param name="samus">The state to prepare for use by retail gameplay systems.</param>
    /// <returns>The supplied state after the installation has bound its required catalogs.</returns>
    private static SamusState PrepareRetailSamusFixture(SamusState samus) => RepositoryInstallation.BindSamus(samus);
    // Tests using retail gameplay must supply the host's installed presentation
    // contract explicitly. Each fixture retains its own bus and mutable runtime.
    /// <summary>Creates a runtime that uses the shared retail installation with the supplied address space.</summary>
    /// <param name="addressSpace">The bus used by this runtime to read and write SNES memory.</param>
    /// <param name="playerInvincibilityEnabled">Whether the runtime enables the player's invincibility option.</param>
    /// <param name="infiniteAmmoEnabled">Whether the runtime enables unlimited ammunition.</param>
    /// <param name="mapRevealMode">The map-reveal option applied to the runtime.</param>
    /// <param name="preventEscapeTimeout">Whether the runtime prevents the escape timer from expiring.</param>
    /// <returns>A runtime with independent mutable gameplay state and the installed retail catalogs.</returns>
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
