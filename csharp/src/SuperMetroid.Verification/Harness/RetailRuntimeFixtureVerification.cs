using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Verification.Harness;

/// <summary>Creates verification fixtures from the repository's installed retail asset catalogs.</summary>
internal static class RetailRuntimeFixtureVerification
{
    /// <summary>Creates a projectile system with the shared installed definitions.</summary>
    internal static SamusProjectileSystem CreateProjectileFixture() => InstalledFixtures.CreateProjectileSystem();

    /// <summary>Creates a bomb system with the shared installed definitions.</summary>
    internal static SamusBombProjectileSystem CreateBombFixture() => InstalledFixtures.CreateBombSystem();
    /// <summary>Binds installed presentation data to a Samus state used in retail gameplay verification.</summary>
    internal static SamusState PrepareRetailSamusFixture(SamusState samus) => InstalledFixtures.BindSamus(samus);
    /// <summary>Creates an isolated retail runtime with the host's installed presentation contract and requested gameplay options.</summary>
    /// <remarks>Each fixture retains its own bus and mutable runtime.</remarks>
    internal static SuperMetroidRuntime CreateRetailRuntimeFixture(
        bool playerInvincibilityEnabled = false,
        bool infiniteAmmoEnabled = false,
        MapRevealMode mapRevealMode = MapRevealMode.None,
        bool preventEscapeTimeout = false,
        SuperMetroid.Core.Progression.SharedRandom? random = null) =>
        RepositoryInstallation.CreateRuntime(playerInvincibilityEnabled,
            infiniteAmmoEnabled, mapRevealMode, preventEscapeTimeout, random);

    /// <summary>Binds the shared installation's runtime catalogs to a restored or bare runtime.</summary>
    internal static void BindRetailRuntimeFixture(SuperMetroidRuntime runtime) =>
        RepositoryInstallation.BindRuntime(runtime);

    /// <summary>The shared installation's stock enemy artwork, loaded once per process.</summary>
    internal static EnemyTileSourceArtwork FixtureEnemyTileArtwork() => InstalledFixtures.EnemyTiles;

    /// <summary>The installed stock room character catalog, which the installed block sheets were composed from (#1466).</summary>
    internal static RoomCharacterAtlases FixtureRoomCharacters => InstalledFixtures.RoomCharacters;

    /// <summary>
    /// Loads a room's asset graph with the shared installation's stock catalogs, replacing only the
    /// catalogs a case supplies. The stock catalogs are the cartridge reference extracted from the ROM.
    /// </summary>
    internal static LoadedRoomAssets LoadFixtureRoomAssets(SelectedRoom room,
        RoomStaticPalettes? paletteArt = null, RoomLevelArtworks? visualLayouts = null,
        InstalledRoomLevelStreams? levelStreams = null) =>
        LoadedRoomAssets.Load(new RoomScrollStorage(), room, InstalledFixtures.RoomAssets with
        {
            Palettes = paletteArt ?? InstalledFixtures.RoomAssets.Palettes,
            VisualLayouts = visualLayouts ?? InstalledFixtures.RoomAssets.VisualLayouts,
            LevelStreams = levelStreams ?? InstalledFixtures.RoomAssets.LevelStreams,
        });
}
