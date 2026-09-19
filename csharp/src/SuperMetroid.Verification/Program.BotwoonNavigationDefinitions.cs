using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBotwoonNavigationDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        ushort Word(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        var holes = new BotwoonHoleDefinition[4];
        for (ushort byteOffset = 0; byteOffset <= 24; byteOffset += 8)
        {
            int address = 0xb3949b + byteOffset;
            BotwoonHoleDefinition hole =
                BotwoonNavigationDefinitions.HoleForByteOffset(byteOffset);
            holes[byteOffset >> 3] = hole;
            AssertEqual(Word(address), hole.Left, $"Botwoon hole {byteOffset:X2} left");
            AssertEqual(Word(address + 2), hole.Right, $"Botwoon hole {byteOffset:X2} right");
            AssertEqual(Word(address + 4), hole.Top, $"Botwoon hole {byteOffset:X2} top");
            AssertEqual(Word(address + 6), hole.Bottom, $"Botwoon hole {byteOffset:X2} bottom");
        }

        var runMovement = typeof(RoomEnemySystem).GetMethod("RunBotwoonMovement", flags)!
            .CreateDelegate<Action<RoomEnemySlot, BotwoonEnemyState>>();
        for (ushort byteOffset = 0; byteOffset <= 248; byteOffset += 8)
        {
            int address = 0xb3e150 + byteOffset;
            BotwoonPathDescriptorDefinition descriptor =
                BotwoonNavigationDefinitions.PathForChoiceByteOffset(byteOffset);
            AssertEqual(Word(address), descriptor.PathPointer,
                $"Botwoon path {byteOffset:X2} pointer");
            AssertEqual(unchecked((short)Word(address + 2)), descriptor.Direction,
                $"Botwoon path {byteOffset:X2} direction");
            AssertEqual(Word(address + 4), descriptor.TargetHoleByteOffset,
                $"Botwoon path {byteOffset:X2} target hole");
            AssertEqual((ushort)0, Word(address + 6),
                $"Botwoon path {byteOffset:X2} alignment word");

            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies, new BotwoonNavigationReadGuard(rom));
            RoomEnemySlot head = enemies.Slots[0];
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
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            movementEnemies, new BotwoonNavigationReadGuard(rom));
        RoomEnemySlot movementHead = movementEnemies.Slots[0];
        int movementSampleCount = 0;
        for (int pointer = BotwoonNavigationDefinitions.MovementDataStart;
             pointer < BotwoonNavigationDefinitions.MovementDataEndExclusive;
             pointer += 2)
        {
            sbyte expectedX = unchecked((sbyte)rom.ReadByte(0xb30000 | pointer));
            sbyte expectedY = unchecked((sbyte)rom.ReadByte(0xb30000 | pointer + 1));
            BotwoonMovementSample compiled =
                BotwoonNavigationDefinitions.MovementSampleForPointer((ushort)pointer);
            AssertEqual(expectedX, compiled.X, $"Botwoon movement ${pointer:X4} X");
            AssertEqual(expectedY, compiled.Y, $"Botwoon movement ${pointer:X4} Y");

            foreach (short direction in new short[] { 0, -1 })
            {
                movementHead.XPosition = 0x8000;
                movementHead.YPosition = 0x8000;
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
                    AssertEqual((ushort)0x8000, movementHead.XPosition,
                        $"Botwoon terminating movement ${pointer:X4}/{direction} X");
                    AssertEqual((ushort)0x8000, movementHead.YPosition,
                        $"Botwoon terminating movement ${pointer:X4}/{direction} Y");
                    continue;
                }

                int sign = direction < 0 ? -1 : 1;
                AssertEqual(unchecked((ushort)(0x8000 + sign * expectedX)),
                    movementHead.XPosition,
                    $"Botwoon production movement ${pointer:X4}/{direction} X");
                AssertEqual(unchecked((ushort)(0x8000 + sign * expectedY)),
                    movementHead.YPosition,
                    $"Botwoon production movement ${pointer:X4}/{direction} Y");
                AssertEqual(unchecked((ushort)(pointer + (direction < 0 ? -2 : 2))),
                    state.PathPointer,
                    $"Botwoon production movement ${pointer:X4}/{direction} pointer");
            }

            movementSampleCount++;
        }

        var detectHole = typeof(RoomEnemySystem).GetMethod("DetectBotwoonHole", flags)!
            .CreateDelegate<Action<RoomEnemySlot, BotwoonEnemyState>>();
        for (ushort byteOffset = 0; byteOffset <= 24; byteOffset += 8)
        {
            BotwoonHoleDefinition hole = holes[byteOffset >> 3];
            var head = new RoomEnemySystem().Slots[0];
            var state = new BotwoonEnemyState(head) { RingByteOffset = 0x0234 };
            head.XPosition = hole.Left;
            head.YPosition = hole.Top;
            detectHole(head, state);
            AssertTrue(state.InsideHole, $"Botwoon hole {byteOffset:X2} includes left/top");
            AssertTrue(state.HoleLatch, $"Botwoon hole {byteOffset:X2} latches");
            AssertEqual((ushort)0x0234, state.SavedHoleRingByteOffset,
                $"Botwoon hole {byteOffset:X2} saves ring offset");

            state.HoleLatch = false;
            state.InsideHole = false;
            head.XPosition = hole.Right;
            detectHole(head, state);
            AssertTrue(!state.InsideHole,
                $"Botwoon hole {byteOffset:X2} excludes right boundary");
            state.HoleLatch = false;
            head.XPosition = hole.Left;
            head.YPosition = hole.Bottom;
            detectHole(head, state);
            AssertTrue(!state.InsideHole,
                $"Botwoon hole {byteOffset:X2} excludes bottom boundary");
        }

        for (ushort byteOffset = 0; byteOffset <= 24; byteOffset += 8)
        {
            BotwoonHoleDefinition hole = holes[byteOffset >> 3];
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies, new BotwoonNavigationReadGuard(rom));
            RoomEnemySlot head = enemies.Slots[0];
            head.XPosition = hole.TargetX;
            head.YPosition = hole.TargetY;
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
            AssertEqual(hole.TargetX, head.XPosition,
                $"Botwoon hole {byteOffset:X2} exact-target X");
            AssertEqual(hole.TargetY, head.YPosition,
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
        RoomEnemySlot invalidHead = invalidEnemies.Slots[0];
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

    private sealed class BotwoonNavigationReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xb3949b and < 0xb394bb or
                >= 0xb3a058 and < 0xb3e150 or
                >= 0xb3e150 and < 0xb3e250
                ? throw new InvalidOperationException(
                    $"Botwoon attempted migrated navigation-definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
