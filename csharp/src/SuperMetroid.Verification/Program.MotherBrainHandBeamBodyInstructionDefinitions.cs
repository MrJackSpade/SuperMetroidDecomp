using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Checks that every hand-beam list word has one compiled owner and production dispatch matches the ROM.</summary>
    private static void VerifyMotherBrainHandBeamBodyInstructionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo readMechanics = typeof(RoomEnemySystem).GetMethod(
            "ReadEnemyInstructionMechanicsWord", flags)!;
        MethodInfo readVisual = typeof(RoomEnemySystem).GetMethod(
            "ReadEnemyVisualSelector", flags)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;
        var enemies = new RoomEnemySystem();
        busField.SetValue(enemies, new MotherBrainHandBeamListReadGuard(rom));
        var body = new RoomEnemySlot(0)
        {
            EnemyDefinitionPointer = 0xec7f,
            Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 },
        };

        int visualWords = 0;
        int mechanicsWords = 0;
        for (int pointer = MotherBrainHandBeamBodyInstructionDefinitions.Start;
            pointer <= MotherBrainHandBeamBodyInstructionDefinitions.End; pointer += 2)
        {
            ushort address = (ushort)pointer;
            ushort compiled;
            try
            {
                compiled = MotherBrainHandBeamBodyInstructionDefinitions.ReadMechanicsWord(
                    address);
                AssertEqual(compiled, (ushort)readMechanics.Invoke(enemies,
                        [body, address])!,
                    $"production Mother Brain hand-beam mechanics $A9:{pointer:X4}");
                mechanicsWords++;
            }
            catch (InvalidDataException)
            {
                compiled = MotherBrainHandBeamBodyInstructionDefinitions.ReadVisualSelector(
                    address);
                AssertEqual(compiled, (ushort)readVisual.Invoke(enemies,
                        [body, address])!,
                    $"production Mother Brain hand-beam visual selector $A9:{pointer:X4}");
                visualWords++;
            }
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), 0xa90000 | pointer),
                compiled, $"Mother Brain hand-beam source word $A9:{pointer:X4}");
        }
        AssertEqual(67, mechanicsWords + visualWords,
            "every hand-beam body list word has exactly one compiled owner");
        AssertEqual(15, visualWords,
            "all initial, dust-record, and terminal hand-beam visual selectors");
        AssertThrows<InvalidDataException>(
            () => MotherBrainHandBeamBodyInstructionDefinitions.ReadMechanicsWord(0x9a46),
            "hand-beam visual word is not mechanical");
        AssertThrows<InvalidDataException>(
            () => MotherBrainHandBeamBodyInstructionDefinitions.ReadVisualSelector(0x9a42),
            "hand-beam pose opcode is not a visual selector");
        AssertThrows<InvalidDataException>(
            () => MotherBrainHandBeamBodyInstructionDefinitions.ReadMechanicsWord(0x9ac8),
            "native code after the hand-beam list is not copied as data");

        Console.WriteLine(
            $"Mother Brain hand-beam body: {mechanicsWords} mechanics and " +
            $"{visualWords} visual-selector words match the pinned ROM without list reads.");
    }

    /// <summary>Wraps an address space and rejects runtime reads from the compiled hand-beam instruction lists.</summary>
    /// <param name="source">The underlying address space for reads outside the guarded list range and for writes.</param>
    private sealed class MotherBrainHandBeamListReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-import reads through the hand-beam list restriction.</summary>
        /// <param name="address">The address requested by the importer.</param>
        /// <returns>The underlying byte when the address is outside the compiled list range.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects runtime reads from the compiled lists and delegates other reads.</summary>
        /// <param name="address">The address to read.</param>
        /// <returns>The underlying byte when the read is permitted.</returns>
        public byte ReadByte(int address) =>
            address is >= 0xa99a42 and <= 0xa99ac7
                ? throw new InvalidOperationException(
                    $"Mother Brain reread compiled hand-beam list ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        /// <param name="address">The destination address.</param>
        /// <param name="value">The byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
