using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyChootPatternDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var initializeMethod = typeof(RoomEnemySystem).GetMethod("InitializeChoot", flags)!;

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
        }

        AssertThrows<InvalidDataException>(
            () => ChootPatternDefinitions.ForIndex(5),
            "sixth Choot pointer-table alias is not an authored pattern");
        AssertThrows<InvalidDataException>(
            () => ChootPatternDefinitions.ForIndex(ushort.MaxValue),
            "restored Choot pattern index does not wrap into the catalog");
        Console.WriteLine(
            "Choot pattern definitions: five native selectors/distances and twenty real initializers pass with selector and distance reads forbidden.");
    }

    private static ushort ReadChootPatternWord(SuperMetroidAddressSpace source, int address) =>
        (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);

    private sealed class ChootPatternReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa2df5e and < 0xa2df74 or
                >= 0xa2d974 and < 0xa2d976 or
                >= 0xa2da9e and < 0xa2daa0 or
                >= 0xa2dbc8 and < 0xa2dbca or
                >= 0xa2dd42 and < 0xa2dd44 or
                >= 0xa2df5c and < 0xa2df5e
                ? throw new InvalidOperationException(
                    $"Choot attempted migrated pattern-definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
