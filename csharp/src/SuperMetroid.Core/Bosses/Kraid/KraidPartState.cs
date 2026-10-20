using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One Kraid part's own state beside its enemy slot: the body, arm, three lints, foot and two fingernails each have
/// one, in that slot order. It keeps the part's common-record words once each, with a named view per meaning the parts
/// give them; the words are emptied with the record when the part is killed (<see cref="ClearWithRecord"/>).
/// </summary>
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
    public NativeCountdown ThinkTimer { get; internal set; }

    /// <summary>
    /// Gets whether this fingernail's last launch was the fixed-left spawn rather than the body-side one. A nail
    /// launches body-side whenever its sibling's last launch was fixed-left.
    /// </summary>
    public bool AlternateSpawn { get; internal set; }

    /// <summary>Whether this fingernail has launched at least once (its parameter one); the cartridge sets it and nothing reads it back.</summary>
    public bool NailLaunched { get; internal set; }

    /// <summary>The function word.</summary>
    private int _functionWord;

    /// <summary>The word holding a lint's travel or a fingernail's horizontal fraction.</summary>
    private int _travelOrHorizontalFractionWord;

    /// <summary>The word holding the body's head timer, a lint's X offset or a fingernail's horizontal whole pixels.</summary>
    private int _headTimerOffsetOrHorizontalWholeWord;

    /// <summary>The word holding a fingernail's vertical velocity fraction.</summary>
    private int _verticalFractionWord;

    /// <summary>The word holding a fingernail's vertical velocity whole pixels.</summary>
    private int _verticalWholeWord;

    /// <summary>The word holding the function timer, or the body's ceiling-rock offset or rising-rock timer.</summary>
    private int _timerOrRockWord;

    /// <summary>The bank-$A7 function the part dispatches, held as the cartridge's word.</summary>
    public KraidAiFunction Function
    {
        get => (KraidAiFunction)_functionWord;
        internal set => _functionWord = (ushort)value;
    }

    /// <summary>The updates before the part's function timer installs <see cref="NextFunction"/>.</summary>
    public int FunctionTimer
    {
        get => _timerOrRockWord;
        internal set => _timerOrRockWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The body's next growth ceiling rock, a byte offset into the ceiling-rock positions, two per rock.</summary>
    public int CeilingRockOffset
    {
        get => _timerOrRockWord;
        internal set => _timerOrRockWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The updates left in the body's current rising-rock phase; a rock falls at each multiple of its cadence.</summary>
    public int RisingRockTimer
    {
        get => _timerOrRockWord;
        internal set => _timerOrRockWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The body's head-instruction timer.</summary>
    public int HeadTimer
    {
        get => _headTimerOffsetOrHorizontalWholeWord;
        internal set => _headTimerOffsetOrHorizontalWholeWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>A lint's X offset from the body, as the cartridge's word: zero or minus sixteen.</summary>
    public int LintXOffset
    {
        get => _headTimerOffsetOrHorizontalWholeWord;
        internal set => _headTimerOffsetOrHorizontalWholeWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The pixels a lint has travelled out of Kraid's mouth while it is produced.</summary>
    public int LintTravel
    {
        get => _travelOrHorizontalFractionWord;
        internal set => _travelOrHorizontalFractionWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// A fingernail's signed 16.16 velocity in pixels per update. Its four words are shared with the other parts' views:
    /// the horizontal fraction with <see cref="LintTravel"/> and the horizontal whole pixels with <see cref="HeadTimer"/>.
    /// </summary>
    public FixedVector<RoomPixels> NailVelocity
    {
        get => FixedVector<RoomPixels>.FromWords(
            checked((ushort)_headTimerOffsetOrHorizontalWholeWord), checked((ushort)_travelOrHorizontalFractionWord),
            checked((ushort)_verticalWholeWord), checked((ushort)_verticalFractionWord));
        internal set
        {
            _travelOrHorizontalFractionWord = value.X.Fraction;
            _headTimerOffsetOrHorizontalWholeWord = value.X.WholeWord;
            _verticalFractionWord = value.Y.Fraction;
            _verticalWholeWord = value.Y.WholeWord;
        }
    }

    /// <summary>Empties the values the cartridge keeps in the part's common record.</summary>
    [IntendedDuplicate("each state clears its own native words; the shared shape is only the assignment of zero")]
    internal void ClearWithRecord()
    {
        _functionWord = 0;
        _travelOrHorizontalFractionWord = 0;
        _headTimerOffsetOrHorizontalWholeWord = 0;
        _verticalFractionWord = 0;
        _verticalWholeWord = 0;
        _timerOrRockWord = 0;
    }
}
