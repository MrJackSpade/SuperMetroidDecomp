using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Bull;

/// <summary>Compares Bull's compiled movement setup with retail acceleration, deceleration, and speed tables.</summary>
internal static class BullMovementDefinitionsVerification
{
    /// <summary>Checks all 104 parameter pairs through the real initializer, including timers and initial instruction.</summary>
    internal static void VerifyCompiledBullMovement(CartridgeImportAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        Action<RoomEnemySlot> initialize = enemies.BullBehavior.InitializeBull;
        var slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Bull, slot.Definition);
        for (ushort interval = 0; interval < 13; interval++)
        for (ushort speed = 0; speed < 8; speed++)
        {
            slot.SetSpawnParameter1(EnemyDefinitionId.Bull, interval);
            slot.SetSpawnParameter2(EnemyDefinitionId.Bull, speed);
            initialize(slot); // No loaded cartridge bus; exercises the real initializer.
            var state = enemies.BullStates[0];
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8d895 + interval * 4)), (state ?? throw new InvalidOperationException("Verification requires state.")).AccelerationIntervalTimerReset, "Bull compiled acceleration");
            AssertEqual(state.AccelerationIntervalTimerReset, state.AccelerationIntervalTimer, "Bull live timer initialization");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8d897 + interval * 4)), state.DecelerationIntervalTimerReset, "Bull compiled deceleration");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8d885 + speed * 2)), state.MaxSpeed.InTwoHundredFiftySixthsOfPixel, "Bull compiled speed");
            AssertEqual(BullEnemyFunction.MovementDelay, state.Function, "Bull initial function");
            AssertEqual(16, state.ActivationTimer, "Bull native activation delay");
            AssertEqual(1, slot.AnimationClock.Hold.RawValue, "Bull instruction timer");
            AssertEqual(0, slot.AnimationClock.Repeats.RawValue, "Bull loop timer");
            AssertEqual(new AnimationPlayback<BullSegment>(BullAnimations.StartingWith(BullSegment.Normal), AnimationPlace.StartOfAnimation), state.Playback, "Bull initial animation");
        }
        AssertThrows<InvalidDataException>(() => slot.SetSpawnParameter2(EnemyDefinitionId.Bull, 8), "Bull import rejects the speed selector past its table");
        AssertThrows<InvalidDataException>(() => slot.SetSpawnParameter1(EnemyDefinitionId.Bull, 13), "Bull import rejects the interval selector past its table");
        Console.WriteLine("Bull definitions: all 104 authored initializer combinations match native tables without a loaded bus; live timers and instruction setup preserved.");
    }
}
