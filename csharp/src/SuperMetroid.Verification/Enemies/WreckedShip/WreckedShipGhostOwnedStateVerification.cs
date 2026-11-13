using SuperMetroid.Core.Enemies.WreckedShip;
namespace SuperMetroid.Verification.Enemies.WreckedShip;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Wrecked Ship ghost state the species owns: the range of its flicker step.</summary>
internal static class WreckedShipGhostOwnedStateVerification
{
    /// <summary>
    /// Confirms a ghost's next flicker step reaches the position past the last step, which ends the flicker, and no further.
    /// </summary>
    internal static void VerifyWreckedShipGhostOwnedState()
    {
        var state = new WreckedShipGhostEnemyState
        {
            Action = WreckedShipGhostAction.BobbingWhileVisible,
            PhaseTimer = 120,
            NextFlickerStep = WreckedShipGhostAppearanceDefinitions.FlickerSteps.Count,
            BobbingOrigin = new(0, 96),
        };
        AssertThrows<ArgumentOutOfRangeException>(() => state.NextFlickerStep = WreckedShipGhostAppearanceDefinitions.FlickerSteps.Count + 1,
            "the next flicker step rejects a position beyond the flicker's end");
        AssertThrows<ArgumentOutOfRangeException>(() => state.NextFlickerStep = -1,
            "the next flicker step rejects a position before the first step");
        Console.WriteLine("Wrecked Ship ghost owned state: the next flicker step's range matches.");
    }

    /// <summary>Runs the focused Wrecked Ship ghost owned-state check.</summary>
    [VerificationCommand("--wrecked-ship-ghost-owned-state")]
    internal static int WreckedShipGhostOwnedStateCommand()
    {
        VerifyWreckedShipGhostOwnedState();
        return 0;
    }
}
