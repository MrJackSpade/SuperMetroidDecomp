using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks X-ray's warm-up: its exact update count, its blending rule, and the debugger decoding boundary.</summary>
internal static class XrayWarmUpVerification
{
    /// <summary>
    /// #1631: X-ray's beam spends exactly eight warm-up updates after admission, with the room's blend shown from the
    /// third, and its own step first runs on the ninth update, reading Run then. The debugger boundary rejects undefined
    /// saved subsystem flags.
    /// </summary>
    internal static void VerifyXrayWarmUp()
    {
        // The instruction list's eight setup calls each spend one update; $91:D27F first runs after call one, together
        // with call two, so blending shows from the third.
        XrayWarmUp warmUp = XrayWarmUp.Started;
        for (int spent = 0; spent < 8; spent++)
        {
            AssertTrue(!warmUp.IsOver, $"the warm-up is still running after {spent} updates");
            AssertEqual(spent >= 2, warmUp.ShowsBlendedRoom, $"blending after {spent} warm-up updates");
            warmUp = warmUp.Spend();
        }
        AssertTrue(warmUp.IsOver, "the warm-up is over after eight updates");
        AssertEqual(XrayWarmUp.Over, warmUp, "an ended warm-up is the warm-up that is over");
        AssertTrue(warmUp.ShowsBlendedRoom, "the room shows its blend once the warm-up is over");
        AssertThrows<InvalidOperationException>(() => XrayWarmUp.Over.Spend(), "an ended warm-up cannot be spent");

        // Production stepping spends one warm-up update per beam call, leaves the beam unchanged, and starts the beam's
        // own step on the ninth call whichever of Run held or released it then reads.
        foreach (bool holdingRun in new[] { true, false })
        {
            SamusState samus = XrayBeamFixtures.AdmittedStanding();
            AssertEqual(XrayWarmUp.Started, samus.Xray.WarmUp, "X-ray admission starts the warm-up");
            ControllerButton input = holdingRun ? ControllerButton.B : ControllerButton.None;
            for (int update = 1; update <= 8; update++)
            {
                XrayBeamStepResult step = samus.Xray.StepBeam(samus, input);
                AssertEqual(8 - update, samus.Xray.WarmUp.UpdatesLeft, $"warm-up update {update} spends one update");
                AssertEqual(XrayBeamPhase.NoBeam, samus.Xray.BeamPhase, $"warm-up update {update} leaves the beam unstarted");
                AssertTrue(!step.Completed && samus.Xray.IsActive, $"warm-up update {update} keeps X-ray active");
            }
            samus.Xray.StepBeam(samus, input);
            AssertEqual(holdingRun ? XrayBeamPhase.Widening : XrayBeamPhase.Closing, samus.Xray.BeamPhase,
                $"the ninth update runs the beam's first step with Run {(holdingRun ? "held" : "released")}");
        }

        // Debugger-state boundary: undefined saved enum values are rejected.
        AssertThrows<InvalidDataException>(() => DebuggerEnumValues.Decode(typeof(XraySuspendedSubsystems), (byte)0x80),
            "undefined flags bits are rejected at the boundary");
        Console.WriteLine("X-ray warm-up: eight updates, blending from the third, the beam's first step on the ninth.");
    }
}
