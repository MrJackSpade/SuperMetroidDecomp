using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Nuclear;
namespace SuperMetroid.Verification.Enemies.Nuclear;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Nuclear Waffle state the species owns: its initial wait and the cleared record.</summary>
internal static class NuclearWaffleOwnedStateVerification
{
    /// <summary>
    /// Initializes a Nuclear Waffle and confirms it starts waiting for its population wait, then confirms a kill
    /// empties the behaviour and the wait kept in the common record while the sweep it owns outlives the kill.
    /// </summary>
    internal static void VerifyNuclearWaffleOwnedState()
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Puromi, slot.Definition);
        slot.SetSpawnParameter1(SnesLittleEndian.Word(24, 32));
        slot.SetSpawnParameter2(SnesLittleEndian.Word(1, 6));
        slot.Position = slot.Position.WithWhole(new(384, 512));
        enemies.NuclearWaffleBehavior.InitializeNuclearWaffle(slot);
        NuclearWaffleEnemyState state = enemies.NuclearWaffleStates[0] ?? throw new InvalidOperationException("Nuclear Waffle has no state.");
        AssertEqual<NuclearWaffleEnemyFunction?>(NuclearWaffleEnemyFunction.Waiting, state.Function, "a new Nuclear Waffle waits");
        AssertEqual(NativeCountdown.Start(6), state.WaitingTimer, "the wait starts at the population's reset value");

        slot.Clear();
        AssertEqual<NuclearWaffleEnemyFunction?>(null, state.Function, "a kill empties the behaviour");
        AssertEqual(NativeCountdown.Start(0), state.WaitingTimer, "a kill empties the wait");
        AssertTrue(state.SweepsForward, "the sweep direction outlives the kill");
        Console.WriteLine("Nuclear Waffle owned state: the initial wait and the cleared record match.");
    }

    /// <summary>Runs the focused Nuclear Waffle owned-state check.</summary>
    [VerificationCommand("--nuclear-waffle-owned-state")]
    internal static int NuclearWaffleOwnedStateCommand()
    {
        VerifyNuclearWaffleOwnedState();
        return 0;
    }
}
