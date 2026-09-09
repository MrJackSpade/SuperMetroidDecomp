using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyStatueSplash()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = CartridgeRoomHeader.Load(bus, 0xa66a);
        var assets = CartridgeRoomAssets.Load(bus, room);
        int splashes = 0;
        for (ushort random = 0; random < 64; random++)
        {
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
                new SnesVram(), new SnesCgram(), () => random);
            foreach (var actor in enemies.EnemyProjectiles) actor.Kind = 0;
            enemies.TourianStatueWaterY = 216;
            enemies.SpawnTourianUnlockEffect(0, false);
            var eye = enemies.EnemyProjectiles.Single(p => p.IsActive);
            RoomEnemyProjectileSlot? particle = null;
            for (int frame = 0; frame < 200 && particle is null; frame++)
            {
                enemies.StepEnemyProjectiles(assets.LevelData, null);
                particle = enemies.EnemyProjectiles.FirstOrDefault(p => p.IsActive && (ushort)p.Kind == TourianStatueRomData.Particle);
            }
            AssertTrue(particle is not null, "retail eye instruction spawned a particle");
            int angle = unchecked((byte)(random - 32));
            ushort nativeX = RomDataReader.ReadWordFixedBank(bus, 0xa0b3c3 + (angle + 64) * 2);
            AssertEqual(nativeX, particle!.XVelocity, $"native launch velocity for random {random}");
            ushort nativeY = unchecked((ushort)(4 * RomDataReader.ReadWordFixedBank(bus, 0xa0b3c3 + angle * 2) + 16));
            AssertEqual(nativeY, particle.YVelocity, $"native launch gravity for random {random}");
            // Children run later in the same descending pool pass, so seed the
            // subsequent integration from the already-advanced production position.
            int x = (particle.XPosition << 16) | particle.XSubposition;
            int y = (particle.YPosition << 16) | particle.YSubposition;
            short vy = unchecked((short)particle.YVelocity);
            bool crossed = false;
            for (int frame = 0; frame < 200 && particle.IsActive; frame++)
            {
                int previousY = (y >> 16) & 0xffff;
                x = unchecked(x + (unchecked((short)nativeX) << 8));
                y = unchecked(y + (vy << 8));
                bool crosses = (((216 - previousY) ^ (216 - ((y >> 16) & 0xffff))) & 0x8000) != 0;
                enemies.StepEnemyProjectiles(assets.LevelData, null);
                if (crosses)
                {
                    var splash = enemies.EnemyProjectiles.FirstOrDefault(p => p.IsActive && (ushort)p.Kind == TourianStatueRomData.Splash);
                    AssertTrue(splash is not null, "surface crossing spawned splash");
                    AssertEqual(unchecked((ushort)(x >> 16)), splash!.XPosition, "splash X follows native particle trajectory");
                    AssertEqual(212, splash.YPosition, "splash is four pixels above water surface");
                    // Isolate the spawned actor after the real crossing so every
                    // emitted sprite can be compared against its native screen origin.
                    foreach (var other in enemies.EnemyProjectiles)
                        if (other != splash) other.Kind = 0;
                    int visibleFrames = 0;
                    ushort splashX = unchecked((ushort)(x >> 16));
                    for (int age = 0; age < 64 && splash.IsActive; age++)
                    {
                        var expectedOam = new OamBuffer();
                        expectedOam.BeginFrame();
                        expectedOam.AddEnemyProjectileSpritemap(bus, splash.SpritemapPointer,
                            unchecked((ushort)(splashX - 32)), 164, splash.GraphicsIndex, true);
                        expectedOam.FinalizeFrame();
                        var actualOam = new OamBuffer();
                        actualOam.BeginFrame();
                        enemies.DrawEnemyProjectiles(actualOam, 32, 48);
                        actualOam.FinalizeFrame();
                        AssertTrue(actualOam.LowTable.SequenceEqual(expectedOam.LowTable) &&
                            actualOam.HighTable.SequenceEqual(expectedOam.HighTable), "splash OAM stays attached to native surface after camera subtraction");
                        if (actualOam.LastFinalizedSpriteCount != 0) visibleFrames++;
                        enemies.StepEnemyProjectiles(assets.LevelData, null);
                    }
                    AssertTrue(visibleFrames > 0 && !splash.IsActive, "splash displays and expires through retail animation");
                    crossed = true;
                    splashes++;
                    break;
                }
                vy = unchecked((short)(vy + 16));
            }
            AssertTrue(crossed, $"particle {random} reaches the water");
        }
        Console.WriteLine($"Statue particles: all 64 launch angles and {splashes} splash crossings passed.");
    }
}
