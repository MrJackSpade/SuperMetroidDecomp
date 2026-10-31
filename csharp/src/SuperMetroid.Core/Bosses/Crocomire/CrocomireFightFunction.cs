namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's fight-AI function, held in body variable C. Each member names the entry of the
/// 21-entry table it ports; the slot stores the member, not the table offset.
/// </summary>
public enum CrocomireFightFunction
{
    /// <summary>Restarts the initial animation with timer one.</summary>
    ResetAnimation = 0,
    /// <summary>Installs the forward-step list and returns the fight index to sleeping.</summary>
    StepForward,
    /// <summary>Wakes into the first-damage wait when Samus is within 224 horizontal pixels.</summary>
    Sleeping,
    /// <summary>Responds to queued damage steps or selects a charge below room X=768.</summary>
    SteppingForward,
    /// <summary>Spawns shots with successive even parameters through 18 unless damage interrupts with a backward step.</summary>
    ProjectileAttack,
    /// <summary>Retains the charge list until a damage latch selects backward stepping.</summary>
    NearSpikeWallCharge,
    /// <summary>Consumes one queued step per animation dispatch before returning to forward stepping.</summary>
    SteppingBack,
    /// <summary>Continues the retreat until room X reaches 768.</summary>
    BackingOffSpikeWall,
    /// <summary>Unused: installs a roar and selects forward stepping.</summary>
    RoarAndStepForwardUnused,
    /// <summary>Loops the roar until damage installs a backward step and advances to the second-damage wait.</summary>
    WaitingForFirstDamage,
    /// <summary>Loops the roar until a second damage latch enters ordinary backward stepping.</summary>
    WaitingForSecondDamage,
    /// <summary>Unused: a separate copy of the second-damage wait.</summary>
    WaitingForSecondDamageUnused,
    /// <summary>Decrements the reaction step count until fewer than two remain, then restores forward stepping.</summary>
    PowerBombCharge,
    /// <summary>Unused: converts damage into a one-step retreat or otherwise installs the charge roar.</summary>
    NearSpikeWallChargeUnused,
    /// <summary>Unused: restarts art with a 32-count setup for the unused attack branch.</summary>
    ResetAnimationUnused,
    /// <summary>Unused: chooses a charge or move-until-claw-contact branch from the fight's charge latch.</summary>
    ChooseAttackUnused,
    /// <summary>Unused: retains initial art until the step count reaches zero and begins the claw sequence.</summary>
    StepForwardUnused,
    /// <summary>Unused: branches on room X=672 and the claw-contact latch; one branch selects the unwritten entry past the table.</summary>
    MoveUntilSamusUnused,
    /// <summary>Unused: counts down claw cycles while resetting the tongue and publishing the fight's claw latch.</summary>
    MoveClawsUnused,
    /// <summary>Unused: adjusts attack flags and installs a forward step before the moving-claws branch.</summary>
    StepForwardVariantUnused,
    /// <summary>Unused: consumes claw-contact cycles or restores the saved fight index from the projectile counter.</summary>
    MovingClawsUnused,
    /// <summary>Offset $2A, one past the table: the unused move-until-Samus branch selects it, and the original game would read the word after the table as a routine. The port rejects it.</summary>
    UnwrittenTableEntry,
}
