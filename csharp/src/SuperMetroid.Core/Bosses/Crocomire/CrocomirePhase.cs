namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The phase of Crocomire's fight: what his fight does each time his body animation's fight cue fires. Its values are
/// arbitrary; nothing reads or orders them.
/// </summary>
public enum CrocomirePhase
{
    /// <summary>
    /// The phase a defeated room's corpse holds, which the fight never enters: restarts the initial animation with timer
    /// one.
    /// </summary>
    ResetAnimation,
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
    /// <summary>Loops the roar until damage installs a backward step and advances to the second-damage wait.</summary>
    WaitingForFirstDamage,
    /// <summary>Loops the roar until a second damage latch enters ordinary backward stepping.</summary>
    WaitingForSecondDamage,
    /// <summary>Decrements the reaction step count until fewer than two remain, then restores forward stepping.</summary>
    PowerBombCharge,
}
