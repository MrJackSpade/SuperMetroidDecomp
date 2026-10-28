using SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks compiled room-shake vectors and the runtime's alternating background and projectile offsets.</summary>
internal static class RoomShakeDefinitionsVerification
{
    /// <summary>Compares all rendered shake types, frozen behavior, and non-rendered timer preservation.</summary>
    internal static void VerifyRoomShakeDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyRoomShakeBg1Algorithm), () => VerifyRoomShakeBg1Algorithm(rom));
        Suite(nameof(VerifyRoomShakeBg2Algorithm), () => VerifyRoomShakeBg2Algorithm(rom));
        Suite(nameof(VerifyRoomShakeProjectileAlgorithm), () => VerifyRoomShakeProjectileAlgorithm(rom));
        const ushort renderedTypeCount = 36;

        var guarded = NewRoomShakeReadGuard(rom);
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);

        for (ushort type = 0; type < renderedTypeCount; type++)
        {
            RoomShakeDefinition definition = RoomShakeDefinitions.ForType(type);
            enemies.Earthquake.EarthquakeType = type;
            enemies.Earthquake.EarthquakeTimer = WrappingCountdown.Start(1);
            RoomShakeFrameResult positive = enemies.HandleRoomShaking(timeIsFrozen: false);
            AssertEqual(new RoomShakeFrameResult(definition.Bg1, definition.Bg2),
                positive,
                $"room shake type {type} positive production frame");
            AssertEqual((ushort)0, enemies.Earthquake.EarthquakeTimer.RawValue,
                $"room shake type {type} positive timer consumption");

            enemies.Earthquake.EarthquakeTimer = WrappingCountdown.Start(2);
            RoomShakeFrameResult negative = enemies.HandleRoomShaking(timeIsFrozen: false);
            AssertEqual(new RoomShakeFrameResult(
                    new(unchecked((short)-definition.Bg1.X), unchecked((short)-definition.Bg1.Y)),
                    new(unchecked((short)-definition.Bg2.X), unchecked((short)-definition.Bg2.Y))),
                negative,
                $"room shake type {type} alternating production frame");

            enemies.Earthquake.EarthquakeTimer = WrappingCountdown.Start(1);
            var positiveProjectile = enemies.Earthquake.GetEnemyProjectileShake(false);
            AssertEqual(definition.Projectile, positiveProjectile,
                $"room shake type {type} positive projectile displacement");

            enemies.Earthquake.EarthquakeTimer = WrappingCountdown.Start(2);
            var negativeProjectile = enemies.Earthquake.GetEnemyProjectileShake(false);
            AssertEqual(-definition.Projectile,
                negativeProjectile,
                $"room shake type {type} alternating projectile displacement");
        }

        AssertThrows<InvalidDataException>(
            () => RoomShakeDefinitions.ForType(renderedTypeCount),
            "room shake type outside rendered table");

        enemies.Earthquake.EarthquakeType = RoomEarthquakeDefinitions.FirstNonRenderedType;
        enemies.Earthquake.EarthquakeTimer = WrappingCountdown.Start(4);
        AssertEqual(default(RoomShakeFrameResult), enemies.HandleRoomShaking(timeIsFrozen: false),
            "non-rendered earthquake type does not access compiled definitions");
        AssertEqual((ushort)4, enemies.Earthquake.EarthquakeTimer.RawValue,
            "non-rendered earthquake type preserves timer");
        AssertEqual(Offset16<RoomPixels>.Zero,
            enemies.Earthquake.GetEnemyProjectileShake(false),
            "non-rendered earthquake type has no projectile displacement");

        // The type holds the cartridge's whole word as a non-negative number: its largest value is still an
        // ordinary non-rendered type, and anything outside the word is rejected instead of wrapping.
        enemies.Earthquake.EarthquakeType = ushort.MaxValue;
        enemies.Earthquake.EarthquakeTimer = WrappingCountdown.Start(4);
        AssertEqual(default(RoomShakeFrameResult), enemies.HandleRoomShaking(timeIsFrozen: false),
            "largest earthquake type is non-rendered");
        AssertEqual((ushort)4, enemies.Earthquake.EarthquakeTimer.RawValue,
            "largest earthquake type preserves timer");
        AssertThrows<ArgumentOutOfRangeException>(() => { enemies.Earthquake.EarthquakeType = ushort.MaxValue + 1; },
            "earthquake type above the cartridge word is rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => { enemies.Earthquake.EarthquakeType = -1; },
            "negative earthquake type is rejected");

        // Timer bits are OR'd into the running timer, keeping its existing bits.
        enemies.Earthquake.EarthquakeTimer = WrappingCountdown.Start(1);
        enemies.Earthquake.AddTimerBits(RoomEarthquakeDefinitions.RisingLiquidTimerBits);
        AssertEqual((ushort)(1 | RoomEarthquakeDefinitions.RisingLiquidTimerBits), enemies.Earthquake.EarthquakeTimer.RawValue,
            "earthquake timer bits keep the running timer's bits");
        AssertThrows<ArgumentOutOfRangeException>(() => enemies.Earthquake.AddTimerBits(ushort.MaxValue + 1),
            "earthquake timer bits above the cartridge word are rejected");

        enemies.Earthquake.EarthquakeType = 0;
        AssertEqual(default(RoomShakeFrameResult), enemies.HandleRoomShaking(timeIsFrozen: true),
            "frozen room shake does not access compiled definitions");
        AssertEqual(Offset16<RoomPixels>.Zero,
            enemies.Earthquake.GetEnemyProjectileShake(true),
            "frozen projectile shake does not access compiled definitions");

        Console.WriteLine(
            "Room shake definitions: all 216 native words and 144 real background/projectile phase selections pass with both source tables forbidden.");
    }

    /// <summary>Compares BG1 displacement pairs with their native table entries.</summary>
    private static void VerifyRoomShakeBg1Algorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyRoomShakeVector), () => VerifyRoomShakeVector(rom, RoomShakeDefinitions.Bg1ReferenceAddress, 8,
            definition => new OctantVector(definition.Bg1.X, definition.Bg1.Y), "BG1"));

    /// <summary>Compares BG2 displacement pairs with their native table entries.</summary>
    private static void VerifyRoomShakeBg2Algorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyRoomShakeVector), () => VerifyRoomShakeVector(rom, RoomShakeDefinitions.Bg2ReferenceAddress, 8,
            definition => new OctantVector(definition.Bg2.X, definition.Bg2.Y), "BG2"));

    /// <summary>Compares enemy-projectile displacement pairs with their native table entries.</summary>
    private static void VerifyRoomShakeProjectileAlgorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyRoomShakeVector), () => VerifyRoomShakeVector(rom, RoomShakeDefinitions.ProjectileReferenceAddress, 4,
            definition => new OctantVector(definition.Projectile.X, definition.Projectile.Y), "projectile"));

    /// <summary>Checks one compiled vector field against all 36 native rows and rejects invalid type indices.</summary>
    private static void VerifyRoomShakeVector(SuperMetroidAddressSpace rom, int source, int stride,
        Func<RoomShakeDefinition, OctantVector> select, string label)
    {
        short Word(int address) => unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        for (ushort type = 0; type < 36; type++)
            AssertEqual(new OctantVector(Word(source + type * stride), Word(source + type * stride + 2)),
                select(RoomShakeDefinitions.ForType(type)), $"Original shake {label} vector {type}");
        AssertThrows<InvalidDataException>(() => select(RoomShakeDefinitions.ForType(36)), $"Shake {label} upper bound");
        AssertThrows<InvalidDataException>(() => select(RoomShakeDefinitions.ForType(ushort.MaxValue)), $"Shake {label} invalid maximum");
    }

    /// <summary>Forbids reads of the migrated background and projectile shake tables during runtime checks.</summary>
    private static CartridgeReadGuard NewRoomShakeReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa0872d and < 0xa0884d or
                >= 0x86846b and < 0x8684fb,
            address => $"Room shaking attempted migrated displacement read ${address:X6}.");
}
