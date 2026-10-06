using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyShaktoolSegmentDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyShaktoolPropertySelection), () => VerifyShaktoolPropertySelection(rom));
        Suite(nameof(VerifyShaktoolOwnerOffsetAlgorithm), () => VerifyShaktoolOwnerOffsetAlgorithm(rom));
        Suite(nameof(VerifyShaktoolInitialInstructionSelection), () => VerifyShaktoolInitialInstructionSelection(rom));
        Suite(nameof(VerifyShaktoolLayerSelection), () => VerifyShaktoolLayerSelection(rom));
        Suite(nameof(VerifyShaktoolCallbackSelection), () => VerifyShaktoolCallbackSelection(rom));
        Suite(nameof(VerifyShaktoolInitialAngleAlgorithm), () => VerifyShaktoolInitialAngleAlgorithm(rom));
        Suite(nameof(VerifyShaktoolAngularVelocityAlgorithm), () => VerifyShaktoolAngularVelocityAlgorithm(rom));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        ushort Word(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var definitions = new ShaktoolSegmentDefinition[7];
        for (int index = 0; index < definitions.Length; index++)
        {
            ShaktoolSegmentDefinition definition = ShaktoolSegmentDefinitions.ForIndex(index);
            definitions[index] = definition;
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

    private static void VerifyShaktoolPropertySelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativePropertiesAddress,
            definition => definition.PropertyMask, "property selection"));

    private static void VerifyShaktoolInitialAngleAlgorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeInitialAngleAddress,
            definition => definition.InitialOrbitAngle, "initial angle algorithm"));

    private static void VerifyShaktoolAngularVelocityAlgorithm(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolAngularVelocityDefinitions.ReferenceAddress,
            definition => definition.AngularVelocity, "angular velocity algorithm"));
        for (int index = 0; index < 7; index++)
            AssertEqual(ShaktoolSegmentDefinitions.ForIndex(index).AngularVelocity,
                ShaktoolAngularVelocityDefinitions.ForSegment(index), "Shaktool velocity alias");
        foreach (int invalid in new[] { -1, 7, int.MinValue, int.MaxValue })
            AssertThrows<InvalidDataException>(() => ShaktoolAngularVelocityDefinitions.ForSegment(invalid),
                "Shaktool velocity alias bounds");
    }

    private static void VerifyShaktoolOwnerOffsetAlgorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeOwnerOffsetAddress,
            definition => definition.OwnerNativeOffset, "owner offset"));

    private static void VerifyShaktoolInitialInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeInstructionAddress,
            definition => definition.InitialInstruction, "initial instruction selection"));

    private static void VerifyShaktoolLayerSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeLayerAddress,
            definition => definition.Layer, "layer selection"));

    private static void VerifyShaktoolCallbackSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeCallbackAddress,
            definition => (ushort)definition.PreInstruction, "callback selection"));

    private static void VerifyShaktoolDefinitionField(SuperMetroidAddressSpace rom,
        int nativeAddress, Func<ShaktoolSegmentDefinition, ushort> select, string label)
    {
        for (int index = 0; index < 7; index++)
        {
            int address = nativeAddress + 2 * index;
            ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(expected, select(ShaktoolSegmentDefinitions.ForIndex(index)),
                $"Shaktool {label} original word {index}");
        }
        foreach (int invalid in new[] { -1, 7, int.MinValue, int.MaxValue })
            AssertThrows<InvalidDataException>(() => select(ShaktoolSegmentDefinitions.ForIndex(invalid)),
                $"Shaktool {label} rejects {invalid}");
    }

    private sealed class ShaktoolSegmentReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) =>
            address is >= 0xaade95 and < 0xaadf05
                ? throw new InvalidOperationException(
                    $"Shaktool attempted migrated segment-definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
