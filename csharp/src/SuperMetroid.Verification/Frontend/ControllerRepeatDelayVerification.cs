using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Frontend;

/// <summary>Checks the controller's repeat delays and the held-input reload.</summary>
internal static class ControllerRepeatDelayVerification
{
/// <summary>
/// The controller repeat delays and the held-input reload are frame counts that are only reloaded into countdowns: a
/// delay of zero still lets the countdown wrap below zero exactly as before, and a delay past the countdown's range is
/// rejected where it is set.
/// </summary>
internal static void VerifyRepeatAndHeldInputDelays()
{
    var controller = new ControllerInputState();
    controller.Latch(SnesButton.Left);
    controller.Latch(SnesButton.Left);
    AssertEqual(ushort.MaxValue, controller.RepeatTimer.RawValue, "a zero repeat delay wraps the repeat countdown below zero");
    AssertEqual(SnesButton.None, controller.NewlyPressedWithRepeat, "a wrapped repeat countdown does not pulse");
    AssertThrows<ArgumentOutOfRangeException>(() => controller.InitialRepeatDelay = ushort.MaxValue + 1,
        "a repeat delay past the countdown's range is rejected");
    AssertThrows<ArgumentOutOfRangeException>(() => new ControllerInputState(subsequentRepeatDelay: -1),
        "a negative repeat delay is rejected");

    var system = new SuperMetroid.Core.Progression.Bank80SystemState();
    system.UpdateHeldInput(0, SnesButton.Left, SnesButton.None);
    AssertEqual(SnesButton.None, system.TimedHeldInput, "a held-input change restarts the delay");
    system.UpdateHeldInput(0, SnesButton.Left, SnesButton.None);
    AssertEqual(SnesButton.Left, system.TimedHeldInput, "a zero reload activates on the first stable update by wrapping below zero");
    AssertEqual(0, system.TimedHeldInputTimer.RawValue, "the expired held-input countdown is pinned to zero");
    AssertThrows<ArgumentOutOfRangeException>(() => system.UpdateHeldInput(ushort.MaxValue + 1, SnesButton.Left, SnesButton.None),
        "a held-input reload past the countdown's range is rejected");
}
}
