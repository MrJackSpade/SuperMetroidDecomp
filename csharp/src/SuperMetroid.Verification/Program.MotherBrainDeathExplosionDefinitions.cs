using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMotherBrainDeathExplosionDefinitions(SuperMetroidAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 3; parameter++)
        {
            AssertEqual(ReadMotherBrainExplosionWord(rom, 0x86c929 + parameter * 2),
                MotherBrainDeathExplosionDefinitions.InstructionList(parameter),
                $"Mother Brain death explosion instruction {parameter}");
        }

        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainDeathExplosion",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo motherBrainField = typeof(RoomEnemySystem).GetField(
            "_motherBrain", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var guarded = new MotherBrainDeathExplosionReadGuard(rom);
        for (ushort parameter = 0; parameter < 3; parameter++)
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            var body = new RoomEnemySlot(0) { XPosition = 0x0100, YPosition = 0x0200 };
            motherBrainField.SetValue(enemies, new MotherBrainEnemyState(body));
            var request = new MotherBrainDeathExplosionRequest(
                PatternIndex: 0,
                XOffset: -7,
                YOffset: 9,
                XPosition: 0,
                YPosition: 0,
                ProjectileParameter: parameter,
                SoundEffect: 0);
            spawn.Invoke(enemies, [request]);

            RoomEnemyProjectileSlot projectile =
                enemies.EnemyProjectiles.Single(candidate => candidate.IsActive);
            AssertEqual(RoomEnemyProjectileKind.MotherBrainDeathExplosion, projectile.Kind,
                $"production Mother Brain death explosion kind {parameter}");
            AssertEqual(MotherBrainDeathExplosionDefinitions.InstructionList(parameter),
                projectile.InstructionPointer,
                $"production Mother Brain death explosion instruction {parameter}");
            AssertEqual(0x00f9, projectile.XPosition,
                $"production Mother Brain death explosion X {parameter}");
            AssertEqual(0x0209, projectile.YPosition,
                $"production Mother Brain death explosion Y {parameter}");
            AssertEqual(unchecked((ushort)-7), projectile.XVelocity,
                $"production Mother Brain death explosion X offset {parameter}");
            AssertEqual(9, projectile.YVelocity,
                $"production Mother Brain death explosion Y offset {parameter}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainDeathExplosionDefinitions.InstructionList(3),
            "Mother Brain death explosion parameter past table");
        Console.WriteLine(
            "Mother Brain death explosions: three native program selectors and all three real body-relative spawns pass with the selector table forbidden.");
    }

    private static ushort ReadMotherBrainExplosionWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class MotherBrainDeathExplosionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0x86c929 and < 0x86c92f
                ? throw new InvalidOperationException(
                    $"Mother Brain death explosion attempted migrated selector read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
