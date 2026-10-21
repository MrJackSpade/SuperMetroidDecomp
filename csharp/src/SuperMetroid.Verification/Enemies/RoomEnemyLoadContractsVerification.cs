using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>Checks room-enemy loading interfaces and standalone service capability boundaries.</summary>
internal static class RoomEnemyLoadContractsVerification
{
    /// <summary>
    /// #633: the room-enemy load contract is the union of its children's contracts, each child contract
    /// exposes only its own capabilities, the concrete request rejects missing required capabilities, and
    /// standalone services fail at an uninstalled dependency instead of returning a default.
    /// </summary>
    internal static void VerifyRoomEnemyLoadContracts()
    {
        AssertTrue(typeof(IRoomEnemyPopulationSource).IsAssignableFrom(typeof(IRoomEnemyLoad)) &&
            typeof(IRoomEnemyActorContext).IsAssignableFrom(typeof(IRoomEnemyLoad)),
            "the load contract includes the population source and actor context its child loaders read");
        AssertTrue(typeof(IChozoStatueRoomServices).IsAssignableFrom(typeof(IRoomEnemyServices)) &&
            typeof(IMotherBrainRoomServices).IsAssignableFrom(typeof(IRoomEnemyServices)),
            "room services include every child service contract");

        static string[] Members(Type contract) => contract.GetProperties()
            .Concat(contract.GetInterfaces().SelectMany(parent => parent.GetProperties()))
            .Select(property => property.Name).Order(StringComparer.Ordinal).ToArray();
        AssertTrue(Members(typeof(IChozoStatueRoomServices)).SequenceEqual(["RoomScroll", "SamusControls"]),
            "the Chozo statue contract exposes only Samus's control lock and the scroll zones");
        AssertTrue(Members(typeof(IMotherBrainRoomServices)).SequenceEqual(["MotherBrain", "RoomScroll"]),
            "the Mother Brain contract exposes only its room effects and the scroll zones");

        var services = new StandaloneRoomEnemyServices(() => 0x1234);
        AssertEqual(GunshipLoadScenario.Ordinary, new RoomEnemyLoadRequest(
            RoomEnemyPopulationDefinitions.DraygonState1MotherBrainState2, RoomEnemyGraphicsSetDefinitions.Draygon1MotherBrain2, BgArtworkFixtures.For(new SnesVram()), new SnesCgram()).GunshipScenario,
            "the ordinary gunship scenario is the named default");

        AssertEqual(0x1234, services.Random.Generate(), "the standalone RNG generates through its installed generator");
        AssertThrows<InvalidOperationException>(() => services.Random.Read(), "an uninstalled RNG read fails at the dependency");
        AssertThrows<InvalidOperationException>(() => services.Events.Has(EventNumber.ZebesAwake), "uninstalled event bits fail at the dependency");
        int seed = 0;
        var seeded = new StandaloneRoomEnemyServices(() => 0) { ReadRandom = () => seed, SeedRandom = value => seed = value };
        seeded.Random.Seed(0x0017);
        AssertEqual(0x0017, seeded.Random.Read(), "installed RNG seed reads and writes share the word");
    }

    /// <summary>Runs the focused room-enemy load and service contract check.</summary>
    [VerificationCommand("--room-enemy-load-contracts")]
    internal static int RoomEnemyLoadContractsCommand()
    {
        Suite(nameof(VerifyRoomEnemyLoadContracts), () => VerifyRoomEnemyLoadContracts());
        return 0;
    }
}
