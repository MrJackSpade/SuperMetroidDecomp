using SuperMetroid.Verification.Bosses.Botwoon;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Verification.Fx;

/// <summary>
/// Checks every earthquake shake's displacement against the cartridge's shake vectors, and the room earthquake's
/// alternating background and projectile offsets, its frozen and stopped frames, its sustain and its continuous shake.
/// </summary>
internal static class RoomShakeDefinitionsVerification
{
    /// <summary>Compares all shown shakes, frozen behavior, sustained and continuous shaking.</summary>
    internal static void VerifyRoomShakeDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyRoomShakeBg1Algorithm), () => VerifyRoomShakeBg1Algorithm(rom));
        Suite(nameof(VerifyRoomShakeBg2Algorithm), () => VerifyRoomShakeBg2Algorithm(rom));
        Suite(nameof(VerifyRoomShakeProjectileAlgorithm), () => VerifyRoomShakeProjectileAlgorithm(rom));

        var guarded = NewRoomShakeReadGuard(rom);
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);

        for (int type = 0; type < NativeEarthquakeTypes.ShownTypeCount; type++)
        {
            EarthquakeShake shake = NativeEarthquakeTypes.Shake(type);
            RoomShakeDefinition definition = shake.Displacement;
            enemies.Earthquake.Start(shake, 1);
            RoomShakeFrameResult positive = enemies.HandleRoomShaking(timeIsFrozen: false);
            AssertEqual(new RoomShakeFrameResult(definition.Foreground, definition.Background),
                positive,
                $"room shake {shake} positive production frame");
            AssertTrue(!enemies.Earthquake.IsShaking, $"room shake {shake} positive frame consumes its last frame");

            enemies.Earthquake.Start(shake, 2);
            RoomShakeFrameResult negative = enemies.HandleRoomShaking(timeIsFrozen: false);
            AssertEqual(new RoomShakeFrameResult(
                    new(unchecked((short)-definition.Foreground.X), unchecked((short)-definition.Foreground.Y)),
                    new(unchecked((short)-definition.Background.X), unchecked((short)-definition.Background.Y))),
                negative,
                $"room shake {shake} alternating production frame");

            enemies.Earthquake.Start(shake, 1);
            var positiveProjectile = enemies.Earthquake.GetEnemyProjectileShake(false);
            AssertEqual(definition.Projectile, positiveProjectile,
                $"room shake {shake} positive projectile displacement");

            enemies.Earthquake.Start(shake, 2);
            var negativeProjectile = enemies.Earthquake.GetEnemyProjectileShake(false);
            AssertEqual(-definition.Projectile,
                negativeProjectile,
                $"room shake {shake} alternating projectile displacement");
            AssertEqual(type >= 18, shake.ShakesEnemies, $"room shake {shake} shakes the enemies exactly from type 18");
        }

        EarthquakeShake light = NativeEarthquakeTypes.Shake(0);
        enemies.Earthquake.Start(light, 4);
        AssertEqual(default(RoomShakeFrameResult), enemies.HandleRoomShaking(timeIsFrozen: true),
            "frozen room shake does not displace the layers");
        AssertEqual(4, enemies.Earthquake.RemainingFrames, "frozen room shake keeps the earthquake's length");
        AssertEqual(Offset16<RoomPixels>.Zero,
            enemies.Earthquake.GetEnemyProjectileShake(true),
            "frozen projectile shake does not displace projectiles");

        enemies.Earthquake.Stop();
        AssertEqual(default(RoomShakeFrameResult), enemies.HandleRoomShaking(timeIsFrozen: false),
            "a stopped earthquake does not displace the layers");
        AssertEqual<EarthquakeShake?>(light, enemies.Earthquake.Shake, "a stopped earthquake keeps its shake for a producer that runs it on");

        // A sustain moves a countdown in the first half of its 64-frame round on by 32 and keeps one in the later half.
        enemies.Earthquake.Start(light, 1);
        enemies.Earthquake.Sustain(RoomEarthquakeDefinitions.RisingLiquidShake);
        AssertEqual(33, enemies.Earthquake.RemainingFrames, "a sustain keeps the running earthquake's frames and adds its 32");
        AssertEqual<EarthquakeShake?>(RoomEarthquakeDefinitions.RisingLiquidShake, enemies.Earthquake.Shake, "a sustain gives the room its shake");
        enemies.Earthquake.Sustain(RoomEarthquakeDefinitions.RisingLiquidShake);
        AssertEqual(33, enemies.Earthquake.RemainingFrames, "a sustain leaves an earthquake already in its sustained half");

        // A continuous shake never stops and keeps alternating its sides through the countdown's wrap.
        enemies.Earthquake.Start(light, 3);
        enemies.Earthquake.KeepShaking();
        RoomShakeDefinition lightDefinition = light.Displacement;
        for (int frame = 0; frame < 12; frame++)
        {
            RoomShakeFrameResult continuous = enemies.HandleRoomShaking(timeIsFrozen: false);
            // Frames left 3, 2, 1, 0, then the wrapped 65535, 65534...: the far side shows while the frames left, counted
            // in pairs, are odd.
            int framesLeft = (3 - frame) & ushort.MaxValue;
            bool farSide = framesLeft / RoomEarthquakeDefinitions.StepsPerShakeSide % 2 != 0;
            AssertEqual(farSide ? -lightDefinition.Foreground : lightDefinition.Foreground, continuous.Foreground,
                $"a continuous shake alternates its sides at frame {frame}");
            AssertTrue(enemies.Earthquake.IsShaking, $"a continuous shake keeps shaking at frame {frame}");
        }
        AssertThrows<InvalidOperationException>(() => _ = enemies.Earthquake.RemainingFrames,
            "a continuous shake has no frames left to read");
        enemies.Earthquake.Start(light, 30);
        AssertTrue(enemies.Earthquake.IsShakingWith(light, 30), "a producer's start ends the continuous shake");

        var unshaken = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // A room no producer has shaken runs on with the shake a new game starts with (#1594).
        unshaken.Earthquake.RunFor(4);
        AssertTrue(unshaken.Earthquake.IsShakingWith(RoomEarthquakeDefinitions.UnshakenRoomShake, 4),
            "an earthquake run on before any producer gives the room a shake shows the shake a new game starts with");

        Console.WriteLine(
            "Room shake definitions: all 216 cartridge words and 144 real background/projectile phase selections pass with both source tables forbidden.");
    }

    /// <summary>Compares BG1 displacement pairs with their native table entries.</summary>
    private static void VerifyRoomShakeBg1Algorithm(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyRoomShakeVector), () => VerifyRoomShakeVector(rom, NativeEarthquakeTypes.Bg1ReferenceAddress, 8,
            definition => new OctantVector(definition.Foreground.X, definition.Foreground.Y), "BG1"));

    /// <summary>Compares BG2 displacement pairs with their native table entries.</summary>
    private static void VerifyRoomShakeBg2Algorithm(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyRoomShakeVector), () => VerifyRoomShakeVector(rom, NativeEarthquakeTypes.Bg2ReferenceAddress, 8,
            definition => new OctantVector(definition.Background.X, definition.Background.Y), "BG2"));

    /// <summary>Compares enemy-projectile displacement pairs with their native table entries.</summary>
    private static void VerifyRoomShakeProjectileAlgorithm(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyRoomShakeVector), () => VerifyRoomShakeVector(rom, NativeEarthquakeTypes.ProjectileReferenceAddress, 4,
            definition => new OctantVector(definition.Projectile.X, definition.Projectile.Y), "projectile"));

    /// <summary>Checks one displacement of every shake against the native row its type number indexes.</summary>
    private static void VerifyRoomShakeVector(CartridgeImportAddressSpace rom, int source, int stride,
        Func<RoomShakeDefinition, OctantVector> select, string label)
    {
        short Word(int address) => unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        for (int type = 0; type < NativeEarthquakeTypes.ShownTypeCount; type++)
            AssertEqual(new OctantVector(Word(source + type * stride), Word(source + type * stride + 2)),
                select(NativeEarthquakeTypes.Shake(type).Displacement), $"Original shake {label} vector {type}");
    }

    /// <summary>Forbids reads of the migrated background and projectile shake tables during runtime checks.</summary>
    private static CartridgeReadGuard NewRoomShakeReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa0872d and < 0xa0884d or
                >= 0x86846b and < 0x8684fb,
            address => $"Room shaking attempted migrated displacement read ${address:X6}.");
}
