namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's brain (head) animations order, run by her dedicated brain list rather than an enemy
/// slot. The unreachable frames after the initial dummy and the unused flash and
/// unused head animation are not authored.
/// </summary>
public enum MotherBrainHeadSegment
{
    /// <summary>Mother Brain head animation: stretching, phase 2, part 0.</summary>
    StretchingPhase2,
    /// <summary>Mother Brain head animation: stretching, phase 2, part 1.</summary>
    StretchingPhase2Loop,
    /// <summary>Mother Brain head animation: stretching, phase 3, part 0.</summary>
    StretchingPhase3,
    /// <summary>Mother Brain head animation: stretching, phase 3, part 1.</summary>
    StretchingPhase3Loop,
    /// <summary>Mother Brain head animation: hyper beam recoil, part 0.</summary>
    HyperBeamRecoil,
    /// <summary>Mother Brain head animation: hyper beam recoil, part 1.</summary>
    HyperBeamRecoilLoop,
    /// <summary>Mother Brain head animation: initial.</summary>
    Initial,
    /// <summary>Mother Brain head animation: decapitated, part 0.</summary>
    Decapitated,
    /// <summary>Mother Brain head animation: decapitated, part 1.</summary>
    DecapitatedLoop,
    /// <summary>Mother Brain head animation: dying drool, part 0.</summary>
    DyingDrool,
    /// <summary>Mother Brain head animation: dying drool, part 1.</summary>
    DyingDroolBurst,
    /// <summary>Mother Brain head animation: dying drool, part 2.</summary>
    DyingDroolLoop,
    /// <summary>Mother Brain head animation: firing rainbow beam.</summary>
    FiringRainbowBeam,
    /// <summary>Mother Brain head animation: neutral, phase 2, part 0.</summary>
    NeutralPhase2,
    /// <summary>Mother Brain head animation: neutral, phase 2, part 1.</summary>
    NeutralPhase2Hold,
    /// <summary>Mother Brain head animation: neutral, phase 3, part 0.</summary>
    NeutralPhase3,
    /// <summary>Mother Brain head animation: neutral, phase 3, part 1.</summary>
    NeutralPhase3Hold,
    /// <summary>Mother Brain head animation: corpse, part 0.</summary>
    Corpse,
    /// <summary>Mother Brain head animation: corpse, part 1.</summary>
    CorpseLoop,
    /// <summary>Mother Brain head animation: attacking, 4 onion rings, phase 2.</summary>
    FourOnionRingsPhase2,
    /// <summary>Mother Brain head animation: attacking, 2 onion rings, phase 2.</summary>
    TwoOnionRingsPhase2,
    /// <summary>Mother Brain head animation: attacking, Baby Metroid.</summary>
    AttackingBabyMetroid,
    /// <summary>Mother Brain head animation: attacking Samus, 4 onion rings, phase 3.</summary>
    AttackingSamusFourOnionRingsPhase3,
    /// <summary>Mother Brain head animation: attacking, 4 onion rings, phase 3.</summary>
    FourOnionRingsPhase3,
    /// <summary>Mother Brain head animation: attacking, Bomb, phase 2.</summary>
    BombPhase2,
    /// <summary>Mother Brain head animation: attacking, Bomb, phase 3.</summary>
    BombPhase3,
    /// <summary>Mother Brain head animation: attacking, laser.</summary>
    Laser,
    /// <summary>Mother Brain head animation: charging rainbow beam, part 0.</summary>
    ChargingRainbowBeam,
    /// <summary>Mother Brain head animation: charging rainbow beam, part 1.</summary>
    ChargingRainbowBeamLoop,
}
