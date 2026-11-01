namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// A PLM family's typed animation script (#1296). The slot keeps the
/// original frame-hold gate: the handler advances the program on the pass its hold expires, so each draw lasts
/// exactly as long as the authored one did.
/// </summary>
internal abstract class PlmAnimation
{
    /// <summary>
    /// Runs the program from where it rests until it draws, sleeps or deletes the PLM, as one pass of the
    /// PLM handler does.
    /// </summary>
    internal abstract void Advance(PlmSlot slot, PlmStepContext context);

    /// <summary>Continues at the program the animation linked, as a wake jumps to the link instruction.</summary>
    internal abstract void WakeAtLink();

    /// <summary>Continues past the sleep the animation rests on, as a wake steps over the sleep instruction.</summary>
    internal abstract void WakePastSleep();

    /// <summary>The program the animation is running, as its family's program enum.</summary>
    internal abstract Enum RunningProgram { get; }

    /// <summary>Names the family and its running program for diagnostics that report what a slot runs.</summary>
    public override string ToString() => $"{GetType().Name} running {RunningProgram}";
}
