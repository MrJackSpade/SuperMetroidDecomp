using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Represents shaktool segment definitions verification.</summary>
internal static class ShaktoolSegmentDefinitionsVerification
{
    /// <summary>verify shaktool segment definitions.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Shaktool segment definitions")]
    internal static void VerifyShaktoolSegmentDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyShaktoolPropertySelection), () => VerifyShaktoolPropertySelection(rom));
        Suite(nameof(VerifyShaktoolOwnerOffsetAlgorithm), () => VerifyShaktoolOwnerOffsetAlgorithm(rom));
        Suite(nameof(VerifyShaktoolInitialInstructionSelection), () => VerifyShaktoolInitialInstructionSelection(rom));
        Suite(nameof(VerifyShaktoolLayerSelection), () => VerifyShaktoolLayerSelection(rom));
        Suite(nameof(VerifyShaktoolCallbackSelection), () => VerifyShaktoolCallbackSelection(rom));
        Suite(nameof(VerifyShaktoolInitialAngleAlgorithm), () => VerifyShaktoolInitialAngleAlgorithm(rom));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var definitions = new ShaktoolSegmentDefinition[7];
        for (int index = 0; index < definitions.Length; index++)
        {
            ShaktoolSegmentDefinition definition = ShaktoolSegmentDefinitions.ForIndex(index);
            definitions[index] = definition;
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaadef7 + index * 2)),
                $"Shaktool segment {index} initialization subtrahend");
        }

        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewShaktoolSegmentReadGuard(rom));
        Action<RoomEnemySlot> initialize = enemies.ShaktoolBehavior.InitializeShaktool;
        for (int index = 0; index < definitions.Length; index++)
        {
            ShaktoolSegmentDefinition definition = definitions[index];
            RoomEnemySlot slot = enemies.SlotTable.Slots[index];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Shaktool;
            slot.Parameter2 = unchecked((ushort)(index * 2));
            slot.Properties = 0x0010;
            slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(0x0100 + index * 16)), 0x0200));
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
        ushort resetCursor = 0x1234;
        AssertTrue(enemies.ShaktoolBehavior.TryProcessShaktoolInstruction(
                enemies.SlotTable.Slots[0], (ushort)ShaktoolInstruction.Instruction_Shaktool_ResetShaktoolFunctions, ref resetCursor),
            "Shaktool production reset callback handled");
        AssertEqual((ushort)0x1236, resetCursor,
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

    /// <summary>verify shaktool property selection.</summary>
    private static void VerifyShaktoolPropertySelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativePropertiesAddress,
            definition => definition.PropertyMask, "property selection"));

    /// <summary>verify shaktool initial angle algorithm.</summary>
    internal static void VerifyShaktoolInitialAngleAlgorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeInitialAngleAddress,
            definition => definition.InitialOrbitAngle, "initial angle algorithm"));

    /// <summary>verify shaktool owner offset algorithm.</summary>
    private static void VerifyShaktoolOwnerOffsetAlgorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeOwnerOffsetAddress,
            definition => definition.OwnerNativeOffset, "owner offset"));

    /// <summary>verify shaktool initial instruction selection.</summary>
    private static void VerifyShaktoolInitialInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeInstructionAddress,
            definition => definition.InitialInstruction, "initial instruction selection"));

    /// <summary>verify shaktool layer selection.</summary>
    private static void VerifyShaktoolLayerSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeLayerAddress,
            definition => definition.Layer, "layer selection"));

    /// <summary>verify shaktool callback selection.</summary>
    private static void VerifyShaktoolCallbackSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeCallbackAddress,
            definition => (ushort)definition.PreInstruction, "callback selection"));

    /// <summary>verify shaktool definition field.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Shaktool definition field")]
    private static void VerifyShaktoolDefinitionField(SuperMetroidAddressSpace rom,
        int nativeAddress, Func<ShaktoolSegmentDefinition, ushort> select, string label)
    {
        for (int index = 0; index < 7; index++)
        {
            int address = nativeAddress + 2 * index;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, select(ShaktoolSegmentDefinitions.ForIndex(index)),
                $"Shaktool {label} original word {index}");
        }
        foreach (int invalid in new[] { -1, 7, int.MinValue, int.MaxValue })
            AssertThrows<InvalidDataException>(() => select(ShaktoolSegmentDefinitions.ForIndex(invalid)),
                $"Shaktool {label} rejects {invalid}");
    }

    /// <summary>new shaktool segment read guard.</summary>
    private static CartridgeReadGuard NewShaktoolSegmentReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xaade95 and < 0xaadf05,
            address => $"Shaktool attempted migrated segment-definition read ${address:X6}.");
}
