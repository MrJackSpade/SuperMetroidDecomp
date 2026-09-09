using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyProjectileQuake()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xa66a);
        var enemies = runtime.Enemies;
        enemies.StepEnemyProjectiles(runtime.LevelData!, runtime.Samus!);
        var actor = enemies.EnemyProjectiles.First(p => p.Kind == RoomEnemyProjectileKind.TourianStatueRidley);
        foreach (var other in enemies.EnemyProjectiles)
            if (other != actor) other.Kind = 0;
        int cases = 0;
        foreach (ushort type in Enumerable.Range(0, 37).Select(x => (ushort)x))
        foreach (ushort timer in new ushort[] { 0, 1, 2, 3, 4 })
        foreach (bool frozen in new[] { false, true })
        foreach (var priority in new[] { EnemyProjectileDrawPriority.High, EnemyProjectileDrawPriority.Low })
        foreach (short position in new short[] { -129, -128, -1, 0, 128, 255, 383, 384 })
        {
            actor.DrawPriority = priority;
            actor.XPosition = actor.YPosition = unchecked((ushort)position);
            enemies.EarthquakeType = type;
            enemies.EarthquakeTimer = timer;
            short dx = 0, dy = 0;
            if (timer != 0 && type < 36 && !frozen)
            {
                dx = unchecked((short)RomDataReader.ReadWordFixedBank(bus, 0x86846b + type * 4));
                dy = unchecked((short)RomDataReader.ReadWordFixedBank(bus, 0x86846d + type * 4));
                if ((timer & 2) != 0) { dx = unchecked((short)-dx); dy = unchecked((short)-dy); }
            }
            ushort x = unchecked((ushort)(position + dx)), y = unchecked((ushort)(position + dy));
            var expected = new OamBuffer();
            expected.BeginFrame();
            // Independent native $86:83D6 origin culling and spritemap dispatch.
            if (((x + 128) & 0xfe00) == 0 && ((y + 128) & 0xfe00) == 0)
                expected.AddEnemyProjectileSpritemap(bus, actor.SpritemapPointer, x, y,
                    actor.GraphicsIndex, originYIsOnScreen: (y >> 8) == 0);
            expected.FinalizeFrame();
            var actual = new OamBuffer();
            actual.BeginFrame();
            if (priority == EnemyProjectileDrawPriority.High)
                enemies.DrawHighPriorityEnemyProjectiles(actual, 0, 0, frozen);
            else enemies.DrawLowPriorityEnemyProjectiles(actual, 0, 0, frozen);
            actual.FinalizeFrame();
            AssertTrue(actual.LowTable.SequenceEqual(expected.LowTable) && actual.HighTable.SequenceEqual(expected.HighTable),
                $"native quake OAM: type={type}, timer={timer}, priority={priority}, origin={position}");
            AssertEqual(timer, enemies.EarthquakeTimer, "drawing leaves quake timer untouched");
            AssertEqual(unchecked((ushort)position), actor.XPosition, "drawing preserves world X");
            AssertEqual(unchecked((ushort)position), actor.YPosition, "drawing preserves world Y");
            cases++;
        }
        Console.WriteLine($"Projectile quake: {cases} native OAM/edge/timer cases passed.");
    }
}
