using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledBullMovement(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var enemies = new RoomEnemySystem();
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeBull", BindingFlags.Instance | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var slot = enemies.Slots[0];
        for (ushort interval = 0; interval < 13; interval++)
        for (ushort speed = 0; speed < 8; speed++)
        {
            slot.Parameter1 = interval;
            slot.Parameter2 = speed;
            initialize(slot); // No loaded cartridge bus; exercises the real initializer.
            var state = enemies.BullStates[0]!;
            AssertEqual(Word(0xa8d895 + interval * 4), state.AccelerationIntervalTimerReset, "Bull compiled acceleration");
            AssertEqual(state.AccelerationIntervalTimerReset, state.AccelerationIntervalTimer, "Bull live timer initialization");
            AssertEqual(Word(0xa8d897 + interval * 4), state.DecelerationIntervalTimerReset, "Bull compiled deceleration");
            AssertEqual(Word(0xa8d885 + speed * 2), state.MaxSpeed, "Bull compiled speed");
            AssertEqual(BullEnemyFunction.MovementDelay, state.Function, "Bull initial function");
            AssertEqual(16, state.ActivationTimer, "Bull native activation delay");
            AssertEqual(1, slot.InstructionTimer, "Bull instruction timer");
            AssertEqual(0, slot.Timer, "Bull loop timer");
            AssertEqual(0xd841, slot.CurrentInstruction, "Bull native initial instruction");
        }
        AssertThrows<InvalidDataException>(() => BullMovementDefinitions.MaximumSpeed(8), "Bull unknown speed selector");
        AssertThrows<InvalidDataException>(() => BullMovementDefinitions.Intervals(13), "Bull unknown interval selector");
        Console.WriteLine("Bull definitions: all 104 authored initializer combinations match native tables without a loaded bus; live timers and instruction setup preserved.");
    }
}
