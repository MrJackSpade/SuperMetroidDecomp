using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
namespace SuperMetroid.Verification.Enemies.WreckedShip;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Wrecked Ship ghost state the species owns: the range of its flicker offset.</summary>
internal static class WreckedShipGhostOwnedStateVerification
{
    /// <summary>
    /// Confirms a ghost's flicker offset rejects a value above sixteen bits.
    /// </summary>
    internal static void VerifyWreckedShipGhostOwnedState()
    {
        var state = new WreckedShipGhostEnemyState
        {
            Function = WreckedShipGhostAiFunction.BobbingWhileVisible,
            PhaseTimer = 120,
            FlickerTableOffset = 6,
            BobbingOrigin = new(0, 96),
        };
        AssertThrows<ArgumentOutOfRangeException>(() => state.FlickerTableOffset = ushort.MaxValue + 1,
            "the flicker offset rejects a value above sixteen bits");
        Console.WriteLine("Wrecked Ship ghost owned state: the flicker offset's range matches.");
    }

    /// <summary>Runs the focused Wrecked Ship ghost owned-state check.</summary>
    [VerificationCommand("--wrecked-ship-ghost-owned-state")]
    internal static int WreckedShipGhostOwnedStateCommand()
    {
        VerifyWreckedShipGhostOwnedState();
        return 0;
    }
}
