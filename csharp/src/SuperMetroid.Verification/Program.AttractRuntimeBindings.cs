using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyAttractRuntimeBindings()
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
        AssertThrows<InvalidOperationException>(() => _ = new SuperMetroidRuntime(memory),
            "omitting installed palettes reproduces the attract runtime construction failure");
        var game = new SuperMetroidGame(memory, new SuperMetroidGameOptions
            { Invincibility = true, InfiniteAmmo = true, PreventEscapeTimeout = true });
        game.BindGameplayBasePalettes(palettes);
        game.BindEnemyTileArtwork(enemyArtwork);
        game.BindStandardObjectArt(objects);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var create = typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime", flags)!
            .CreateDelegate<Action<bool>>(game);
        var ownerField = typeof(SuperMetroidGame).GetField("runtime", flags)!;
        ushort incomingRandom = game.DispatcherRandomNumber;
        create(true);
        var demo = (SuperMetroidRuntime)ownerField.GetValue(game)!;
        AssertTrue(demo.Cgram.Colors.SequenceEqual(palettes.Initial), "attract runtime loads all installed initial colors");
        AssertTrue(ReferenceEquals(objects, demo.StandardObjectArt), "attract runtime binds installed standard objects");
        AssertTrue(ReferenceEquals(enemyArtwork, demo.Enemies.TileArtwork), "attract runtime binds installed enemy artwork");
        AssertEqual(incomingRandom, demo.System.RandomNumber, "attract runtime preserves frontend random state");
        AssertTrue(!demo.PlayerInvincibilityEnabled && !demo.InfiniteAmmoEnabled && !demo.PreventEscapeTimeout,
            "attract runtime ignores host cheats");
        create(false);
        var selected = (SuperMetroidRuntime)ownerField.GetValue(game)!;
        AssertTrue(!ReferenceEquals(demo, selected), "selected play and attract playback own separate runtimes");
        AssertTrue(selected.Cgram.Colors.SequenceEqual(palettes.Initial), "selected runtime retains installed palette binding");
        AssertTrue(selected.PlayerInvincibilityEnabled && selected.InfiniteAmmoEnabled && selected.PreventEscapeTimeout,
            "selected runtime retains host options");
        Console.WriteLine("Attract runtime bindings: missing-palette failure reproduced; shared construction loads installed colors/artwork without ROM, preserves RNG, and isolates demo options.");
    }
}
