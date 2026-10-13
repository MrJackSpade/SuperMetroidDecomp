using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;
using static ImportedSpritemapsVerification;
using static RetailRuntimeFixtureVerification;
using static VerificationHelpersVerification;

internal static class ProjectileQuakeVerification
{
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the $86:846B earthquake displacement table")]
    internal static void VerifyProjectileQuake()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xa66a);
        var enemies = runtime.Enemies;
        enemies.StepEnemyProjectiles(runtime.Room.LevelData!, runtime.Player.Samus!);
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
            enemies.Earthquake.EarthquakeTimer = NativeCountdown.FromRaw(timer);
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
            // Independent native $86:83D6 origin culling and spritemap dispatch.
            if (((x + 128) & 0xfe00) == 0 && ((y + 128) & 0xfe00) == 0)
                DrawImportedEnemyProjectileSpritemap(bus, expected, nativeSpritemap, new(x, y),
                    actor.GraphicsIndex, originYIsOnScreen: (SnesLittleEndian.HighByte(y)) == 0);
            expected.FinalizeFrame();
            var actual = new OamBuffer();
            actual.BeginFrame();
            if (priority == EnemyProjectileDrawPriority.High)
                enemies.Projectiles.DrawHighPriorityEnemyProjectiles(actual, new(0, 0), frozen);
            else enemies.Projectiles.DrawLowPriorityEnemyProjectiles(actual, new(0, 0), frozen);
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

    [VerificationCommand("--projectile-quake")]
    internal static int ProjectileQuakeCommand()
    {
        Suite(nameof(VerifyProjectileQuake), () => VerifyProjectileQuake());
        return 0;
    }
}
