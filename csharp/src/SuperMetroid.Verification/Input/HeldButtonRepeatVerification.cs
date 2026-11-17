using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Input;

/// <summary>Checks the pause screen's held-button repeat.</summary>
internal static class HeldButtonRepeatVerification
{
    /// <summary>
    /// The pause screen's delay of three: a fresh press is not held on its own update, the first held update is a change,
    /// three unchanged updates stay quiet and the fourth and every one after repeat; a release, or letting go of one button
    /// of a chord, is a change that restarts the delay. A delay of zero repeats on the first unchanged update.
    /// </summary>
    internal static void VerifyHeldButtonRepeat()
    {
        const ControllerButton button = ControllerButton.A;
        var repeat = new HeldButtonRepeat();

        repeat.Update(3, button, button);
        AssertEqual(ControllerButton.None, repeat.Repeating, "a fresh press is not held");

        repeat.Update(3, button, ControllerButton.None);
        AssertEqual(0, repeat.UnchangedUpdates, "the first held update is a change");
        AssertEqual(ControllerButton.None, repeat.Repeating, "a change does not repeat");

        for (int unchanged = 1; unchanged <= 3; unchanged++)
        {
            repeat.Update(3, button, ControllerButton.None);
            AssertEqual(ControllerButton.None, repeat.Repeating, $"unchanged update {unchanged} waits through the delay");
        }

        for (int repeated = 0; repeated < 3; repeated++)
        {
            repeat.Update(3, button, ControllerButton.None);
            AssertEqual(button, repeat.Repeating, "every unchanged update past the delay repeats");
        }

        repeat.Update(3, ControllerButton.None, ControllerButton.None);
        AssertEqual(ControllerButton.None, repeat.Repeating, "a release restarts the delay");

        const ControllerButton chord = ControllerButton.Left | ControllerButton.A;
        var chordRepeat = new HeldButtonRepeat();
        chordRepeat.Update(0, chord, ControllerButton.None);
        chordRepeat.Update(0, chord, ControllerButton.None);
        AssertEqual(chord, chordRepeat.Repeating, "a delay of zero repeats on the first unchanged update");
        chordRepeat.Update(0, ControllerButton.Left, ControllerButton.None);
        AssertEqual(ControllerButton.None, chordRepeat.Repeating, "letting go of one button of a chord is a change");
        chordRepeat.Update(0, ControllerButton.Left, ControllerButton.None);
        AssertEqual(ControllerButton.Left, chordRepeat.Repeating, "the remaining button repeats after the delay");

        AssertThrows<ArgumentOutOfRangeException>(() => chordRepeat.Update(-1, ControllerButton.Left, ControllerButton.None),
            "a negative delay is rejected");
    }
}
