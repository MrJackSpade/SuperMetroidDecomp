using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>Checks Shaktool's seven compiled segment initializers against the cartridge and production enemy setup.</summary>
internal static class ShaktoolSegmentDefinitionsVerification
{
    /// <summary>Exercises each segment's properties, orbit, layer, instruction, and group callback reset with ROM reads forbidden.</summary>
    internal static void VerifyShaktoolSegmentDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyShaktoolPropertySelection), () => VerifyShaktoolPropertySelection(rom));
        Suite(nameof(VerifyShaktoolOwnerOffsetAlgorithm), () => VerifyShaktoolOwnerOffsetAlgorithm(rom));
        Suite(nameof(VerifyShaktoolInitialInstructionSelection), () => VerifyShaktoolInitialInstructionSelection(rom));
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

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot> initialize = enemies.ShaktoolBehavior.InitializeShaktool;
        for (int index = 0; index < definitions.Length; index++)
        {
            ShaktoolSegmentDefinition definition = definitions[index];
            RoomEnemySlot slot = enemies.SlotTable.Slots[index];
            slot.Bind(EnemyDefinitionId.Shaktool, slot.Definition);
            slot.SetSpawnParameter2(EnemyDefinitionId.Shaktool, unchecked((ushort)(index * 2)));
            slot.Contact = EnemyContact.BlocksPlasmaBeam;
            slot.Processing = EnemyProcessing.None;
            slot.Presence = EnemyPresence.None;
            slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(0x0100 + index * 16)), 0x0200));
            initialize(slot);

            ShaktoolSegmentState state = (enemies.ShaktoolSegments[index] ?? throw new InvalidOperationException("Verification requires enemies.ShaktoolSegments[index]."));
            AssertEqual(EnemyContact.BlocksPlasmaBeam | definition.Contact,
                slot.Contact, $"Shaktool production contact {index}");
            AssertEqual(definition.Processing,
                slot.Processing, $"Shaktool production processing {index}");
            AssertEqual(EnemyPresence.None,
                slot.Presence, $"Shaktool production presence {index}");
            AssertEqual(definition.FrameBehavior, state.FrameBehavior,
                $"Shaktool production callback {index}");
            AssertEqual(definition.AngularVelocity, state.AngularVelocity,
                $"Shaktool production angular velocity {index}");
            AssertEqual(definition.InitialOrbitAngle, state.OrbitAngle,
                $"Shaktool production orbit angle {index}");
            AssertEqual(new AnimationPlayback<ShaktoolProgram>(ShaktoolAnimations.StartingWith(definition.InitialProgram), AnimationPlace.StartOfAnimation), state.Playback,
                $"Shaktool production initial list {index}");
            AssertEqual((ushort)1, slot.AnimationClock.Hold.RawValue,
                $"Shaktool production instruction timer {index}");
        }

        foreach (ShaktoolSegmentState? state in enemies.ShaktoolSegments.Take(7))
            (state ?? throw new InvalidOperationException("Verification requires state.")).FrameBehavior = ShaktoolFrameBehavior.Idle;
        // The reset cue carries out its reaction and the animation plays on: a cue's reaction continues nowhere else.
        enemies.ShaktoolBehavior.ApplyShaktoolCue(
            enemies.SlotTable.Slots[0], enemies.ShaktoolSegments[0] ?? throw new InvalidOperationException("Verification requires enemies.ShaktoolSegments[0]."),
            new ShaktoolCue(ShaktoolCueKind.ResetFunctions));
        for (int index = 0; index < definitions.Length; index++)
        {
            AssertEqual(definitions[index].FrameBehavior,
                (enemies.ShaktoolSegments[index] ?? throw new InvalidOperationException("Verification requires enemies.ShaktoolSegments[index].")).FrameBehavior,
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
    private static void VerifyShaktoolPropertySelection(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativePropertiesAddress,
            definition => checked((ushort)EnemyPropertyWordCodec.EncodePropertyWord(new EnemyPropertyState(
                definition.Contact, definition.Processing, EnemyPresence.None, CollidesByFrameHitboxes: false, NewInstructionFrame: false))),
            "property selection"));

    /// <summary>Checks the initial orbit angle selected for all seven body segments.</summary>
    internal static void VerifyShaktoolInitialAngleAlgorithm(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeInitialAngleAddress,
            definition => definition.InitialOrbitAngle.RawValue, "initial angle algorithm"));

    /// <summary>Checks each segment's offset back to the controlling Shaktool slot.</summary>
    private static void VerifyShaktoolOwnerOffsetAlgorithm(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeOwnerOffsetAddress,
            definition => checked((ushort)(definition.OwnerSlotDistance * SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.RecordSize)), "owner offset"));

    /// <summary>Checks the first animation instruction list selected for each segment.</summary>
    private static void VerifyShaktoolInitialInstructionSelection(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeInstructionAddress,
            definition => NativeEnemyAnimationOracle.EntryOf(ShaktoolNativeListAddresses.Entries, definition.InitialProgram), "initial instruction selection"));

    /// <summary>Checks that each segment's initial pre-instruction is the one its cartridge callback word stands for.</summary>
    private static void VerifyShaktoolCallbackSelection(CartridgeImportAddressSpace rom)
    {
        for (int index = 0; index < 7; index++)
        {
            ushort word = CartridgeImportSource.Require(rom).ReadCartridgeWord(ShaktoolSegmentDefinitions.NativeCallbackAddress + 2 * index);
            AssertEqual(ShaktoolNativeWords.DecodeFrameBehavior(word, "Shaktool segment callback"), ShaktoolSegments.ForIndex(index).FrameBehavior,
                $"Shaktool callback selection {index}");
        }
    }

    /// <summary>Compares one seven-word native table with its compiled definition and rejects out-of-range segment indices.</summary>
    private static void VerifyShaktoolDefinitionField(CartridgeImportAddressSpace rom,
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
