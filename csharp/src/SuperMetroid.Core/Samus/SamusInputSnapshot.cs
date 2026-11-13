using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus;

/// <summary>The input Samus's handlers see: the filtered held and new input of the previous draw, the auto-jump timer and pending jump, and the pose input locks for Shinespark and Crystal Flash.</summary>
internal sealed class SamusInputSnapshot
{
    /// <summary>
    /// Whether Samus holds a stored shine as the footstep sound, her body's draw and the jump into a shinespark windup
    /// test it: while her stored-shine countdown runs, or while Crystal Flash's palette runs
    /// (<see cref="CrystalFlashReadsAsStoredShine"/>). A grab-interrupted Flash keeps its palette running indefinitely, so
    /// the jump admission still sees a shine without a Stored phase.
    /// </summary>
    public static bool HoldsShine(SamusState samus) => CrystalFlashReadsAsStoredShine.HoldsShine(
        samus.CrystalFlash.SpecialPaletteKind == SamusSpecialPaletteType.CrystalFlash, samus.Shinespark.ShineTimer);

    /// <summary>
    /// The Fire edge retained by the post-draw input filter
    /// after drawing Samus and projectiles. Missile admission and the grapple's inactive phase accept either
    /// this previous-frame edge or the current frame's edge, allowing a press that first
    /// exits a non-firing pose to fire on the next frame. Demo alpha replaces it with
    /// the preceding scripted edge before running the normal HUD dispatcher.
    /// </summary>
    public ControllerButton PreviousDrawNewInput { get; internal set; }

    /// <summary>
    /// Consecutive held-Jump history updated by the post-draw input filter: a wrapping count, 0 through
    /// <see cref="WrappingCounter.LargestCount"/>, kept narrow because its wrap is gameplay (#1627). A Jump held past the
    /// largest count continues from zero, and the auto-jump compares the count with its limit by the sign of their
    /// difference, so a hold long enough reads as fresh again (<see cref="JumpHoldReadsAsFresh"/>).
    /// </summary>
    public int AutoJumpTimer
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, WrappingCounter.LargestCount, value);
    }

    /// <summary>Held input published by the post-draw input filter and the main loop's tail.</summary>
    public ControllerButton PreviousDrawHeldInput { get; internal set; }

    /// <summary>The pose input handler currently selects the one-shot auto-jump handler.</summary>
    public bool AutoJumpInputPending { get; internal set; }

    /// <summary>
    /// Shinespark launch installed the empty pose-input handler. This
    /// survives an independently replaced movement handler until input is restored,
    /// for example by normal landing's one-shot auto-jump handler.
    /// </summary>
    public bool ShinesparkPoseInputLocked { get; internal set; }

    /// <summary>Crystal Flash's empty input handler survives grab/release until input is restored.</summary>
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
    /// Whether a jump held for <paramref name="heldUpdates"/> reads as below the auto-jump limit: the game compares the
    /// hold with the limit by the sign of their difference (<see cref="WrappingCounter.IsBehind"/>), so a jump held for
    /// half the count's range past the limit counts as pressed again, until the count continues from zero.
    /// </summary>
    private static bool JumpHoldReadsAsFresh(int heldUpdates) =>
        WrappingCounter.IsBehind(heldUpdates, SamusAutoJumpDefinitions.TimerComparisonLimit);

    /// <summary>Publishes held input without advancing Samus's draw-time owners.</summary>
    internal void PublishMainLoopHeldInput(ControllerButton held) => PreviousDrawHeldInput = held;

    /// <summary>The post-draw input filter increments only when Jump is held in both draw samples.</summary>
    internal void SnapshotDrawInput(ControllerButton held, ControllerButton newlyPressed)
    {
        ControllerButton jump = ControllerButton.A;
        AutoJumpTimer = (held & PreviousDrawHeldInput & jump) != ControllerButton.None
            ? WrappingCounter.IncrementCount(AutoJumpTimer) : 0;
        PreviousDrawHeldInput = held;
        PreviousDrawNewInput = newlyPressed;
    }

    /// <summary>Health-feedback tail of the post-draw input filter, independent of knockback admission.</summary>
    internal static void UpdateHurtFlashFromHealthLoss(SamusState samus)
    {
        if (samus.Health < samus.PreviousHealthForHurtCheck && samus.HurtFlash.IsIdle)
            samus.HurtFlash.Start();
        samus.PreviousHealthForHurtCheck = samus.Health;
    }
}
