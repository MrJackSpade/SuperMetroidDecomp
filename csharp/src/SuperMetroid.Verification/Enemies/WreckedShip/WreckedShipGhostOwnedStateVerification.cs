using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
namespace SuperMetroid.Verification.Enemies.WreckedShip;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Wrecked Ship ghost state the species owns: the cleared record and the extension that outlives it.</summary>
internal static class WreckedShipGhostOwnedStateVerification
{
    /// <summary>
    /// Installs a visible bobbing ghost state on a slot and confirms a kill empties the behaviour, phase timer and
    /// flicker offset kept in the common record while the bobbing origin in the extension outlives it.
    /// </summary>
    internal static void VerifyWreckedShipGhostOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Coven, slot.Definition);
        var state = new WreckedShipGhostEnemyState
        {
            Function = WreckedShipGhostAiFunction.BobbingWhileVisible,
            PhaseTimer = WrappingCountdown.Start(120),
            FlickerTableOffset = 6,
            BobbingOrigin = new(0, 96),
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        slot.Clear();
        AssertEqual<WreckedShipGhostAiFunction?>(null, state.Function, "a kill empties the behaviour");
        AssertEqual(WrappingCountdown.Start(0), state.PhaseTimer, "a kill empties the phase timer");
        AssertEqual(0, state.FlickerTableOffset, "a kill empties the flicker offset");
        AssertEqual(new Position16<RoomPixels>(0, 96), state.BobbingOrigin, "the bobbing origin outlives the kill");
        AssertThrows<ArgumentOutOfRangeException>(() => state.FlickerTableOffset = ushort.MaxValue + 1,
            "the flicker offset rejects a value above sixteen bits");
        Console.WriteLine("Wrecked Ship ghost owned state: the cleared record and the surviving extension match.");
    }

    /// <summary>Runs the focused Wrecked Ship ghost owned-state check.</summary>
    [VerificationCommand("--wrecked-ship-ghost-owned-state")]
    internal static int WreckedShipGhostOwnedStateCommand()
    {
        VerifyWreckedShipGhostOwnedState();
        return 0;
    }
}
