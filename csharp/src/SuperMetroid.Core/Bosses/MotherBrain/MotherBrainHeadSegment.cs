namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's brain (head) programs order, run by her dedicated brain list rather than an enemy
/// slot. The unreachable frames after the initial dummy and the unused flash and
/// unused head program are not authored.
/// </summary>
public enum MotherBrainHeadSegment
{
    /// <summary>Mother Brain head animation script: stretching, phase 2, part 0.</summary>
    StretchingPhase2,
    /// <summary>Mother Brain head animation script: stretching, phase 2, part 1.</summary>
    StretchingPhase2Loop,
    /// <summary>Mother Brain head animation script: stretching, phase 3, part 0.</summary>
    StretchingPhase3,
    /// <summary>Mother Brain head animation script: stretching, phase 3, part 1.</summary>
    StretchingPhase3Loop,
    /// <summary>Mother Brain head animation script: hyper beam recoil, part 0.</summary>
    HyperBeamRecoil,
    /// <summary>Mother Brain head animation script: hyper beam recoil, part 1.</summary>
    HyperBeamRecoilLoop,
    /// <summary>Mother Brain head animation script: initial.</summary>
    Initial,
    /// <summary>Mother Brain head animation script: decapitated, part 0.</summary>
    Decapitated,
    /// <summary>Mother Brain head animation script: decapitated, part 1.</summary>
    DecapitatedLoop,
    /// <summary>Mother Brain head animation script: dying drool, part 0.</summary>
    DyingDrool,
    /// <summary>Mother Brain head animation script: dying drool, part 1.</summary>
    DyingDroolBurst,
    /// <summary>Mother Brain head animation script: dying drool, part 2.</summary>
    DyingDroolLoop,
    /// <summary>Mother Brain head animation script: firing rainbow beam.</summary>
    FiringRainbowBeam,
    /// <summary>Mother Brain head animation script: neutral, phase 2, part 0.</summary>
    NeutralPhase2,
    /// <summary>Mother Brain head animation script: neutral, phase 2, part 1.</summary>
    NeutralPhase2Hold,
    /// <summary>Mother Brain head animation script: neutral, phase 3, part 0.</summary>
    NeutralPhase3,
    /// <summary>Mother Brain head animation script: neutral, phase 3, part 1.</summary>
    NeutralPhase3Hold,
    /// <summary>Mother Brain head animation script: corpse, part 0.</summary>
    Corpse,
    /// <summary>Mother Brain head animation script: corpse, part 1.</summary>
    CorpseLoop,
    /// <summary>Mother Brain head animation script: attacking, 4 onion rings, phase 2.</summary>
    FourOnionRingsPhase2,
    /// <summary>Mother Brain head animation script: attacking, 2 onion rings, phase 2.</summary>
    TwoOnionRingsPhase2,
    /// <summary>Mother Brain head animation script: attacking, Baby Metroid.</summary>
    AttackingBabyMetroid,
    /// <summary>Mother Brain head animation script: attacking Samus, 4 onion rings, phase 3.</summary>
    AttackingSamusFourOnionRingsPhase3,
    /// <summary>Mother Brain head animation script: attacking, 4 onion rings, phase 3.</summary>
    FourOnionRingsPhase3,
    /// <summary>Mother Brain head animation script: attacking, Bomb, phase 2.</summary>
    BombPhase2,
    /// <summary>Mother Brain head animation script: attacking, Bomb, phase 3.</summary>
    BombPhase3,
    /// <summary>Mother Brain head animation script: attacking, laser.</summary>
    Laser,
    /// <summary>Mother Brain head animation script: charging rainbow beam, part 0.</summary>
    ChargingRainbowBeam,
    /// <summary>Mother Brain head animation script: charging rainbow beam, part 1.</summary>
    ChargingRainbowBeamLoop,
}
