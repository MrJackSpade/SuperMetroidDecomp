using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the three native materialization sounds and verifies two production callback cycles without reading their source table.</summary>
    /// <param name="rom">Retail cartridge address space used for expected sound words and unrelated enemy reads.</param>
    private static void VerifyPhantoonSoundDefinitions(SuperMetroidAddressSpace rom)
    {
        const int sourceAddress = 0xa7cded;
        for (ushort index = 0; index < 3; index++)
        {
            ushort expected = (ushort)(
                rom.ReadByte(sourceAddress + index * 2) |
                rom.ReadByte(sourceAddress + index * 2 + 1) << 8);
            AssertEqual(expected, PhantoonSoundDefinitions.MaterializationSound(index),
                $"Phantoon materialization sound {index}");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => PhantoonSoundDefinitions.MaterializationSound(3),
            "Phantoon materialization sound past definitions");

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new PhantoonSoundReadGuard(rom));
        RoomEnemySlot body = enemies.Slots[0];
        body.EnemyDefinitionPointer = RoomEnemySystem.PhantoonBodyDefinition;
        var state = new PhantoonEnemyState(body)
        {
            Eye = enemies.Slots[1],
            Tentacles = enemies.Slots[2],
            Mouth = enemies.Slots[3],
        };
        typeof(RoomEnemySystem).GetField("_phantoonState", flags)!.SetValue(enemies, state);
        var process = typeof(RoomEnemySystem).GetMethod(
            "ProcessPhantoonInstructionFunction", flags)!
            .CreateDelegate<Func<RoomEnemySlot, ushort, byte, bool>>(enemies);

        for (ushort call = 0; call < 6; call++)
        {
            ushort index = (ushort)(call % 3);
            AssertTrue(!process(
                    body,
                    PhantoonInstructionCodes.PlayPhantoonMaterializationSFX,
                    0),
                $"Phantoon materialization callback {call} resumes instruction stream");
            AssertEqual(PhantoonSoundDefinitions.MaterializationSound(index),
                state.LastMaterializationSound!.Value,
                $"Phantoon materialization callback {call} sound");
            AssertEqual((ushort)((index + 1) % 3), state.MaterializationSoundIndex,
                $"Phantoon materialization callback {call} next index");
        }

        Console.WriteLine(
            "Phantoon sound definitions: all three native selections and two complete production callback cycles pass with the source table forbidden.");
    }

    /// <summary>Wraps an address space and fails if Phantoon reads the migrated materialization-sound table.</summary>
    /// <param name="source">Address space used outside the guarded sound-table range and as the destination for writes.</param>
    private sealed class PhantoonSoundReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-source reads through the materialization-sound table guard.</summary>
        /// <param name="address">Cartridge address to read.</param>
        /// <returns>The source byte when the address is outside the guarded sound table.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the migrated sound-table range and delegates other addresses to the wrapped source.</summary>
        /// <param name="address">Address of the byte to read.</param>
        /// <returns>The wrapped source's byte for an address outside the guarded range.</returns>
        public byte ReadByte(int address) => address is >= 0xa7cded and < 0xa7cdf3
            ? throw new InvalidOperationException(
                $"Phantoon attempted migrated materialization sound read ${address:X6}.")
            : source.ReadByte(address);

        /// <summary>Forwards the byte write unchanged to the wrapped address space.</summary>
        /// <param name="address">Address where the byte is written.</param>
        /// <param name="value">Byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
