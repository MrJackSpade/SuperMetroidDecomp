using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyAtomicMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0xa8e380;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new AtomicInstructionReadGuard(new TestAddressSpace()));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeAtomic", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];

        for (ushort selector = 0; selector < 4; selector++)
        {
            int address = instructionTable + selector * 2;
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, AtomicMovementDefinitions.InitialInstructionList(selector),
                $"compiled Atomic instruction selector {selector}");

            slot.Parameter1 = selector;
            slot.Parameter2 = (ushort)(selector * 7);
            initialize(slot);
            AssertEqual(native, slot.CurrentInstruction,
                $"Atomic initializer instruction selector {selector}");

            AtomicEnemyState state = enemies.AtomicStates[0]!;
            ushort speedOffset = (ushort)(slot.Parameter2 * 8);
            (short positiveWhole, ushort positiveFraction) =
                EnemyLinearSpeedDefinitions.Read(speedOffset);
            (short negativeWhole, ushort negativeFraction) =
                EnemyLinearSpeedDefinitions.Read((ushort)(speedOffset + 4));
            AssertEqual(unchecked((ushort)positiveWhole), state.SpeedWhole,
                $"Atomic positive whole speed {selector}");
            AssertEqual(positiveFraction, state.SpeedFraction,
                $"Atomic positive fractional speed {selector}");
            AssertEqual(unchecked((ushort)negativeWhole), state.NegativeSpeedWhole,
                $"Atomic negative whole speed {selector}");
            AssertEqual(negativeFraction, state.NegativeSpeedFraction,
                $"Atomic negative fractional speed {selector}");
        }

        AssertThrows<InvalidDataException>(
            () => AtomicMovementDefinitions.InitialInstructionList(4),
            "Atomic selector beyond authored table");
        AssertThrows<InvalidDataException>(
            () => AtomicMovementDefinitions.InitialInstructionList(ushort.MaxValue),
            "Atomic restored selector does not read adjacent code");

        Console.WriteLine(
            "Atomic movement definitions: four native instruction selectors and production initializers pass with table reads forbidden.");
    }

    private sealed class AtomicInstructionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa8e380 and < 0xa8e388
            ? throw new InvalidOperationException(
                $"Atomic initializer attempted migrated instruction read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
