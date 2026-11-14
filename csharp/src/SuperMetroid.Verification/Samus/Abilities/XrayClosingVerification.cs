using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks X-ray's closing: its exact update count, when the beam's window closes, and when Samus is released.</summary>
internal static class XrayClosingVerification
{
    /// <summary>
    /// #1793: once the beam reads Run released, it spends exactly two closing updates of frozen time, its window showing
    /// only until the first is spent, and the update after them releases Samus and the room's time.
    /// </summary>
    internal static void VerifyXrayClosing()
    {
        XrayClosing closing = XrayClosing.Started;
        for (int spent = 0; spent < XrayClosing.Updates; spent++)
        {
            AssertTrue(!closing.IsOver, $"the closing is still running after {spent} updates");
            AssertEqual(spent == 0, closing.ShowsBeam, $"the beam's window after {spent} closing updates");
            closing = closing.Spend();
        }
        AssertEqual(2, XrayClosing.Updates, "the closing lasts two updates");
        AssertEqual(XrayClosing.Over, closing, "an ended closing is the closing that is over");
        AssertTrue(!closing.ShowsBeam, "the beam's window is closed once the closing is over");
        AssertThrows<InvalidOperationException>(() => XrayClosing.Over.Spend(), "an ended closing cannot be spent");

        // Production stepping: the beam's first step after the warm-up reads Run released and starts the closing; each
        // closing update keeps time frozen and Samus held; the update after the last releases her.
        SamusState samus = XrayBeamFixtures.AdmittedStanding();
        for (int update = 0; update < XrayWarmUp.Updates; update++)
            samus.Xray.StepBeam(samus, ControllerButton.None);
        samus.Xray.StepBeam(samus, ControllerButton.None);
        AssertEqual(XrayBeamPhase.Closing, samus.Xray.BeamPhase, "Run's release starts the beam's closing");
        AssertEqual(XrayClosing.Started, samus.Xray.Closing, "the closing starts with none of its updates spent");
        for (int update = 1; update <= XrayClosing.Updates; update++)
        {
            XrayBeamStepResult step = samus.Xray.StepBeam(samus, ControllerButton.None);
            AssertEqual(XrayClosing.Updates - update, samus.Xray.Closing.UpdatesLeft, $"closing update {update} spends one update");
            AssertEqual(update < XrayClosing.Updates ? XrayBeamPhase.Closing : XrayBeamPhase.Finish, samus.Xray.BeamPhase,
                $"the beam's phase after closing update {update}");
            AssertTrue(!step.Completed && samus.Xray.IsActive && samus.Xray.TimeIsFrozen && samus.Xray.OwnsSamusControl,
                $"closing update {update} keeps time frozen and Samus held");
        }
        XrayBeamStepResult last = samus.Xray.StepBeam(samus, ControllerButton.None);
        AssertTrue(last.Completed && !samus.Xray.IsActive && !samus.Xray.TimeIsFrozen && !samus.Xray.OwnsSamusControl,
            "the update after the closing releases Samus and the room's time");
        AssertEqual(XrayClosing.Over, samus.Xray.Closing, "a released X-ray holds the closing that is over");
        Console.WriteLine("X-ray closing: two updates of frozen time, the window closed after the first, Samus released on the third.");
    }
}
