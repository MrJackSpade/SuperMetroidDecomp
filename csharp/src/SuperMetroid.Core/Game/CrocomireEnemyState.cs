using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Even-valued indexes consumed by Crocomire's 21-entry fight-AI table.</summary>
public enum CrocomireFightFunction : ushort
{
    /// <summary>Table offset $00 selects <c>$A4:86DE FightAI_Crocomire_0_LockUp_SetInitialInstList</c>, which restarts the initial animation with timer one.</summary>
    ResetAnimation = 0x00,
    /// <summary>Table offset $02 selects <c>$A4:86E8 FightAI_Crocomire_2_StepForwardUntilOnScreen_StepForward</c>, installing the forward-step list and returning the fight index to sleeping.</summary>
    StepForward = 0x02,
    /// <summary>Table offset $04 selects <c>$A4:86F2 FightAI_Crocomire_4_Asleep</c>, waking into the first-damage wait when Samus is within 224 horizontal pixels.</summary>
    Sleeping = 0x04,
    /// <summary>Table offset $06 selects <c>$A4:8717 FightAI_Crocomire_6_SteppingForward</c>, responding to queued damage steps or selecting a charge below room X=$300.</summary>
    SteppingForward = 0x06,
    /// <summary>Table offset $08 selects <c>$A4:876C FightAI_Crocomire_8_ProjectileAttack</c>, spawning shots with successive even parameters through $12 unless damage interrupts with a backward step.</summary>
    ProjectileAttack = 0x08,
    /// <summary>Table offset $0A selects <c>$A4:87B2 FightAI_Crocomire_A_NearSpikeWallCharge</c>, retaining the charge list until a damage latch selects backward stepping.</summary>
    NearSpikeWallCharge = 0x0a,
    /// <summary>Table offset $0C selects <c>$A4:87CA FightAI_Crocomire_C_SteppingBack</c>, consuming one queued step per instruction dispatch before returning to forward stepping.</summary>
    SteppingBack = 0x0c,
    /// <summary>Table offset $0E selects <c>$A4:87E9 FightAI_Crocomire_E_BackOffFromSpikeWall</c>, continuing the retreat until room X reaches $300.</summary>
    BackingOffSpikeWall = 0x0e,
    /// <summary>Unused offset $10 selects <c>$A4:87FB UNUSED_FightAI_Crocomire_10_RoarAndStepForwards_A487FB</c>, installing a roar and selecting forward stepping.</summary>
    RoarAndStepForwardUnused = 0x10,
    /// <summary>Table offset $12 selects <c>$A4:8812 FightAI_Crocomire_12_WaitForFirstDamage</c>, looping the roar until damage installs a backward step and advances to the second-damage wait.</summary>
    WaitingForFirstDamage = 0x12,
    /// <summary>Table offset $14 selects <c>$A4:8836 FightAI_Crocomire_14_WaitForSecondDamage</c>, looping the roar until a second damage latch enters ordinary backward stepping.</summary>
    WaitingForSecondDamage = 0x14,
    /// <summary>Unused offset $16 selects <c>$A4:885A UNUSED_FightAI_Crocomire_16_WaitForSecondDamage_A4885A</c>, a separate native copy of the offset-$14 damage wait.</summary>
    WaitingForSecondDamageUnused = 0x16,
    /// <summary>Table offset $18 selects <c>$A4:887E FightAI_Crocomire_18_PowerBombedCharge</c>, decrementing the reaction step count until fewer than two remain, then restoring forward stepping.</summary>
    PowerBombCharge = 0x18,
    /// <summary>Unused offset $1A selects <c>$A4:889A UNUSED_FightAI_Crocomire_1A_DoNearSpikeWallCharge_A4889A</c>, converting damage into a one-step retreat or otherwise installing the charge roar.</summary>
    NearSpikeWallChargeUnused = 0x1a,
    /// <summary>Unused offset $1C selects <c>$A4:88D2 FightAI_Crocomire_1C_UnusedSequence_SetInitialInstList</c>, restarting art with a 32-count setup for the unused attack branch.</summary>
    ResetAnimationUnused = 0x1c,
    /// <summary>Unused offset $1E selects <c>$A4:891B UNUSED_FightAI_Crocomire_1E_ChooseForwardMovingAttack_A4891B</c>, choosing a charge or move-until-claw-contact branch from fight bit $0100.</summary>
    ChooseAttackUnused = 0x1e,
    /// <summary>Unused offset $20 selects <c>$A4:8940 UNUSED_FightAI_Crocomire_20_DoNothingAndStepForward_A48940</c>, retaining initial art until the step count reaches zero and begins the claw sequence.</summary>
    StepForwardUnused = 0x20,
    /// <summary>Unused offset $22 selects <c>$A4:895E UNUSED_FightAI_Crocomire_22_MoveForwardUntilHitSamus_A4895E</c>, branching on room X=$2A0 and claw-contact bit $4000; one native branch selects the unwritten $2A entry.</summary>
    MoveUntilSamusUnused = 0x22,
    /// <summary>Unused offset $24 selects <c>$A4:89A8 UNUSED_FightAI_Crocomire_24_MoveClaws_StepForward_A489A8</c>, counting down claw cycles while resetting the tongue word and publishing fight bit $0400.</summary>
    MoveClawsUnused = 0x24,
    /// <summary>Unused offset $26 selects <c>$A4:89DE UNUSED_FightAI_Crocomire_26_StepForward_A489DE</c>, adjusting attack flags and installing a forward step before the moving-claws branch.</summary>
    StepForwardVariantUnused = 0x26,
    /// <summary>Unused offset $28 selects <c>$A4:89F9 UNUSED_FightAI_Crocomire_28_MovingClaws_A489F9</c>, consuming claw-contact cycles or restoring the saved fight index from the projectile word.</summary>
    MovingClawsUnused = 0x28,
}

