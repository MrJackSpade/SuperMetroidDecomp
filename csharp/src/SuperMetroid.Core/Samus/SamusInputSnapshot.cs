using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus;

/// <summary>The input Samus's handlers see: the filtered held and new input of the previous draw, the auto-jump timer and pending jump, and the pose input locks for Shinespark and Crystal Flash.</summary>
internal sealed class SamusInputSnapshot
{
    /// <summary>
    /// Live WRAM $0A68: ordinary stored-shine countdown or Crystal Flash's palette
    /// timer. A grab-interrupted Flash keeps the latter ticking indefinitely, so
    /// native jump admission still sees a nonzero shine without a Stored phase.
    /// </summary>
    public static int SharedShineTimer(SamusState samus) => samus.CrystalFlash.SpecialPaletteKind == SamusSpecialPaletteType.CrystalFlash
        ? samus.CrystalFlash.SpecialPaletteTimer.RawValue : samus.Shinespark.ShineTimer.RawValue;

    /// <summary>
    /// WRAM $0E00, joypad1_newinput_samusfilter: the Fire edge retained by $90:EAB3
    /// after drawing Samus and projectiles. Missile admission and grapple's inactive function accept either
    /// this previous-frame edge or the current NMI edge, allowing a press that first
    /// exits a non-firing pose to fire on the next frame. Demo alpha replaces it with
    /// the preceding scripted edge before running the normal HUD dispatcher.
    /// </summary>
    public ControllerButton PreviousDrawNewInput { get; internal set; }

    /// <summary>WRAM $0AF4, 0 through 65535: consecutive held-Jump history updated by $90:EAB3.</summary>
    public int AutoJumpTimer
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>WRAM $0DFE: held input published by $90:EAB3 and main-loop tail $82:8AB0.</summary>
    public ControllerButton PreviousDrawHeldInput { get; internal set; }

    /// <summary>PoseInputHandler currently selects the one-shot $90:E926 auto-jump handler.</summary>
    public bool AutoJumpInputPending { get; internal set; }

    /// <summary>
    /// Shinespark launch installed the RTS pose-input handler ($91:FACA). This
    /// survives an independently replaced movement handler until input is restored,
    /// for example by normal landing's one-shot auto-jump handler ($91:F1EC).
    /// </summary>
    public bool ShinesparkPoseInputLocked { get; internal set; }

    /// <summary>Crystal Flash's RTS input pointer survives grab/release until native input restoration.</summary>
    public bool CrystalFlashPoseInputLocked { get; internal set; }

    /// <summary>Local input substitution from $90:E926; does not alter the controller's real edge.</summary>
    internal ControllerButton ConsumeAutoJumpInput(ControllerButton newlyPressed)
    {
        if (!AutoJumpInputPending) return newlyPressed;
        ShinesparkPoseInputLocked = false;
        CrystalFlashPoseInputLocked = false;
        AutoJumpInputPending = false;
        if (AutoJumpTimer != 0 && unchecked((short)(AutoJumpTimer - SamusAutoJumpDefinitions.TimerComparisonLimit)) < 0)
        {
            AutoJumpTimer = 0;
            newlyPressed |= ControllerButton.A;
        }
        return newlyPressed;
    }

    /// <summary>$82:8AB0 publishes held input without advancing Samus's draw-time owners.</summary>
    internal void PublishMainLoopHeldInput(ControllerButton held) => PreviousDrawHeldInput = held;

    /// <summary>Native $90:EAB3 increments only when Jump is held in both draw samples.</summary>
    internal void SnapshotDrawInput(ControllerButton held, ControllerButton newlyPressed)
    {
        ControllerButton jump = ControllerButton.A;
        AutoJumpTimer = (held & PreviousDrawHeldInput & jump) != ControllerButton.None
            ? unchecked((ushort)(AutoJumpTimer + 1)) : 0;
        PreviousDrawHeldInput = held;
        PreviousDrawNewInput = newlyPressed;
    }

    /// <summary>Health-feedback tail of $90:EAB3, independent of knockback admission.</summary>
    internal static void UpdateHurtFlashFromHealthLoss(SamusState samus)
    {
        if (samus.Health < samus.PreviousHealthForHurtCheck && samus.HurtFlashCounter == 0)
            samus.HurtFlashCounter = 1;
        samus.PreviousHealthForHurtCheck = samus.Health;
    }
}
