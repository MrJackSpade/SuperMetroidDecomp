using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Kago;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Kago state the species owns: its behaviour once initialized.</summary>
internal static class KagoOwnedStateVerification
{
    /// <summary>Initializes a Kago and confirms it has a behaviour.</summary>
    internal static void VerifyKagoOwnedState()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Kago, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnParameter1(EnemyDefinitionId.Kago, 10);
        KagoBehavior.InitializeKago(slot);
        KagoEnemyState state = enemies.KagoStates[0] ?? throw new InvalidDataException("Kago initializer did not publish typed state.");
        AssertTrue(state.Action is not null, "an initialized Kago has a behaviour");
        Console.WriteLine("Kago owned state: an initialized Kago behaves.");
    }

    /// <summary>Runs the focused Kago owned-state check.</summary>
    [VerificationCommand("--kago-owned-state")]
    internal static int KagoOwnedStateCommand()
    {
        VerifyKagoOwnedState();
        return 0;
    }
}
