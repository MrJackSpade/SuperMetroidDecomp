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
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Puromi, slot.Definition);
        slot.SetSpawnParameter1(EnemyDefinitionId.Puromi, LittleEndianBytes.Word(24, 32));
        slot.SetSpawnParameter2(EnemyDefinitionId.Puromi, LittleEndianBytes.Word(1, 6));
        slot.Position = slot.Position.WithWhole(new(384, 512));
        enemies.NuclearWaffleBehavior.InitializeNuclearWaffle(slot);
        NuclearWaffleEnemyState state = enemies.NuclearWaffleStates[0] ?? throw new InvalidOperationException("Nuclear Waffle has no state.");
        AssertEqual<NuclearWaffleAction?>(NuclearWaffleAction.Waiting, state.Action, "a new Nuclear Waffle waits");
        AssertEqual(6, state.WaitingTimer, "the wait starts at the population's reset value");
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
