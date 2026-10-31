using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The state of one Beetom. The arc's speed index, the function, both countdowns and the last controller input are
/// values the cartridge keeps in the enemy's common record, so they are emptied with that record when the enemy is
/// killed (<see cref="ClearWithRecord"/>). Its facing is inherited from the previous Beetom in its slot
/// (<see cref="SlotInheritance{TOccupant}"/>).
/// </summary>
public sealed class BeetomEnemyState
{
    /// <summary>The largest quadratic-speed record index a falling or flung Beetom accelerates to.</summary>
    internal const int MaximumYSpeedTableIndex = 64;

    /// <summary>
    /// Starts one Beetom, facing the way the previous Beetom in its slot last faced, or left for the slot's first.
    /// </summary>
    /// <remarks>
    /// Setup (<c>InitAI_Beetom</c>) never writes the facing, and the touch handler reads it to choose the drain's side
    /// before the Beetom's first action decision writes it, so a fresh Beetom touched at once drains facing its
    /// predecessor's way. Every other value setup writes itself.
    /// </remarks>
    /// <param name="previous">The slot's previous Beetom, or null for the slot's first in the room.</param>
    internal BeetomEnemyState(BeetomEnemyState? previous) => FacingRight = previous?.FacingRight ?? false;

    /// <summary>
    /// Quadratic-speed record index for the current arc, from zero to <see cref="MaximumYSpeedTableIndex"/> or the
    /// arc's larger starting index: rising lowers it by the arc's step and restarts it at zero at the apex, falling
    /// raises it up to the maximum, so it never leaves that range.
    /// </summary>
    public int YSpeedTableIndex { get; internal set; }
    /// <summary>The per-update behaviour, including separate decision and setup updates; none once a kill has emptied the record.</summary>
    public BeetomEnemyFunction? Function { get; internal set; }
    /// <summary>Idle/crawl countdown in gameplay updates; expiration tests the decrement's sign, not equality with zero.</summary>
    public WrappingCountdown FunctionTimer { get; internal set; }
    /// <summary>Remaining controller changes needed to escape attachment, restarted at 64 by the initial touch.</summary>
    public WrappingCountdown ButtonCounter { get; internal set; }
    /// <summary>Controller input last observed by initialization or the attached counter; any differing input, including releases, decrements <see cref="ButtonCounter"/>.</summary>
    public ControllerButton PreviousController1Input { get; internal set; }
    /// <summary>Where the typed animation (#641) resumes; installation also resets the common instruction timer and loop counter.</summary>
    internal AnimationProgramCursor<BeetomProgram> Program { get; set; }
    /// <summary>Short-hop starting record index, the first multiple of four whose integrated quadratic speeds reach the short hop's height.</summary>
    public int InitialShortLeapYSpeedIndex { get; internal set; }
    /// <summary>Long-hop starting record index, the first multiple of five whose integrated quadratic speeds reach the long hop's height.</summary>
    public int InitialLongLeapYSpeedIndex { get; internal set; }
    /// <summary>Lunge starting record index, the first multiple of three whose integrated quadratic speeds reach the lunge's height.</summary>
    public int InitialLungeYSpeedIndex { get; internal set; }
    /// <summary>Arc phase: false uses the rising speed-table half; passing the apex switches to the falling half at index zero.</summary>
    public bool Falling { get; internal set; }
    /// <summary>Attachment latch set on first touch and cleared after escape or the common-shot callback; it prevents repeated attachment setup.</summary>
    public bool AttachedToSamus { get; internal set; }
    /// <summary>Whether the Beetom faces right rather than left, inherited from the slot's previous Beetom; collision reverses it, while the flung state moves opposite this facing.</summary>
    public bool FacingRight { get; internal set; }

    /// <summary>
    /// One rising update of an arc: lowers the speed index by <paramref name="step"/>; once that would pass below
    /// zero the arc has reached its apex, so the index restarts at zero and the arc falls.
    /// </summary>
    /// <param name="step">The arc's rising step, three to five records.</param>
    internal void StepRisingSpeed(int step)
    {
        YSpeedTableIndex -= step;
        if (YSpeedTableIndex >= 0)
            return;
        YSpeedTableIndex = 0;
        Falling = true;
    }

    /// <summary>One falling update: raises the speed index by <paramref name="step"/>, holding at <see cref="MaximumYSpeedTableIndex"/>.</summary>
    /// <param name="step">The arc's falling step, one to five records.</param>
    internal void StepFallingSpeed(int step) =>
        YSpeedTableIndex = Math.Min(MaximumYSpeedTableIndex, YSpeedTableIndex + step);

    /// <summary>
    /// Empties the values the cartridge keeps in the enemy's common record, when a kill empties that record: no
    /// function, a zero speed index and countdowns, and no previous input.
    /// </summary>
    internal void ClearWithRecord()
    {
        YSpeedTableIndex = 0;
        Function = null;
        FunctionTimer = WrappingCountdown.Start(0);
        ButtonCounter = WrappingCountdown.Start(0);
        PreviousController1Input = ControllerButton.None;
    }
}
