using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.Kago.Artwork;
namespace SuperMetroid.Verification.Enemies.Kago;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Kago state the species owns: its cleared record and the hit counter that outlives it.</summary>
internal static class KagoOwnedStateVerification
{
    /// <summary>
    /// Initializes a Kago, marks it fast and dying, and confirms a kill empties the behaviour, the fast loop and the
    /// death latch kept in the common record while the hit counter outlives it.
    /// </summary>
    internal static void VerifyKagoOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Kago, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = KagoFrameToolingDefinitions.Bank });
        slot.SetSpawnParameter1(10);
        KagoBehavior.InitializeKago(slot);
        KagoEnemyState state = enemies.KagoStates[0] ?? throw new InvalidDataException("Kago initializer did not publish typed state.");
        AssertTrue(state.Function is not null, "an initialized Kago has a behaviour");
        state.UsesFastAnimation = true;
        state.DeathAnimationStarted = true;
        var hits = state.HitCounter;

        slot.Clear();
        AssertEqual<KagoEnemyFunction?>(null, state.Function, "a kill empties the behaviour");
        AssertTrue(!state.UsesFastAnimation, "a kill returns the shell to the slow loop");
        AssertTrue(!state.DeathAnimationStarted, "a kill empties the death latch");
        AssertEqual(hits, state.HitCounter, "the hit counter outlives the kill");
        Console.WriteLine("Kago owned state: the cleared record and the surviving hit counter match.");
    }

    /// <summary>Runs the focused Kago owned-state check.</summary>
    [VerificationCommand("--kago-owned-state")]
    internal static int KagoOwnedStateCommand()
    {
        VerifyKagoOwnedState();
        return 0;
    }
}
