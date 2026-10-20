using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;

/// <summary>Retail comparison for Botwoon's four hole bounds, 32 path choices and bank-$B3 movement streams.</summary>
/// <remarks>
/// The cartridge names holes by eight-byte table offsets, path choices by eight-byte descriptor offsets and movement
/// samples by two-byte bank-$B3 pointers. This oracle owns those translations; the simulation holds hole indices,
/// path choices and sample indices.
/// </remarks>
internal static class BotwoonNavigationDefinitionsVerification
{
    /// <summary>Bytes per cartridge hole record and per path descriptor record.</summary>
    private const int RecordBytes = 8;

    /// <summary>Bytes per cartridge movement sample (signed X then Y).</summary>
    private const int SampleBytes = 2;

    /// <summary>The compiled sample index of a cartridge movement-sample pointer.</summary>
    private static int SampleIndexOf(int pointer) =>
        (pointer - BotwoonNavigationDefinitionsConstants.MovementDataStart) / SampleBytes;

    /// <summary>Checks each hole's exclusive right edge through the production detection routine and rejects invalid hole indices.</summary>
    internal static void VerifyBotwoonHoleRightBounds(SuperMetroidAddressSpace rom)
    {
        Action<RoomEnemySlot, BotwoonEnemyState> detect = BotwoonBehavior.DetectBotwoonHole;
        for (int offset = 0; offset < 32; offset += 8)
        {
            ushort native = ReadRomWord(rom, 0xb3949d + offset);
            AssertEqual(native, BotwoonNavigationDefinitions.HoleAt(offset / RecordBytes).BottomRight.X, "Botwoon native right boundary");
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
            AssertThrows<InvalidDataException>(() => BotwoonNavigationDefinitions.HoleAt(invalid), "Botwoon hole index domain");
    }

    /// <summary>Checks each hole's exclusive bottom edge through the production detection routine.</summary>
    internal static void VerifyBotwoonHoleBottomBounds(SuperMetroidAddressSpace rom)
    {
        Action<RoomEnemySlot, BotwoonEnemyState> detect = BotwoonBehavior.DetectBotwoonHole;
        for (int offset = 0; offset < 32; offset += 8)
        {
            ushort native = ReadRomWord(rom, 0xb394a1 + offset);
            AssertEqual(native, BotwoonNavigationDefinitions.HoleAt(offset / RecordBytes).BottomRight.Y, "Botwoon native bottom boundary");
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
        Suite(nameof(VerifyBotwoonPathPointerMapping), () => VerifyBotwoonPathPointerMapping(rom));
        Suite(nameof(VerifyBotwoonPathDirectionMapping), () => VerifyBotwoonPathDirectionMapping(rom));
        Suite(nameof(VerifyBotwoonPathDestinationMapping), () => VerifyBotwoonPathDestinationMapping(rom));
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
            // The cartridge rewinds a reverse stream's pointer by four bytes: two samples.
            AssertEqual(SampleIndexOf(direction < 0 ? pointer - 4 : pointer), state.PathSampleIndex, "Botwoon production path sample and reverse rewind");
            AssertEqual(direction, state.PathDirection, "Botwoon production path direction");
            AssertEqual(ReadRomWord(rom, 0xb3e154 + offset), RecordBytes * state.TargetHoleIndex, "Botwoon production destination");
            AssertEqual(BotwoonMovementFunction.FollowAuthoredPath, state.MovementFunction, "Botwoon path handoff");
        }
        AssertEqual(32, BotwoonNavigationDefinitions.PathChoiceCount, "Botwoon has 32 path choices");
        foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<InvalidDataException>(() => BotwoonNavigationDefinitions.PathForChoice(invalid), "Botwoon descriptor rejects every invalid path choice");
        Console.WriteLine("Botwoon path descriptors: all 32 sample, direction and destination fields and production installations match native records; invalid choices rejected.");
    }

    /// <summary>Checks path starts and signed stream separators, including the distinct early terminal marker.</summary>
    private static void VerifyBotwoonPathPointerMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] boundaries = [0xa058,0xa328,0xa6ba,0xaa22,0xadfc,0xb168,0xb554,0xb954,
            0xbc84,0xc084,0xc28e,0xc68e,0xc9ca,0xcdca,0xd13e,0xd4a0,0xd87e,
            0xda00,0xdb9a,0xdd40,0xde7c,0xdfde,0xe14e];
        foreach (ushort pointer in boundaries)
        {
            AssertEqual((ushort)0x0080, ReadRomWord(rom, 0xb30000 | pointer), "Botwoon native stream separator");
            AssertEqual(new Offset16<RoomPixels>(sbyte.MinValue, 0), BotwoonNavigationDefinitions.MovementSample(SampleIndexOf(pointer)), "Botwoon compiled stream separator");
        }
        AssertEqual((ushort)0xf080, ReadRomWord(rom, 0xb3a6b4), "Botwoon early terminal is distinct from stream separator");
        for (int offset = 0; offset < 256; offset += 8)
            AssertEqual(SampleIndexOf(ReadRomWord(rom, 0xb3e150 + offset)),
                BotwoonNavigationDefinitions.PathForChoice(offset / RecordBytes).FirstSampleIndex, "Botwoon native forward/reverse descriptor start");
    }

    /// <summary>Checks forward and reverse traversal words for every authored path choice.</summary>
    private static void VerifyBotwoonPathDirectionMapping(SuperMetroidAddressSpace rom)
    {
        for (int offset = 0; offset < 256; offset += 8)
            AssertEqual((short)ReadRomWord(rom, 0xb3e152 + offset),
                BotwoonNavigationDefinitions.PathForChoice(offset / RecordBytes).Direction, "Botwoon native traversal direction");
    }

    /// <summary>Checks destination holes, including the repeated hidden choice in the native table.</summary>
    private static void VerifyBotwoonPathDestinationMapping(SuperMetroidAddressSpace rom)
    {
        for (int offset = 0; offset < 256; offset += 8)
            AssertEqual(ReadRomWord(rom, 0xb3e154 + offset),
                RecordBytes * BotwoonNavigationDefinitions.PathForChoice(offset / RecordBytes).TargetHoleIndex, "Botwoon native destination including repeated hidden choice");
    }

    /// <summary>Compares hole rectangles and movement samples against retail words while guarding compiled source ranges during production navigation.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Botwoon navigation definitions")]
    internal static void VerifyBotwoonNavigationDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyBotwoonHoleRightBounds), () => VerifyBotwoonHoleRightBounds(rom));
        Suite(nameof(VerifyBotwoonHoleBottomBounds), () => VerifyBotwoonHoleBottomBounds(rom));

        var holes = new BotwoonHoleDefinition[4];
        for (int holeIndex = 0; holeIndex < holes.Length; holeIndex++)
        {
            int address = 0xb3949b + RecordBytes * holeIndex;
            BotwoonHoleDefinition hole = BotwoonNavigationDefinitions.HoleAt(holeIndex);
            holes[holeIndex] = hole;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address), hole.TopLeft.X, $"Botwoon hole {holeIndex} left");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4)), hole.TopLeft.Y, $"Botwoon hole {holeIndex} top");
        }

        Action<RoomEnemySlot, BotwoonEnemyState> runMovement = BotwoonBehavior.RunBotwoonMovement;
        for (int choice = 0; choice < BotwoonNavigationDefinitions.PathChoiceCount; choice++)
        {
            int address = 0xb3e150 + RecordBytes * choice;
            BotwoonPathDescriptorDefinition descriptor =
                BotwoonNavigationDefinitions.PathForChoice(choice);
            AssertEqual(SampleIndexOf(CartridgeImportSource.Require(rom).ReadCartridgeWord(address)), descriptor.FirstSampleIndex,
                $"Botwoon path {choice} first sample");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2))), descriptor.Direction,
                $"Botwoon path {choice} direction");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4)), RecordBytes * descriptor.TargetHoleIndex,
                $"Botwoon path {choice} target hole");
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6)),
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
            int expectedSample = descriptor.Direction < 0
                ? descriptor.FirstSampleIndex - 2
                : descriptor.FirstSampleIndex;
            AssertEqual(expectedSample, state.PathSampleIndex,
                $"Botwoon production path {choice} sample");
            AssertEqual(descriptor.Direction, state.PathDirection,
                $"Botwoon production path {choice} direction");
            AssertEqual(descriptor.TargetHoleIndex, state.TargetHoleIndex,
                $"Botwoon production path {choice} target hole");
            AssertEqual(BotwoonMovementFunction.FollowAuthoredPath, state.MovementFunction,
                $"Botwoon production path {choice} handoff");
        }

        var movementEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot movementHead = movementEnemies.SlotTable.Slots[0];
        int movementSampleCount = 0;
        for (int pointer = BotwoonNavigationDefinitionsConstants.MovementDataStart;
             pointer < BotwoonNavigationDefinitionsConstants.MovementDataEndExclusive;
             pointer += SampleBytes)
        {
            int sampleIndex = SampleIndexOf(pointer);
            sbyte expectedX = unchecked((sbyte)rom.ReadCartridgeByte(0xb30000 | pointer));
            sbyte expectedY = unchecked((sbyte)rom.ReadCartridgeByte(0xb30000 | pointer + 1));
            Offset16<RoomPixels> compiled =
                BotwoonNavigationDefinitions.MovementSample(sampleIndex);
            AssertEqual(expectedX, compiled.X, $"Botwoon movement ${pointer:X4} X");
            AssertEqual(expectedY, compiled.Y, $"Botwoon movement ${pointer:X4} Y");

            foreach (short direction in new short[] { 0, -1 })
            {
                movementHead.Position = movementHead.Position.WithWhole(new(0x8000, 0x8000));
                var state = new BotwoonEnemyState()
                {
                    MovementFunction = BotwoonMovementFunction.FollowAuthoredPath,
                    PathSampleIndex = sampleIndex,
                    PathDirection = direction,
                    Speed = 1,
                };
                runMovement(movementHead, state);

                bool terminates = expectedX == sbyte.MinValue || expectedY == sbyte.MinValue;
                AssertEqual(terminates, state.PathComplete,
                    $"Botwoon production movement ${pointer:X4}/{direction} completion");
                if (terminates)
                {
                    AssertEqual(sampleIndex, state.PathSampleIndex,
                        $"Botwoon terminating movement ${pointer:X4}/{direction} sample");
                    AssertEqual((ushort)0x8000, movementHead.Position.X.Whole,
                        $"Botwoon terminating movement ${pointer:X4}/{direction} X");
                    AssertEqual((ushort)0x8000, movementHead.Position.Y.Whole,
                        $"Botwoon terminating movement ${pointer:X4}/{direction} Y");
                    continue;
                }

                int sign = direction < 0 ? -1 : 1;
                AssertEqual(unchecked((ushort)(0x8000 + sign * expectedX)),
                    movementHead.Position.X.Whole,
                    $"Botwoon production movement ${pointer:X4}/{direction} X");
                AssertEqual(unchecked((ushort)(0x8000 + sign * expectedY)),
                    movementHead.Position.Y.Whole,
                    $"Botwoon production movement ${pointer:X4}/{direction} Y");
                AssertEqual(sampleIndex + sign,
                    state.PathSampleIndex,
                    $"Botwoon production movement ${pointer:X4}/{direction} sample");
            }

            movementSampleCount++;
        }
        AssertEqual(movementSampleCount, BotwoonMovementSampleData.Count,
            "Botwoon compiled movement corpus covers exactly the cartridge's sample range");

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
            AssertEqual(SnesAngle.FromTableIndex(191), state.MovementAngle,
                $"Botwoon hole {holeIndex} exact-target movement angle");
            AssertEqual(hole.Target.X, head.Position.X.Whole,
                $"Botwoon hole {holeIndex} exact-target X");
            AssertEqual(hole.Target.Y, head.Position.Y.Whole,
                $"Botwoon hole {holeIndex} exact-target Y");
        }

        AssertThrows<InvalidDataException>(
            () => BotwoonNavigationDefinitions.HoleAt(-1),
            "Botwoon hole index before table");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigationDefinitions.HoleAt(4),
            "Botwoon hole index past table");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigationDefinitions.PathForChoice(-1),
            "Botwoon path choice before table");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigationDefinitions.PathForChoice(32),
            "Botwoon path choice past table");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigationDefinitions.MovementSample(-1),
            "Botwoon movement sample before corpus");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigationDefinitions.MovementSample(BotwoonMovementSampleData.Count),
            "Botwoon movement sample after corpus");

        var invalidEnemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot invalidHead = invalidEnemies.SlotTable.Slots[0];
        var invalidState = new BotwoonEnemyState()
        {
            MovementFunction = BotwoonMovementFunction.LoadAuthoredPath,
            PathChoice = BotwoonNavigationDefinitions.PathChoiceCount,
            PathSampleIndex = 77,
            PathDirection = -7,
            TargetHoleIndex = 3,
        };
        AssertThrows<InvalidDataException>(
            () => runMovement(invalidHead, invalidState),
            "Botwoon invalid production path selector");
        AssertEqual(77, invalidState.PathSampleIndex,
            "Botwoon invalid path leaves sample unchanged");
        AssertEqual((short)-7, invalidState.PathDirection,
            "Botwoon invalid path leaves direction unchanged");
        AssertEqual(3, invalidState.TargetHoleIndex,
            "Botwoon invalid path leaves target unchanged");
        AssertEqual(BotwoonMovementFunction.LoadAuthoredPath, invalidState.MovementFunction,
            "Botwoon invalid path leaves callback unchanged");

        Console.WriteLine(
            $"Botwoon navigation definitions: 144 native words, {movementSampleCount:N0} signed " +
            "movement pairs in both directions, all 32 real descriptor handoffs, four movement " +
            "targets and rectangle boundaries pass with fixed definition reads forbidden.");
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
