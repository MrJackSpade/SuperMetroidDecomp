using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Named view of the six generic words in Crocomire's physical body slot. These names come
/// from the bank-$A4 disassembly.
/// </summary>
public sealed class CrocomireEnemyState
{
    /// <summary>Projects the generic body slot's six variable words into Crocomire's combat and death roles.</summary>
    internal CrocomireEnemyState(RoomEnemySlot body) => Body = body;

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

    /// <summary>Native variable A: the main/death dispatcher phase; <see cref="CrocomireDeathPhase.Fighting"/> runs the fight and bridge checks, and <see cref="CrocomireDeathPhase.DefeatedRoomAdvance"/> displays an already-defeated corpse.</summary>
    public CrocomireDeathPhase DeathSequenceIndex
    {
        get => ClosedNativeWords.Decode<CrocomireDeathPhase>(Body.VariableA, "Crocomire death-sequence index");
        internal set => Body.VariableA = (ushort)value;
    }

    /// <summary>Native variable B: the fight latches and the saturating low-nibble hit count.</summary>
    public CrocomireFightFlags FightFlags
    {
        get => (CrocomireFightFlags)Body.VariableB;
        internal set => Body.VariableB = value.ToNativeWord();
    }

    /// <summary>Native variable C: the fight-AI function, run when the body program's fight-AI cue fires rather than once per main-AI frame.</summary>
    public CrocomireFightFunction FightFunction
    {
        get => (CrocomireFightFunction)Body.VariableC;
        internal set => Body.VariableC = (ushort)value;
    }

    /// <summary>
    /// Native variable D during combat: queued backward steps (and the unused attacks' move counts). Producers store
    /// an authored count of at most thirty-two or add at most three per mouth hit while stepping back consumes them,
    /// so the count stays far inside <see cref="MaximumStepCount"/>. A store outside that range is rejected rather
    /// than wrapped, because the power-bomb charge's signed comparison is exact only inside it.
    /// </summary>
    public int StepCounter
    {
        get => Body.VariableD;
        internal set => Body.VariableD = checked((ushort)Ensure.BetweenInclusive(0, MaximumStepCount, value));
    }

    /// <summary>The largest queued step count the fight AI's signed comparisons read as the cartridge does.</summary>
    private const int MaximumStepCount = short.MaxValue;

    /// <summary>
    /// Native variable D in the submerged pause and the skeleton tile upload: the frames left in
    /// that death phase, a saturating countdown (<c>LDA</c>, <c>BEQ</c>, <c>DEC</c>).
    /// </summary>
    public NativeCountdown DeathPhaseDelay
    {
        get => NativeCountdown.FromRaw(Body.VariableD);
        internal set => Body.VariableD = value.RawValue;
    }

    /// <summary>
    /// Native variable D while the hidden wall rumbles: the cursor of the current target in the authored rumble
    /// schedule, between zero and the schedule's terminator cursor, stepping by the schedule's two-byte targets.
    /// </summary>
    public int RumbleTableOffset
    {
        get => Body.VariableD;
        internal set => Body.VariableD = checked((ushort)value);
    }

    /// <summary>
    /// Native variable D while the body sinks or rises: the vertical acceleration in 1/256 pixel per frame per
    /// frame. The sink clamps its whole part at forty-eight and the rise at thirty-one, so it never leaves the word.
    /// </summary>
    public int VerticalAcceleration
    {
        get => Body.VariableD;
        internal set => Body.VariableD = checked((ushort)value);
    }

    /// <summary>
    /// Native variable E during combat: the shot and power-bomb reaction word, cleared at wake-up and set to ten
    /// by every reaction. No shipped routine reads it back.
    /// </summary>
    public int ReactionTimer
    {
        get => Body.VariableE;
        internal set => Body.VariableE = checked((ushort)value);
    }

    /// <summary>
    /// Native variable E while the body sinks or rises: the vertical speed in 1/256 pixel per frame. The sink
    /// clamps its whole part at three and the rise stops it at zero, so it stays below four pixels per frame.
    /// </summary>
    public int VerticalSpeed
    {
        get => Body.VariableE;
        internal set => Body.VariableE = checked((ushort)value);
    }

    /// <summary>
    /// Native variable F during the projectile attack: the even spawn parameter, cleared when the attack starts
    /// and advanced by two through eighteen.
    /// </summary>
    public int ProjectileCounter
    {
        get => Body.VariableF;
        internal set => Body.VariableF = checked((ushort)value);
    }

    /// <summary>Native variable F in the unused claw attack: the fight function restored when the claws stop.</summary>
    internal CrocomireFightFunction SavedFightFunction
    {
        get => (CrocomireFightFunction)Body.VariableF;
        set => Body.VariableF = (ushort)value;
    }

    /// <summary>
    /// Native variable F while the body sinks or rises: the 1/256-pixel vertical position carried between frames.
    /// The cartridge keeps it in the word's high byte and leaves the low byte as the sequence cleared it.
    /// </summary>
    public int VerticalSubpixel
    {
        get => SnesLittleEndian.HighByte(Body.VariableF);
        internal set => Body.VariableF = SnesLittleEndian.Word(SnesLittleEndian.LowByte(Body.VariableF), checked((byte)value));
    }

    /// <summary>
    /// Native variables E and F while the skeleton breaks out and collapses: its 16.16 horizontal speed in room
    /// pixels per frame, the fraction in E and the whole pixels in F.
    /// </summary>
    internal SnesSignedSixteenSixteen SkeletonVelocity
    {
        get => SnesSignedSixteenSixteen.FromParts(unchecked((short)Body.VariableF), Body.VariableE);
        set
        {
            Body.VariableE = value.Fraction;
            Body.VariableF = value.WholeWord;
        }
    }

    /// <summary>
    /// Starts a sink or rise from rest: clears the vertical speed and the whole subpixel word, then sets the
    /// starting <paramref name="acceleration"/> in 1/256 pixel per frame per frame.
    /// </summary>
    internal void StartVerticalMotion(int acceleration)
    {
        Body.VariableE = 0;
        Body.VariableF = 0;
        VerticalAcceleration = acceleration;
    }
}
