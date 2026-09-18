using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyShaktoolSegmentDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        ushort Word(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        int[] tables =
        [
            0xaade95,
            0xaadea3,
            0xaadeb1,
            0xaadebf,
            0xaadecd,
            0xaadedb,
            0xaadee9,
        ];

        var definitions = new ShaktoolSegmentDefinition[7];
        for (int index = 0; index < definitions.Length; index++)
        {
            ShaktoolSegmentDefinition definition = ShaktoolSegmentDefinitions.ForIndex(index);
            definitions[index] = definition;
            ushort[] actual =
            [
                definition.PropertyMask,
                definition.OwnerNativeOffset,
                definition.InitialOrbitAngle,
                definition.InitialInstruction,
                definition.Layer,
                (ushort)definition.PreInstruction,
                definition.AngularVelocity,
            ];
            for (int field = 0; field < actual.Length; field++)
            {
                AssertEqual(Word(tables[field] + index * 2), actual[field],
                    $"Shaktool segment {index} definition field {field}");
            }
            AssertEqual((ushort)0, Word(0xaadef7 + index * 2),
                $"Shaktool segment {index} initialization subtrahend");
        }

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies, new ShaktoolSegmentReadGuard(rom));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeShaktool", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        for (int index = 0; index < definitions.Length; index++)
        {
            ShaktoolSegmentDefinition definition = definitions[index];
            RoomEnemySlot slot = enemies.Slots[index];
            slot.EnemyDefinitionPointer = RoomEnemySystem.ShaktoolDefinition;
            slot.Parameter2 = unchecked((ushort)(index * 2));
            slot.Properties = 0x0010;
            slot.XPosition = unchecked((ushort)(0x0100 + index * 16));
            slot.YPosition = 0x0200;
            initialize(slot);

            ShaktoolSegmentState state = enemies.ShaktoolSegments[index]!;
            AssertEqual(unchecked((ushort)(0x0010 | definition.PropertyMask)),
                slot.Properties, $"Shaktool production properties {index}");
            AssertEqual((ushort)0, state.OwnerNativeIndex,
                $"Shaktool production owner {index}");
            AssertEqual(definition.PreInstruction, state.PreInstruction,
                $"Shaktool production callback {index}");
            AssertEqual(definition.AngularVelocity, state.AngularVelocity,
                $"Shaktool production angular velocity {index}");
            AssertEqual(definition.InitialOrbitAngle, state.OrbitAngle,
                $"Shaktool production orbit angle {index}");
            AssertEqual(definition.InitialInstruction, slot.CurrentInstruction,
                $"Shaktool production initial list {index}");
            AssertEqual(definition.Layer, slot.Layer,
                $"Shaktool production layer {index}");
            AssertEqual((ushort)1, slot.InstructionTimer,
                $"Shaktool production instruction timer {index}");
        }

        foreach (ShaktoolSegmentState state in enemies.ShaktoolSegments.Take(7)!)
            state.PreInstruction = ShaktoolPreInstruction.IdleAfterAttack;
        MethodInfo resetMethod = typeof(RoomEnemySystem).GetMethod(
            "TryProcessShaktoolInstruction", flags)!;
        object[] arguments =
        [
            enemies.Slots[0],
            ShaktoolInstructionCodes.Instruction_Shaktool_ResetShaktoolFunctions,
            (ushort)0x1234,
        ];
        AssertTrue((bool)resetMethod.Invoke(enemies, arguments)!,
            "Shaktool production reset callback handled");
        AssertEqual((ushort)0x1236, (ushort)arguments[2],
            "Shaktool production reset cursor");
        for (int index = 0; index < definitions.Length; index++)
        {
            AssertEqual(definitions[index].PreInstruction,
                enemies.ShaktoolSegments[index]!.PreInstruction,
                $"Shaktool production reset callback {index}");
        }

        AssertThrows<InvalidDataException>(
            () => ShaktoolSegmentDefinitions.ForIndex(-1),
            "negative Shaktool segment definition");
        AssertThrows<InvalidDataException>(
            () => ShaktoolSegmentDefinitions.ForIndex(7),
            "Shaktool segment definition past table");
        Console.WriteLine(
            "Shaktool segment definitions: 56 native words, all seven real initializers and the group callback reset pass with source tables forbidden.");
    }

    private sealed class ShaktoolSegmentReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xaade95 and < 0xaadf05
                ? throw new InvalidOperationException(
                    $"Shaktool attempted migrated segment-definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
