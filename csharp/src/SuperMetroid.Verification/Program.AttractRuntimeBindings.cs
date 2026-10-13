using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static EnemyContentIdentityFixtureVerification;

/// <summary>Checks installed artwork, palette, random state, and cheat-option ownership in attract and selected play.</summary>
internal static class AttractRuntimeBindingsVerification
{
    /// <summary>Reproduces missing-palette construction failure and verifies cartridge-free attract runtime bindings.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyAttractRuntimeBindings()
    {
        var palettes = GameplayBasePaletteCatalog.Load(new MemoryStream(GameplayBasePaletteCatalog.Write(
            new GameplayBasePaletteDocument(GameplayBasePaletteFormat.Version,
                Enumerable.Range(0, SnesCgram.ColorCount).Select(index => new PaletteRgb5
                    { Red = index % 32, Green = (index / 8) % 32, Blue = (index / 4) % 32 }).ToArray(),
                Enumerable.Range(0, GameplayBasePaletteFormat.SpriteColorCount)
                    .Select(_ => new PaletteRgb5 { Red = 1, Green = 2, Blue = 3 }).ToArray()))));
        var enemyArtwork = new EnemyIdentityFixture().Build();
        var objects = enemyArtwork.KraidBackground!.RoomBackgroundTiles;
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        // Exact cause of #1171: the previous attract path constructed this owner
        // without passing its already-installed palette catalog.
        AssertThrows<InvalidOperationException>(() => _ = new SuperMetroidRuntime(memory, HostGameplayOverrides.None),
            "omitting installed palettes reproduces the attract runtime construction failure");
        var game = new SuperMetroidGame(memory, new SuperMetroidGameOptions
            { Invincibility = true, InfiniteAmmo = true, PreventEscapeTimeout = true });
        game.BindArtwork(game.Artwork with
        {
            GameplayBasePalettes = palettes,
            EnemyTileArtwork = enemyArtwork,
            StandardObjectArt = objects,
        });
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var create = new Action<bool>(game.CreateGameplayRuntime);
        ushort incomingRandom = game.DispatcherRandomNumber;
        create(true);
        var demo = game.runtime;
        AssertTrue(demo.Cgram.Colors.SequenceEqual(palettes.Initial), "attract runtime loads all installed initial colors");
        AssertTrue(ReferenceEquals(objects, demo.Artwork.StandardObjectArt), "attract runtime binds installed standard objects");
        AssertTrue(ReferenceEquals(enemyArtwork, demo.Enemies.TileArtwork), "attract runtime binds installed enemy artwork");
        AssertEqual(incomingRandom, demo.System.RandomNumber, "attract runtime preserves frontend random state");
        AssertTrue(!demo.Options.PlayerInvincibilityEnabled && !demo.Options.InfiniteAmmoEnabled && !demo.Options.PreventEscapeTimeout,
            "attract runtime ignores host cheats");
        create(false);
        var selected = game.runtime;
        AssertTrue(!ReferenceEquals(demo, selected), "selected play and attract playback own separate runtimes");
        AssertTrue(selected.Cgram.Colors.SequenceEqual(palettes.Initial), "selected runtime retains installed palette binding");
        AssertTrue(selected.Options.PlayerInvincibilityEnabled && selected.Options.InfiniteAmmoEnabled && selected.Options.PreventEscapeTimeout,
            "selected runtime retains host options");
        Console.WriteLine("Attract runtime bindings: missing-palette failure reproduced; shared construction loads installed colors/artwork without ROM, preserves RNG, and isolates demo options.");
    }

    /// <summary>Runs the focused attract runtime binding verification command.</summary>
    [VerificationCommand("--attract-runtime-bindings")]
    internal static int AttractRuntimeBindingsCommand()
    {
        Suite(nameof(VerifyAttractRuntimeBindings), () => VerifyAttractRuntimeBindings());
        return 0;
    }
}
