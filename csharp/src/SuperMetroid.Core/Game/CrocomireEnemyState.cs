using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Crocomire's fight-AI function, held in body variable C. Each member names the entry of the
/// 21-entry table at <c>$A4:86B3</c> it ports; the slot stores the member, not the table offset.
/// </summary>
public enum CrocomireFightFunction : ushort
{
    /// <summary>Table offset $00 selects <c>$A4:86DE FightAI_Crocomire_0_LockUp_SetInitialInstList</c>, which restarts the initial animation with timer one.</summary>
    ResetAnimation = 0,
    /// <summary>Table offset $02 selects <c>$A4:86E8 FightAI_Crocomire_2_StepForwardUntilOnScreen_StepForward</c>, installing the forward-step list and returning the fight index to sleeping.</summary>
    StepForward,
    /// <summary>Table offset $04 selects <c>$A4:86F2 FightAI_Crocomire_4_Asleep</c>, waking into the first-damage wait when Samus is within 224 horizontal pixels.</summary>
    Sleeping,
    /// <summary>Table offset $06 selects <c>$A4:8717 FightAI_Crocomire_6_SteppingForward</c>, responding to queued damage steps or selecting a charge below room X=$300.</summary>
    SteppingForward,
    /// <summary>Table offset $08 selects <c>$A4:876C FightAI_Crocomire_8_ProjectileAttack</c>, spawning shots with successive even parameters through $12 unless damage interrupts with a backward step.</summary>
    ProjectileAttack,
    /// <summary>Table offset $0A selects <c>$A4:87B2 FightAI_Crocomire_A_NearSpikeWallCharge</c>, retaining the charge list until a damage latch selects backward stepping.</summary>
    NearSpikeWallCharge,
    /// <summary>Table offset $0C selects <c>$A4:87CA FightAI_Crocomire_C_SteppingBack</c>, consuming one queued step per instruction dispatch before returning to forward stepping.</summary>
    SteppingBack,
    /// <summary>Table offset $0E selects <c>$A4:87E9 FightAI_Crocomire_E_BackOffFromSpikeWall</c>, continuing the retreat until room X reaches $300.</summary>
    BackingOffSpikeWall,
    /// <summary>Unused offset $10 selects <c>$A4:87FB UNUSED_FightAI_Crocomire_10_RoarAndStepForwards_A487FB</c>, installing a roar and selecting forward stepping.</summary>
    RoarAndStepForwardUnused,
    /// <summary>Table offset $12 selects <c>$A4:8812 FightAI_Crocomire_12_WaitForFirstDamage</c>, looping the roar until damage installs a backward step and advances to the second-damage wait.</summary>
    WaitingForFirstDamage,
    /// <summary>Table offset $14 selects <c>$A4:8836 FightAI_Crocomire_14_WaitForSecondDamage</c>, looping the roar until a second damage latch enters ordinary backward stepping.</summary>
    WaitingForSecondDamage,
    /// <summary>Unused offset $16 selects <c>$A4:885A UNUSED_FightAI_Crocomire_16_WaitForSecondDamage_A4885A</c>, a separate native copy of the offset-$14 damage wait.</summary>
    WaitingForSecondDamageUnused,
    /// <summary>Table offset $18 selects <c>$A4:887E FightAI_Crocomire_18_PowerBombedCharge</c>, decrementing the reaction step count until fewer than two remain, then restoring forward stepping.</summary>
    PowerBombCharge,
    /// <summary>Unused offset $1A selects <c>$A4:889A UNUSED_FightAI_Crocomire_1A_DoNearSpikeWallCharge_A4889A</c>, converting damage into a one-step retreat or otherwise installing the charge roar.</summary>
    NearSpikeWallChargeUnused,
    /// <summary>Unused offset $1C selects <c>$A4:88D2 FightAI_Crocomire_1C_UnusedSequence_SetInitialInstList</c>, restarting art with a 32-count setup for the unused attack branch.</summary>
    ResetAnimationUnused,
    /// <summary>Unused offset $1E selects <c>$A4:891B UNUSED_FightAI_Crocomire_1E_ChooseForwardMovingAttack_A4891B</c>, choosing a charge or move-until-claw-contact branch from fight bit $0100.</summary>
    ChooseAttackUnused,
    /// <summary>Unused offset $20 selects <c>$A4:8940 UNUSED_FightAI_Crocomire_20_DoNothingAndStepForward_A48940</c>, retaining initial art until the step count reaches zero and begins the claw sequence.</summary>
    StepForwardUnused,
    /// <summary>Unused offset $22 selects <c>$A4:895E UNUSED_FightAI_Crocomire_22_MoveForwardUntilHitSamus_A4895E</c>, branching on room X=$2A0 and claw-contact bit $4000; one native branch selects the unwritten $2A entry.</summary>
    MoveUntilSamusUnused,
    /// <summary>Unused offset $24 selects <c>$A4:89A8 UNUSED_FightAI_Crocomire_24_MoveClaws_StepForward_A489A8</c>, counting down claw cycles while resetting the tongue word and publishing fight bit $0400.</summary>
    MoveClawsUnused,
    /// <summary>Unused offset $26 selects <c>$A4:89DE UNUSED_FightAI_Crocomire_26_StepForward_A489DE</c>, adjusting attack flags and installing a forward step before the moving-claws branch.</summary>
    StepForwardVariantUnused,
    /// <summary>Unused offset $28 selects <c>$A4:89F9 UNUSED_FightAI_Crocomire_28_MovingClaws_A489F9</c>, consuming claw-contact cycles or restoring the saved fight index from the projectile word.</summary>
    MovingClawsUnused,
    /// <summary>Offset $2A, one past the table: the unused move-until-Samus branch selects it, and the cartridge would read the word after the table as a routine. The port rejects it.</summary>
    UnwrittenTableEntry,
}

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
