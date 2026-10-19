using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
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

    /// <summary>The death-sequence phase word.</summary>
    private int _deathPhaseWord;

    /// <summary>The fight-flags word.</summary>
    private int _fightFlagsWord;

    /// <summary>The fight-function word.</summary>
    private int _fightFunctionWord;

    /// <summary>The word holding the queued steps, a death-phase delay, the rumble cursor or the vertical acceleration.</summary>
    private int _stepsDelayRumbleOrAccelerationWord;

    /// <summary>The word holding the reaction timer, the vertical speed or the skeleton speed's fraction.</summary>
    private int _reactionSpeedOrSkeletonFractionWord;

    /// <summary>
    /// The word holding the projectile counter, the saved fight function, the vertical subpixel or the skeleton speed's
    /// whole pixels.
    /// </summary>
    private int _projectileSubpixelOrSkeletonWholeWord;

    /// <summary>Physical $DDBF body record owning the fight/death state words and room-pixel coordinates; BG2 body artwork follows this actor.</summary>
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

    /// <summary>The tongue's animation program.</summary>
    internal CrocomireTongueProgram TongueProgram { get; set; }

    /// <summary>The next step of the tongue's program.</summary>
    internal int TonguePosition { get; set; }

    /// <summary>Physical $DDFF tongue record, attached during initialization; bridge collapse sleeps and hides it, while later melting phases reuse its actor.</summary>
    public RoomEnemySlot? Tongue { get; internal set; }

    /// <summary>The main/death dispatcher phase; <see cref="CrocomireDeathPhase.Fighting"/> runs the fight and bridge checks, and <see cref="CrocomireDeathPhase.DefeatedRoomAdvance"/> displays an already-defeated corpse.</summary>
    public CrocomireDeathPhase DeathSequenceIndex
    {
        get => ClosedNativeWords.Decode<CrocomireDeathPhase>(checked((ushort)_deathPhaseWord), "Crocomire death-sequence index");
        internal set => _deathPhaseWord = (ushort)value;
    }

    /// <summary>The fight latches and the saturating low-nibble hit count.</summary>
    public CrocomireFightFlags FightFlags
    {
        get => (CrocomireFightFlags)checked((ushort)_fightFlagsWord);
        internal set => _fightFlagsWord = value.ToNativeWord();
    }

    /// <summary>The fight-AI function, run when the body program's fight-AI cue fires rather than once per main-AI frame.</summary>
    public CrocomireFightFunction FightFunction
    {
        get => (CrocomireFightFunction)checked((ushort)_fightFunctionWord);
        internal set => _fightFunctionWord = (ushort)value;
    }

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
    public NativeCountdown DeathPhaseDelay
    {
        get => NativeCountdown.FromRaw(checked((ushort)_stepsDelayRumbleOrAccelerationWord));
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

    /// <summary>In the unused claw attack: the fight function restored when the claws stop.</summary>
    internal CrocomireFightFunction SavedFightFunction
    {
        get => (CrocomireFightFunction)checked((ushort)_projectileSubpixelOrSkeletonWholeWord);
        set => _projectileSubpixelOrSkeletonWholeWord = (ushort)value;
    }

    /// <summary>
    /// While the body sinks or rises: the 1/256-pixel vertical position carried between frames. The cartridge keeps it
    /// in the word's high byte and leaves the low byte as the sequence cleared it.
    /// </summary>
    public int VerticalSubpixel
    {
        get => SnesLittleEndian.HighByte(checked((ushort)_projectileSubpixelOrSkeletonWholeWord));
        internal set => _projectileSubpixelOrSkeletonWholeWord = SnesLittleEndian.Word(SnesLittleEndian.LowByte(checked((ushort)_projectileSubpixelOrSkeletonWholeWord)), checked((byte)value));
    }

    /// <summary>
    /// While the skeleton breaks out and collapses: its 16.16 horizontal speed in room pixels per frame.
    /// </summary>
    internal SnesSignedSixteenSixteen SkeletonVelocity
    {
        get => SnesSignedSixteenSixteen.FromParts(unchecked((short)checked((ushort)_projectileSubpixelOrSkeletonWholeWord)), checked((ushort)_reactionSpeedOrSkeletonFractionWord));
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
    internal void ClearWithRecord()
    {
        _deathPhaseWord = 0;
        _fightFlagsWord = 0;
        _fightFunctionWord = 0;
        _stepsDelayRumbleOrAccelerationWord = 0;
        _reactionSpeedOrSkeletonFractionWord = 0;
        _projectileSubpixelOrSkeletonWholeWord = 0;
    }
}
