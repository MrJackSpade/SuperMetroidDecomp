using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Retail comparison for Botwoon's four hole bounds, 32 path choices and authored paths.</summary>
/// <remarks>
/// The cartridge names holes by eight-byte table offsets, path choices by eight-byte descriptor offsets and movement
/// samples by two-byte bank-$B3 pointers into one sample table. This oracle and <see cref="BotwoonPathCodec"/> own
/// those translations; the simulation holds hole indices, path choices, and path and step indices.
/// </remarks>
internal static class BotwoonNavigationDefinitionsVerification
{
    /// <summary>Bytes per cartridge hole record and per path descriptor record.</summary>
    private const int RecordBytes = 8;

    /// <summary>The bytes the cartridge rewinds a backward walk's sample pointer by before its first step.</summary>
    private const int BackwardRewindBytes = 4;

    /// <summary>Checks each hole's exclusive right edge through the production detection routine and rejects invalid hole indices.</summary>
    internal static void VerifyBotwoonHoleRightBounds(SuperMetroidAddressSpace rom)
    {
        Action<RoomEnemySlot, BotwoonEnemyState> detect = BotwoonBehavior.DetectBotwoonHole;
        for (int offset = 0; offset < 32; offset += 8)
        {
            ushort native = ReadRomWord(rom, 0xb3949d + offset);
            AssertEqual(native, BotwoonNavigation.HoleAt(offset / RecordBytes).BottomRight.X, "Botwoon native right boundary");
            var head = new RoomEnemySystem(new EnemySession()).SlotTable.Slots[0];
            head.Position = head.Position.WithWholeY(ReadRomWord(rom, 0xb3949f + offset));
            foreach (int x in new[] {native - 1, native})
            {
                head.Position = head.Position.WithWholeX((ushort)x);
                var state = new BotwoonEnemyState();
                detect(head, state);
                AssertEqual(x < native, state.InsideHole, "Botwoon right boundary is exclusive");
            }
        }
        AssertEqual(4, BotwoonNavigationDefinitions.HoleCount, "Botwoon has four holes");
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<InvalidDataException>(() => BotwoonNavigation.HoleAt(invalid), "Botwoon hole index domain");
    }

    /// <summary>Checks each hole's exclusive bottom edge through the production detection routine.</summary>
    internal static void VerifyBotwoonHoleBottomBounds(SuperMetroidAddressSpace rom)
    {
        Action<RoomEnemySlot, BotwoonEnemyState> detect = BotwoonBehavior.DetectBotwoonHole;
        for (int offset = 0; offset < 32; offset += 8)
        {
            ushort native = ReadRomWord(rom, 0xb394a1 + offset);
            AssertEqual(native, BotwoonNavigation.HoleAt(offset / RecordBytes).BottomRight.Y, "Botwoon native bottom boundary");
            var head = new RoomEnemySystem(new EnemySession()).SlotTable.Slots[0];
            head.Position = head.Position.WithWholeX(ReadRomWord(rom, 0xb3949b + offset));
            foreach (int y in new[] {native - 1, native})
            {
                head.Position = head.Position.WithWholeY((ushort)y);
                var state = new BotwoonEnemyState();
                detect(head, state);
                AssertEqual(y < native, state.InsideHole, "Botwoon bottom boundary is exclusive");
            }
        }
    }

