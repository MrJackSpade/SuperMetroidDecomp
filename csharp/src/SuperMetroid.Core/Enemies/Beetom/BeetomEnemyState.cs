using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The state of one Beetom. The arc's speed index, the function, both countdowns and the last controller input are
/// values the cartridge keeps in the enemy's common record, so they are emptied with that record when the enemy is
/// killed (<see cref="ClearWithRecord"/>); the rest live in the slot's <see cref="BeetomExtensionWords"/>, which
/// outlive the occupant until room load.
/// </summary>
public sealed class BeetomEnemyState
{
    /// <summary>The largest quadratic-speed record index a falling or flung Beetom accelerates to.</summary>
    internal const int MaximumYSpeedTableIndex = 64;

    /// <summary>The slot's Beetom extension words, kept for the room across occupants.</summary>
    private readonly BeetomExtensionWords _words;

    /// <summary>Binds one Beetom to its slot's extension words.</summary>
    internal BeetomEnemyState(BeetomExtensionWords words) => _words = words;

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
    public SnesButton PreviousController1Input { get; internal set; }
    /// <summary>Where the typed animation (#641) resumes; installation also resets the common instruction timer and loop counter.</summary>
    internal AnimationProgramCursor<BeetomProgram> Program { get; set; }
    /// <summary>Short-hop starting record index, the first multiple of four whose integrated unaligned quadratic-table words reach a height of $3000.</summary>
    public int InitialShortLeapYSpeedIndex { get => _words.InitialShortLeapYSpeedIndex; internal set => _words.InitialShortLeapYSpeedIndex = value; }
    /// <summary>Long-hop starting record index, the first multiple of five whose integrated unaligned quadratic-table words reach a height of $4000.</summary>
    public int InitialLongLeapYSpeedIndex { get => _words.InitialLongLeapYSpeedIndex; internal set => _words.InitialLongLeapYSpeedIndex = value; }
    /// <summary>Lunge starting record index, the first multiple of three whose integrated unaligned quadratic-table words reach a height of $3000.</summary>
    public int InitialLungeYSpeedIndex { get => _words.InitialLungeYSpeedIndex; internal set => _words.InitialLungeYSpeedIndex = value; }
    /// <summary>Arc phase: false uses the rising speed-table half; passing the apex switches to the falling half at index zero.</summary>
    public bool Falling { get => _words.Falling; internal set => _words.Falling = value; }
    /// <summary>Attachment latch set on first touch and cleared after escape or the common-shot callback; it prevents repeated attachment setup.</summary>
    public bool AttachedToSamus { get => _words.AttachedToSamus; internal set => _words.AttachedToSamus = value; }
    /// <summary>Whether the Beetom faces right rather than left; collision reverses it, while the flung state moves opposite this facing.</summary>
    public bool FacingRight { get => _words.FacingRight; internal set => _words.FacingRight = value; }
    /// <summary>Signed room-pixel Samus-minus-Beetom offset captured on initial attachment; retained although following snaps X directly to Samus and uses Samus Y minus four.</summary>
    public Offset16<RoomPixels> InitialAttachmentOffset { get => _words.InitialAttachmentOffset; internal set => _words.InitialAttachmentOffset = value; }

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
        PreviousController1Input = SnesButton.None;
    }
}
