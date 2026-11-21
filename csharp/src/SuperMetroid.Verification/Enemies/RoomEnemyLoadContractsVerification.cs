using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>Checks room-enemy loading interfaces and standalone service capability boundaries.</summary>
internal static class RoomEnemyLoadContractsVerification
{
    /// <summary>
    /// #633: the room-enemy load contract is the union of its children's contracts, each child contract
    /// exposes only its own capabilities, and standalone services fail at an uninstalled dependency instead of returning a
    /// default. Every member of the concrete request is required (#2379), which the compiler enforces.
    /// </summary>
    internal static void VerifyRoomEnemyLoadContracts()
    {
        // The contracts' unions are the compiler's: the load is its population source and actor context, and the room
        // services are every child service contract.
        Func<IRoomEnemyLoad, IRoomEnemyPopulationSource> population = static load => load;
        Func<IRoomEnemyLoad, IRoomEnemyActorContext> actors = static load => load;
        Func<IRoomEnemyServices, IChozoStatueRoomServices> statue = static room => room;
        Func<IRoomEnemyServices, IMotherBrainRoomServices> motherBrain = static room => room;
        AssertTrue(population is not null && actors is not null && statue is not null && motherBrain is not null,
            "the load and room-service contracts include their children's contracts");

        var services = new StandaloneRoomEnemyServices(() => 0x1234);
        AssertEqual(0x1234, services.Random.Generate(), "the standalone RNG generates through its installed generator");
        AssertThrows<InvalidOperationException>(() => services.Random.Read(), "an uninstalled RNG read fails at the dependency");
        AssertThrows<InvalidOperationException>(() => services.Events.Has(StoryEvent.ZebesAwake), "uninstalled event bits fail at the dependency");
        int seed = 0;
        var seeded = new StandaloneRoomEnemyServices(() => 0) { ReadRandom = () => seed, SeedRandom = value => seed = value };
        seeded.Random.Reseed(SpeciesRandomReseed.BeetomPlacement);
        AssertEqual(SpeciesRandomReseed.BeetomPlacement.Seed.Value, seeded.Random.Read(), "installed RNG seed reads and writes share the word");
    }

    /// <summary>Runs the focused room-enemy load and service contract check.</summary>
    [VerificationCommand("--room-enemy-load-contracts")]
    internal static int RoomEnemyLoadContractsCommand()
    {
        Suite(nameof(VerifyRoomEnemyLoadContracts), () => VerifyRoomEnemyLoadContracts());
        return 0;
    }
}
