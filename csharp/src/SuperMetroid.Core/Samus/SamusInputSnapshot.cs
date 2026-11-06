using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus;

/// <summary>The input Samus's handlers see: the filtered held and new input of the previous draw, the auto-jump timer and pending jump, and the pose input locks for Shinespark and Crystal Flash.</summary>
internal sealed class SamusInputSnapshot
{
    /// <summary>
    /// The live special-palette timer: ordinary stored-shine countdown or Crystal Flash's palette
    /// timer. A grab-interrupted Flash keeps the latter ticking indefinitely, so
    /// the game's jump admission still sees a nonzero shine without a Stored phase.
    /// </summary>
    public static int SharedShineTimer(SamusState samus) => samus.CrystalFlash.SpecialPaletteKind == SamusSpecialPaletteType.CrystalFlash
        ? samus.CrystalFlash.SpecialPaletteTimer.RemainingFrames : samus.Shinespark.ShineTimer.RemainingFrames;

    /// <summary>
    /// Joypad1_newinput_samusfilter: the Fire edge retained by the post-draw input filter
    /// after drawing Samus and projectiles. Missile admission and grapple's inactive function accept either
    /// this previous-frame edge or the current frame's edge, allowing a press that first
    /// exits a non-firing pose to fire on the next frame. Demo alpha replaces it with
    /// the preceding scripted edge before running the normal HUD dispatcher.
    /// </summary>
    public ControllerButton PreviousDrawNewInput { get; internal set; }

    /// <summary>0 through 65535: consecutive held-Jump history updated by the post-draw input filter.</summary>
    public int AutoJumpTimer
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Held input published by the post-draw input filter and the main loop's tail.</summary>
    public ControllerButton PreviousDrawHeldInput { get; internal set; }

    /// <summary>PoseInputHandler currently selects the one-shot auto-jump handler.</summary>
    public bool AutoJumpInputPending { get; internal set; }

    /// <summary>
    /// Shinespark launch installed the empty pose-input handler. This
    /// survives an independently replaced movement handler until input is restored,
    /// for example by normal landing's one-shot auto-jump handler.
    /// </summary>
    public bool ShinesparkPoseInputLocked { get; internal set; }

    /// <summary>Crystal Flash's empty input pointer survives grab/release until input is restored.</summary>
    public bool CrystalFlashPoseInputLocked { get; internal set; }

    /// <summary>Local input substitution from the auto-jump handler; does not alter the controller's real edge.</summary>
    internal ControllerButton ConsumeAutoJumpInput(ControllerButton newlyPressed)
    {
        if (!AutoJumpInputPending) return newlyPressed;
        ShinesparkPoseInputLocked = false;
        CrystalFlashPoseInputLocked = false;
        AutoJumpInputPending = false;
        if (AutoJumpTimer != 0 && JumpHoldReadsAsFresh(AutoJumpTimer))
        {
            AutoJumpTimer = 0;
            newlyPressed |= ControllerButton.A;
        }
        return newlyPressed;
    }

    /// <summary>
    /// The held-jump count from which a hold reads as fresh again: the game compares the hold with the auto-jump limit by
    /// the sign of their difference, so a jump held for half the counter's 65536-update range past the limit counts as
    /// pressed again, until the count continues from zero.
    /// </summary>
    private const int JumpHoldReadsFreshFrom = 32768 + SamusAutoJumpDefinitions.TimerComparisonLimit;

    /// <summary>Whether a jump held for <paramref name="heldUpdates"/> reads as below the auto-jump limit.</summary>
    private static bool JumpHoldReadsAsFresh(int heldUpdates) =>
        heldUpdates < SamusAutoJumpDefinitions.TimerComparisonLimit || heldUpdates >= JumpHoldReadsFreshFrom;

    /// <summary>Publishes held input without advancing Samus's draw-time owners.</summary>
    internal void PublishMainLoopHeldInput(ControllerButton held) => PreviousDrawHeldInput = held;

    /// <summary>The post-draw input filter increments only when Jump is held in both draw samples.</summary>
    internal void SnapshotDrawInput(ControllerButton held, ControllerButton newlyPressed)
    {
        ControllerButton jump = ControllerButton.A;
        AutoJumpTimer = (held & PreviousDrawHeldInput & jump) != ControllerButton.None
            ? WrappingCounter.IncrementWord(AutoJumpTimer) : 0;
        PreviousDrawHeldInput = held;
        PreviousDrawNewInput = newlyPressed;
    }

    /// <summary>Health-feedback tail of the post-draw input filter, independent of knockback admission.</summary>
    internal static void UpdateHurtFlashFromHealthLoss(SamusState samus)
    {
        if (samus.Health < samus.PreviousHealthForHurtCheck && samus.HurtFlashCounter == 0)
            samus.HurtFlashCounter = 1;
        samus.PreviousHealthForHurtCheck = samus.Health;
    }
}
