using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Polyp;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Polyp state the species owns: its initial behaviour.</summary>
internal static class PolypOwnedStateVerification
{
    /// <summary>
    /// Initializes a Polyp and confirms it starts waiting for Samus with no cooldown.
    /// </summary>
    internal static void VerifyPolypOwnedState()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().SeedRandom = _ => { };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.LavaRocks, slot.Definition);
        enemies.PolypBehavior.InitializePolyp(slot);
        PolypEnemyState state = slot.RequireSpeciesState<PolypEnemyState>();
        AssertEqual<PolypAction?>(PolypAction.WaitingForSamus, state.Action, "a new Polyp waits for Samus");
        AssertEqual(0, state.CooldownTimer, "a new Polyp has no cooldown");
        Console.WriteLine("Polyp owned state: the initial behaviour matches.");
    }

    /// <summary>Runs the focused Polyp owned-state check.</summary>
    [VerificationCommand("--polyp-owned-state")]
    internal static int PolypOwnedStateCommand()
    {
        VerifyPolypOwnedState();
        return 0;
    }
}