/// <summary>
/// Named view of the six generic words in Crocomire's physical body slot. These names come
/// from the bank-$A4 disassembly; the raw flag word deliberately remains a <see cref="ushort"/>
/// because several bits are overloaded by unused authored fight states.
/// </summary>
public sealed class CrocomireEnemyState
{
    /// <summary>Projects the generic body slot's six variable words into Crocomire's combat and death roles.</summary>
    internal CrocomireEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Physical $DDBF body record owning the fight/death state words and room-pixel coordinates; BG2 body artwork follows this actor.</summary>
    public RoomEnemySlot Body { get; }
    /// <summary>Physical $DDFF tongue record, attached during initialization; bridge collapse sleeps and hides it, while later melting phases reuse its actor.</summary>
    public RoomEnemySlot? Tongue { get; internal set; }

    /// <summary>Native variable A: even byte offset into the bank-$A4 main/death dispatcher; zero runs the fight/bridge checks, two begins collapse, and $54 displays an already-defeated corpse.</summary>
    public CrocomireDeathPhase DeathSequenceIndex
    {
        get => ClosedNativeWords.Decode<CrocomireDeathPhase>(Body.VariableA, "Crocomire death-sequence index");
        internal set => Body.VariableA = (ushort)value;
    }

    /// <summary>Native variable B: raw fight latches, including damage bit $0800 and a saturating low-nibble hit count; unused attack branches assign additional overlapping bit meanings.</summary>
    public ushort FightFlags
    {
        get => Body.VariableB;
        internal set => Body.VariableB = value;
    }

    /// <summary>Native variable C: even byte offset into the 21-word table at $A4:86B3, consumed when instruction $A4:86A6 runs rather than once per main-AI frame.</summary>
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

    /// <summary>Native variable F: even projectile parameter advancing by two through $12, or saved fight index in unused claw AI; death reuses it for subposition or the whole word paired with <see cref="ReactionTimer"/>.</summary>
    public ushort ProjectileCounter
    {
        get => Body.VariableF;
        internal set => Body.VariableF = value;
    }
}
