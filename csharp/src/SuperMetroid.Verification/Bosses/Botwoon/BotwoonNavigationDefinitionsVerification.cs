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
internal static class BotwoonNavigationDefinitionsVerification
{
    /// <summary>Checks each hole's exclusive right edge through the production detection routine and rejects invalid offsets.</summary>
    internal static void VerifyBotwoonHoleRightBounds(SuperMetroidAddressSpace rom)
    {
        Action<RoomEnemySlot, BotwoonEnemyState> detect = BotwoonBehavior.DetectBotwoonHole;
        for (int offset = 0; offset < 32; offset += 8)
        {
            ushort native = ReadRomWord(rom, 0xb3949d + offset);
            AssertEqual(native, BotwoonNavigationDefinitions.HoleForByteOffset((ushort)offset).BottomRight.X, "Botwoon native right boundary");
            var head = new RoomEnemySystem().SlotTable.Slots[0];
            head.Position = head.Position.WithWholeY(ReadRomWord(rom, 0xb3949f + offset));
            foreach (int x in new[] {native - 1, native})
            {
                head.Position = head.Position.WithWholeX((ushort)x);
                var state = new BotwoonEnemyState(head);
                detect(head, state);
                AssertEqual(x < native, state.InsideHole, "Botwoon right boundary is exclusive");
            }
        }
        for (int offset = 0; offset <= ushort.MaxValue; offset++)
            if (offset > 24 || offset % 8 != 0)
                AssertThrows<InvalidDataException>(() => BotwoonNavigationDefinitions.HoleForByteOffset((ushort)offset), "Botwoon hole bound domain");
    }

