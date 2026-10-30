using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's body state. These names come from the bank-$A4 disassembly. The cartridge reuses the last three
/// common-record words between phases, and a phase can read what the previous one left in them, so the state keeps each
/// record word once and every phase's named view reads and writes it. The words are emptied with the record when the
/// body is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class CrocomireEnemyState
{
    /// <summary>Creates the state for Crocomire's body slot.</summary>
    internal CrocomireEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>The saturating count of mouth hits (<see cref="HitCount"/>).</summary>
    private int _hitCount;

    /// <summary>The word holding the queued steps, a death-phase delay, the rumble cursor or the vertical acceleration.</summary>
    private int _stepsDelayRumbleOrAccelerationWord;

    /// <summary>The word holding the reaction timer, the vertical speed or the skeleton speed's fraction.</summary>
    private int _reactionSpeedOrSkeletonFractionWord;

    /// <summary>
    /// The word holding the projectile counter, the saved fight function, the vertical subpixel or the skeleton speed's
    /// whole pixels.
    /// </summary>
    private int _projectileSubpixelOrSkeletonWholeWord;

    /// <summary>Physical $DDBF body record owning the fight/death state words and room-pixel coordinates; his body layer follows this actor.</summary>
    public RoomEnemySlot Body { get; }
    /// <summary>Where the body's typed program resumes; it replaces the native instruction pointer.</summary>
    internal AnimationProgramCursor<CrocomireBodyProgram> BodyCursor { get; set; }

    /// <summary>The body frame shown last, or null before the first frame.</summary>
    internal CrocomireBodyFrame? BodyFrame { get; set; }

    /// <summary>
    /// The program the body's cursor belongs to. A cursor resting on a fall-through already belongs to the next
    /// program, as the native instruction pointer then points into it.
    /// </summary>
    internal CrocomireBodyProgram BodyResidentProgram => CrocomireBodyAnimations.Programs.ResidentProgram(BodyCursor);

    /// <summary>The camera position the last main update saw; its X places his body layer and decides whether the body is on screen.</summary>
    internal Position16<RoomPixels> Camera { get; set; }

    /// <summary>The tongue's animation program.</summary>
    internal CrocomireTongueProgram TongueProgram { get; set; }

    /// <summary>The next step of the tongue's program.</summary>
    internal int TonguePosition { get; set; }

    /// <summary>Physical $DDFF tongue record, attached during initialization; bridge collapse sleeps and hides it, while later melting phases reuse its actor.</summary>
    public RoomEnemySlot? Tongue { get; internal set; }

    /// <summary>The main/death dispatcher phase; <see cref="CrocomireDeathPhase.Fighting"/> runs the fight and bridge checks, and <see cref="CrocomireDeathPhase.DefeatedRoomAdvance"/> displays an already-defeated corpse.</summary>
    public CrocomireDeathPhase DeathSequenceIndex { get; internal set; }

    /// <summary>The fight latches. The cartridge keeps them in one word with <see cref="HitCount"/> in its low four bits; nothing reads that word another way.</summary>
    public CrocomireFightFlags FightFlags { get; internal set; }

    /// <summary>The saturating count of mouth hits, 0..<see cref="CrocomireMouthHits.MaximumCount"/>.</summary>
    public int HitCount
    {
        get => _hitCount;
        internal set => _hitCount = Ensure.BetweenInclusive(0, CrocomireMouthHits.MaximumCount, value);
    }

    /// <summary>The fight-AI function, run when the body program's fight-AI cue fires rather than once per main-AI frame.</summary>
    public CrocomireFightFunction FightFunction { get; internal set; }

    /// <summary>
    /// During combat: queued backward steps (and the unused attacks' move counts). Producers store an authored count of
    /// at most thirty-two or add at most three per mouth hit while stepping back consumes them, so the count stays far
    /// inside <see cref="MaximumStepCount"/>. A store outside that range is rejected rather than wrapped, because the
    /// power-bomb charge's signed comparison is exact only inside it.
    /// </summary>
    public int StepCounter
    {
        get => checked((ushort)_stepsDelayRumbleOrAccelerationWord);
        internal set => _stepsDelayRumbleOrAccelerationWord = checked((ushort)Ensure.BetweenInclusive(0, MaximumStepCount, value));
    }

    /// <summary>The largest queued step count the fight AI's signed comparisons read as the cartridge does.</summary>
    private const int MaximumStepCount = short.MaxValue;

    /// <summary>
    /// In the submerged pause and the skeleton tile upload: the frames left in that death phase, a saturating countdown
    /// (<c>LDA</c>, <c>BEQ</c>, <c>DEC</c>).
    /// </summary>
    public WrappingCountdown DeathPhaseDelay
    {
        get => WrappingCountdown.FromRaw(checked((ushort)_stepsDelayRumbleOrAccelerationWord));
        internal set => _stepsDelayRumbleOrAccelerationWord = value.RawValue;
    }

    /// <summary>
    /// While the hidden wall rumbles: the cursor of the current target in the authored rumble schedule, between zero
    /// and the schedule's terminator cursor, stepping by the schedule's two-byte targets.
    /// </summary>
    public int RumbleTableOffset
    {
        get => checked((ushort)_stepsDelayRumbleOrAccelerationWord);
        internal set => _stepsDelayRumbleOrAccelerationWord = checked((ushort)value);
    }

    /// <summary>
    /// While the body sinks or rises: the vertical acceleration in 1/256 pixel per frame per frame. The sink clamps its
    /// whole part at forty-eight and the rise at thirty-one, so it never leaves the word.
    /// </summary>
    public int VerticalAcceleration
    {
        get => checked((ushort)_stepsDelayRumbleOrAccelerationWord);
        internal set => _stepsDelayRumbleOrAccelerationWord = checked((ushort)value);
    }

    /// <summary>
    /// During combat: the shot and power-bomb reaction word, cleared at wake-up and set to ten by every reaction. No
    /// shipped routine reads it back.
    /// </summary>
    public int ReactionTimer
    {
        get => checked((ushort)_reactionSpeedOrSkeletonFractionWord);
        internal set => _reactionSpeedOrSkeletonFractionWord = checked((ushort)value);
    }

    /// <summary>
    /// While the body sinks or rises: the vertical speed in 1/256 pixel per frame. The sink clamps its whole part at
    /// three and the rise stops it at zero, so it stays below four pixels per frame.
    /// </summary>
    public int VerticalSpeed
    {
        get => checked((ushort)_reactionSpeedOrSkeletonFractionWord);
        internal set => _reactionSpeedOrSkeletonFractionWord = checked((ushort)value);
    }

    /// <summary>
    /// During the projectile attack: the even spawn parameter, cleared when the attack starts and advanced by two
    /// through eighteen.
    /// </summary>
    public int ProjectileCounter
    {
        get => checked((ushort)_projectileSubpixelOrSkeletonWholeWord);
        internal set => _projectileSubpixelOrSkeletonWholeWord = checked((ushort)value);
    }

    /// <summary>
    /// In the unused claw attack: the fight function restored when the claws stop. The cartridge parks it in the
    /// projectile counter's word; only that unused attack writes or reads it, so the retail fight never observes the
    /// overlap and it is kept apart.
    /// </summary>
    internal CrocomireFightFunction SavedFightFunction { get; set; }

    /// <summary>
    /// The bits of the shared word below <see cref="VerticalSubpixel"/>, which the sink sequence clears and then leaves
    /// alone. The subpixel stays in that word because the projectile counter and the skeleton's whole position use it
    /// too, at other times.
    /// </summary>
    private const int SinkClearedBits = 8;

    /// <summary>
    /// While the body sinks or rises: the 1/256-pixel vertical position carried between frames. The cartridge keeps it
    /// in the word's high byte and leaves the low byte as the sequence cleared it.
    /// </summary>
    public int VerticalSubpixel
    {
        get => unchecked((byte)(checked((ushort)_projectileSubpixelOrSkeletonWholeWord) >> SinkClearedBits));
        internal set => _projectileSubpixelOrSkeletonWholeWord =
            unchecked((byte)checked((ushort)_projectileSubpixelOrSkeletonWholeWord)) | checked((byte)value) << SinkClearedBits;
    }

    /// <summary>
    /// While the skeleton breaks out and collapses: its 16.16 horizontal speed in room pixels per frame.
    /// </summary>
    internal SignedSixteenSixteen SkeletonVelocity
    {
        get => SignedSixteenSixteen.FromParts(unchecked((short)checked((ushort)_projectileSubpixelOrSkeletonWholeWord)), checked((ushort)_reactionSpeedOrSkeletonFractionWord));
        set
        {
            _reactionSpeedOrSkeletonFractionWord = value.Fraction;
            _projectileSubpixelOrSkeletonWholeWord = value.WholeWord;
        }
    }

    /// <summary>
    /// Starts a sink or rise from rest: clears the vertical speed and the whole subpixel word, then sets the starting
    /// <paramref name="acceleration"/> in 1/256 pixel per frame per frame.
    /// </summary>
    internal void StartVerticalMotion(int acceleration)
    {
        _reactionSpeedOrSkeletonFractionWord = 0;
        _projectileSubpixelOrSkeletonWholeWord = 0;
        VerticalAcceleration = acceleration;
    }

    /// <summary>Empties the values the cartridge keeps in the body's common record.</summary>
    [IntendedDuplicate("each state clears its own native words; the shared shape is only the assignment of zero")]
    internal void ClearWithRecord()
    {
        DeathSequenceIndex = CrocomireDeathPhase.Fighting;
        FightFlags = CrocomireFightFlags.None;
        _hitCount = 0;
        FightFunction = CrocomireFightFunction.ResetAnimation;
        SavedFightFunction = CrocomireFightFunction.ResetAnimation;
        _stepsDelayRumbleOrAccelerationWord = 0;
        _reactionSpeedOrSkeletonFractionWord = 0;
        _projectileSubpixelOrSkeletonWholeWord = 0;
    }
}
