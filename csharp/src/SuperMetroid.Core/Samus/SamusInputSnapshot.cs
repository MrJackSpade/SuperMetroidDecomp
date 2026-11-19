using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus;

/// <summary>The input Samus's input and movement phases see: the filtered held and new input of the previous draw, the auto-jump timer and pending jump, and the pose input locks for Shinespark and Crystal Flash.</summary>
internal sealed class SamusInputSnapshot
{
    /// <summary>
    /// Whether Samus holds a stored shine as the footstep sound, her body's draw and the jump into a shinespark windup
    /// test it: while her stored-shine countdown runs, or while Crystal Flash's shade runs
    /// (<see cref="CrystalFlashReadsAsStoredShine"/>). A grab-interrupted Flash keeps its shade running indefinitely, so
    /// the jump admission still sees a shine without a Stored phase.
    /// </summary>
    public static bool HoldsShine(SamusState samus) => CrystalFlashReadsAsStoredShine.HoldsShine(
        samus.SpecialColouring.Holds(SamusSpecialColouringKind.CrystalFlash), samus.Shinespark.ShineTimer.UpdatesLeft);

    /// <summary>
    /// The Fire edge retained by the post-draw input filter
    /// after drawing Samus and projectiles. Missile admission and the grapple's inactive phase accept either
    /// this previous-frame edge or the current frame's edge, allowing a press that first
    /// exits a non-firing pose to fire on the next frame. The demo's input phase replaces it with
    /// the preceding scripted edge before running the normal HUD step.
    /// </summary>
    public GameplayAction PressesSeenLastUpdate { get; internal set; }

    /// <summary>
    /// Consecutive held-Jump history updated by the post-draw input filter: a wrapping count, 0 through
    /// <see cref="WrappingCounter.LargestCount"/>, kept narrow because its wrap is gameplay (#1627). A Jump held past the
    /// largest count continues from zero, and the auto-jump compares the count with its limit by whether their
    /// difference is negative, so a hold long enough reads as fresh again (<see cref="JumpHoldReadsAsFresh"/>).
    /// </summary>
    public int AutoJumpTimer
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, WrappingCounter.LargestCount, value);
    }

    /// <summary>Held input published by the post-draw input filter and at the end of each update.</summary>
    public GameplayAction PreviousDrawHeldInput { get; private set; }

    /// <summary>The one-shot auto-jump, when a landing or a turn has installed it as Samus's pose input.</summary>
    public AutoJumpInput AutoJump { get; } = new();

    /// <summary>
    /// Shinespark launch locked her pose input. This
    /// survives an independent change of her movement owner until input is restored,
    /// for example by normal landing's one-shot auto-jump.
    /// </summary>
    public bool ShinesparkPoseInputLocked { get; internal set; }

    /// <summary>Crystal Flash's pose-input lock survives grab/release until input is restored.</summary>
    public bool CrystalFlashPoseInputLocked { get; internal set; }

    /// <summary>
    /// The input phase's substitution from the auto-jump, when installed: it runs once and reinstates her ordinary input; it
    /// does not alter the controller's real edge.
    /// </summary>
    internal GameplayAction ApplyAutoJumpInput(GameplayAction newlyPressed)
    {
        if (!AutoJump.Take()) return newlyPressed;
        ShinesparkPoseInputLocked = false;
        CrystalFlashPoseInputLocked = false;
        if (AutoJumpTimer != 0 && JumpHoldReadsAsFresh(AutoJumpTimer))
        {
            AutoJumpTimer = 0;
            newlyPressed |= GameplayAction.Jump;
        }
        return newlyPressed;
    }

    /// <summary>
    /// Whether a jump held for <paramref name="heldUpdates"/> reads as below the auto-jump limit: the game compares the
    /// hold with the limit by whether their difference is negative (<see cref="WrappingCounter.IsBehind"/>), so a jump held for
    /// half the count's range past the limit counts as pressed again, until the count continues from zero.
    /// </summary>
    private static bool JumpHoldReadsAsFresh(int heldUpdates) =>
        WrappingCounter.IsBehind(heldUpdates, SamusAutoJumpDefinitions.TimerComparisonLimit);

    /// <summary>Publishes held input without advancing Samus's draw-time owners.</summary>
    internal void PublishHeldInput(GameplayAction held) => PreviousDrawHeldInput = held;

    /// <summary>The post-draw input filter increments only when Jump is held in both draw samples.</summary>
    internal void SnapshotDrawInput(GameplayAction held, GameplayAction newlyPressed)
    {
        GameplayAction jump = GameplayAction.Jump;
        AutoJumpTimer = (held & PreviousDrawHeldInput & jump) != GameplayAction.None
            ? WrappingCounter.IncrementCount(AutoJumpTimer) : 0;
        PreviousDrawHeldInput = held;
        PressesSeenLastUpdate = newlyPressed;
    }

    /// <summary>Flashes Samus's hurt colours when she has lost health, after the input filter that follows the draw, whether or not knockback is admitted.</summary>
    internal static void UpdateHurtFlashFromHealthLoss(SamusState samus)
    {
        if (samus.Health < samus.PreviousHealthForHurtCheck && samus.HurtFlash.IsIdle)
            samus.HurtFlash.Start();
        samus.PreviousHealthForHurtCheck = samus.Health;
    }
}