    /// <summary>Matches all 32 native path descriptors and checks their installation into live Botwoon movement state.</summary>
    internal static void VerifyBotwoonPathDescriptorMappings(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyBotwoonPathStartMapping), () => VerifyBotwoonPathStartMapping(rom));
        Suite(nameof(VerifyBotwoonPathDirectionMapping), () => VerifyBotwoonPathDirectionMapping(rom));
        Suite(nameof(VerifyBotwoonPathDestinationMapping), () => VerifyBotwoonPathDestinationMapping(rom));
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        Action<RoomEnemySlot, BotwoonEnemyState> run = BotwoonBehavior.RunBotwoonMovement;
        for (int offset = 0; offset < 256; offset += 8)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot head = enemies.SlotTable.Slots[0];
            var state = new BotwoonEnemyState() { MovementFunction = BotwoonMovementFunction.LoadAuthoredPath,
                PathChoice = offset / RecordBytes, Speed = 0 };
            ushort pointer = ReadRomWord(rom, 0xb3e150 + offset);
            short direction = (short)ReadRomWord(rom, 0xb3e152 + offset);
            run(head, state);
            BotwoonPathPosition start = BotwoonPathCodec.Locate(cartridge, direction < 0 ? pointer - BackwardRewindBytes : pointer);
            AssertEqual(start.Path, state.PathIndex, "Botwoon production path");
            AssertEqual(start.Step, state.PathStep, "Botwoon production first step and backward rewind");
            AssertEqual(direction < 0 ? BotwoonPathTraversal.Backward : BotwoonPathTraversal.Forward, state.PathTraversal,
                "Botwoon production path direction");
            AssertEqual(ReadRomWord(rom, 0xb3e154 + offset), RecordBytes * state.TargetHoleIndex, "Botwoon production destination");
            AssertEqual(BotwoonMovementFunction.FollowAuthoredPath, state.MovementFunction, "Botwoon path handoff");
        }
        AssertEqual(32, BotwoonNavigationDefinitions.PathChoiceCount, "Botwoon has 32 path choices");
        foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<InvalidDataException>(() => BotwoonNavigation.PathForChoice(invalid), "Botwoon descriptor rejects every invalid path choice");
        Console.WriteLine("Botwoon path descriptors: all 32 path, first-step, direction and destination fields and production installations match native records; invalid choices rejected.");
    }

    /// <summary>
    /// Checks the table's separators, including the distinct early terminator, and that each descriptor's sample
    /// pointer starts its walk on the path and step the typed descriptor names.
    /// </summary>
    private static void VerifyBotwoonPathStartMapping(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort[] boundaries = [0xa058,0xa328,0xa6ba,0xaa22,0xadfc,0xb168,0xb554,0xb954,
            0xbc84,0xc084,0xc28e,0xc68e,0xc9ca,0xcdca,0xd13e,0xd4a0,0xd87e,
            0xda00,0xdb9a,0xdd40,0xde7c,0xdfde,0xe14e];
        foreach (ushort pointer in boundaries)
            AssertEqual((ushort)0x0080, ReadRomWord(rom, 0xb30000 | pointer), "Botwoon native stream separator");
        AssertTrue(boundaries.Select(pointer => 0xb30000 | pointer).SequenceEqual(BotwoonPathCodec.SeparatorAddresses(cartridge)),
            "Botwoon separators are exactly the native stream boundaries");
        AssertEqual((ushort)0xf080, ReadRomWord(rom, 0xb3a6b4), "Botwoon early terminal is distinct from stream separator");
        for (int offset = 0; offset < 256; offset += 8)
        {
            ushort pointer = ReadRomWord(rom, 0xb3e150 + offset);
            short direction = (short)ReadRomWord(rom, 0xb3e152 + offset);
            BotwoonPathDescriptorDefinition descriptor = BotwoonNavigation.PathForChoice(offset / RecordBytes);
            BotwoonPathPosition start = BotwoonPathCodec.Locate(cartridge, direction < 0 ? pointer - BackwardRewindBytes : pointer);
            AssertEqual(start.Path, descriptor.Path, "Botwoon native descriptor path");
            AssertEqual(start.Step, descriptor.FirstStep, "Botwoon native descriptor first step");
        }
    }

    /// <summary>Checks forward and reverse traversal words for every authored path choice.</summary>
    private static void VerifyBotwoonPathDirectionMapping(SuperMetroidAddressSpace rom)
    {
        for (int offset = 0; offset < 256; offset += 8)
            AssertEqual((short)ReadRomWord(rom, 0xb3e152 + offset) < 0 ? BotwoonPathTraversal.Backward : BotwoonPathTraversal.Forward,
                BotwoonNavigation.PathForChoice(offset / RecordBytes).Traversal, "Botwoon native traversal direction");
    }

    /// <summary>Checks destination holes, including the repeated hidden choice in the native table.</summary>
    private static void VerifyBotwoonPathDestinationMapping(SuperMetroidAddressSpace rom)
    {
        for (int offset = 0; offset < 256; offset += 8)
            AssertEqual(ReadRomWord(rom, 0xb3e154 + offset),
                RecordBytes * BotwoonNavigation.PathForChoice(offset / RecordBytes).TargetHoleIndex, "Botwoon native destination including repeated hidden choice");
    }

    /// <summary>
    /// Compares hole rectangles, the authored paths and every step's production movement in both directions against
    /// the retail tables.
    /// </summary>
    internal static void VerifyBotwoonNavigationDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyBotwoonHoleRightBounds), () => VerifyBotwoonHoleRightBounds(rom));
        Suite(nameof(VerifyBotwoonHoleBottomBounds), () => VerifyBotwoonHoleBottomBounds(rom));
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);

        var holes = new BotwoonHoleDefinition[4];
        for (int holeIndex = 0; holeIndex < holes.Length; holeIndex++)
        {
            int address = 0xb3949b + RecordBytes * holeIndex;
            BotwoonHoleDefinition hole = BotwoonNavigation.HoleAt(holeIndex);
            holes[holeIndex] = hole;
            AssertEqual(cartridge.ReadCartridgeWord(address), hole.TopLeft.X, $"Botwoon hole {holeIndex} left");
            AssertEqual(cartridge.ReadCartridgeWord((address + 4)), hole.TopLeft.Y, $"Botwoon hole {holeIndex} top");
        }

        Action<RoomEnemySlot, BotwoonEnemyState> runMovement = BotwoonBehavior.RunBotwoonMovement;
        for (int choice = 0; choice < BotwoonNavigationDefinitions.PathChoiceCount; choice++)
        {
            int address = 0xb3e150 + RecordBytes * choice;
            BotwoonPathDescriptorDefinition descriptor =
                BotwoonNavigation.PathForChoice(choice);
            AssertEqual((ushort)0, cartridge.ReadCartridgeWord((address + 6)),
                $"Botwoon path {choice} alignment word");

            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot head = enemies.SlotTable.Slots[0];
            var state = new BotwoonEnemyState()
            {
                MovementFunction = BotwoonMovementFunction.LoadAuthoredPath,
                PathChoice = choice,
                Speed = 0,
            };
            runMovement(head, state);
            AssertEqual(descriptor.Path, state.PathIndex, $"Botwoon production path {choice} path");
            AssertEqual(descriptor.FirstStep, state.PathStep, $"Botwoon production path {choice} first step");
            AssertEqual(descriptor.Traversal, state.PathTraversal, $"Botwoon production path {choice} direction");
            AssertEqual(descriptor.TargetHoleIndex, state.TargetHoleIndex,
                $"Botwoon production path {choice} target hole");
            AssertEqual(BotwoonMovementFunction.FollowAuthoredPath, state.MovementFunction,
                $"Botwoon production path {choice} handoff");
        }

        // The typed paths are the cartridge's, step for step.
        BotwoonPathStep[][] native = BotwoonPathCodec.Paths(cartridge);
        AssertEqual(native.Length, BotwoonPathDefinitions.Paths.Length, "Botwoon authored path count");
        for (int path = 0; path < native.Length; path++)
        {
            AssertEqual(native[path].Length, BotwoonPathDefinitions.Paths[path].Length, $"Botwoon path {path} length");
            for (int step = 0; step < native[path].Length; step++)
                AssertEqual(native[path][step], BotwoonPathDefinitions.Paths[path][step], $"Botwoon path {path} step {step}");
        }

        // Every position a walk can reach, one before the first step through one after the last, moves the head by
        // the cartridge's sample at that position, or ends the walk where the cartridge reads a marker.
        var movementEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot movementHead = movementEnemies.SlotTable.Slots[0];
        List<int> separators = BotwoonPathCodec.SeparatorAddresses(cartridge);
        int movementStepCount = 0;
        for (int path = 0; path < BotwoonPathDefinitions.Paths.Length; path++)
        {
            for (int step = -1; step <= BotwoonPathDefinitions.Paths[path].Length; step++)
            {
                int address = BotwoonPathCodec.AddressOf(separators, path, step);
                Offset16<RoomPixels> expected = BotwoonPathCodec.Sample(cartridge, address);
                bool terminates = BotwoonPathCodec.IsMarker(cartridge, address);
                foreach (BotwoonPathTraversal traversal in Enum.GetValues<BotwoonPathTraversal>())
                {
                    movementHead.Position = movementHead.Position.WithWhole(new(0x8000, 0x8000));
                    var state = new BotwoonEnemyState()
                    {
                        MovementFunction = BotwoonMovementFunction.FollowAuthoredPath,
                        PathIndex = path,
                        PathStep = step,
                        PathTraversal = traversal,
                        Speed = 1,
                    };
                    runMovement(movementHead, state);

                    string context = $"Botwoon path {path} step {step} ${address:X6} {traversal}";
                    AssertEqual(terminates, state.PathComplete, $"{context} completion");
                    if (terminates)
                    {
                        AssertEqual(step, state.PathStep, $"{context} terminating step");
                        AssertEqual((ushort)0x8000, movementHead.Position.X.Whole, $"{context} terminating X");
                        AssertEqual((ushort)0x8000, movementHead.Position.Y.Whole, $"{context} terminating Y");
                        continue;
                    }

                    int sign = traversal == BotwoonPathTraversal.Backward ? -1 : 1;
                    AssertEqual(unchecked((ushort)(0x8000 + sign * expected.X)), movementHead.Position.X.Whole, $"{context} X");
                    AssertEqual(unchecked((ushort)(0x8000 + sign * expected.Y)), movementHead.Position.Y.Whole, $"{context} Y");
                    AssertEqual(step + sign, state.PathStep, $"{context} next step");
                }
                movementStepCount++;
            }
        }

        Action<RoomEnemySlot, BotwoonEnemyState> detectHole = BotwoonBehavior.DetectBotwoonHole;
        for (int holeIndex = 0; holeIndex < holes.Length; holeIndex++)
        {
            BotwoonHoleDefinition hole = holes[holeIndex];
            var head = new RoomEnemySystem(new EnemySession()).SlotTable.Slots[0];
            var state = new BotwoonEnemyState() { RingIndex = 141 };
            head.Position = head.Position.WithWhole(hole.TopLeft);
            detectHole(head, state);
            AssertTrue(state.InsideHole, $"Botwoon hole {holeIndex} includes left/top");
            AssertTrue(state.HoleLatch, $"Botwoon hole {holeIndex} latches");
            AssertEqual(141, state.SavedHoleRingIndex,
                $"Botwoon hole {holeIndex} saves ring index");

            state.HoleLatch = false;
            state.InsideHole = false;
            head.Position = head.Position.WithWholeX(hole.BottomRight.X);
            detectHole(head, state);
            AssertTrue(!state.InsideHole,
                $"Botwoon hole {holeIndex} excludes right boundary");
            state.HoleLatch = false;
            head.Position = head.Position.WithWhole(hole.TopLeft.WithY(hole.BottomRight.Y));
            detectHole(head, state);
            AssertTrue(!state.InsideHole,
                $"Botwoon hole {holeIndex} excludes bottom boundary");
        }

        for (int holeIndex = 0; holeIndex < holes.Length; holeIndex++)
        {
            BotwoonHoleDefinition hole = holes[holeIndex];
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot head = enemies.SlotTable.Slots[0];
            head.Position = head.Position.WithWhole(hole.Target);
            var state = new BotwoonEnemyState()
            {
                MovementFunction = BotwoonMovementFunction.MoveTowardHole,
                TargetHoleIndex = holeIndex,
                Speed = 0,
            };
            runMovement(head, state);
            // At the exact target the cartridge's angle routine yields 129; the movement angle is a quarter turn minus it.
            AssertEqual(RotationAngle.FromTableIndex(191), state.MovementAngle,
                $"Botwoon hole {holeIndex} exact-target movement angle");
            AssertEqual(hole.Target.X, head.Position.X.Whole,
                $"Botwoon hole {holeIndex} exact-target X");
            AssertEqual(hole.Target.Y, head.Position.Y.Whole,
                $"Botwoon hole {holeIndex} exact-target Y");
        }

        AssertThrows<InvalidDataException>(
            () => BotwoonNavigation.HoleAt(-1),
            "Botwoon hole index before table");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigation.HoleAt(4),
            "Botwoon hole index past table");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigation.PathForChoice(-1),
            "Botwoon path choice before table");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigation.PathForChoice(32),
            "Botwoon path choice past table");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigation.HasStep(0, -2),
            "Botwoon path position before the walk's end");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigation.HasStep(0, BotwoonPathDefinitions.Paths[0].Length + 1),
            "Botwoon path position past the walk's end");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigation.HasStep(BotwoonPathDefinitions.Paths.Length, 0),
            "Botwoon path past the authored paths");

        var invalidEnemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot invalidHead = invalidEnemies.SlotTable.Slots[0];
        var invalidState = new BotwoonEnemyState()
        {
            MovementFunction = BotwoonMovementFunction.LoadAuthoredPath,
            PathChoice = BotwoonNavigationDefinitions.PathChoiceCount,
            PathIndex = 7,
            PathStep = 77,
            PathTraversal = BotwoonPathTraversal.Backward,
            TargetHoleIndex = 3,
        };
        AssertThrows<InvalidDataException>(
            () => runMovement(invalidHead, invalidState),
            "Botwoon invalid production path selector");
        AssertEqual(7, invalidState.PathIndex,
            "Botwoon invalid path leaves path unchanged");
        AssertEqual(77, invalidState.PathStep,
            "Botwoon invalid path leaves step unchanged");
        AssertEqual(BotwoonPathTraversal.Backward, invalidState.PathTraversal,
            "Botwoon invalid path leaves direction unchanged");
        AssertEqual(3, invalidState.TargetHoleIndex,
            "Botwoon invalid path leaves target unchanged");
        AssertEqual(BotwoonMovementFunction.LoadAuthoredPath, invalidState.MovementFunction,
            "Botwoon invalid path leaves callback unchanged");

        Console.WriteLine(
            $"Botwoon navigation definitions: 144 native words, all {BotwoonPathDefinitions.Paths.Length} authored " +
            $"paths step for step, {movementStepCount:N0} walk positions in both directions, all 32 real descriptor " +
            "handoffs, four movement targets and rectangle boundaries pass.");
    }


    /// <summary>Runs Botwoon navigation definition verification against the retail cartridge.</summary>
    [VerificationCommand("--botwoon-navigation-definitions")]
    internal static int BotwoonNavigationDefinitionsCommand()
    {
        Suite(nameof(VerifyBotwoonNavigationDefinitions), () => VerifyBotwoonNavigationDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
