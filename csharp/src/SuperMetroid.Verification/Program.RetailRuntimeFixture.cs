using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Represents retail runtime fixture verification.</summary>
internal static class RetailRuntimeFixtureVerification
{
    /// <summary>Creates projectile fixture.</summary>
    internal static SamusProjectileSystem CreateProjectileFixture() => InstalledFixtures.CreateProjectileSystem();

    /// <summary>Creates bomb fixture.</summary>
    internal static SamusBombProjectileSystem CreateBombFixture() => InstalledFixtures.CreateBombSystem();
    /// <summary>Prepare retail samus fixture.</summary>
    internal static SamusState PrepareRetailSamusFixture(SamusState samus) => InstalledFixtures.BindSamus(samus);
    // Tests using retail gameplay must supply the host's installed presentation
    // contract explicitly. Each fixture retains its own bus and mutable runtime.
    /// <summary>Creates retail runtime fixture.</summary>
    internal static SuperMetroidRuntime CreateRetailRuntimeFixture(
        ISnesAddressSpace addressSpace,
        bool playerInvincibilityEnabled = false,
        bool infiniteAmmoEnabled = false,
        MapRevealMode mapRevealMode = MapRevealMode.None,
        bool preventEscapeTimeout = false) =>
        RepositoryInstallation.CreateRuntime(addressSpace, playerInvincibilityEnabled,
            infiniteAmmoEnabled, mapRevealMode, preventEscapeTimeout);

    /// <summary>Binds the shared installation's runtime catalogs to a restored or bare runtime.</summary>
    internal static void BindRetailRuntimeFixture(SuperMetroidRuntime runtime) =>
        RepositoryInstallation.BindRuntime(runtime);


    /// <summary>The shared installation's stock enemy artwork, loaded once per process.</summary>
    internal static EnemyTileArtworkCatalog FixtureEnemyTileArtwork() => InstalledFixtures.EnemyTiles;

    /// <summary>
    /// Loads a room's asset graph with the shared installation's stock catalogs, replacing only the
    /// catalogs a case supplies. The stock catalogs are the cartridge reference extracted from the ROM.
    /// </summary>
    internal static CartridgeRoomAssets LoadFixtureRoomAssets(ISnesAddressSpace bus, CartridgeRoomHeader room,
        RoomCharacterAtlasCatalog? characterArt = null, RoomStaticPaletteCatalog? paletteArt = null,
        RoomMetatileCatalog? metatileArt = null, RoomVisualLayoutCatalog? visualLayouts = null) =>
        CartridgeRoomAssets.Load(bus, room, InstalledFixtures.RoomAssets with
        {
            Characters = characterArt ?? InstalledFixtures.RoomAssets.Characters,
            Palettes = paletteArt ?? InstalledFixtures.RoomAssets.Palettes,
            Metatiles = metatileArt ?? InstalledFixtures.RoomAssets.Metatiles,
            VisualLayouts = visualLayouts ?? InstalledFixtures.RoomAssets.VisualLayouts,
        });
}
