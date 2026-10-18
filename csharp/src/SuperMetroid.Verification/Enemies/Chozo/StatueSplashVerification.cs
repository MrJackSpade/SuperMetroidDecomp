using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Enemies.Chozo;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Tourian statue particle launch and water splash behavior against native data.</summary>
internal static class StatueSplashVerification
{
    /// <summary>Exercises all 64 launch values through projectile stepping, crossing, and splash OAM lifetime.</summary>
    internal static void VerifyStatueSplash()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0xa66a);
        var assets = LoadFixtureRoomAssets(room);
        int splashes = 0;
        for (ushort random = 0; random < 64; random++)
        {
            var enemies = new RoomEnemySystem { TileArtwork = FixtureEnemyTileArtwork() };
            enemies.Load(new RoomEnemyLoadRequest(CompiledRoomEnemyDefinitions.Instance, room.State.EnemyPopulation, room.State.EnemyGraphicsSet, new SnesVram(), new SnesCgram(), new StandaloneRoomEnemyServices(() => random)));
            foreach (var actor in enemies.Projectiles.EnemyProjectiles) actor.Kind = 0;
            enemies.TourianUnlockBehavior.TourianStatueWaterSurface = new(0, 216);
            enemies.TourianUnlockBehavior.SpawnTourianUnlockEffect(0, false);
            var eye = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
            RoomEnemyProjectileSlot? particle = null;
            for (int frame = 0; frame < 200 && particle is null; frame++)
            {
                enemies.StepEnemyProjectiles(assets.LevelData, null);
                particle = enemies.Projectiles.EnemyProjectiles.FirstOrDefault(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.TourianStatueParticle);
            }
            AssertTrue(particle is not null, "retail eye instruction spawned a particle");
            int angle = unchecked((byte)(random - 32));
            ushort nativeX = ReadNativeStatueParticleWord(bus, angle + 64);
            AssertEqual(nativeX, (particle ?? throw new InvalidOperationException("Verification requires particle.")).Velocity.X.RawValue, $"native launch velocity for random {random}");
            ushort nativeY = unchecked((ushort)(4 * ReadNativeStatueParticleWord(bus, angle) + 16));
            AssertEqual(nativeY, particle.Velocity.Y.RawValue, $"native launch gravity for random {random}");
            // Children run later in the same descending pool pass, so seed the
            // subsequent integration from the already-advanced production position.
            int x = (SnesSixteenSixteen.FromWhole(particle.Position.X.Whole)) | particle.Position.X.Fraction;
            int y = (SnesSixteenSixteen.FromWhole(particle.Position.Y.Whole)) | particle.Position.Y.Fraction;
            short vy = unchecked((short)particle.Velocity.Y.RawValue);
            bool crossed = false;
            for (int frame = 0; frame < 200 && particle.IsActive; frame++)
            {
                int previousY = unchecked((ushort)SnesSixteenSixteen.WholeOf(y));
                x = unchecked(x + SnesSixteenSixteen.FromEightEight(unchecked((short)nativeX)));
                y = unchecked(y + SnesSixteenSixteen.FromEightEight(vy));
                bool crosses = (((216 - previousY) ^ (216 - unchecked((ushort)SnesSixteenSixteen.WholeOf(y)))) & 0x8000) != 0;
                enemies.StepEnemyProjectiles(assets.LevelData, null);
                if (crosses)
                {
                    var splash = enemies.Projectiles.EnemyProjectiles.FirstOrDefault(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.TourianStatueSplash);
                    AssertTrue(splash is not null, "surface crossing spawned splash");
                    AssertEqual(SnesFixedPosition.WholeOf(x), (splash ?? throw new InvalidOperationException("Verification requires splash.")).Position.X.Whole, "splash X follows native particle trajectory");
                    AssertEqual(212, splash.Position.Y.Whole, "splash is four pixels above water surface");
                    // Isolate the spawned actor after the real crossing so every
                    // emitted sprite can be compared against its native screen origin.
                    foreach (var other in enemies.Projectiles.EnemyProjectiles)
                        if (other != splash) other.Kind = 0;
                    int visibleFrames = 0;
                    ushort splashX = SnesFixedPosition.WholeOf(x);
                    for (int age = 0; age < 64 && splash.IsActive; age++)
                    {
                        var expectedOam = new OamBuffer();
                        expectedOam.BeginFrame();
                        DrawImportedEnemyProjectileSpritemap(bus, expectedOam, NativeEnemyProjectileSpritemap(bus, splash),
                            new(unchecked((ushort)(splashX - 32)), 164), checked((ushort)splash.GraphicsIndex), true);
                        expectedOam.FinalizeFrame();
                        var actualOam = new OamBuffer();
                        actualOam.BeginFrame();
                        enemies.DrawEnemyProjectiles(actualOam, new(32, 48));
                        actualOam.FinalizeFrame();
                        AssertTrue(actualOam.ImageEquals(expectedOam), "splash OAM stays attached to native surface after camera subtraction");
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

    /// <summary>Reads one native launch-table word for the independent trajectory reference.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the $A0:B3C3 statue-particle launch table")]
    private static ushort ReadNativeStatueParticleWord(ISnesAddressSpace bus, int index) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0xa0b3c3 + index * 2);

    /// <summary>Runs the retail Tourian statue particle and splash check.</summary>
    [VerificationCommand("--statue-splash")]
    internal static int StatueSplashCommand()
    {
        Suite(nameof(VerifyStatueSplash), () => VerifyStatueSplash());
        return 0;
    }
}
