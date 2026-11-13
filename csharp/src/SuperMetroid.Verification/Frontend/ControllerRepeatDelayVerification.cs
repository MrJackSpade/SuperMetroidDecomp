using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Frontend;

/// <summary>Checks the held-input reload.</summary>
internal static class ControllerRepeatDelayVerification
{
/// <summary>
/// The held-input reload is a frame count that is only reloaded into a countdown: a reload of zero still lets the
/// countdown wrap below zero exactly as before, and a reload past the countdown's range is rejected where it is given.
/// </summary>
internal static void VerifyRepeatAndHeldInputDelays()
{
    var system = new SuperMetroid.Core.Progression.SharedGameState(new SharedRandom());
    system.UpdateHeldInput(0, ControllerButton.Left, ControllerButton.None);
    AssertEqual(ControllerButton.None, system.TimedHeldInput, "a held-input change restarts the delay");
    system.UpdateHeldInput(0, ControllerButton.Left, ControllerButton.None);
    AssertEqual(ControllerButton.Left, system.TimedHeldInput, "a zero reload activates on the first stable update by wrapping below zero");
    AssertEqual(0, system.TimedHeldInputTimer.RawValue, "the expired held-input countdown is pinned to zero");
    system.UpdateHeldInput(WrappingCountdown.LongestStart, ControllerButton.Right, ControllerButton.None);
    AssertEqual(WrappingCountdown.LongestStart, system.TimedHeldInputTimer.RawValue, "the longest reload starts the countdown at its longest start");
    AssertThrows<ArgumentOutOfRangeException>(() => system.UpdateHeldInput(WrappingCountdown.LongestStart + 1, ControllerButton.Left, ControllerButton.None),
        "a held-input reload past the countdown's longest start is rejected");
}
}
