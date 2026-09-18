using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMotherBrainDoorFragmentDefinitions(SuperMetroidAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 8; parameter++)
        {
            int byteOffset = parameter * 4;
            MotherBrainDoorFragmentDefinition definition =
                MotherBrainDoorFragmentDefinitions.ForParameter(parameter);
            AssertEqual(unchecked((short)ReadMotherBrainFragmentWord(rom, 0x86c992 + byteOffset)),
                definition.XOffset, $"Mother Brain door fragment X offset {parameter}");
            AssertEqual(unchecked((short)ReadMotherBrainFragmentWord(rom, 0x86c994 + byteOffset)),
                definition.YOffset, $"Mother Brain door fragment Y offset {parameter}");
            AssertEqual(unchecked((short)ReadMotherBrainFragmentWord(rom, 0x86c9b2 + byteOffset)),
                definition.XVelocity, $"Mother Brain door fragment X velocity {parameter}");
            AssertEqual(unchecked((short)ReadMotherBrainFragmentWord(rom, 0x86c9b4 + byteOffset)),
                definition.YVelocity, $"Mother Brain door fragment Y velocity {parameter}");
        }

        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainDoorFragment",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var guarded = new MotherBrainDoorFragmentReadGuard(rom);
        for (ushort parameter = 0; parameter < 8; parameter++)
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            spawn.Invoke(enemies, [parameter]);

            MotherBrainDoorFragmentDefinition definition =
                MotherBrainDoorFragmentDefinitions.ForParameter(parameter);
            RoomEnemyProjectileSlot fragment =
                enemies.EnemyProjectiles.Single(projectile => projectile.IsActive);
            AssertEqual(RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment, fragment.Kind,
                $"production Mother Brain door fragment kind {parameter}");
            AssertEqual(unchecked((ushort)(MotherBrainDeathRomData.DoorX + definition.XOffset)),
                fragment.XPosition, $"production Mother Brain door fragment X {parameter}");
            AssertEqual(unchecked((ushort)(MotherBrainDeathRomData.DoorY + definition.YOffset)),
                fragment.YPosition, $"production Mother Brain door fragment Y {parameter}");
            AssertEqual(unchecked((ushort)definition.XVelocity), fragment.XVelocity,
                $"production Mother Brain door fragment X velocity {parameter}");
            AssertEqual(unchecked((ushort)definition.YVelocity), fragment.YVelocity,
                $"production Mother Brain door fragment Y velocity {parameter}");
            AssertEqual(MotherBrainDeathRomData.DoorFragmentLifetime, fragment.Variable0,
                $"production Mother Brain door fragment lifetime {parameter}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainDoorFragmentDefinitions.ForParameter(8),
            "Mother Brain door fragment parameter past table");
        Console.WriteLine(
            "Mother Brain door fragments: 32 native physical words and all eight real spawns pass with offset/velocity tables forbidden.");
    }

    private static ushort ReadMotherBrainFragmentWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class MotherBrainDoorFragmentReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0x86c992 and < 0x86c9d2
                ? throw new InvalidOperationException(
                    $"Mother Brain door fragment attempted migrated table read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
