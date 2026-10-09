using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares all eight fragment definitions with cartridge tables and verifies production spawning uses only the compiled data.</summary>
    /// <param name="rom">ROM address space providing the independent physical table values.</param>
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

    /// <summary>Reads a little-endian word from the cartridge's fragment-definition tables.</summary>
    /// <param name="bus">Address space containing the reference bytes.</param>
    /// <param name="address">Address of the word's low-order byte.</param>
    /// <returns>The decoded 16-bit table word.</returns>
    private static ushort ReadMotherBrainFragmentWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Guards the migrated Mother Brain fragment tables while forwarding other cartridge access.</summary>
    /// <param name="source">Underlying address space used for accesses outside the guarded table range.</param>
    private sealed class MotherBrainDoorFragmentReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Forwards import reads through the guarded byte-read implementation.</summary>
        /// <param name="address">Cartridge address of the requested byte.</param>
        /// <returns>The byte from the underlying address space when it is outside the guarded range.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects accesses to the migrated fragment tables and delegates all other reads.</summary>
        /// <param name="address">Address of the byte to read.</param>
        /// <returns>The underlying byte for an address outside the guarded tables.</returns>
        public byte ReadByte(int address) =>
            address is >= 0x86c992 and < 0x86c9d2
                ? throw new InvalidOperationException(
                    $"Mother Brain door fragment attempted migrated table read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a byte write unchanged to the wrapped address space.</summary>
        /// <param name="address">Destination address for the write.</param>
        /// <param name="value">Byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
