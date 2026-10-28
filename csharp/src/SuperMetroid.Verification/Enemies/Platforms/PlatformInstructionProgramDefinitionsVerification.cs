using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Platforms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Tripper and Kamer platform instruction loops, selectors, and native visual exports.</summary>
internal static class PlatformInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge and runs the platform instruction suite.</summary>
    internal static void VerifyPlatformInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyPlatformInstructionProgramDefinitions), () => VerifyPlatformInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Exercises eight species, direction, and motion programs with compiled mechanics reads forbidden.</summary>
    internal static void VerifyPlatformInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {

        Suite(nameof(VerifyTripperFrozenFrameSelection), () => VerifyTripperFrozenFrameSelection(rom));
        Suite(nameof(VerifyPlatformInitialProgramSelection), () => VerifyPlatformInitialProgramSelection(rom));
        Suite(nameof(VerifyPlatformAnimationProgramSelection), () => VerifyPlatformAnimationProgramSelection(rom));

        Action<RoomEnemySlot, PlatformEnemyState> installMoving = PlatformsBehavior.InstallPlatformVerticallyMovingForCurrentDirection;
        Action<RoomEnemySlot, PlatformEnemyState> installStill = PlatformsBehavior.InstallPlatformVerticallyStillForCurrentDirection;

        PlatformProgramCase[] cases =
        [
            new(true, true, PlatformHorizontalMovement.Left,
                PlatformProgram.KamerMovingLeft),
            new(true, true, PlatformHorizontalMovement.Right,
                PlatformProgram.KamerMovingRight),
            new(true, false, PlatformHorizontalMovement.Left,
                PlatformProgram.KamerStillLeft),
            new(true, false, PlatformHorizontalMovement.Right,
                PlatformProgram.KamerStillRight),
            new(false, true, PlatformHorizontalMovement.Left,
                PlatformProgram.TripperMovingLeft),
            new(false, true, PlatformHorizontalMovement.Right,
                PlatformProgram.TripperMovingRight),
            new(false, false, PlatformHorizontalMovement.Left,
                PlatformProgram.TripperStillMovingLeft),
            new(false, false, PlatformHorizontalMovement.Right,
                PlatformProgram.TripperStillMovingRight),
        ];

        foreach (PlatformProgramCase testCase in cases)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(testCase.IsKamer
                ? EnemyDefinitionId.Kamer2
                : EnemyDefinitionId.Tripper, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.SetSpawnParameter1((ushort)testCase.Direction);
            slot.SetSpawnParameter2(0x0101);
            slot.Position = slot.Position.WithWholeY(0x0100);
            PlatformsBehavior.InitializePlatform(slot);

            PlatformEnemyState state = (enemies.PlatformStates[0] ?? throw new InvalidOperationException("Verification requires enemies.PlatformStates[0]."));
            PlatformProgram expectedInitial = testCase.IsKamer
                ? testCase.Direction == PlatformHorizontalMovement.Left
                    ? PlatformProgram.KamerStillLeft
                    : PlatformProgram.KamerStillRight
                : testCase.Direction == PlatformHorizontalMovement.Left
                    ? PlatformProgram.TripperStillMovingLeft
                    : PlatformProgram.TripperStillMovingRight;
            AssertEqual(AnimationProgramCursor<PlatformProgram>.Start(expectedInitial), state.Program,
                $"real {(testCase.IsKamer ? "Kamer" : "Tripper")} initializer " +
                $"selects {testCase.Direction} still program");

            (testCase.IsMoving ? installMoving : installStill)(slot, (state));
            AssertEqual(AnimationProgramCursor<PlatformProgram>.Start(testCase.Program), state.Program,
                $"production platform installer selects {testCase.Program}");

            // The direction cue runs with the first pose; four more updates show the rest of the four-pose loop and
            // return to the same place in it.
            state.XMovement = testCase.Direction == PlatformHorizontalMovement.Left
                ? PlatformHorizontalMovement.Right
                : PlatformHorizontalMovement.Left;
            ExecutePlatformProgram(slot, callCount: 1);
            AssertEqual(testCase.Direction, state.XMovement, $"platform program {testCase.Program} publishes direction");
            AnimationProgramCursor<PlatformProgram>? firstPose = state.Program;
            ExecutePlatformProgram(slot, callCount: 4);
            AssertEqual(firstPose, state.Program, $"platform program {testCase.Program} loops through four poses");
        }

        Suite(nameof(VerifyPlatformVisualPointers), () => VerifyPlatformVisualPointers(rom));
        Suite(nameof(VerifyPlatformExportIdentities), () => VerifyPlatformExportIdentities(rom));
        Console.WriteLine(
            "Tripper/Kamer: program selection, all eight production-installed programs' direction cues and loops, " +
            "the shown spritemaps and the export identities pass.");
        Suite(nameof(PlatformAnimationsVerification.VerifyPlatformAnimations), () => PlatformAnimationsVerification.VerifyPlatformAnimations(rom));
    }

    /// <summary>Forces the selected platform's instruction timer across several interpreter calls.</summary>
    private static void ExecutePlatformProgram(RoomEnemySlot slot,
        int callCount)
    {
        object?[] arguments =
            [slot, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
        for (int call = 0; call < callCount; call++)
        {
            slot.InstructionTimer = WrappingCountdown.Start(1);
            EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>Checks that the programs show exactly the sixteen animated maps and their native sprite counts.</summary>
    private static void VerifyPlatformVisualPointers(SuperMetroidAddressSpace rom)
    {
        ushort[] kamer = [0xa021, 0xa02d, 0xa039, 0xa045];
        ushort[] threeEntryTripper = [0x9f29, 0x9f3a, 0x9f4b, 0x9f5c, 0x9f6d, 0x9f7e, 0x9fa5, 0x9fe2];
        ushort[] fourEntryTripper = [0x9f8f, 0x9fb6, 0x9fcc, 0x9ff3];
        foreach (ushort pointer in kamer)
            AssertEqual((ushort)2, CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | pointer), "Kamer native two-entry map");
        foreach (ushort pointer in threeEntryTripper)
            AssertEqual((ushort)3, CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | pointer), "Tripper native three-entry map");
        foreach (ushort pointer in fourEntryTripper)
            AssertEqual((ushort)4, CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | pointer), "Tripper native four-entry map");
        AssertTrue(PlatformAnimationsConstants.Spritemaps(Enum.GetValues<PlatformProgram>()).ToHashSet()
                .SetEquals(kamer.Concat(threeEntryTripper).Concat(fourEntryTripper)),
            "platform programs show exactly the sixteen animated maps");
    }
    /// <summary>Checks published frame order, asset names, and consecutive native record sizes.</summary>
    private static void VerifyPlatformExportIdentities(SuperMetroidAddressSpace rom)
    {
        // Independent published export order and names; addresses are the native map labels.
        PlatformInstructionProgramDefinitionsExpected[] expected =
        [
            new PlatformInstructionProgramDefinitionsExpected(0x9f29, "tripper_moving_left_0"), new PlatformInstructionProgramDefinitionsExpected(0x9f3a, "tripper_moving_left_1"),
            new PlatformInstructionProgramDefinitionsExpected(0x9f4b, "tripper_moving_left_2"), new PlatformInstructionProgramDefinitionsExpected(0x9f5c, "tripper_moving_right_0"),
            new PlatformInstructionProgramDefinitionsExpected(0x9f6d, "tripper_moving_right_1"), new PlatformInstructionProgramDefinitionsExpected(0x9f7e, "tripper_moving_right_2"),
            new PlatformInstructionProgramDefinitionsExpected(0x9f8f, "tripper_still_moving_left_0"), new PlatformInstructionProgramDefinitionsExpected(0x9fa5, "tripper_still_moving_left_1"),
            new PlatformInstructionProgramDefinitionsExpected(0x9fb6, "tripper_still_moving_left_2"), new PlatformInstructionProgramDefinitionsExpected(0x9fcc, "tripper_still_moving_right_0"),
            new PlatformInstructionProgramDefinitionsExpected(0x9fe2, "tripper_still_moving_right_1"), new PlatformInstructionProgramDefinitionsExpected(0x9ff3, "tripper_still_moving_right_2"),
            new PlatformInstructionProgramDefinitionsExpected(0xa009, "tripper_frozen_moving_left"), new PlatformInstructionProgramDefinitionsExpected(0xa015, "tripper_frozen_moving_right"),
            new PlatformInstructionProgramDefinitionsExpected(0xa021, "tripper_kamer_platform_0"), new PlatformInstructionProgramDefinitionsExpected(0xa02d, "tripper_kamer_platform_1"),
            new PlatformInstructionProgramDefinitionsExpected(0xa039, "tripper_kamer_platform_2"), new PlatformInstructionProgramDefinitionsExpected(0xa045, "tripper_kamer_platform_3"),
        ];
        NativeEnemySpritemapDefinition[] actual = TripperKamerVisualDefinitions.Frames();
        AssertEqual(expected.Length, actual.Length, "platform export frame count");
        for (int i = 0; i < expected.Length; i++)
        {
            AssertEqual((byte)0xa3, actual[i].Bank, "platform export bank");
            AssertEqual(expected[i].Pointer, actual[i].Pointer, "platform export native map pointer and order");
            AssertEqual(expected[i].Name, actual[i].Name, "platform export stable asset identity");
            int nativeSize = 2 + 5 * CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | expected[i].Pointer);
            ushort next = i + 1 < expected.Length ? expected[i + 1].Pointer : (ushort)0xa051;
            AssertEqual(next, actual[i].Pointer + nativeSize, "platform export consecutive native record boundary");
        }
    }
    /// <summary>Checks the native initial list for every full-word horizontal direction value.</summary>
    private static void VerifyPlatformInitialProgramSelection(SuperMetroidAddressSpace rom)
    {
        // LDY immediate operands in the two native initializers.
        foreach (bool isKamer in new[] { false, true })
        {
            ushort left = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | (isKamer ? (ushort)0x9caa : (ushort)0x9cbe));
            ushort right = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | (isKamer ? (ushort)0x9cb6 : (ushort)0x9cca));
            for (int direction = 0; direction <= ushort.MaxValue; direction++)
                AssertEqual(direction == 0 ? left : right,
                    NativeEnemyProgramOracle.EntryOf(PlatformNativeListAddresses.Entries,
                        PlatformPrograms.SelectProgram(isKamer, Axis.Horizontal, (PlatformHorizontalMovement)direction)),
                    "platform initial program uses full-word zero/nonzero direction");
        }
    }

    /// <summary>Checks moving and still list selection for both species and every direction value.</summary>
    private static void VerifyPlatformAnimationProgramSelection(SuperMetroidAddressSpace rom)
    {
        // Each native helper first loads Tripper's list, then substitutes Kamer's list.
        foreach (bool moving in new[] { false, true })
        foreach (bool isKamer in new[] { false, true })
        {
            ushort leftOperand = moving
                ? (isKamer ? (ushort)0x9e54 : (ushort)0x9e4b)
                : (isKamer ? (ushort)0x9e8e : (ushort)0x9e85);
            ushort rightOperand = moving
                ? (isKamer ? (ushort)0x9e71 : (ushort)0x9e68)
                : (isKamer ? (ushort)0x9eab : (ushort)0x9ea2);
            ushort left = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | leftOperand);
            ushort right = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | rightOperand);
            for (int direction = 0; direction <= ushort.MaxValue; direction++)
                AssertEqual(direction == 0 ? left : right,
                    NativeEnemyProgramOracle.EntryOf(PlatformNativeListAddresses.Entries,
                        PlatformPrograms.SelectProgram(isKamer, moving ? Axis.Vertical : Axis.Horizontal, (PlatformHorizontalMovement)direction)),
                    "platform animation program uses species, motion and full-word direction");
        }
    }
    /// <summary>Checks the native zero-versus-nonzero direction choice for frozen Tripper frames.</summary>
    private static void VerifyTripperFrozenFrameSelection(SuperMetroidAddressSpace rom)
    {
        // Immediate operands of the native zero/nonzero branches, not an inferred pointer table.
        ushort left = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | 0x9f23);
        ushort right = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | 0x9f1b);
        AssertEqual(left, TripperKamerVisualDefinitions.FrozenMovingLeft, "native frozen-left sprite identity");
        AssertEqual(right, TripperKamerVisualDefinitions.FrozenMovingRight, "native frozen-right sprite identity");
        for (int direction = 0; direction <= ushort.MaxValue; direction++)
            AssertEqual(direction == 0 ? left : right,
                TripperKamerVisualDefinitionsConstants.FrozenFrame((PlatformHorizontalMovement)direction),
                "Tripper frozen selection preserves complete native zero/nonzero domain");
        AssertEqual((ushort)2, CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | left), "Tripper frozen-left two-piece map");
        AssertEqual((ushort)2, CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | right), "Tripper frozen-right two-piece map");
    }
    /// <summary>One expected platform program for a species, motion state, and horizontal direction.</summary>
    internal readonly record struct PlatformProgramCase(
        bool IsKamer,
        bool IsMoving,
        PlatformHorizontalMovement Direction,
        PlatformProgram Program);

    /// <summary>Runs the standalone Tripper and Kamer instruction verification command.</summary>
    [VerificationCommand("--platform-instruction-program-definitions")]
    internal static int PlatformInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyPlatformInstructionProgramDefinitions), () => VerifyPlatformInstructionProgramDefinitions());
        return 0;
    }
}
