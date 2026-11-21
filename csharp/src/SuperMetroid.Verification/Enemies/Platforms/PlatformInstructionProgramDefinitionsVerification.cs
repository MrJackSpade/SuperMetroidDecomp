using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
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
        CartridgeImportAddressSpace rom)
    {

        Suite(nameof(VerifyTripperFrozenFrameSelection), () => VerifyTripperFrozenFrameSelection(rom));
        Suite(nameof(VerifyPlatformInitialProgramSelection), () => VerifyPlatformInitialProgramSelection(rom));
        Suite(nameof(VerifyPlatformAnimationProgramSelection), () => VerifyPlatformAnimationProgramSelection(rom));

        Action<RoomEnemyPlace, PlatformEnemyState> installMoving = PlatformsBehavior.StartPlatformMovingAnimationForHeading;
        Action<RoomEnemyPlace, PlatformEnemyState> installStill = PlatformsBehavior.StartPlatformStillAnimationForHeading;

        PlatformProgramCase[] cases =
        [
            new(true, true, HorizontalDirection.Left,
                PlatformSegment.SuspensorPlatformHeadingLeft),
            new(true, true, HorizontalDirection.Right,
                PlatformSegment.SuspensorPlatformHeadingRight),
            new(true, false, HorizontalDirection.Left,
                PlatformSegment.SuspensorPlatformHeadingLeft),
            new(true, false, HorizontalDirection.Right,
                PlatformSegment.SuspensorPlatformHeadingRight),
            new(false, true, HorizontalDirection.Left,
                PlatformSegment.TripperMovingLeft),
            new(false, true, HorizontalDirection.Right,
                PlatformSegment.TripperMovingRight),
            new(false, false, HorizontalDirection.Left,
                PlatformSegment.TripperStillMovingLeft),
            new(false, false, HorizontalDirection.Right,
                PlatformSegment.TripperStillMovingRight),
        ];

        foreach (PlatformProgramCase testCase in cases)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(testCase.IsKamer
                ? EnemyDefinitionId.SuspensorPlatform
                : EnemyDefinitionId.Tripper, place.KeptDefinition);
            place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            place.SetSpawnParameter2(EnemyDefinitionId.Tripper, 0x0101);
            place.SetSpawnSettings(place.HeldOccupant().SpawnSettings<PlatformSpawnSettings>() with { Movement = testCase.Direction });
            place.Position = place.Position.WithWholeY(0x0100);
            PlatformsBehavior.InitializePlatform(place.HeldOccupant().SpawnSettings<PlatformSpawnSettings>(), place);

            PlatformEnemyState state = (enemies.PlatformStates[0] ?? throw new InvalidOperationException("Verification requires enemies.PlatformStates[0]."));
            PlatformSegment expectedInitial = testCase.IsKamer
                ? testCase.Direction == HorizontalDirection.Left
                    ? PlatformSegment.SuspensorPlatformHeadingLeft
                    : PlatformSegment.SuspensorPlatformHeadingRight
                : testCase.Direction == HorizontalDirection.Left
                    ? PlatformSegment.TripperStillMovingLeft
                    : PlatformSegment.TripperStillMovingRight;
            AssertEqual(new AnimationPlayback<PlatformSegment>(PlatformAnimations.StartingWith(expectedInitial), AnimationPlace.StartOfAnimation), state.Playback,
                $"real {(testCase.IsKamer ? "Kamer" : "Tripper")} initializer " +
                $"selects {testCase.Direction} still program");

            (testCase.IsMoving ? installMoving : installStill)(place, (state));
            AssertEqual(new AnimationPlayback<PlatformSegment>(PlatformAnimations.StartingWith(testCase.Program), AnimationPlace.StartOfAnimation), state.Playback,
                $"production platform installer selects {testCase.Program}");

            // The direction cue runs with the first pose; four more updates show the rest of the four-pose loop and
            // return to the same place in it.
            state.XMovement = testCase.Direction == HorizontalDirection.Left
                ? HorizontalDirection.Right
                : HorizontalDirection.Left;
            ExecutePlatformProgram(place, callCount: 1);
            AssertEqual(testCase.Direction, state.XMovement, $"platform program {testCase.Program} publishes direction");
            AnimationPlayback<PlatformSegment>? firstPose = state.Playback;
            ExecutePlatformProgram(place, callCount: 4);
            AssertEqual(firstPose, state.Playback, $"platform program {testCase.Program} loops through four poses");
        }

        Suite(nameof(VerifyPlatformVisualPointers), () => VerifyPlatformVisualPointers(rom));
        Suite(nameof(VerifyPlatformExportIdentities), () => VerifyPlatformExportIdentities(rom));
        Console.WriteLine(
            "Tripper/Kamer: program selection, all eight production-installed programs' direction cues and loops, " +
            "the shown spritemaps and the export identities pass.");
        Suite(nameof(PlatformAnimationsVerification.VerifyPlatformAnimations), () => PlatformAnimationsVerification.VerifyPlatformAnimations(rom));
    }

    /// <summary>Forces the selected platform's instruction timer across several interpreter calls.</summary>
    private static void ExecutePlatformProgram(RoomEnemyPlace place,
        int callCount)
    {
        object?[] arguments =
            [place, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
        for (int call = 0; call < callCount; call++)
        {
            place.AnimationClock.StepOnNextUpdate();
            EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>
    /// Checks that the programs show exactly the fourteen animated pictures and their native sprite counts; the long jet's
    /// second records store its picture again (#2272), and the lights' records store the Kamer's bank-$A2 light pictures
    /// again (#2307).
    /// </summary>
    private static void VerifyPlatformVisualPointers(CartridgeImportAddressSpace rom)
    {
        ushort[] lights = [0xa021, 0xa02d, 0xa039, 0xa045];
        ushort[] kamerLights = [0xf468, 0xf474, 0xf480, 0xf48c];
        ushort[] threeEntryTripper = [0x9f29, 0x9f3a, 0x9f5c, 0x9f6d, 0x9fa5, 0x9fe2];
        ushort[] fourEntryTripper = [0x9f8f, 0x9fb6, 0x9fcc, 0x9ff3];
        foreach (ushort pointer in lights)
            AssertEqual((ushort)2, rom.ReadCartridgeWordFixedBank(0xa30000 | pointer), "platform light native two-entry map");
        AssertTrue(lights.Select(pointer => SuperMetroid.AssetExtraction.Enemies.EnemyFrameCartridgeLookupTooling.TryFrameAt(0xa3, pointer))
                .SequenceEqual(kamerLights.Select(pointer => SuperMetroid.AssetExtraction.Enemies.EnemyFrameCartridgeLookupTooling.TryFrameAt(0xa2, pointer))),
            "the suspensor platform's light records show the Kamer's light frames");
        foreach (ushort pointer in threeEntryTripper)
            AssertEqual((ushort)3, rom.ReadCartridgeWordFixedBank(0xa30000 | pointer), "Tripper native three-entry map");
        foreach (ushort pointer in fourEntryTripper)
            AssertEqual((ushort)4, rom.ReadCartridgeWordFixedBank(0xa30000 | pointer), "Tripper native four-entry map");
        AssertTrue(PlatformAnimationsConstants.Spritemaps(Enum.GetValues<PlatformSegment>()).ToHashSet()
                .SetEquals(lights.Concat(threeEntryTripper).Concat(fourEntryTripper)),
            "platform programs show exactly the fourteen animated pictures");
    }
    /// <summary>Checks published frame order, asset names, and consecutive native record sizes.</summary>
    private static void VerifyPlatformExportIdentities(CartridgeImportAddressSpace rom)
    {
        // Independent published export order and names; addresses are the native map labels.
        PlatformInstructionProgramDefinitionsExpected[] expected =
        [
            new PlatformInstructionProgramDefinitionsExpected(0x9f29, "tripper_moving_left_0"), new PlatformInstructionProgramDefinitionsExpected(0x9f3a, "tripper_moving_left_1"),
            new PlatformInstructionProgramDefinitionsExpected(0x9f5c, "tripper_moving_right_0"), new PlatformInstructionProgramDefinitionsExpected(0x9f6d, "tripper_moving_right_1"),
            new PlatformInstructionProgramDefinitionsExpected(0x9f8f, "tripper_still_moving_left_0"), new PlatformInstructionProgramDefinitionsExpected(0x9fa5, "tripper_still_moving_left_1"),
            new PlatformInstructionProgramDefinitionsExpected(0x9fb6, "tripper_still_moving_left_2"), new PlatformInstructionProgramDefinitionsExpected(0x9fcc, "tripper_still_moving_right_0"),
            new PlatformInstructionProgramDefinitionsExpected(0x9fe2, "tripper_still_moving_right_1"), new PlatformInstructionProgramDefinitionsExpected(0x9ff3, "tripper_still_moving_right_2"),
            new PlatformInstructionProgramDefinitionsExpected(0xa009, "tripper_frozen_moving_left"), new PlatformInstructionProgramDefinitionsExpected(0xa015, "tripper_frozen_moving_right"),
        ];
        NativeEnemySpritemapDefinition[] actual = TripperKamerVisualDefinitions.Frames();
        AssertEqual(expected.Length, actual.Length, "platform export frame count");
        for (int i = 0; i < expected.Length; i++)
        {
            AssertEqual((byte)0xa3, actual[i].Bank, "platform export bank");
            AssertEqual(expected[i].Pointer, actual[i].Pointer, "platform export native map pointer and order");
            AssertEqual(expected[i].Name, actual[i].Name, "platform export stable asset identity");
            ushort next = i + 1 < expected.Length ? expected[i + 1].Pointer : (ushort)0xa051;
            AssertEqual(next, RecordAfter(rom, actual[i].Pointer), "platform export consecutive native record boundary");
        }
    }
    /// <summary>
    /// Where the installed record after the one at <paramref name="pointer"/> starts: past any record that stores an
    /// installed picture again, such as the long jet's second records (#2272) and the lights (#2307).
    /// </summary>
    private static int RecordAfter(CartridgeImportAddressSpace rom, ushort pointer)
    {
        int next = pointer + 2 + 5 * rom.ReadCartridgeWordFixedBank(0xa30000 | pointer);
        while (SuperMetroid.AssetExtraction.Enemies.EnemyFrameCartridgeLocationDefinitions.AlsoStoredAt.ContainsKey(new(0xa3, (ushort)next)))
            next += 2 + 5 * rom.ReadCartridgeWordFixedBank(0xa30000 | next);
        return next;
    }

    /// <summary>Checks the native initial list for both horizontal directions.</summary>
    private static void VerifyPlatformInitialProgramSelection(CartridgeImportAddressSpace rom)
    {
        // LDY immediate operands in the two native initializers.
        foreach (bool isKamer in new[] { false, true })
        {
            ushort left = rom.ReadCartridgeWordFixedBank(0xa30000 | (isKamer ? (ushort)0x9caa : (ushort)0x9cbe));
            ushort right = rom.ReadCartridgeWordFixedBank(0xa30000 | (isKamer ? (ushort)0x9cb6 : (ushort)0x9cca));
            foreach (HorizontalDirection direction in Enum.GetValues<HorizontalDirection>())
                AssertEqual(NativeEnemyAnimationOracle.SegmentOf(PlatformNativeListAddresses.Entries, direction == HorizontalDirection.Left ? left : right),
                    PlatformAnimationChoices.SelectAnimation(isKamer ? PlatformKind.SuspensorPlatform : PlatformKind.Tripper, PlatformVerticalArt.Still, direction),
                    $"platform initial program faces {direction}");
        }
    }

    /// <summary>Checks moving and still list selection for both species and both directions.</summary>
    private static void VerifyPlatformAnimationProgramSelection(CartridgeImportAddressSpace rom)
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
            ushort left = rom.ReadCartridgeWordFixedBank(0xa30000 | leftOperand);
            ushort right = rom.ReadCartridgeWordFixedBank(0xa30000 | rightOperand);
            foreach (HorizontalDirection direction in Enum.GetValues<HorizontalDirection>())
                AssertEqual(NativeEnemyAnimationOracle.SegmentOf(PlatformNativeListAddresses.Entries, direction == HorizontalDirection.Left ? left : right),
                    PlatformAnimationChoices.SelectAnimation(isKamer ? PlatformKind.SuspensorPlatform : PlatformKind.Tripper, moving ? PlatformVerticalArt.Moving : PlatformVerticalArt.Still, direction),
                    $"platform animation program uses species, motion and direction {direction}");
        }
    }
    /// <summary>Checks the native left-versus-right direction choice for frozen Tripper frames.</summary>
    private static void VerifyTripperFrozenFrameSelection(CartridgeImportAddressSpace rom)
    {
        // Immediate operands of the native zero/nonzero branches, not an inferred pointer table.
        ushort left = rom.ReadCartridgeWordFixedBank(0xa30000 | 0x9f23);
        ushort right = rom.ReadCartridgeWordFixedBank(0xa30000 | 0x9f1b);
        AssertEqual(left, TripperKamerVisualDefinitions.FrozenMovingLeft, "native frozen-left sprite identity");
        AssertEqual(right, TripperKamerVisualDefinitions.FrozenMovingRight, "native frozen-right sprite identity");
        foreach (HorizontalDirection direction in Enum.GetValues<HorizontalDirection>())
            AssertEqual(direction == HorizontalDirection.Left ? left : right,
                TripperKamerVisualDefinitionsConstants.FrozenFrame(direction),
                $"Tripper frozen selection faces {direction}");
        AssertEqual((ushort)2, rom.ReadCartridgeWordFixedBank(0xa30000 | left), "Tripper frozen-left two-piece map");
        AssertEqual((ushort)2, rom.ReadCartridgeWordFixedBank(0xa30000 | right), "Tripper frozen-right two-piece map");
    }
    /// <summary>One expected platform program for a species, motion state, and horizontal direction.</summary>
    internal readonly record struct PlatformProgramCase(
        bool IsKamer,
        bool IsMoving,
        HorizontalDirection Direction,
        PlatformSegment Program);

    /// <summary>Runs the standalone Tripper and Kamer instruction verification command.</summary>
    [VerificationCommand("--platform-instruction-program-definitions")]
    internal static int PlatformInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyPlatformInstructionProgramDefinitions), () => VerifyPlatformInstructionProgramDefinitions());
        return 0;
    }
}
