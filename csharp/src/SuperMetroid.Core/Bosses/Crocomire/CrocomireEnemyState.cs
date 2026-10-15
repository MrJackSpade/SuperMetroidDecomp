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
        internal set => Body.VariableB = (ushort)value;
    }

    /// <summary>Native variable C: the fight-AI function, run when the body program's fight-AI cue fires rather than once per main-AI frame.</summary>
    public CrocomireFightFunction FightFunction
    {
        get => (CrocomireFightFunction)Body.VariableC;
        internal set => Body.VariableC = (ushort)value;
    }

    /// <summary>Native variable D during combat: queued backward steps (and the unused attacks' move counts).</summary>
    public ushort StepCounter
    {
        get => Body.VariableD;
        internal set => Body.VariableD = value;
    }

    /// <summary>
    /// Native variable D in the submerged pause and the skeleton tile upload: the frames left in
    /// that death phase, a saturating countdown (<c>LDA</c>, <c>BEQ</c>, <c>DEC</c>).
    /// </summary>
    public NativeCountdown DeathPhaseDelay
    {
        get => NativeCountdown.FromRaw(Body.VariableD);
        internal set => Body.VariableD = value.RawValue;
    }

    /// <summary>Native variable D while the hidden wall rumbles: the byte offset of the current rumble-table record.</summary>
    public ushort RumbleTableOffset
    {
        get => Body.VariableD;
        internal set => Body.VariableD = value;
    }

    /// <summary>Native variable D while the body sinks or the skeleton rises: the 8.8 vertical acceleration word.</summary>
    public ushort VerticalAcceleration
    {
        get => Body.VariableD;
        internal set => Body.VariableD = value;
    }

    /// <summary>Native variable E: shot/power-bomb reaction word initialized to ten; death reuses it for 8.8 vertical speed or the fractional low word of skeleton 16.16 motion.</summary>
    public ushort ReactionTimer
    {
        get => Body.VariableE;
        internal set => Body.VariableE = value;
    }

    /// <summary>Native variable F: even projectile parameter advancing by two through 18, or the saved fight function in unused claw AI; death reuses it for subposition or the whole word paired with <see cref="ReactionTimer"/>.</summary>
    public ushort ProjectileCounter
    {
        get => Body.VariableF;
        internal set => Body.VariableF = value;
    }
}
