using SuperMetroid.Core.Enemies.WreckedShip;
namespace SuperMetroid.Verification.Enemies.WreckedShip;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Wrecked Ship ghost state the species owns: the order of its flicker's steps.</summary>
internal static class WreckedShipGhostOwnedStateVerification
{
    /// <summary>
    /// Confirms a ghost's flicker steps run from the first through each timing in order and end after the last (#2451),
    /// each step carrying its own timing and the step after it (#2512).
    /// </summary>
    internal static void VerifyWreckedShipGhostOwnedState()
    {
        IReadOnlyList<WreckedShipGhostFlickerTiming> timings = WreckedShipGhostAppearanceDefinitions.FlickerTimings;
        WreckedShipGhostFlickerStep? step = WreckedShipGhostFlickerStep.First;
        for (int position = 0; position < timings.Count; position++)
        {
            WreckedShipGhostFlickerStep taken = step ?? throw new InvalidOperationException($"The ghost's flicker ended before its step {position}.");
            AssertEqual(timings[position].Duration, taken.Duration, $"flicker step {position} lasts its timing's updates");
            AssertEqual(timings[position].Visibility, taken.Visibility, $"flicker step {position} shows the ghost as its timing says");
            step = taken.Next;
        }
        AssertEqual<WreckedShipGhostFlickerStep?>(null, step, "the ghost's flicker has no step after its last");
        Console.WriteLine("Wrecked Ship ghost owned state: the flicker's steps run in order and end after the last.");
    }

    /// <summary>Runs the focused Wrecked Ship ghost owned-state check.</summary>
    [VerificationCommand("--wrecked-ship-ghost-owned-state")]
    internal static int WreckedShipGhostOwnedStateCommand()
    {
        VerifyWreckedShipGhostOwnedState();
        return 0;
    }
}
