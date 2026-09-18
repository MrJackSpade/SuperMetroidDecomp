using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDraygonIntroDanceDefinitions(SuperMetroidAddressSpace rom)
    {
        VerifyDraygonIntroLatencyDefinitions(rom);
        VerifyDraygonIntroMovementDefinitions(rom);
        VerifyCompleteDraygonIntroTrajectory(rom);

        Console.WriteLine(
            "Draygon intro dance definitions: all four latency words and all 1,104 " +
            "reachable movement records match the pinned ROM; the complete 1,232-frame " +
            "production trajectory passes with migrated ROM reads forbidden.");
    }

    private static void VerifyDraygonIntroLatencyDefinitions(SuperMetroidAddressSpace rom)
    {
        const int source = 0xa5a19f;
        for (int slotIndex = 28; slotIndex <= 31; slotIndex++)
        {
            int address = source + (slotIndex - 28) * 2;
            short expected = unchecked((short)(
                rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
            AssertEqual(expected,
                DraygonIntroDanceDefinitions.MovementLatencyForSlot(slotIndex),
                $"Draygon intro Evir slot {slotIndex} latency");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => DraygonIntroDanceDefinitions.MovementLatencyForSlot(27),
            "Draygon intro Evir slot before native range");
        AssertThrows<ArgumentOutOfRangeException>(
            () => DraygonIntroDanceDefinitions.MovementLatencyForSlot(32),
            "Draygon intro Evir slot after native range");
    }

    private static void VerifyDraygonIntroMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int source = DraygonIntroDanceDefinitions.NativeMovementStreamAddress;
        for (ushort offset = 0;
             offset <= DraygonIntroDanceDefinitions.LastMovementStreamOffset;
             offset += DraygonIntroDanceDefinitions.StreamIndexAdvance)
        {
            byte nativeX = rom.ReadByte(source + offset);
            byte nativeY = rom.ReadByte(source + offset + 1);
            DraygonIntroMovement actual =
                DraygonIntroDanceDefinitions.ResolveMovement(offset);

            bool expectedDelete = nativeX == 0x80 && nativeY == 0x80;
            AssertEqual(expectedDelete, actual.DeletesSprite,
                $"Draygon intro movement ${offset:X4} delete sentinel");
            AssertEqual(expectedDelete ? (sbyte)0 : unchecked((sbyte)nativeX), actual.XDelta,
                $"Draygon intro movement ${offset:X4} X delta");
            AssertEqual(expectedDelete ? (sbyte)0 : unchecked((sbyte)nativeY), actual.YDelta,
                $"Draygon intro movement ${offset:X4} Y delta");
        }

        AssertThrows<InvalidDataException>(
            () => DraygonIntroDanceDefinitions.ResolveMovement(1),
            "Draygon intro rejects a non-native unaligned restored stream index");
        AssertThrows<InvalidDataException>(
            () => DraygonIntroDanceDefinitions.ResolveMovement(
                unchecked((ushort)(DraygonIntroDanceDefinitions.LastMovementStreamOffset + 4))),
            "Draygon intro rejects a restored stream index beyond the compiled route");
    }

    private static void VerifyCompleteDraygonIntroTrajectory(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new DraygonIntroDefinitionReadGuard(rom));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0));
        var run = typeof(RoomEnemySystem).GetMethod(
            "RunDraygonIntroDance",
            flags)!.CreateDelegate<Action<DraygonEnemyState, SamusState?, byte>>(enemies);
        var state = new DraygonEnemyState(enemies.Slots[0]);

        ushort[] expectedX = new ushort[4];
        ushort[] expectedY = new ushort[4];
        bool[] expectedActive = new bool[4];
        for (int slotIndex = 28; slotIndex <= 31; slotIndex++)
        {
            RoomSpriteObjectSlot sprite = enemies.RoomSpriteObjects[slotIndex];
            sprite.Kind = RoomSpriteObjectKind.DraygonIntroEvir;
            sprite.InstructionPointer = 1;
            sprite.XPosition = expectedX[slotIndex - 28] = 0x0100;
            sprite.YPosition = expectedY[slotIndex - 28] = 0x0180;
            expectedActive[slotIndex - 28] = true;
        }

        ushort expectedStreamIndex = 0;
        for (int frame = 0; frame < DraygonIntroDanceDefinitions.DurationFrames; frame++)
        {
            for (int slotIndex = 31; slotIndex >= 28; slotIndex--)
            {
                int actor = slotIndex - 28;
                ushort streamIndex = unchecked((ushort)(
                    expectedStreamIndex +
                    DraygonIntroDanceDefinitions.MovementLatencyForSlot(slotIndex)));
                if (unchecked((short)streamIndex) < 0 || !expectedActive[actor])
                    continue;

                byte xDelta = rom.ReadByte(
                    DraygonIntroDanceDefinitions.NativeMovementStreamAddress + streamIndex);
                byte yDelta = rom.ReadByte(
                    DraygonIntroDanceDefinitions.NativeMovementStreamAddress + streamIndex + 1);
                if (xDelta == 0x80 && yDelta == 0x80)
                {
                    expectedActive[actor] = false;
                    expectedX[actor] = 0;
                    expectedY[actor] = 0;
                    continue;
                }

                expectedX[actor] = unchecked((ushort)(
                    expectedX[actor] + unchecked((sbyte)xDelta)));
                expectedY[actor] = unchecked((ushort)(
                    expectedY[actor] + unchecked((sbyte)yDelta)));
            }

            run(state, null, 1);
            expectedStreamIndex = unchecked((ushort)(
                expectedStreamIndex + DraygonIntroDanceDefinitions.StreamIndexAdvance));

            AssertEqual(expectedStreamIndex, state.FightIntroDanceIndex,
                $"Draygon intro frame {frame} stream index");
            for (int slotIndex = 28; slotIndex <= 31; slotIndex++)
            {
                int actor = slotIndex - 28;
                RoomSpriteObjectSlot actual = enemies.RoomSpriteObjects[slotIndex];
                AssertEqual(expectedActive[actor], actual.IsActive,
                    $"Draygon intro frame {frame} slot {slotIndex} active state");
                AssertEqual(expectedX[actor], actual.XPosition,
                    $"Draygon intro frame {frame} slot {slotIndex} X");
                AssertEqual(expectedY[actor], actual.YPosition,
                    $"Draygon intro frame {frame} slot {slotIndex} Y");
            }
        }

        AssertEqual(DraygonIntroDanceDefinitions.DurationFrames, state.FunctionTimer,
            "Draygon intro exact native duration");
        run(state, null, 1);
        AssertEqual(DraygonAiFunction.SwoopRightSetup, state.Function,
            "Draygon intro transitions to right-swoop setup after the native duration");
        AssertEqual((ushort)0, state.FunctionTimer,
            "Draygon intro clears the function timer at the native handoff");

        // A restored index can be corrupt while the owner is still inside its native
        // duration. Exercise the production dispatcher so this cannot regress into an
        // arbitrary bank-$A5 read hidden behind a catalog-only unit test.
        var malformedEnemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            malformedEnemies,
            new DraygonIntroDefinitionReadGuard(rom));
        var malformedRun = typeof(RoomEnemySystem).GetMethod(
            "RunDraygonIntroDance",
            flags)!.CreateDelegate<Action<DraygonEnemyState, SamusState?, byte>>(malformedEnemies);
        var malformedState = new DraygonEnemyState(malformedEnemies.Slots[0])
        {
            // Slot 31 subtracts $0200, selecting invalid offset one immediately.
            FightIntroDanceIndex = 0x0201,
        };
        RoomSpriteObjectSlot malformedSprite = malformedEnemies.RoomSpriteObjects[31];
        malformedSprite.Kind = RoomSpriteObjectKind.DraygonIntroEvir;
        malformedSprite.InstructionPointer = 1;
        AssertThrows<InvalidDataException>(
            () => malformedRun(malformedState, null, 1),
            "Production Draygon intro rejects malformed restored movement without a ROM fallback");
    }

    private sealed class DraygonIntroDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            bool migratedLatency = address is >= 0xa5a19f and < 0xa5a1a7;
            bool migratedMovement = address >=
                    DraygonIntroDanceDefinitions.NativeMovementStreamAddress &&
                address <= DraygonIntroDanceDefinitions.NativeMovementStreamAddress +
                    DraygonIntroDanceDefinitions.LastMovementStreamOffset + 1;
            return migratedLatency || migratedMovement
                ? throw new InvalidOperationException(
                    $"Draygon intro attempted migrated definition read ${address:X6}.")
                : source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
