using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Enemies.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks enemy-projectile drawing under room earthquake displacement.</summary>
internal static class ProjectileQuakeVerification
{
    /// <summary>Compares native OAM across quake types, timers, freeze state, priority, and screen edges.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyProjectileQuake()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Statues);
        var enemies = runtime.Enemies;
        enemies.StepEnemyProjectiles(runtime.Room.LoadedLevelData, runtime.Player.Samus);
        var actor = enemies.Projectiles.EnemyProjectiles.First(p => p.Kind == RoomEnemyProjectileKind.TourianStatueRidley);
        foreach (var other in enemies.Projectiles.EnemyProjectiles)
            if (other != actor) other.Kind = 0;
        ushort nativeSpritemap = NativeEnemyProjectileSpritemap(bus, actor);
        int cases = 0;
        foreach (ushort type in Enumerable.Range(0, 37).Select(x => (ushort)x))
        foreach (ushort timer in new ushort[] { 0, 1, 2, 3, 4 })
        foreach (bool frozen in new[] { false, true })
        foreach (var priority in new[] { EnemyProjectileDrawPriority.High, EnemyProjectileDrawPriority.Low })
        foreach (short position in new short[] { -129, -128, -1, 0, 128, 255, 383, 384 })
        {
            actor.DrawPriority = priority;
            actor.Position = actor.Position.WithWhole(new(unchecked((ushort)position), unchecked((ushort)position)));
            enemies.Earthquake.EarthquakeType = type;
            enemies.Earthquake.EarthquakeTimer = WrappingCountdown.FromRaw(timer);
            short dx = 0, dy = 0;
            if (timer != 0 && type < 36 && !frozen)
            {
                dx = unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x86846b + type * 4));
                dy = unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x86846d + type * 4));
                if ((timer & 2) != 0) { dx = unchecked((short)-dx); dy = unchecked((short)-dy); }
            }
            ushort x = unchecked((ushort)(position + dx)), y = unchecked((ushort)(position + dy));
            var expected = new OamBuffer();
            expected.BeginFrame();
            // Independent native $86:83D6 origin culling and spritemap dispatch. The sprite list places every part at its
            // signed position and leaves clipping at the screen edges to the backend; native OAM instead parks a part that
            // crosses the top or bottom edge at Y=$F0, so the oracle places the native parts without that clip.
            if (((x + 128) & 0xfe00) == 0 && ((y + 128) & 0xfe00) == 0)
                ImportedSpritemapOracle.DrawEnemy(CartridgeImportSource.Require(bus), expected, new NativeEnemySpritemapDraw
                {
                    Bank = 0x8d, Pointer = nativeSpritemap, Origin = new(x, y),
                    PaletteBits = SuperMetroid.Core.Assets.NativeSpriteGraphicsWords.PaletteBits(actor.Graphics.PaletteRow),
                    BaseTileIndex = unchecked((byte)actor.Graphics.TileBase), ClipVerticalWrap = false,
                    OriginYIsOnScreen = (SnesLittleEndian.HighByte(y)) == 0,
                });
            expected.FinalizeFrame();
            var actual = new OamBuffer();
            actual.BeginFrame();
            if (priority == EnemyProjectileDrawPriority.High)
                enemies.Projectiles.DrawHighPriorityEnemyProjectilesAsOam(actual, new(0, 0), frozen);
            else enemies.Projectiles.DrawLowPriorityEnemyProjectilesAsOam(actual, new(0, 0), frozen);
            actual.FinalizeFrame();
            string quakeCase = $"type={type}, timer={timer}, priority={priority}, origin={position}";
            AssertSameBytes(expected.LowTable, actual.LowTable, $"native quake OAM low table: {quakeCase}");
            AssertSameBytes(expected.HighTable, actual.HighTable, $"native quake OAM high table: {quakeCase}");
            AssertEqual(timer, enemies.Earthquake.EarthquakeTimer.RawValue, "drawing leaves quake timer untouched");
            AssertEqual(unchecked((ushort)position), actor.Position.X.Whole, "drawing preserves world X");
            AssertEqual(unchecked((ushort)position), actor.Position.Y.Whole, "drawing preserves world Y");
            cases++;
        }
        Console.WriteLine($"Projectile quake: {cases} native OAM/edge/timer cases passed.");
    }

    /// <summary>Runs the focused projectile quake OAM check.</summary>
    [VerificationCommand("--projectile-quake")]
    internal static int ProjectileQuakeCommand()
    {
        Suite(nameof(VerifyProjectileQuake), () => VerifyProjectileQuake());
        return 0;
    }
}
