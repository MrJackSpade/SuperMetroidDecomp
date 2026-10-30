using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>Checks Shaktool's seven compiled segment initializers against the cartridge and production enemy setup.</summary>
internal static class ShaktoolSegmentDefinitionsVerification
{
    /// <summary>Exercises each segment's properties, orbit, layer, instruction, and group callback reset with ROM reads forbidden.</summary>
    internal static void VerifyShaktoolSegmentDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyShaktoolPropertySelection), () => VerifyShaktoolPropertySelection(rom));
        Suite(nameof(VerifyShaktoolOwnerOffsetAlgorithm), () => VerifyShaktoolOwnerOffsetAlgorithm(rom));
        Suite(nameof(VerifyShaktoolInitialInstructionSelection), () => VerifyShaktoolInitialInstructionSelection(rom));
        Suite(nameof(VerifyShaktoolLayerSelection), () => VerifyShaktoolLayerSelection(rom));
        Suite(nameof(VerifyShaktoolCallbackSelection), () => VerifyShaktoolCallbackSelection(rom));
        Suite(nameof(VerifyShaktoolInitialAngleAlgorithm), () => VerifyShaktoolInitialAngleAlgorithm(rom));
        var definitions = new ShaktoolSegmentDefinition[7];
        for (int index = 0; index < definitions.Length; index++)
        {
            ShaktoolSegmentDefinition definition = ShaktoolSegments.ForIndex(index);
            definitions[index] = definition;
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaadef7 + index * 2)),
                $"Shaktool segment {index} initialization subtrahend");
        }

        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot> initialize = enemies.ShaktoolBehavior.InitializeShaktool;
        for (int index = 0; index < definitions.Length; index++)
        {
            ShaktoolSegmentDefinition definition = definitions[index];
            RoomEnemySlot slot = enemies.SlotTable.Slots[index];
            slot.Bind(EnemyDefinitionId.Shaktool, slot.Definition);
            slot.SetSpawnParameter2(unchecked((ushort)(index * 2)));
            slot.Properties = EnemyPropertyWord.FromWord(0x0010);
            slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(0x0100 + index * 16)), 0x0200));
            initialize(slot);

            ShaktoolSegmentState state = (enemies.ShaktoolSegments[index] ?? throw new InvalidOperationException("Verification requires enemies.ShaktoolSegments[index]."));
            AssertEqual((ushort)(0x0010 | (ushort)definition.PropertyMask),
                slot.Properties.ToWord(), $"Shaktool production properties {index}");
            AssertEqual(0, state.OwnerSlotIndex,
                $"Shaktool production owner {index}");
            AssertEqual(definition.PreInstruction, state.PreInstruction,
                $"Shaktool production callback {index}");
            AssertEqual(definition.AngularVelocity, state.AngularVelocity,
                $"Shaktool production angular velocity {index}");
            AssertEqual(definition.InitialOrbitAngle, state.OrbitAngle,
                $"Shaktool production orbit angle {index}");
            AssertEqual(AnimationProgramCursor<ShaktoolProgram>.Start(definition.InitialProgram), state.Program,
                $"Shaktool production initial list {index}");
            AssertEqual(definition.Layer, slot.Layer,
                $"Shaktool production layer {index}");
            AssertEqual((ushort)1, slot.InstructionTimer.RawValue,
                $"Shaktool production instruction timer {index}");
        }

        foreach (ShaktoolSegmentState? state in enemies.ShaktoolSegments.Take(7))
            (state ?? throw new InvalidOperationException("Verification requires state.")).PreInstruction = ShaktoolPreInstruction.IdleAfterAttack;
        AssertEqual(null, enemies.ShaktoolBehavior.ApplyShaktoolCue(
                enemies.SlotTable.Slots[0], enemies.ShaktoolSegments[0] ?? throw new InvalidOperationException("Verification requires enemies.ShaktoolSegments[0]."),
                new ShaktoolCue(ShaktoolCueKind.ResetFunctions)),
            "Shaktool production reset cue runs on");
        for (int index = 0; index < definitions.Length; index++)
        {
            AssertEqual(definitions[index].PreInstruction,
                (enemies.ShaktoolSegments[index] ?? throw new InvalidOperationException("Verification requires enemies.ShaktoolSegments[index].")).PreInstruction,
                $"Shaktool production reset callback {index}");
        }

        AssertThrows<InvalidDataException>(
            () => ShaktoolSegments.ForIndex(-1),
            "negative Shaktool segment definition");
        AssertThrows<InvalidDataException>(
            () => ShaktoolSegments.ForIndex(7),
            "Shaktool segment definition past table");
        Console.WriteLine(
            "Shaktool segment definitions: 56 native words, all seven real initializers and the group callback reset pass with source tables forbidden.");
    }

    /// <summary>Matches each segment's added property bits with the native lookup table.</summary>
    private static void VerifyShaktoolPropertySelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativePropertiesAddress,
            definition => (ushort)definition.PropertyMask, "property selection"));

    /// <summary>Checks the initial orbit angle selected for all seven body segments.</summary>
    internal static void VerifyShaktoolInitialAngleAlgorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeInitialAngleAddress,
            definition => definition.InitialOrbitAngle.RawValue, "initial angle algorithm"));

    /// <summary>Checks each segment's offset back to the controlling Shaktool slot.</summary>
    private static void VerifyShaktoolOwnerOffsetAlgorithm(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeOwnerOffsetAddress,
            definition => checked((ushort)(definition.OwnerSlotDistance * SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.RecordSize)), "owner offset"));

    /// <summary>Checks the first animation instruction list selected for each segment.</summary>
    private static void VerifyShaktoolInitialInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeInstructionAddress,
            definition => NativeEnemyProgramOracle.EntryOf(ShaktoolNativeListAddresses.Entries, definition.InitialProgram), "initial instruction selection"));

    /// <summary>Checks the rendering layer assigned to each segment at initialization.</summary>
    private static void VerifyShaktoolLayerSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeLayerAddress,
            definition => definition.Layer, "layer selection"));

    /// <summary>Checks the initial per-segment pre-instruction callback selection.</summary>
    private static void VerifyShaktoolCallbackSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeCallbackAddress,
            definition => ShaktoolNativeWords.ShaktoolPreInstructions.Word(definition.PreInstruction), "callback selection"));

    /// <summary>Compares one seven-word native table with its compiled definition and rejects out-of-range segment indices.</summary>
    private static void VerifyShaktoolDefinitionField(SuperMetroidAddressSpace rom,
        int nativeAddress, Func<ShaktoolSegmentDefinition, int> select, string label)
    {
        for (int index = 0; index < 7; index++)
        {
            int address = nativeAddress + 2 * index;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, select(ShaktoolSegments.ForIndex(index)),
                $"Shaktool {label} original word {index}");
        }
        foreach (int invalid in new[] { -1, 7, int.MinValue, int.MaxValue })
            AssertThrows<InvalidDataException>(() => select(ShaktoolSegments.ForIndex(invalid)),
                $"Shaktool {label} rejects {invalid}");
    }

}
