using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the compiled sprite and sound choices against ROM and verifies all random/inherited handoffs through the production spawner.</summary>
    /// <param name="rom">Cartridge address space used to compare the source tables and wrapped by the table-read guard.</param>
    private static void VerifyZebesEscapeExplosionDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 8; index++)
        {
            ZebesEscapeExplosionDefinition definition =
                ZebesEscapeExplosionDefinitions.ForIndex(index);
            AssertEqual(rom.ReadByte(0x8fc1d6 + index), (byte)definition.SpriteKind,
                $"Zebes escape explosion sprite {index}");
            AssertEqual(rom.ReadByte(0x8fc1de + index), definition.SoundEffect,
                $"Zebes escape explosion sound {index}");
        }

        var guarded = new ZebesEscapeExplosionReadGuard(rom);
        FieldInfo busField = typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (ushort choice = 0; choice < 16; choice++)
        for (ushort inherited = 0; inherited < 8; inherited++)
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            enemies.SpawnEscapeExplosion(0x1234, 0x5678, choice, inherited);

            int selectedIndex = choice < 8 ? choice : inherited;
            ZebesEscapeExplosionDefinition expected =
                ZebesEscapeExplosionDefinitions.ForIndex(selectedIndex);
            RoomSpriteObjectSlot sprite = enemies.RoomSpriteObjects.Single(slot => slot.IsActive);
            AssertEqual(expected.SpriteKind, sprite.Kind,
                $"production escape sprite choice {choice}, inherited {inherited}");
            AssertEqual(0x1234, sprite.XPosition,
                $"production escape X choice {choice}, inherited {inherited}");
            AssertEqual(0x5678, sprite.YPosition,
                $"production escape Y choice {choice}, inherited {inherited}");

            bool queuesSound = choice < 8 && expected.SoundEffect != 0;
            AssertEqual(queuesSound ? 1 : 0, enemies.SoundRequests.Count,
                $"production escape sound count choice {choice}, inherited {inherited}");
            if (queuesSound)
            {
                AssertEqual(new EnemySoundRequest(
                        SoundEffectId.FromCartridge(
                            SoundEffectLibrary.Library2,
                            expected.SoundEffect),
                        6),
                    enemies.SoundRequests.Single(),
                    $"production escape sound choice {choice}, inherited {inherited}");
            }
        }

        AssertThrows<InvalidDataException>(
            () => ZebesEscapeExplosionDefinitions.ForIndex(-1),
            "negative Zebes escape explosion index");
        AssertThrows<InvalidDataException>(
            () => ZebesEscapeExplosionDefinitions.ForIndex(8),
            "Zebes escape explosion index past table");

        Console.WriteLine(
            "Zebes escape explosion definitions: sixteen native bytes and all 128 random/inherited selection handoffs pass with source tables forbidden.");
    }

    /// <summary>Prevents the production escape-explosion path from rereading its native sprite and sound tables.</summary>
    /// <param name="source">Address space used for reads and writes outside the guarded table range.</param>
    private sealed class ZebesEscapeExplosionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes an imported-cartridge byte request through the escape-table read guard.</summary>
        /// <param name="address">CPU-visible address of the requested cartridge byte.</param>
        /// <returns>The delegated byte when the address is outside the guarded tables.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Reads a mapped byte unless it belongs to the native escape-explosion sprite or sound table.</summary>
        /// <param name="address">CPU-visible address of the requested byte.</param>
        /// <returns>The byte from the wrapped address space when the address is allowed.</returns>
        /// <exception cref="InvalidOperationException">The address is within the guarded sprite or sound table range.</exception>
        public byte ReadByte(int address) =>
            address is >= 0x8fc1d6 and < 0x8fc1e6
                ? throw new InvalidOperationException(
                    $"Zebes escape explosion attempted migrated table read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a mapped byte write to the wrapped address space.</summary>
        /// <param name="address">CPU-visible address to write.</param>
        /// <param name="value">Byte value to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
