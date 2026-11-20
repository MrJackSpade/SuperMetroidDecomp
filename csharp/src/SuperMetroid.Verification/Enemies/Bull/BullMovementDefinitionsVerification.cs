using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Bull;

/// <summary>Compares Bull's compiled movement setup with retail acceleration, deceleration, and speed tables.</summary>
internal static class BullMovementDefinitionsVerification
{
    /// <summary>Checks all 104 parameter pairs through the real initializer, including timers and initial instruction.</summary>
    internal static void VerifyCompiledBullMovement(CartridgeImportAddressSpace rom)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        Action<RoomEnemyPlace> initialize = fixturePlace => enemies.BullBehavior.InitializeBull(fixturePlace.HeldOccupant(), fixturePlace);
        var place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Bull, place.KeptDefinition);
        for (ushort interval = 0; interval < 13; interval++)
        for (ushort speed = 0; speed < 8; speed++)
        {
            place.SetSpawnParameter1(EnemyDefinitionId.Bull, interval);
            place.SetSpawnParameter2(EnemyDefinitionId.Bull, speed);
            initialize(place); // No loaded cartridge bus; exercises the real initializer.
            var state = enemies.BullStates[0] ?? throw new InvalidOperationException("Verification requires state.");
            BullSpeedIntervals intervals = place.HeldOccupant().SpawnSettings<BullSpawnSettings>().Intervals;
            AssertEqual(rom.ReadCartridgeWord((0xa8d895 + interval * 4)), intervals.Acceleration, "Bull compiled acceleration");
            AssertEqual(intervals.Acceleration, state.AccelerationIntervalTimer.UpdatesLeft, "Bull live timer initialization");
            AssertEqual(rom.ReadCartridgeWord((0xa8d897 + interval * 4)), intervals.Deceleration, "Bull compiled deceleration");
            AssertEqual(rom.ReadCartridgeWord((0xa8d885 + speed * 2)), place.HeldOccupant().SpawnSettings<BullSpawnSettings>().MaximumSpeed.InTwoHundredFiftySixthsOfPixel, "Bull compiled speed");
            AssertEqual(BullAction.MovementDelay, state.Action, "Bull initial function");
            AssertEqual(16, state.ActivationTimer.UpdatesLeft, "Bull native activation delay");
            AssertEqual(1, place.AnimationClock.Hold, "Bull instruction timer");
            AssertEqual(new AnimationPlayback<BullSegment>(BullAnimations.StartingWith(BullSegment.Idle), AnimationPlace.StartOfAnimation), state.Playback, "Bull initial animation");
        }
        AssertThrows<InvalidDataException>(() => place.SetSpawnParameter2(EnemyDefinitionId.Bull, 8), "Bull import rejects the speed selector past its table");
        AssertThrows<InvalidDataException>(() => place.SetSpawnParameter1(EnemyDefinitionId.Bull, 13), "Bull import rejects the interval selector past its table");
        Console.WriteLine("Bull definitions: all 104 authored initializer combinations match native tables without a loaded bus; live timers and instruction setup preserved.");
    }
}
