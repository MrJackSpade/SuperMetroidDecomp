using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One Kraid part's own state beside its enemy slot: the body, arm, three lints, foot and two fingernails each have
/// one, in that slot order. The values the part's record keeps are emptied with the record when the part is killed
/// (<see cref="ClearWithRecord"/>).
/// </summary>
/// <remarks>
/// The original game keeps several of these in one record value (#1476): the body's head timer, a lint's X offset and
/// a nail's horizontal whole pixels; a lint's travel and a nail's horizontal fraction; and the function timer, the
/// body's next ceiling rock and its rising-rock timer. Each is held on its own because none is read under another
/// meaning. Each part kind is a fixed slot that reads only its own meanings, and within the body the rising-rock phases
/// load their timer before reading it, and the ceiling rocks start from the zero the function timer leaves when it
/// expires into growth.
/// </remarks>
public sealed class KraidPartState
{
    /// <summary>Where the part's typed program resumes; it replaces the native instruction pointer.</summary>
    internal AnimationProgramCursor<KraidPartProgram> Cursor { get; set; }

    /// <summary>
    /// Gets the function a part's function timer installs when it expires. The cartridge keeps it in the same word as
    /// <see cref="ThinkTimer"/>; no part reads one after writing the other (the foot writes its think timer whenever it
    /// starts a second-phase walk and its next function whenever it starts a timed phase), so each is held on its own.
    /// </summary>
    public KraidAiFunction NextFunction { get; internal set; }

    /// <summary>Gets the second-phase foot's think timer, which expires when its decrement reaches zero.</summary>
    public WrappingCountdown ThinkTimer { get; internal set; }

    /// <summary>
    /// Gets whether this fingernail's last launch was the fixed-left spawn rather than the body-side one. A nail
    /// launches body-side whenever its sibling's last launch was fixed-left.
    /// </summary>
    public bool AlternateSpawn { get; internal set; }

    /// <summary>Whether this fingernail has launched at least once (its parameter one); the cartridge sets it and nothing reads it back.</summary>
    public bool NailLaunched { get; internal set; }

    /// <summary>The bank-$A7 function the part dispatches.</summary>
    public KraidAiFunction Function { get; internal set; }

    /// <summary>The updates before the part's function timer installs <see cref="NextFunction"/>.</summary>
    public int FunctionTimer
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// The body's next growth ceiling rock, one of <see cref="KraidCeilingRocks.Count"/> rocks; equal to the count once
    /// every rock has fallen.
    /// </summary>
    public int CeilingRock
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, KraidCeilingRocks.Count, value);
    }

    /// <summary>The updates left in the body's current rising-rock phase; a rock falls at each multiple of its cadence.</summary>
    public int RisingRockTimer
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The body's head-instruction timer.</summary>
    public int HeadTimer
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>A lint's X offset from the body, as the cartridge stores it: zero or minus sixteen wrapped to 65520.</summary>
    public int LintXOffset
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The pixels a lint has travelled out of Kraid's mouth while it is produced.</summary>
    public int LintTravel
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>A fingernail's signed 16.16 velocity in pixels per update.</summary>
    public FixedVector<RoomPixels> NailVelocity { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the part's common record.</summary>
    [IntendedDuplicate("each state clears its own record values; the shared shape is only the assignment of zero")]
    internal void ClearWithRecord()
    {
        Function = KraidAiFunction.None;
        FunctionTimer = 0;
        CeilingRock = 0;
        RisingRockTimer = 0;
        HeadTimer = 0;
        LintXOffset = 0;
        LintTravel = 0;
        NailVelocity = default;
    }
}
