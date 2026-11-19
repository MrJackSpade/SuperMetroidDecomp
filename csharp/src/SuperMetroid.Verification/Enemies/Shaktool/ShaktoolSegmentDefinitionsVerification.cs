using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

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
            ShaktoolSegmentDefinition definition = ShaktoolSegmentDefinitions.Segments[index];
            definitions[index] = definition;
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaadef7 + index * 2)),
                $"Shaktool segment {index} initialization subtrahend");
        }

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemyPlace> initialize = enemies.ShaktoolBehavior.InitializeShaktool;
        for (int index = 0; index < definitions.Length; index++)
        {
            ShaktoolSegmentDefinition definition = definitions[index];
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[index];
            place.Bind(EnemyDefinitionId.Shaktool, place.Definition);
            place.SetSpawnParameter2(EnemyDefinitionId.Shaktool, unchecked((ushort)(index * 2)));
            place.Contact = EnemyContact.BlocksPlasmaBeam;
            place.Processing = EnemyProcessing.None;
            place.Presence = EnemyPresence.None;
            place.Position = place.Position.WithWhole(new(unchecked((ushort)(0x0100 + index * 16)), 0x0200));
            initialize(place);

            ShaktoolSegmentState state = (enemies.ShaktoolSegments[index] ?? throw new InvalidOperationException("Verification requires enemies.ShaktoolSegments[index]."));
            AssertEqual(EnemyContact.BlocksPlasmaBeam | definition.Contact,
                place.Contact, $"Shaktool production contact {index}");
            AssertEqual(definition.Processing,
                place.Processing, $"Shaktool production processing {index}");
            AssertEqual(EnemyPresence.None,
                place.Presence, $"Shaktool production presence {index}");
            AssertEqual(definition.Motion, state.Motion,
                $"Shaktool production callback {index}");
            AssertEqual(definition.AngularVelocity, state.AngularVelocity,
                $"Shaktool production angular velocity {index}");
            AssertEqual(definition.InitialOrbitAngle, state.OrbitAngle,
                $"Shaktool production orbit angle {index}");
            AssertEqual(new AnimationPlayback<ShaktoolSegment>(ShaktoolAnimations.StartingWith(definition.InitialAnimation), AnimationPlace.StartOfAnimation), state.Playback,
                $"Shaktool production initial list {index}");
            AssertEqual(1, place.AnimationClock.Hold,
                $"Shaktool production instruction timer {index}");
        }

        foreach (ShaktoolSegmentState? state in enemies.ShaktoolSegments.Take(7))
            (state ?? throw new InvalidOperationException("Verification requires state.")).Motion = ShaktoolSegmentMotion.Idle;
        // The reset cue carries out its reaction and the animation plays on: a cue's reaction continues nowhere else.
        enemies.ShaktoolBehavior.ApplyShaktoolCue(enemies.Places.InUpdateOrder[0], new ShaktoolCue(ShaktoolCueKind.ResumeMotion));
        for (int index = 0; index < definitions.Length; index++)
        {
            AssertEqual(definitions[index].Motion,
                (enemies.ShaktoolSegments[index] ?? throw new InvalidOperationException("Verification requires enemies.ShaktoolSegments[index].")).Motion,
                $"Shaktool production reset callback {index}");
        }

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
            definition => checked((ushort)(definition.OwnerPlaceDistance * SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.RecordSize)), "owner offset"));

    /// <summary>Checks the first animation instruction list selected for each segment.</summary>
    private static void VerifyShaktoolInitialInstructionSelection(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyShaktoolDefinitionField), () => VerifyShaktoolDefinitionField(rom, ShaktoolSegmentDefinitions.NativeInstructionAddress,
            definition => NativeEnemyAnimationOracle.EntryOf(ShaktoolNativeListAddresses.Entries, definition.InitialAnimation), "initial instruction selection"));

    /// <summary>Checks that each segment's initial pre-instruction is the one its cartridge callback word stands for.</summary>
    private static void VerifyShaktoolCallbackSelection(CartridgeImportAddressSpace rom)
    {
        for (int index = 0; index < 7; index++)
        {
            ushort word = CartridgeImportSource.Require(rom).ReadCartridgeWord(ShaktoolSegmentDefinitions.NativeCallbackAddress + 2 * index);
            AssertEqual(ShaktoolNativeWords.DecodeMotion(word, "Shaktool segment callback"), ShaktoolSegmentDefinitions.Segments[index].Motion,
                $"Shaktool callback selection {index}");
        }
    }

    /// <summary>Compares one seven-word native table, in chain order, with the chain's compiled definitions.</summary>
    private static void VerifyShaktoolDefinitionField(CartridgeImportAddressSpace rom,
        int nativeAddress, Func<ShaktoolSegmentDefinition, int> select, string label)
    {
        for (int index = 0; index < 7; index++)
        {
            int address = nativeAddress + 2 * index;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, select(ShaktoolSegmentDefinitions.Segments[index]),
                $"Shaktool {label} original word {index}");
        }
    }

}
