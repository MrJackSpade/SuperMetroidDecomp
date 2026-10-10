using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks all seven Ceres door selectors against the ROM tables and production initialization path.</summary>
    /// <param name="rom">ROM address space used to compare the selector words with their native table values.</param>
    private static void VerifyCeresDoorInitializationDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0xa6f52c;
        const int functionTable = 0xa6f72b;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeCeresDoor", flags)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;
        FieldInfo vramField = typeof(RoomEnemySystem).GetField("_vram", flags)!;
        FieldInfo cgramField = typeof(RoomEnemySystem).GetField("_cgram", flags)!;
        var guarded = new CeresDoorInitializationReadGuard(rom);

        for (ushort variant = 0; variant < 7; variant++)
        {
            CeresDoorInitializationDefinition definition =
                CeresDoorInitializationDefinitions.For(variant);
            AssertEqual(ReadCeresDoorInitializationWord(rom, functionTable + variant * 2),
                definition.MainFunction,
                $"Ceres door function selector {variant}");
            AssertEqual(ReadCeresDoorInitializationWord(rom, instructionTable + variant * 2),
                definition.InstructionList,
                $"Ceres door instruction selector {variant}");

            var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
            busField.SetValue(enemies, guarded);
            vramField.SetValue(enemies, new SnesVram());
            cgramField.SetValue(enemies, new SnesCgram());
            RoomEnemySlot slot = enemies.Slots[0];
            slot.Parameter1 = variant;

            initialize.Invoke(enemies, [slot]);

            AssertEqual(definition.MainFunction, slot.VariableA,
                $"production Ceres door function selector {variant}");
            AssertEqual(definition.InstructionList, slot.CurrentInstruction,
                $"production Ceres door instruction selector {variant}");
            AssertEqual((ushort)1, slot.InstructionTimer,
                $"production Ceres door instruction timer {variant}");
            AssertEqual((ushort)0, slot.Timer,
                $"production Ceres door general timer {variant}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresDoorInitializationDefinitions.For(7),
            "Ceres door selector past definitions");
        Console.WriteLine(
            "Ceres door initialization definitions: all fourteen native selector words and seven production initializers pass with both source tables forbidden.");
    }

    /// <summary>Reads one little-endian 16-bit selector from the cartridge address space.</summary>
    /// <param name="bus">Address space providing the two source bytes.</param>
    /// <param name="address">Address of the selector's low byte.</param>
    /// <returns>The word formed from the byte at <paramref name="address"/> and the following byte.</returns>
    private static ushort ReadCeresDoorInitializationWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Prevents production initialization from reading the two Ceres selector tables during this verification.</summary>
    /// <param name="source">Underlying address space used for reads outside the forbidden table ranges and for all writes.</param>
    private sealed class CeresDoorInitializationReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-source reads through the guard's forbidden-range check.</summary>
        /// <param name="address">Cartridge address requested by the caller.</param>
        /// <returns>The byte from the wrapped <c>source</c> unless the address belongs to a guarded table.</returns>
        /// <exception cref="InvalidOperationException">The requested address belongs to either guarded selector table.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects Ceres initialization-table reads and forwards other reads to the wrapped address space.</summary>
        /// <param name="address">Address to read from the underlying cartridge bus.</param>
        /// <returns>The requested byte when the address is outside both guarded table ranges.</returns>
        /// <exception cref="InvalidOperationException">The production initializer attempts to read either Ceres selector table.</exception>
        public byte ReadByte(int address) =>
            address is >= 0xa6f52c and < 0xa6f53a or >= 0xa6f72b and < 0xa6f739
                ? throw new InvalidOperationException(
                    $"Ceres door attempted migrated initialization-table read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards writes unchanged so initialization behavior can be exercised while table reads remain guarded.</summary>
        /// <param name="address">Address receiving the write.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
