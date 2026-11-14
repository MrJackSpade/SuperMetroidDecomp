using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Retail comparison for Botwoon's four hole bounds, 32 path descriptors and authored paths.</summary>
/// <remarks>
/// The cartridge names holes by eight-byte table offsets, walks by eight-byte descriptor offsets (four per hole, the
/// sixteen in the open before the sixteen inside the wall) and movement samples by two-byte bank-$B3 pointers into one
/// sample table. This oracle and <see cref="BotwoonPathCodec"/> own those translations; the simulation holds typed
/// holes, draws and paths (#1868).
/// </remarks>
internal static class BotwoonNavigationDefinitionsVerification
{
    /// <summary>Bytes per cartridge hole record and per path descriptor record.</summary>
    private const int RecordBytes = 8;

    /// <summary>The bytes the cartridge rewinds a backward walk's sample pointer by before its first step.</summary>
    private const int BackwardRewindBytes = 4;

    /// <summary>The cartridge's path descriptors: four draws for each of the four holes, in the open then inside the wall.</summary>
    private const int DescriptorCount = 32;

    /// <summary>
    /// The hole, route and draw the cartridge's descriptor <paramref name="descriptor"/> stands for: the first sixteen
    /// run in the open and the rest inside the wall, four draws per hole in the hole table's order.
    /// </summary>
    private static (BotwoonHole From, bool InsideWall, int Draw) DescriptorWalk(int descriptor) =>
        (BotwoonNavigationDefinitions.Holes[descriptor % 16 / 4].Hole, descriptor >= 16, descriptor % 4);

    /// <summary>The hole at the cartridge's hole table position <paramref name="holeIndex"/>.</summary>
    private static BotwoonHoleDefinition HoleAtTablePosition(int holeIndex) => BotwoonNavigationDefinitions.Holes[holeIndex];

    /// <summary>The cartridge's position in its path table of the authored path <paramref name="path"/>.</summary>
    private static int TablePosition(BotwoonPathDefinition path) => Array.IndexOf(BotwoonPathDefinitions.Paths, path);

    /// <summary>The walk production starts for the cartridge's descriptor <paramref name="descriptor"/>.</summary>
    private static BotwoonPathWalk WalkOf(int descriptor)
    {
        (BotwoonHole from, bool insideWall, int draw) = DescriptorWalk(descriptor);
        return BotwoonNavigation.WalkFrom(from, insideWall, draw);
    }

