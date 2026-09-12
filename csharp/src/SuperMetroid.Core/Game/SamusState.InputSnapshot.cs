namespace SuperMetroid.Core.Game;

public sealed partial class SamusState
{
    /// <summary>
    /// WRAM $0E00, joypad1_newinput_samusfilter: the Fire edge retained by $90:EAB3
    /// after drawing Samus and projectiles. Missile admission and grapple's inactive function accept either
    /// this previous-frame edge or the current NMI edge, allowing a press that first
    /// exits a non-firing pose to fire on the next frame. Demo alpha replaces it with
    /// the preceding scripted edge before running the normal HUD dispatcher.
    /// </summary>
    public ushort PreviousDrawNewInput { get; internal set; }

    /// <summary>WRAM $0AF4: consecutive held-Jump history updated by $90:EAB3.</summary>
    public ushort AutoJumpTimer { get; internal set; }

    /// <summary>Previous held input sampled by the draw-time $90:EAB3 epilogue.</summary>
    public ushort PreviousDrawHeldInput { get; internal set; }

    /// <summary>PoseInputHandler currently selects the one-shot $90:E926 auto-jump handler.</summary>
    public bool AutoJumpInputPending { get; internal set; }

    /// <summary>Local input substitution from $90:E926; does not alter the controller's real edge.</summary>
    internal ushort ConsumeAutoJumpInput(ushort newlyPressed)
    {
        if (!AutoJumpInputPending) return newlyPressed;
        AutoJumpInputPending = false;
        if (AutoJumpTimer != 0 && unchecked((short)(AutoJumpTimer - SamusAutoJumpDefinitions.TimerComparisonLimit)) < 0)
        {
            AutoJumpTimer = 0;
            newlyPressed |= (ushort)SuperMetroid.Core.Input.SnesButton.A;
        }
        return newlyPressed;
    }

    /// <summary>Native $90:EAB3 increments only when Jump is held in both draw samples.</summary>
    internal void SnapshotDrawInput(ushort held, ushort newlyPressed)
    {
        ushort jump = (ushort)SuperMetroid.Core.Input.SnesButton.A;
        AutoJumpTimer = (held & PreviousDrawHeldInput & jump) != 0
            ? unchecked((ushort)(AutoJumpTimer + 1)) : (ushort)0;
        PreviousDrawHeldInput = held;
        PreviousDrawNewInput = newlyPressed;
    }
}
