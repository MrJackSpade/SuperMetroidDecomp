using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

internal static class RoomShakeDefinitionsVerification
{
    internal static void VerifyRoomShakeDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyRoomShakeBg1Algorithm), () => VerifyRoomShakeBg1Algorithm(rom));
        Suite(nameof(VerifyRoomShakeBg2Algorithm), () => VerifyRoomShakeBg2Algorithm(rom));
        Suite(nameof(VerifyRoomShakeProjectileAlgorithm), () => VerifyRoomShakeProjectileAlgorithm(rom));
        const ushort renderedTypeCount = 36;

        var guarded = NewRoomShakeReadGuard(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guarded);

        for (ushort type = 0; type < renderedTypeCount; type++)
        {
            RoomShakeDefinition definition = RoomShakeDefinitions.ForType(type);
            enemies.Earthquake.EarthquakeType = type;
            enemies.Earthquake.EarthquakeTimer = 1;
            RoomShakeFrameResult positive = enemies.HandleRoomShaking(timeIsFrozen: false);
            AssertEqual(new RoomShakeFrameResult(definition.Bg1, definition.Bg2),
                positive,
                $"room shake type {type} positive production frame");
            AssertEqual((ushort)0, enemies.Earthquake.EarthquakeTimer,
                $"room shake type {type} positive timer consumption");

            enemies.Earthquake.EarthquakeTimer = 2;
            RoomShakeFrameResult negative = enemies.HandleRoomShaking(timeIsFrozen: false);
            AssertEqual(new RoomShakeFrameResult(
                    new(unchecked((short)-definition.Bg1.X), unchecked((short)-definition.Bg1.Y)),
                    new(unchecked((short)-definition.Bg2.X), unchecked((short)-definition.Bg2.Y))),
                negative,
                $"room shake type {type} alternating production frame");

            enemies.Earthquake.EarthquakeTimer = 1;
            var positiveProjectile = enemies.Earthquake.GetEnemyProjectileShake(false);
            AssertEqual(definition.Projectile, positiveProjectile,
                $"room shake type {type} positive projectile displacement");

            enemies.Earthquake.EarthquakeTimer = 2;
            var negativeProjectile = enemies.Earthquake.GetEnemyProjectileShake(false);
            AssertEqual(-definition.Projectile,
                negativeProjectile,
                $"room shake type {type} alternating projectile displacement");
        }

        AssertThrows<InvalidDataException>(
            () => RoomShakeDefinitions.ForType(renderedTypeCount),
            "room shake type outside rendered table");

        enemies.Earthquake.EarthquakeType = RoomFxRomData.Earthquake.FirstNonRenderedType;
        enemies.Earthquake.EarthquakeTimer = 4;
        AssertEqual(default(RoomShakeFrameResult), enemies.HandleRoomShaking(timeIsFrozen: false),
            "non-rendered earthquake type does not access compiled definitions");
        AssertEqual((ushort)4, enemies.Earthquake.EarthquakeTimer,
            "non-rendered earthquake type preserves timer");
        AssertEqual(Offset16<RoomPixels>.Zero,
            enemies.Earthquake.GetEnemyProjectileShake(false),
            "non-rendered earthquake type has no projectile displacement");

        enemies.Earthquake.EarthquakeType = 0;
        AssertEqual(default(RoomShakeFrameResult), enemies.HandleRoomShaking(timeIsFrozen: true),
            "frozen room shake does not access compiled definitions");
        AssertEqual(Offset16<RoomPixels>.Zero,
            enemies.Earthquake.GetEnemyProjectileShake(true),
            "frozen projectile shake does not access compiled definitions");

        Console.WriteLine(
            "Room shake definitions: all 216 native words and 144 real background/projectile phase selections pass with both source tables forbidden.");
    }

    private static void VerifyRoomShakeBg1Algorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyRoomShakeVector), () => VerifyRoomShakeVector(rom, RoomShakeDefinitions.Bg1ReferenceAddress, 8,
            definition => (definition.Bg1.X, definition.Bg1.Y), "BG1"));

    private static void VerifyRoomShakeBg2Algorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyRoomShakeVector), () => VerifyRoomShakeVector(rom, RoomShakeDefinitions.Bg2ReferenceAddress, 8,
            definition => (definition.Bg2.X, definition.Bg2.Y), "BG2"));

    private static void VerifyRoomShakeProjectileAlgorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyRoomShakeVector), () => VerifyRoomShakeVector(rom, RoomShakeDefinitions.ProjectileReferenceAddress, 4,
            definition => (definition.Projectile.X, definition.Projectile.Y), "projectile"));

    [CpuBusBoundary("exact bus test: retail ROM oracle for room shake vector")]
    private static void VerifyRoomShakeVector(SuperMetroidAddressSpace rom, int source, int stride,
        Func<RoomShakeDefinition, (short X, short Y)> select, string label)
    {
        short Word(int address) => unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        for (ushort type = 0; type < 36; type++)
            AssertEqual((Word(source + type * stride), Word(source + type * stride + 2)),
                select(RoomShakeDefinitions.ForType(type)), $"Original shake {label} vector {type}");
        AssertThrows<InvalidDataException>(() => select(RoomShakeDefinitions.ForType(36)), $"Shake {label} upper bound");
        AssertThrows<InvalidDataException>(() => select(RoomShakeDefinitions.ForType(ushort.MaxValue)), $"Shake {label} invalid maximum");
    }

    private static CartridgeReadGuard NewRoomShakeReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa0872d and < 0xa0884d or
                >= 0x86846b and < 0x8684fb,
            address => $"Room shaking attempted migrated displacement read ${address:X6}.");
}
