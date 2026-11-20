namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain's second-phase fight (#2208): her stretch, her thinking and her bomb, laser and hand-beam attacks. Its values are arbitrary.</summary>
public enum MotherBrainPhase2FightPhase
{
    /// <summary>Waits out the menacing shake and starts the stretching head animation and its hold.</summary>
    StretchingShakeHead,

    /// <summary>Waits out the long stretch hold, changes both neck targets, and begins the final wait.</summary>
    StretchingBringHeadUp,

    /// <summary>Completes the final wait, enables small purple breath, and enters ordinary phase-two thinking.</summary>
    StretchingFinish,

    /// <summary>Zero head health starts the rainbow sequence; otherwise standing pose, health, and the current RNG choose walking, ordinary attacks, or the red hand beam.</summary>
    Thinking,

    /// <summary>Runs the choose/cooldown/end sequence, selecting a head animation or a bomb/laser body state from Samus movement and sampled RNG.</summary>
    TryAttack,

    /// <summary>Samples an optional backward target, then advances RNG for the independent crouch-or-fire choice.</summary>
    BombDecideWalking,

    /// <summary>Waits for the stored backward target before choosing crouch or immediate bomb firing.</summary>
    BombWalkingBackwards,

    /// <summary>Requests the slow crouch and waits for its pose before starting the bomb-firing head animation.</summary>
    BombCrouch,

    /// <summary>Holds the post-fire pause, then tries to stand her up in the update it ends.</summary>
    BombFired,

    /// <summary>Asks her to stand until she stands, then returns to phase-two thinking.</summary>
    BombStandUp,

    /// <summary>Chooses neck targets from head/Samus Y ordering and starts the quick-positioning wait.</summary>
    LaserPositionHeadQuickly,

    /// <summary>When the quick positioning runs out, slows her neck, starts her head's laser animation and the recovery
    /// wait.</summary>
    LaserPositionHeadSlowlyAndFire,

    /// <summary>Restores neutral neck targets after the wait and immediately re-enters thinking without the ordinary attack sequence's end phase.</summary>
    LaserFinishAttack,

    /// <summary>Owns the ordinary red hand-beam attack sequence, separate from the health-zero rainbow cutscene.</summary>
    HandBeam,
}
