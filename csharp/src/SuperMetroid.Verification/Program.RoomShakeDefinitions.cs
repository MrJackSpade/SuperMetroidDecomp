using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyRoomShakeDefinitions(SuperMetroidAddressSpace rom)
    {
        const int backgroundAddress = 0xa0872d;
        const int projectileAddress = 0x86846b;
        const ushort renderedTypeCount = 36;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        var guarded = new RoomShakeReadGuard(rom);
        var enemies = new RoomEnemySystem();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guarded);
        MethodInfo projectileShake = typeof(RoomEnemySystem).GetMethod(
            "GetEnemyProjectileShake",
            flags)!;

        for (ushort type = 0; type < renderedTypeCount; type++)
        {
            RoomShakeDefinition definition = RoomShakeDefinitions.ForType(type);
            int backgroundRow = backgroundAddress + type * 8;
            int projectileRow = projectileAddress + type * 4;

            AssertEqual(unchecked((short)ReadWord(rom, backgroundRow)), definition.Bg1X,
                $"room shake type {type} BG1 X");
            AssertEqual(unchecked((short)ReadWord(rom, backgroundRow + 2)), definition.Bg1Y,
                $"room shake type {type} BG1 Y");
            AssertEqual(unchecked((short)ReadWord(rom, backgroundRow + 4)), definition.Bg2X,
                $"room shake type {type} BG2 X");
            AssertEqual(unchecked((short)ReadWord(rom, backgroundRow + 6)), definition.Bg2Y,
                $"room shake type {type} BG2 Y");
            AssertEqual(unchecked((short)ReadWord(rom, projectileRow)), definition.ProjectileX,
                $"room shake type {type} projectile X");
            AssertEqual(unchecked((short)ReadWord(rom, projectileRow + 2)), definition.ProjectileY,
                $"room shake type {type} projectile Y");

            enemies.EarthquakeType = type;
            enemies.EarthquakeTimer = 1;
            RoomShakeFrameResult positive = enemies.HandleRoomShaking(timeIsFrozen: false);
            AssertEqual(new RoomShakeFrameResult(
                    Applied: true,
                    definition.Bg1X,
                    definition.Bg1Y,
                    definition.Bg2X,
                    definition.Bg2Y,
                    ShakesEnemies: type >= RoomFxRomData.Earthquake.FirstEnemyShakingType),
                positive,
                $"room shake type {type} positive production frame");
            AssertEqual((ushort)0, enemies.EarthquakeTimer,
                $"room shake type {type} positive timer consumption");

            enemies.EarthquakeTimer = 2;
            RoomShakeFrameResult negative = enemies.HandleRoomShaking(timeIsFrozen: false);
            AssertEqual(new RoomShakeFrameResult(
                    Applied: true,
                    unchecked((short)-definition.Bg1X),
                    unchecked((short)-definition.Bg1Y),
                    unchecked((short)-definition.Bg2X),
                    unchecked((short)-definition.Bg2Y),
                    ShakesEnemies: type >= RoomFxRomData.Earthquake.FirstEnemyShakingType),
                negative,
                $"room shake type {type} alternating production frame");

            enemies.EarthquakeTimer = 1;
            var positiveProjectile = ((short X, short Y))projectileShake.Invoke(
                enemies,
                [false])!;
            AssertEqual((definition.ProjectileX, definition.ProjectileY), positiveProjectile,
                $"room shake type {type} positive projectile displacement");

            enemies.EarthquakeTimer = 2;
            var negativeProjectile = ((short X, short Y))projectileShake.Invoke(
                enemies,
                [false])!;
            AssertEqual((
                    unchecked((short)-definition.ProjectileX),
                    unchecked((short)-definition.ProjectileY)),
                negativeProjectile,
                $"room shake type {type} alternating projectile displacement");
        }

        AssertThrows<InvalidDataException>(
            () => RoomShakeDefinitions.ForType(renderedTypeCount),
            "room shake type outside rendered table");

        enemies.EarthquakeType = RoomFxRomData.Earthquake.FirstNonRenderedType;
        enemies.EarthquakeTimer = 4;
        AssertEqual(default(RoomShakeFrameResult), enemies.HandleRoomShaking(timeIsFrozen: false),
            "non-rendered earthquake type does not access compiled definitions");
        AssertEqual((ushort)4, enemies.EarthquakeTimer,
            "non-rendered earthquake type preserves timer");
        AssertEqual(((short)0, (short)0),
            ((short X, short Y))projectileShake.Invoke(enemies, [false])!,
            "non-rendered earthquake type has no projectile displacement");

        enemies.EarthquakeType = 0;
        AssertEqual(default(RoomShakeFrameResult), enemies.HandleRoomShaking(timeIsFrozen: true),
            "frozen room shake does not access compiled definitions");
        AssertEqual(((short)0, (short)0),
            ((short X, short Y))projectileShake.Invoke(enemies, [true])!,
            "frozen projectile shake does not access compiled definitions");

        Console.WriteLine(
            "Room shake definitions: all 216 native words and 144 real background/projectile phase selections pass with both source tables forbidden.");
    }

    private sealed class RoomShakeReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa0872d and < 0xa0884d or
                >= 0x86846b and < 0x8684fb
                ? throw new InvalidOperationException(
                    $"Room shaking attempted migrated displacement read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