    /// <summary>Checks each hole's exclusive bottom edge through the production detection routine.</summary>
    internal static void VerifyBotwoonHoleBottomBounds(SuperMetroidAddressSpace rom)
    {
        Action<RoomEnemySlot, BotwoonEnemyState> detect = BotwoonBehavior.DetectBotwoonHole;
        for (int offset = 0; offset < 32; offset += 8)
        {
            ushort native = ReadRomWord(rom, 0xb394a1 + offset);
            AssertEqual(native, BotwoonNavigationDefinitions.HoleForByteOffset((ushort)offset).BottomRight.Y, "Botwoon native bottom boundary");
            var head = new RoomEnemySystem().SlotTable.Slots[0];
            head.Position = head.Position.WithWholeX(ReadRomWord(rom, 0xb3949b + offset));
            foreach (int y in new[] {native - 1, native})
            {
                head.Position = head.Position.WithWholeY((ushort)y);
                var state = new BotwoonEnemyState(head);
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
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot head = enemies.SlotTable.Slots[0];
            var state = new BotwoonEnemyState(head) { MovementFunction = BotwoonMovementFunction.LoadAuthoredPath,
                PathChoiceOffset = (ushort)offset, Speed = 0 };
            ushort pointer = ReadRomWord(rom, 0xb3e150 + offset);
            short direction = (short)ReadRomWord(rom, 0xb3e152 + offset);
            run(head, state);
            AssertEqual(direction < 0 ? (ushort)(pointer - 4) : pointer, state.PathPointer, "Botwoon production path pointer and reverse rewind");
            AssertEqual(direction, state.PathDirection, "Botwoon production path direction");
            AssertEqual(ReadRomWord(rom, 0xb3e154 + offset), state.TargetHoleOffset, "Botwoon production destination");
            AssertEqual(BotwoonMovementFunction.FollowAuthoredPath, state.MovementFunction, "Botwoon path handoff");
        }
        for (int offset = 0; offset <= ushort.MaxValue; offset++)
            if (offset >= 256 || offset % 8 != 0)
                AssertThrows<InvalidDataException>(() => BotwoonNavigationDefinitions.PathForChoiceByteOffset((ushort)offset), "Botwoon descriptor rejects every invalid byte offset");
        Console.WriteLine("Botwoon path descriptors: all 32 pointer, direction and destination fields and production installations match native records; invalid offsets rejected.");
    }

    /// <summary>Checks path pointers and signed stream separators, including the distinct early terminal marker.</summary>
    private static void VerifyBotwoonPathPointerMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] boundaries = [0xa058,0xa328,0xa6ba,0xaa22,0xadfc,0xb168,0xb554,0xb954,
            0xbc84,0xc084,0xc28e,0xc68e,0xc9ca,0xcdca,0xd13e,0xd4a0,0xd87e,
            0xda00,0xdb9a,0xdd40,0xde7c,0xdfde,0xe14e];
        foreach (ushort pointer in boundaries)
        {
            AssertEqual((ushort)0x0080, ReadRomWord(rom, 0xb30000 | pointer), "Botwoon native stream separator");
            AssertEqual(new Offset16<RoomPixels>(sbyte.MinValue, 0), BotwoonNavigationDefinitions.MovementSampleForPointer(pointer), "Botwoon compiled stream separator");
        }
        AssertEqual((ushort)0xf080, ReadRomWord(rom, 0xb3a6b4), "Botwoon early terminal is distinct from stream separator");
        for (int offset = 0; offset < 256; offset += 8)
            AssertEqual(ReadRomWord(rom, 0xb3e150 + offset),
                BotwoonNavigationDefinitions.PathForChoiceByteOffset((ushort)offset).PathPointer, "Botwoon native forward/reverse descriptor pointer");
    }

    /// <summary>Checks forward and reverse traversal words for every authored path choice.</summary>
    private static void VerifyBotwoonPathDirectionMapping(SuperMetroidAddressSpace rom)
    {
        for (int offset = 0; offset < 256; offset += 8)
            AssertEqual((short)ReadRomWord(rom, 0xb3e152 + offset),
                BotwoonNavigationDefinitions.PathForChoiceByteOffset((ushort)offset).Direction, "Botwoon native traversal direction");
    }

    /// <summary>Checks destination-hole offsets, including the repeated hidden choice in the native table.</summary>
    private static void VerifyBotwoonPathDestinationMapping(SuperMetroidAddressSpace rom)
    {
        for (int offset = 0; offset < 256; offset += 8)
            AssertEqual(ReadRomWord(rom, 0xb3e154 + offset),
                BotwoonNavigationDefinitions.PathForChoiceByteOffset((ushort)offset).TargetHoleByteOffset, "Botwoon native destination including repeated hidden choice");
    }

    /// <summary>Compares hole rectangles and movement samples against retail words while guarding compiled source ranges during production navigation.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Botwoon navigation definitions")]
    internal static void VerifyBotwoonNavigationDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyBotwoonHoleRightBounds), () => VerifyBotwoonHoleRightBounds(rom));
        Suite(nameof(VerifyBotwoonHoleBottomBounds), () => VerifyBotwoonHoleBottomBounds(rom));

        var holes = new BotwoonHoleDefinition[4];
        for (ushort byteOffset = 0; byteOffset <= 24; byteOffset += 8)
        {
            int address = 0xb3949b + byteOffset;
            BotwoonHoleDefinition hole =
                BotwoonNavigationDefinitions.HoleForByteOffset(byteOffset);
            holes[byteOffset >> 3] = hole;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address), hole.TopLeft.X, $"Botwoon hole {byteOffset:X2} left");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4)), hole.TopLeft.Y, $"Botwoon hole {byteOffset:X2} top");
        }

        Action<RoomEnemySlot, BotwoonEnemyState> runMovement = BotwoonBehavior.RunBotwoonMovement;
        for (ushort byteOffset = 0; byteOffset <= 248; byteOffset += 8)
        {
            int address = 0xb3e150 + byteOffset;
            BotwoonPathDescriptorDefinition descriptor =
                BotwoonNavigationDefinitions.PathForChoiceByteOffset(byteOffset);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address), descriptor.PathPointer,
                $"Botwoon path {byteOffset:X2} pointer");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2))), descriptor.Direction,
                $"Botwoon path {byteOffset:X2} direction");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4)), descriptor.TargetHoleByteOffset,
                $"Botwoon path {byteOffset:X2} target hole");
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6)),
                $"Botwoon path {byteOffset:X2} alignment word");

            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot head = enemies.SlotTable.Slots[0];
            var state = new BotwoonEnemyState(head)
            {
                MovementFunction = BotwoonMovementFunction.LoadAuthoredPath,
                PathChoiceOffset = byteOffset,
                Speed = 0,
            };
            runMovement(head, state);
            ushort expectedPointer = descriptor.Direction < 0
                ? unchecked((ushort)(descriptor.PathPointer - 4))
                : descriptor.PathPointer;
            AssertEqual(expectedPointer, state.PathPointer,
                $"Botwoon production path {byteOffset:X2} pointer");
            AssertEqual(descriptor.Direction, state.PathDirection,
                $"Botwoon production path {byteOffset:X2} direction");
            AssertEqual(descriptor.TargetHoleByteOffset, state.TargetHoleOffset,
                $"Botwoon production path {byteOffset:X2} target hole");
            AssertEqual(BotwoonMovementFunction.FollowAuthoredPath, state.MovementFunction,
                $"Botwoon production path {byteOffset:X2} handoff");
        }

        var movementEnemies = new RoomEnemySystem();
        movementEnemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot movementHead = movementEnemies.SlotTable.Slots[0];
        int movementSampleCount = 0;
        for (int pointer = BotwoonNavigationDefinitions.MovementDataStart;
             pointer < BotwoonNavigationDefinitions.MovementDataEndExclusive;
             pointer += 2)
        {
            sbyte expectedX = unchecked((sbyte)rom.ReadCartridgeByte(0xb30000 | pointer));
            sbyte expectedY = unchecked((sbyte)rom.ReadCartridgeByte(0xb30000 | pointer + 1));
            Offset16<RoomPixels> compiled =
                BotwoonNavigationDefinitions.MovementSampleForPointer((ushort)pointer);
            AssertEqual(expectedX, compiled.X, $"Botwoon movement ${pointer:X4} X");
            AssertEqual(expectedY, compiled.Y, $"Botwoon movement ${pointer:X4} Y");

            foreach (short direction in new short[] { 0, -1 })
            {
                movementHead.Position = movementHead.Position.WithWhole(new(0x8000, 0x8000));
                var state = new BotwoonEnemyState(movementHead)
                {
                    MovementFunction = BotwoonMovementFunction.FollowAuthoredPath,
                    PathPointer = (ushort)pointer,
                    PathDirection = direction,
                    Speed = 1,
                };
                runMovement(movementHead, state);

                bool terminates = expectedX == sbyte.MinValue || expectedY == sbyte.MinValue;
                AssertEqual(terminates, state.PathComplete,
                    $"Botwoon production movement ${pointer:X4}/{direction} completion");
                if (terminates)
                {
                    AssertEqual((ushort)pointer, state.PathPointer,
                        $"Botwoon terminating movement ${pointer:X4}/{direction} pointer");
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
                AssertEqual(unchecked((ushort)(pointer + (direction < 0 ? -2 : 2))),
                    state.PathPointer,
                    $"Botwoon production movement ${pointer:X4}/{direction} pointer");
            }

            movementSampleCount++;
        }

        Action<RoomEnemySlot, BotwoonEnemyState> detectHole = BotwoonBehavior.DetectBotwoonHole;
        for (ushort byteOffset = 0; byteOffset <= 24; byteOffset += 8)
        {
            BotwoonHoleDefinition hole = holes[byteOffset >> 3];
            var head = new RoomEnemySystem().SlotTable.Slots[0];
            var state = new BotwoonEnemyState(head) { RingIndex = 141 };
            head.Position = head.Position.WithWhole(hole.TopLeft);
            detectHole(head, state);
            AssertTrue(state.InsideHole, $"Botwoon hole {byteOffset:X2} includes left/top");
            AssertTrue(state.HoleLatch, $"Botwoon hole {byteOffset:X2} latches");
            AssertEqual((int?)141, state.SavedHoleRingIndex,
                $"Botwoon hole {byteOffset:X2} saves ring index");

            state.HoleLatch = false;
            state.InsideHole = false;
            head.Position = head.Position.WithWholeX(hole.BottomRight.X);
            detectHole(head, state);
            AssertTrue(!state.InsideHole,
                $"Botwoon hole {byteOffset:X2} excludes right boundary");
            state.HoleLatch = false;
            head.Position = head.Position.WithWhole(hole.TopLeft.WithY(hole.BottomRight.Y));
            detectHole(head, state);
            AssertTrue(!state.InsideHole,
                $"Botwoon hole {byteOffset:X2} excludes bottom boundary");
        }

        for (ushort byteOffset = 0; byteOffset <= 24; byteOffset += 8)
        {
            BotwoonHoleDefinition hole = holes[byteOffset >> 3];
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot head = enemies.SlotTable.Slots[0];
            head.Position = head.Position.WithWhole(hole.Target);
            var state = new BotwoonEnemyState(head)
            {
                MovementFunction = BotwoonMovementFunction.MoveTowardHole,
                TargetHoleOffset = byteOffset,
                Speed = 0,
            };
            runMovement(head, state);
            AssertEqual((ushort)129, state.TargetAngle,
                $"Botwoon hole {byteOffset:X2} exact-target cartridge angle");
            AssertEqual((byte)191, state.MovementAngle,
                $"Botwoon hole {byteOffset:X2} exact-target movement angle");
            AssertEqual(hole.Target.X, head.Position.X.Whole,
                $"Botwoon hole {byteOffset:X2} exact-target X");
            AssertEqual(hole.Target.Y, head.Position.Y.Whole,
                $"Botwoon hole {byteOffset:X2} exact-target Y");
        }

        AssertThrows<InvalidDataException>(
            () => BotwoonNavigationDefinitions.HoleForByteOffset(1),
            "Botwoon unaligned hole offset");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigationDefinitions.HoleForByteOffset(32),
            "Botwoon hole offset past table");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigationDefinitions.PathForChoiceByteOffset(1),
            "Botwoon unaligned path offset");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigationDefinitions.PathForChoiceByteOffset(256),
            "Botwoon path offset past table");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigationDefinitions.MovementSampleForPointer(0xa056),
            "Botwoon movement pointer before corpus");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigationDefinitions.MovementSampleForPointer(0xa059),
            "Botwoon unaligned movement pointer");
        AssertThrows<InvalidDataException>(
            () => BotwoonNavigationDefinitions.MovementSampleForPointer(0xe150),
            "Botwoon movement pointer after corpus");

        var invalidEnemies = new RoomEnemySystem();
        RoomEnemySlot invalidHead = invalidEnemies.SlotTable.Slots[0];
        var invalidState = new BotwoonEnemyState(invalidHead)
        {
            MovementFunction = BotwoonMovementFunction.LoadAuthoredPath,
            PathChoiceOffset = 1,
            PathPointer = 0x1234,
            PathDirection = -7,
            TargetHoleOffset = 0x5678,
        };
        AssertThrows<InvalidDataException>(
            () => runMovement(invalidHead, invalidState),
            "Botwoon invalid production path selector");
        AssertEqual((ushort)0x1234, invalidState.PathPointer,
            "Botwoon invalid path leaves pointer unchanged");
        AssertEqual((short)-7, invalidState.PathDirection,
            "Botwoon invalid path leaves direction unchanged");
        AssertEqual((ushort)0x5678, invalidState.TargetHoleOffset,
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
