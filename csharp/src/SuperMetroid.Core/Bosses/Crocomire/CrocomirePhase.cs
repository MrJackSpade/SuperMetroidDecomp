namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The phase of Crocomire's fight: what his fight does each time his body animation's fight cue fires. Its values are
/// arbitrary; nothing reads or orders them.
/// </summary>
public enum CrocomirePhase
{
    /// <summary>Restarts the initial animation with timer one.</summary>
    ResetAnimation,
    /// <summary>Starts the forward-step animation and returns the fight to sleeping.</summary>
    StepForward,
    /// <summary>Wakes into the first-damage wait when Samus is within 224 horizontal pixels.</summary>
    Sleeping,
    /// <summary>Responds to queued damage steps or selects a charge below room X=768.</summary>
    SteppingForward,
    /// <summary>Spawns shots with successive even parameters through 18 unless damage interrupts with a backward step.</summary>
    ProjectileAttack,
    /// <summary>Keeps the charge animation until a damage latch selects backward stepping.</summary>
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
    /// <summary>Unused: consumes claw-contact cycles or resumes the saved fight phase.</summary>
    MovingClawsUnused,
    /// <summary>A phase the game never wrote: the unused move-until-Samus branch selects it, and the game would run whatever follows its phases as one. It is rejected.</summary>
    UnwrittenTableEntry,
}
