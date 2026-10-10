using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBombTorizoDroolSine(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoLowHealthDrool",
            flags)!;

        for (int random = byte.MinValue; random <= byte.MaxValue; random++)
        {
            VerifyBombTorizoDroolSpawn(
                rom,
                spawn,
                random,
                parameter1: 0x4000,
                expectedIndex: random & 0x01fe,
                "full-circle left-facing");
            VerifyBombTorizoDroolSpawn(
                rom,
                spawn,
                random,
                parameter1: 0xc000,
                expectedIndex: random & 0x01fe,
                "full-circle right-facing");
        }

        for (int randomNibble = 0; randomNibble < 16; randomNibble++)
        {
            VerifyBombTorizoDroolSpawn(
                rom,
                spawn,
                randomNibble,
                parameter1: 0,
                expectedIndex: (224 + randomNibble - 8) << 1,
                "left-facing cone");
            VerifyBombTorizoDroolSpawn(
                rom,
                spawn,
                randomNibble,
                parameter1: 0x8000,
                expectedIndex: (32 + randomNibble - 8) << 1,
                "right-facing cone");
        }

        Console.WriteLine(
            "Bomb Torizo drool sine: 544 real random/facing spawns preserve native XY velocity with the entire signed-sine ROM table forbidden.");
    }

    private static void VerifyBombTorizoDroolSpawn(
        SuperMetroidAddressSpace rom,
        MethodInfo spawn,
        int random,
        ushort parameter1,
        int expectedIndex,
        string scenario)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new BombTorizoDroolSineReadGuard(rom));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(new Queue<ushort>([0, unchecked((ushort)random)]).Dequeue));

        RoomEnemySlot torizo = enemies.Slots[0];
        torizo.XPosition = 0x0400;
        torizo.YPosition = 0x0200;
        torizo.Parameter1 = parameter1;

        spawn.Invoke(enemies, [torizo]);

        RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles[^1];
        AssertTrue(projectile.IsActive,
            $"Bomb Torizo {scenario} random {random:X2} allocation");
        // $86:A621/$A628 index the sign-extended sine and negative-cosine tables with the
        // same word offset: X is the sine and Y the negative cosine of the drool angle.
        AssertEqual(
            ReadBombTorizoDroolWord(rom, EnemyMathReferenceData.SignedSine, expectedIndex),
            projectile.XVelocity,
            $"Bomb Torizo {scenario} random {random:X2} X velocity");
        AssertEqual(
            ReadBombTorizoDroolWord(rom, EnemyMathReferenceData.SignedNegativeCosine, expectedIndex),
            projectile.YVelocity,
            $"Bomb Torizo {scenario} random {random:X2} Y velocity");
        AssertEqual(
            (parameter1 & 0x8000) != 0 ? (ushort)0x0408 : (ushort)0x03f8,
            projectile.XPosition,
            $"Bomb Torizo {scenario} random {random:X2} X origin");
    }

    private static ushort ReadBombTorizoDroolWord(
        SuperMetroidAddressSpace rom,
        int table,
        int wordOffset)
    {
        int address = table + wordOffset;
        return unchecked((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
    }

    private sealed class BombTorizoDroolSineReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) =>
            address is >= 0xa0b3c3 and < 0xa0b643
                ? throw new InvalidOperationException(
                    $"Bomb Torizo drool attempted migrated sine or negative-cosine read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
