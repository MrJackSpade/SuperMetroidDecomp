using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyMotherBrainFallingTubeInstructionDefinitions()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo processInstructions = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions", flags)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;

        for (int index = 0; index < MotherBrainFallingTubeInstructionDefinitions.ListCount;
            index++)
        {
            ushort start = unchecked((ushort)(
                MotherBrainFallingTubeInstructionDefinitions.FirstList +
                index * MotherBrainFallingTubeInstructionDefinitions.ListStride));
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), 0xa90000 | start),
                MotherBrainFallingTubeInstructionDefinitions.ReadMechanicsWord(start),
                $"Mother Brain falling tube {index} duration");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), 0xa90000 | (start + 2)),
                MotherBrainFallingTubeInstructionDefinitions.ReadVisualSelector(
                    unchecked((ushort)(start + 2))),
                $"Mother Brain falling tube {index} visual identity");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), 0xa90000 | (start + 4)),
                MotherBrainFallingTubeInstructionDefinitions.ReadMechanicsWord(
                    unchecked((ushort)(start + 4))),
                $"Mother Brain falling tube {index} terminal opcode");

            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, new MotherBrainFallingTubeReadGuard(rom));
            var tube = new RoomEnemySlot(0)
            {
                EnemyDefinitionPointer = EnemyDefinitionPointers.MotherBrainFallingTube,
                Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 },
                CurrentInstruction = start,
                InstructionTimer = 1,
            };
            object?[] args = [tube, null, null, (ushort)0, (ushort)0,
                (ushort)0, (byte)0];
            processInstructions.Invoke(enemies, args);
            AssertEqual((ushort)(start + 4), tube.CurrentInstruction,
                $"falling tube {index} advances from frame to terminal opcode");
            AssertEqual((ushort)1, tube.InstructionTimer,
                $"falling tube {index} retains its native one-frame duration");
            AssertEqual(MotherBrainFallingTubeInstructionDefinitions.ReadVisualSelector(
                    unchecked((ushort)(start + 2))), tube.SpritemapPointer,
                $"falling tube {index} publishes its own selected composition");
            processInstructions.Invoke(enemies, args);
            AssertEqual((ushort)(start + 4), tube.CurrentInstruction,
                $"falling tube {index} sleeps on the native terminal word");
        }

        AssertThrows<InvalidDataException>(
            () => MotherBrainFallingTubeInstructionDefinitions.ReadMechanicsWord(0x8c6b),
            "tube visual selector is not read as mechanics");
        AssertThrows<InvalidDataException>(
            () => MotherBrainFallingTubeInstructionDefinitions.ReadVisualSelector(0x8c69),
            "tube duration is not read as presentation");
        AssertThrows<InvalidDataException>(
            () => MotherBrainFallingTubeInstructionDefinitions.ReadMechanicsWord(0x8c87),
            "native code after the tube lists is not interpreted as list data");

        Console.WriteLine(
            "Mother Brain falling tubes: five native frame/sleep lists, selected art, " +
            "and guarded ordinary-enemy execution pass.");
    }

    private sealed class MotherBrainFallingTubeReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa98c69 and <= 0xa98c86
                ? throw new InvalidOperationException(
                    $"Mother Brain tube reread compiled source ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
