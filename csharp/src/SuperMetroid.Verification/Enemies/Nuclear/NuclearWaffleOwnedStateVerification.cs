using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Nuclear;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Nuclear Waffle state the species owns: its initial wait.</summary>
internal static class NuclearWaffleOwnedStateVerification
{
    /// <summary>
    /// Initializes a Nuclear Waffle and confirms it starts waiting for its population wait.
    /// </summary>
    internal static void VerifyNuclearWaffleOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.NuclearWaffle, place.Definition);
        place.SetSpawnParameter1(EnemyDefinitionId.NuclearWaffle, LittleEndianBytes.Word(24, 32));
        place.SetSpawnParameter2(EnemyDefinitionId.NuclearWaffle, LittleEndianBytes.Word(1, 6));
        place.Position = place.Position.WithWhole(new(384, 512));
        enemies.NuclearWaffleBehavior.InitializeNuclearWaffle(place);
        NuclearWaffleEnemyState state = enemies.NuclearWaffleStates[0] ?? throw new InvalidOperationException("Nuclear Waffle has no state.");
        AssertEqual<NuclearWaffleAction?>(NuclearWaffleAction.Waiting, state.Action, "a new Nuclear Waffle waits");
        AssertEqual(6 + EnemyWaitDefinitions.RunOutUpdate, state.WaitingTimer, "the wait starts at the population's wait, held as its true duration");
        Console.WriteLine("Nuclear Waffle owned state: the initial wait matches.");
    }

    /// <summary>Runs the focused Nuclear Waffle owned-state check.</summary>
    [VerificationCommand("--nuclear-waffle-owned-state")]
    internal static int NuclearWaffleOwnedStateCommand()
    {
        VerifyNuclearWaffleOwnedState();
        return 0;
    }
}