    /// <summary>Checks each hole's exclusive right edge through the production detection routine.</summary>
    internal static void VerifyBotwoonHoleRightBounds(CartridgeImportAddressSpace rom)
    {
        Action<RoomEnemySlot, BotwoonEnemyState> detect = BotwoonBehavior.DetectBotwoonHole;
        for (int offset = 0; offset < 32; offset += 8)
        {
            ushort native = ReadRomWord(rom, 0xb3949d + offset);
            AssertEqual(native, HoleAtTablePosition(offset / RecordBytes).BottomRight.X, "Botwoon native right boundary");
            var head = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom()).SlotTable.Slots[0];
            head.Position = head.Position.WithWholeY(ReadRomWord(rom, 0xb3949f + offset));
            foreach (int x in new[] {native - 1, native})
            {
                head.Position = head.Position.WithWholeX((ushort)x);
                var state = new BotwoonEnemyState();
                detect(head, state);
                AssertEqual(x < native, state.InsideHole, "Botwoon right boundary is exclusive");
            }
        }
        AssertEqual(4, BotwoonNavigationDefinitions.Holes.Length, "Botwoon has four holes");
        AssertTrue(Enum.GetValues<BotwoonHole>().All(hole => BotwoonNavigation.Hole(hole).Hole == hole), "Each Botwoon hole has its rectangle");
    }

    /// <summary>Checks each hole's exclusive bottom edge through the production detection routine.</summary>
    internal static void VerifyBotwoonHoleBottomBounds(CartridgeImportAddressSpace rom)
    {
        Action<RoomEnemySlot, BotwoonEnemyState> detect = BotwoonBehavior.DetectBotwoonHole;
        for (int offset = 0; offset < 32; offset += 8)
        {
            ushort native = ReadRomWord(rom, 0xb394a1 + offset);
            AssertEqual(native, HoleAtTablePosition(offset / RecordBytes).BottomRight.Y, "Botwoon native bottom boundary");
            var head = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom()).SlotTable.Slots[0];
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
    internal static void VerifyBotwoonPathDescriptorMappings(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyBotwoonPathStartMapping), () => VerifyBotwoonPathStartMapping(rom));
        Suite(nameof(VerifyBotwoonPathDirectionMapping), () => VerifyBotwoonPathDirectionMapping(rom));
        Suite(nameof(VerifyBotwoonPathDestinationMapping), () => VerifyBotwoonPathDestinationMapping(rom));
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        for (int offset = 0; offset < DescriptorCount * RecordBytes; offset += RecordBytes)
        {
            var state = new BotwoonEnemyState() { Movement = BotwoonMovement.MoveTowardHole, PathComplete = true };
            BotwoonBehavior.StartBotwoonWalk(state, WalkOf(offset / RecordBytes));
            ushort pointer = ReadRomWord(rom, 0xb3e150 + offset);
            short direction = (short)ReadRomWord(rom, 0xb3e152 + offset);
            BotwoonPathPosition start = BotwoonPathCodec.Locate(cartridge, direction < 0 ? pointer - BackwardRewindBytes : pointer);
            AssertEqual(start.Path, TablePosition(state.Path ?? throw new InvalidOperationException("The walk sets a path.")), "Botwoon production path");
            AssertEqual(start.Step, state.PathStep, "Botwoon production first step and backward rewind");
            AssertEqual(direction < 0 ? BotwoonPathTraversal.Backward : BotwoonPathTraversal.Forward, state.PathTraversal,
                "Botwoon production path direction");
            AssertEqual(ReadRomWord(rom, 0xb3e154 + offset), RecordBytes * Array.FindIndex(BotwoonNavigationDefinitions.Holes, hole => hole.Hole == state.TargetHole),
                "Botwoon production destination");
            AssertEqual(BotwoonMovement.FollowAuthoredPath, state.Movement, "Botwoon walk handoff");
            AssertTrue(!state.PathComplete, "Botwoon walk starts incomplete");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<InvalidDataException>(() => BotwoonNavigation.WalkFrom(BotwoonHole.Left, insideWall: false, invalid), "Botwoon rejects every invalid draw");
        Console.WriteLine("Botwoon path descriptors: all 32 path, first-step, direction and destination fields and production installations match native records; invalid draws rejected.");
    }

    /// <summary>
    /// Checks the table's separators, including the distinct early terminator, and that each descriptor's sample
    /// pointer starts its walk on the path and step the typed walk names.
    /// </summary>
    private static void VerifyBotwoonPathStartMapping(CartridgeImportAddressSpace rom)
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
        for (int offset = 0; offset < DescriptorCount * RecordBytes; offset += RecordBytes)
        {
            ushort pointer = ReadRomWord(rom, 0xb3e150 + offset);
            short direction = (short)ReadRomWord(rom, 0xb3e152 + offset);
            BotwoonPathWalk walk = WalkOf(offset / RecordBytes);
            BotwoonPathPosition start = BotwoonPathCodec.Locate(cartridge, direction < 0 ? pointer - BackwardRewindBytes : pointer);
            AssertEqual(start.Path, TablePosition(walk.Path), "Botwoon native descriptor path");
            AssertEqual(start.Step, walk.FirstStep, "Botwoon native descriptor first step");
        }
    }

    /// <summary>Checks forward and reverse traversal words for every descriptor.</summary>
    private static void VerifyBotwoonPathDirectionMapping(CartridgeImportAddressSpace rom)
    {
        for (int offset = 0; offset < DescriptorCount * RecordBytes; offset += RecordBytes)
            AssertEqual((short)ReadRomWord(rom, 0xb3e152 + offset) < 0 ? BotwoonPathTraversal.Backward : BotwoonPathTraversal.Forward,
                WalkOf(offset / RecordBytes).Traversal, "Botwoon native traversal direction");
    }

    /// <summary>Checks destination holes, including the repeated wall destination in the native table.</summary>
    private static void VerifyBotwoonPathDestinationMapping(CartridgeImportAddressSpace rom)
    {
        for (int offset = 0; offset < DescriptorCount * RecordBytes; offset += RecordBytes)
        {
            BotwoonHole destination = WalkOf(offset / RecordBytes).Destination;
            AssertEqual(ReadRomWord(rom, 0xb3e154 + offset),
                RecordBytes * Array.FindIndex(BotwoonNavigationDefinitions.Holes, hole => hole.Hole == destination),
                "Botwoon native destination including repeated wall destination");
        }
    }

    /// <summary>
    /// Compares hole rectangles, the authored paths and every step's production movement in both directions against
    /// the retail tables.
    /// </summary>
    internal static void VerifyBotwoonNavigationDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyBotwoonHoleRightBounds), () => VerifyBotwoonHoleRightBounds(rom));
        Suite(nameof(VerifyBotwoonHoleBottomBounds), () => VerifyBotwoonHoleBottomBounds(rom));
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);

        BotwoonHoleDefinition[] holes = BotwoonNavigationDefinitions.Holes;
        for (int holeIndex = 0; holeIndex < holes.Length; holeIndex++)
        {
            int address = 0xb3949b + RecordBytes * holeIndex;
            AssertEqual(cartridge.ReadCartridgeWord(address), holes[holeIndex].TopLeft.X, $"Botwoon hole {holes[holeIndex].Hole} left");
            AssertEqual(cartridge.ReadCartridgeWord((address + 4)), holes[holeIndex].TopLeft.Y, $"Botwoon hole {holes[holeIndex].Hole} top");
        }
        for (int descriptor = 0; descriptor < DescriptorCount; descriptor++)
            AssertEqual((ushort)0, cartridge.ReadCartridgeWord(0xb3e150 + RecordBytes * descriptor + 6),
                $"Botwoon descriptor {descriptor} alignment word");

        // The typed paths are the cartridge's, step for step, and join the holes their walks lead between.
        BotwoonPathStep[][] native = BotwoonPathCodec.Paths(cartridge);
        AssertEqual(native.Length, BotwoonPathDefinitions.Paths.Length, "Botwoon authored path count");
        for (int path = 0; path < native.Length; path++)
        {
            BotwoonPathStep[] steps = BotwoonPathDefinitions.Paths[path].Steps;
            AssertEqual(native[path].Length, steps.Length, $"Botwoon path {path} length");
            for (int step = 0; step < native[path].Length; step++)
                AssertEqual(native[path][step], steps[step], $"Botwoon path {path} step {step}");
        }
        for (int descriptor = 0; descriptor < DescriptorCount; descriptor++)
        {
            (BotwoonHole from, bool insideWall, _) = DescriptorWalk(descriptor);
            BotwoonPathWalk walk = WalkOf(descriptor);
            AssertEqual(insideWall ? BotwoonPathRoute.InsideWall : BotwoonPathRoute.Open, walk.Path.Route, $"Botwoon descriptor {descriptor} route");
            AssertEqual(walk.Traversal == BotwoonPathTraversal.Forward ? from : walk.Path.To, walk.Traversal == BotwoonPathTraversal.Forward ? walk.Path.From : from,
                $"Botwoon descriptor {descriptor} starts at its hole");
            AssertEqual(walk.Destination, walk.Traversal == BotwoonPathTraversal.Forward ? walk.Path.To : walk.Path.From,
                $"Botwoon descriptor {descriptor} reaches its destination");
        }

        // Every position a walk can reach, one before the first step through one after the last, moves the head by
        // the cartridge's sample at that position, or ends the walk where the cartridge reads a marker.
        Action<RoomEnemySlot, BotwoonEnemyState> runMovement = BotwoonBehavior.RunBotwoonMovement;
        var movementEnemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot movementHead = movementEnemies.SlotTable.Slots[0];
        List<int> separators = BotwoonPathCodec.SeparatorAddresses(cartridge);
        int movementStepCount = 0;
        for (int path = 0; path < BotwoonPathDefinitions.Paths.Length; path++)
        {
            BotwoonPathDefinition definition = BotwoonPathDefinitions.Paths[path];
            for (int step = -1; step <= definition.Steps.Length; step++)
            {
                int address = BotwoonPathCodec.AddressOf(separators, path, step);
                Offset16<RoomPixels> expected = BotwoonPathCodec.Sample(cartridge, address);
                bool terminates = BotwoonPathCodec.IsMarker(cartridge, address);
                foreach (BotwoonPathTraversal traversal in Enum.GetValues<BotwoonPathTraversal>())
                {
                    movementHead.Position = movementHead.Position.WithWhole(new(0x8000, 0x8000));
                    var state = new BotwoonEnemyState()
                    {
                        Movement = BotwoonMovement.FollowAuthoredPath,
                        Path = definition,
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
        foreach (BotwoonHoleDefinition hole in holes)
        {
            var head = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom()).SlotTable.Slots[0];
            var state = new BotwoonEnemyState() { RingIndex = 141 };
            head.Position = head.Position.WithWhole(hole.TopLeft);
            detectHole(head, state);
            AssertTrue(state.InsideHole, $"Botwoon hole {hole.Hole} includes left/top");
            AssertTrue(state.HoleLatch, $"Botwoon hole {hole.Hole} latches");
            AssertEqual(141, state.SavedHoleRingIndex,
                $"Botwoon hole {hole.Hole} saves ring index");

            state.HoleLatch = false;
            state.InsideHole = false;
            head.Position = head.Position.WithWholeX(hole.BottomRight.X);
            detectHole(head, state);
            AssertTrue(!state.InsideHole,
                $"Botwoon hole {hole.Hole} excludes right boundary");
            state.HoleLatch = false;
            head.Position = head.Position.WithWhole(hole.TopLeft.WithY(hole.BottomRight.Y));
            detectHole(head, state);
            AssertTrue(!state.InsideHole,
                $"Botwoon hole {hole.Hole} excludes bottom boundary");
        }

        foreach (BotwoonHoleDefinition hole in holes)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot head = enemies.SlotTable.Slots[0];
            head.Position = head.Position.WithWhole(hole.Target);
            var state = new BotwoonEnemyState()
            {
                Movement = BotwoonMovement.MoveTowardHole,
                TargetHole = hole.Hole,
                Speed = 0,
            };
            runMovement(head, state);
            // At the exact target the cartridge's angle routine yields 129; the movement angle is a quarter turn minus it.
            AssertEqual(RotationAngle.FromSteps(191), state.MovementAngle,
                $"Botwoon hole {hole.Hole} exact-target movement angle");
            AssertEqual(hole.Target.X, head.Position.X.Whole,
                $"Botwoon hole {hole.Hole} exact-target X");
            AssertEqual(hole.Target.Y, head.Position.Y.Whole,
                $"Botwoon hole {hole.Hole} exact-target Y");
        }

        BotwoonPathDefinition first = BotwoonPathDefinitions.Paths[0];
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigation.HasStep(first, -2),
            "Botwoon path position before the walk's end");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigation.HasStep(first, first.Steps.Length + 1),
            "Botwoon path position past the walk's end");
        AssertThrows<InvalidOperationException>(
            () => runMovement(movementHead, new BotwoonEnemyState() { Movement = BotwoonMovement.FollowAuthoredPath, Speed = 1 }),
            "Botwoon cannot follow a path before choosing one");

        Console.WriteLine(
            $"Botwoon navigation definitions: 144 native words, all {BotwoonPathDefinitions.Paths.Length} authored " +
            $"paths step for step, {movementStepCount:N0} walk positions in both directions, all 32 descriptor " +
            "walks, four movement targets and rectangle boundaries pass.");
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
