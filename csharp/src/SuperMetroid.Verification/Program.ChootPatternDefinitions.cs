using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyChootPatternDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        var initializeMethod = typeof(RoomEnemySystem).GetMethod("InitializeChoot", flags)!;
        var fallMethod = typeof(RoomEnemySystem).GetMethod("RunChootFall", flags)!;
        var runFall = fallMethod.CreateDelegate<Action<RoomEnemySlot, ChootEnemyState>>();
        int[] frameCounts = [74, 74, 74, 94, 134];

        for (ushort patternIndex = 0; patternIndex < 5; patternIndex++)
        {
            ChootPatternDefinition definition = ChootPatternDefinitions.ForIndex(patternIndex);
            ushort nativePointer = ReadChootPatternWord(rom, 0xa2df5e + patternIndex * 2);
            ushort distancePointer = ReadChootPatternWord(rom, 0xa2df6a + patternIndex * 2);
            AssertEqual(nativePointer, definition.FallingPatternPointer,
                $"Choot pattern pointer {patternIndex}");
            AssertEqual(ReadChootPatternWord(rom, 0xa20000 | distancePointer),
                definition.FallingPatternYDistance,
                $"Choot pattern distance {patternIndex}");

            int pathAddress = 0xa20000 | definition.FallingPatternPointer;
            for (int frameIndex = 0; frameIndex < frameCounts[patternIndex]; frameIndex++)
            {
                ChootFallingPathPoint point = ChootFallingPathDefinitions.At(
                    definition.FallingPatternPointer,
                    frameIndex);
                AssertEqual(ReadChootPatternWord(rom, pathAddress + frameIndex * 4),
                    point.XOffset,
                    $"Choot path X {patternIndex}/{frameIndex}");
                AssertEqual(ReadChootPatternWord(rom, pathAddress + frameIndex * 4 + 2),
                    point.YOffset,
                    $"Choot path Y {patternIndex}/{frameIndex}");
            }

            AssertThrows<InvalidDataException>(
                () => ChootFallingPathDefinitions.At(
                    definition.FallingPatternPointer,
                    frameCounts[patternIndex]),
                $"Choot path {patternIndex} rejects its first post-terminator frame");

            foreach (byte loopCount in new byte[] { 0, 1, 2, 3 })
            {
                var enemies = new RoomEnemySystem();
                typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                    enemies, new ChootPatternReadGuard(rom));
                var initialize = initializeMethod.CreateDelegate<Action<RoomEnemySlot>>(enemies);
                RoomEnemySlot slot = enemies.Slots[0];
                slot.XPosition = 0x3456;
                slot.YPosition = 0x789a;
                slot.Parameter1 = (ushort)((patternIndex << 8) | loopCount);
                initialize(slot);

                ChootEnemyState state = enemies.ChootStates[0]!;
                ushort jumpHeight = unchecked((ushort)(
                    loopCount * (definition.FallingPatternYDistance & 0x00ff)));
                AssertEqual(definition.FallingPatternPointer, state.FallingPatternPointer,
                    $"Choot production pattern pointer {patternIndex}/{loopCount}");
                AssertEqual(definition.FallingPatternYDistance, state.FallingPatternYDistance,
                    $"Choot production pattern distance {patternIndex}/{loopCount}");
                AssertEqual(slot.XPosition, state.InitialFallingXPosition,
                    $"Choot production initial falling X {patternIndex}/{loopCount}");
                AssertEqual(unchecked((ushort)(0x789a - jumpHeight)),
                    state.InitialFallingYPosition,
                    $"Choot production initial falling Y {patternIndex}/{loopCount}");
            }

            var fallEnemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                fallEnemies, new ChootPatternReadGuard(rom));
            var initializeFall = initializeMethod.CreateDelegate<Action<RoomEnemySlot>>(fallEnemies);
            RoomEnemySlot fallSlot = fallEnemies.Slots[0];
            fallSlot.XPosition = 0x3456;
            fallSlot.XSubposition = 0x1111;
            fallSlot.YPosition = 0x789a;
            fallSlot.YSubposition = 0x2222;
            fallSlot.Parameter1 = (ushort)((patternIndex << 8) | 2);
            initializeFall(fallSlot);
            ChootEnemyState fallState = fallEnemies.ChootStates[0]!;
            fallState.FallingXOrigin = 0x4000;
            fallState.FallingYOrigin = 0x5000;
            fallState.FallingPatternIndex = 0;
            fallState.FallingPatternLoopCounter = 1;

            for (int frameIndex = 0; frameIndex < frameCounts[patternIndex] - 1; frameIndex++)
            {
                ChootFallingPathPoint point = ChootFallingPathDefinitions.At(
                    definition.FallingPatternPointer,
                    frameIndex);
                runFall(fallSlot, fallState);
                AssertEqual(unchecked((ushort)(0x4000 + unchecked((short)point.XOffset))),
                    fallSlot.XPosition,
                    $"Choot production fall X {patternIndex}/{frameIndex}");
                AssertEqual(unchecked((ushort)(0x5000 + unchecked((short)point.YOffset))),
                    fallSlot.YPosition,
                    $"Choot production fall Y {patternIndex}/{frameIndex}");
                AssertEqual(unchecked((ushort)((frameIndex + 1) << 8)),
                    fallState.FallingPatternIndex,
                    $"Choot production fall cursor {patternIndex}/{frameIndex}");
            }

            runFall(fallSlot, fallState);
            AssertEqual((ushort)0, fallState.FallingPatternIndex,
                $"Choot production loop cursor reset {patternIndex}");
            AssertEqual((ushort)0, fallState.FallingPatternLoopCounter,
                $"Choot production loop counter {patternIndex}");
            AssertEqual(unchecked((ushort)(0x5000 + definition.FallingPatternYDistance)),
                fallState.FallingYOrigin,
                $"Choot production loop Y advance {patternIndex}");

            fallState.FallingPatternIndex = unchecked((ushort)(
                (frameCounts[patternIndex] - 1) << 8));
            runFall(fallSlot, fallState);
            AssertEqual((ushort)0x3456, fallSlot.XPosition,
                $"Choot production final spawn X {patternIndex}");
            AssertEqual((ushort)0x789a, fallSlot.YPosition,
                $"Choot production final spawn Y {patternIndex}");
            AssertEqual((ushort)0, fallSlot.XSubposition,
                $"Choot production final spawn X fraction {patternIndex}");
            AssertEqual((ushort)0, fallSlot.YSubposition,
                $"Choot production final spawn Y fraction {patternIndex}");
        }

        AssertThrows<InvalidDataException>(
            () => ChootPatternDefinitions.ForIndex(5),
            "sixth Choot pointer-table alias is not an authored pattern");
        AssertThrows<InvalidDataException>(
            () => ChootPatternDefinitions.ForIndex(ushort.MaxValue),
            "restored Choot pattern index does not wrap into the catalog");
        AssertThrows<InvalidDataException>(
            () => ChootFallingPathDefinitions.At(0xffff, 0),
            "restored Choot falling pointer does not read arbitrary bank-A2 data");
        Console.WriteLine(
            "Choot pattern definitions: five selectors/distances, all 450 physical path frames, twenty initializers, and five complete fall loops pass with every source read forbidden.");
    }

    private static ushort ReadChootPatternWord(SuperMetroidAddressSpace source, int address) =>
        (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);

    private sealed class ChootPatternReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa2d84c and < 0xa2df74
                ? throw new InvalidOperationException(
                    $"Choot attempted migrated pattern-definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
