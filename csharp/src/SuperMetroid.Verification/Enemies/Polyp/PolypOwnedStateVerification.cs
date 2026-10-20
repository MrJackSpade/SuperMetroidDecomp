using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Polyp;
namespace SuperMetroid.Verification.Enemies.Polyp;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Polyp state the species owns: its initial behaviour and the cleared record.</summary>
internal static class PolypOwnedStateVerification
{
    /// <summary>
    /// Initializes a Polyp and confirms it starts waiting for Samus with no cooldown, then confirms a kill empties the
    /// behaviour and the cooldown kept in the common record.
    /// </summary>
    internal static void VerifyPolypOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().SeedRandom = _ => { };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.LavaRocks, slot.Definition);
        enemies.PolypBehavior.InitializePolyp(slot);
        PolypEnemyState state = PolypBehavior.RequirePolypState(slot);
        AssertEqual<PolypEnemyFunction?>(PolypEnemyFunction.WaitingForSamus, state.Function, "a new Polyp waits for Samus");
        AssertEqual(NativeCountdown.Start(0), state.CooldownTimer, "a new Polyp has no cooldown");

        state.Function = PolypEnemyFunction.Cooldown;
        state.CooldownTimer = NativeCountdown.Start(5);
        slot.Clear();
        AssertEqual<PolypEnemyFunction?>(null, state.Function, "a kill empties the behaviour");
        AssertEqual(NativeCountdown.Start(0), state.CooldownTimer, "a kill empties the cooldown");
        Console.WriteLine("Polyp owned state: the initial behaviour and the cleared record match.");
    }

    /// <summary>Runs the focused Polyp owned-state check.</summary>
    [VerificationCommand("--polyp-owned-state")]
    internal static int PolypOwnedStateCommand()
    {
        VerifyPolypOwnedState();
        return 0;
    }
}
