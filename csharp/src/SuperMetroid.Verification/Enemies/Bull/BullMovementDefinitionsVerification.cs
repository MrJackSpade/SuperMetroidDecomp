using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Bull;

namespace SuperMetroid.Verification.Enemies.Bull;

/// <summary>Compares Bull's compiled movement setup with retail acceleration, deceleration, and speed tables.</summary>
internal static class BullMovementDefinitionsVerification
{
    /// <summary>Checks all 104 parameter pairs through the real initializer, including timers and initial instruction.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled Bull movement")]
    internal static void VerifyCompiledBullMovement(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        Action<RoomEnemySlot> initialize = enemies.BullBehavior.InitializeBull;
        var slot = enemies.SlotTable.Slots[0];
        for (ushort interval = 0; interval < 13; interval++)
        for (ushort speed = 0; speed < 8; speed++)
        {
            slot.Parameter1 = interval;
            slot.Parameter2 = speed;
            initialize(slot); // No loaded cartridge bus; exercises the real initializer.
            var state = enemies.BullStates[0];
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8d895 + interval * 4)), (state ?? throw new InvalidOperationException("Verification requires state.")).AccelerationIntervalTimerReset, "Bull compiled acceleration");
            AssertEqual(state.AccelerationIntervalTimerReset, state.AccelerationIntervalTimer.RawValue, "Bull live timer initialization");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8d897 + interval * 4)), state.DecelerationIntervalTimerReset, "Bull compiled deceleration");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8d885 + speed * 2)), state.MaxSpeed, "Bull compiled speed");
            AssertEqual(BullEnemyFunction.MovementDelay, state.Function, "Bull initial function");
            AssertEqual(16, state.ActivationTimer.RawValue, "Bull native activation delay");
            AssertEqual(1, slot.InstructionTimer.RawValue, "Bull instruction timer");
            AssertEqual(0, slot.Timer.RawValue, "Bull loop timer");
            AssertEqual(0xd841, slot.CurrentInstruction, "Bull native initial instruction");
        }
        AssertThrows<InvalidDataException>(() => BullMovementDefinitions.MaximumSpeed(8), "Bull unknown speed selector");
        AssertThrows<InvalidDataException>(() => BullMovementDefinitions.Intervals(13), "Bull unknown interval selector");
        Console.WriteLine("Bull definitions: all 104 authored initializer combinations match native tables without a loaded bus; live timers and instruction setup preserved.");
    }
}
