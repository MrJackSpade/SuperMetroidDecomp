using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Runtime.Attract;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Artwork;

/// <summary>Checks installed artwork, palette, random state, and cheat-option ownership in attract and selected play.</summary>
internal static class AttractRuntimeBindingsVerification
{
    /// <summary>Reproduces missing-palette construction failure and verifies cartridge-free attract runtime bindings.</summary>
    internal static void VerifyAttractRuntimeBindings()
    {
        var palettes = GameplayBasePaletteFiles.Read(new MemoryStream(GameplayBasePaletteFiles.Write(
            new GameplayBasePaletteDocument(GameplayBasePaletteFormat.Version,
                Enumerable.Range(0, SnesCgram.ColorCount).Select(index => new PaletteRgb5
                    { Red = index % 32, Green = (index / 8) % 32, Blue = (index / 4) % 32 }).ToArray(),
                Enumerable.Range(0, GameplayBasePaletteFormat.SpriteColorCount)
                    .Select(_ => new PaletteRgb5 { Red = 1, Green = 2, Blue = 3 }).ToArray()))));
        var enemyArtwork = new EnemyIdentityFixture().Build();
        var objects = (enemyArtwork.Bosses.KraidBackground ?? throw new InvalidOperationException("Verification requires enemyArtwork.KraidBackground.")).RoomBackgroundTiles;
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        // Exact cause of #1171: the previous attract path constructed this owner
        // without passing its already-installed palette catalog.
        AssertThrows<InvalidOperationException>(() => _ = SuperMetroidRuntime.Create(HostGameplayOverrides.None, InstalledFixtures.SamusOverreads, initialPaletteArt: null),
            "omitting installed palettes reproduces the attract runtime construction failure");
        var game = new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), new SuperMetroidGameOptions
            { Invincibility = true, InfiniteAmmo = true, PreventEscapeTimeout = true });
        game.BindArtwork(game.Artwork with
        {
            GameplayBasePalettes = palettes,
            EnemyTileArtwork = enemyArtwork.Enemies,
            EnemySheets = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EnemySheets, BossArtwork = enemyArtwork.Bosses, AreaArtwork = enemyArtwork.Areas,
            StandardObjectArt = objects,
        });
        int incomingRandom = game.DispatcherRandomNumber;
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.AttractDemo);
        var demo = game.GameplayRuntimeForVerification;
        AssertTrue(demo.Colours.Colors.SequenceEqual(palettes.Initial), "attract runtime loads all installed initial colors");
        AssertTrue(ReferenceEquals(enemyArtwork.Enemies, demo.EnemySession.TileArtwork), "attract runtime binds installed enemy artwork");
        AssertEqual(incomingRandom, demo.System.RandomNumber, "attract runtime preserves frontend random state");
        AssertTrue(!demo.Options.PlayerInvincibilityEnabled && !demo.Options.InfiniteAmmoEnabled && !demo.Options.PreventEscapeTimeout,
            "attract runtime ignores host cheats");
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var selected = game.GameplayRuntimeForVerification;
        AssertTrue(!ReferenceEquals(demo, selected), "selected play and attract playback own separate runtimes");
        AssertTrue(selected.Colours.Colors.SequenceEqual(palettes.Initial), "selected runtime retains installed palette binding");
        AssertTrue(selected.Options.PlayerInvincibilityEnabled && selected.Options.InfiniteAmmoEnabled && selected.Options.PreventEscapeTimeout,
            "selected runtime retains host options");

        // #1330: one random-number word per session. Each runtime reads and advances the session's instance, so a
        // release to the menu and a recreation keep the exact word with no copy.
        AssertTrue(ReferenceEquals(game.Session.Random, demo.System.Random) && ReferenceEquals(game.Session.Random, selected.System.Random),
            "every runtime shares the session's random-number word");
        selected.System.NextRandom();
        int beforeRelease = game.Session.Random.Read();
        game.Session.ReleaseRuntime();
        AssertEqual(beforeRelease, game.DispatcherRandomNumber, "release keeps the word gameplay advanced");
        int menuFrame = game.Session.Random.Generate();
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        AssertTrue(ReferenceEquals(game.Session.Random, game.GameplayRuntimeForVerification.System.Random),
            "a recreated runtime shares the session's word");
        AssertEqual(menuFrame, game.GameplayRuntimeForVerification.System.RandomNumber, "a recreated runtime starts from the menu's word");
        Console.WriteLine("Attract runtime bindings: missing-palette failure reproduced; shared construction loads installed colors/artwork without ROM, shares the session's RNG word across runtimes, and isolates demo options.");
    }

    /// <summary>Runs the focused attract runtime binding verification command.</summary>
    [VerificationCommand("--attract-runtime-bindings")]
    internal static int AttractRuntimeBindingsCommand()
    {
        Suite(nameof(VerifyAttractRuntimeBindings), () => VerifyAttractRuntimeBindings());
        return 0;
    }
}
