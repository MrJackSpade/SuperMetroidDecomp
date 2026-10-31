namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's brain (head) programs order, run by her dedicated brain list rather than an enemy
/// slot. The unreachable frames after the initial dummy and the unused flash and
/// <c>$A9:9CE7</c> are not authored.
/// </summary>
public enum MotherBrainHeadProgram
{
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase2_0</c>.</summary>
    StretchingPhase2,
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase2_1</c>.</summary>
    StretchingPhase2Loop,
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase3_0</c>.</summary>
    StretchingPhase3,
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase3_1</c>.</summary>
    StretchingPhase3Loop,
    /// <summary><c>InstList_MotherBrainHead_HyperBeamRecoil_0</c>.</summary>
    HyperBeamRecoil,
    /// <summary><c>InstList_MotherBrainHead_HyperBeamRecoil_1</c>.</summary>
    HyperBeamRecoilLoop,
    /// <summary><c>InstList_MotherBrainHead_Initial</c>.</summary>
    Initial,
    /// <summary><c>InstList_MotherBrainHead_Decapitated_0</c>.</summary>
    Decapitated,
    /// <summary><c>InstList_MotherBrainHead_Decapitated_1</c>.</summary>
    DecapitatedLoop,
    /// <summary><c>InstList_MotherBrainHead_DyingDrool_0</c>.</summary>
    DyingDrool,
    /// <summary><c>InstList_MotherBrainHead_DyingDrool_1</c>.</summary>
    DyingDroolBurst,
    /// <summary><c>InstList_MotherBrainHead_DyingDrool_2</c>.</summary>
    DyingDroolLoop,
    /// <summary><c>InstList_MotherBrainHead_FiringRainbowBeam</c>.</summary>
    FiringRainbowBeam,
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase2_0</c>.</summary>
    NeutralPhase2,
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase2_1</c>.</summary>
    NeutralPhase2Hold,
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase3_0</c>.</summary>
    NeutralPhase3,
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase3_1</c>.</summary>
    NeutralPhase3Hold,
    /// <summary><c>InstList_MotherBrainHead_Corpse_0</c>.</summary>
    Corpse,
    /// <summary><c>InstList_MotherBrainHead_Corpse_1</c>.</summary>
    CorpseLoop,
    /// <summary><c>InstList_MotherBrainHead_Attacking_4OnionRings_Phase2</c>.</summary>
    FourOnionRingsPhase2,
    /// <summary><c>InstList_MotherBrainHead_Attacking_2OnionRings_Phase2</c>.</summary>
    TwoOnionRingsPhase2,
    /// <summary><c>InstList_MotherBrainHead_Attacking_BabyMetroid</c>.</summary>
    AttackingBabyMetroid,
    /// <summary><c>InstList_MotherBrainHead_AttackingSamus_4OnionRings_Phase3</c>.</summary>
    AttackingSamusFourOnionRingsPhase3,
    /// <summary><c>InstList_MotherBrainHead_Attacking_4OnionRings_Phase3</c>.</summary>
    FourOnionRingsPhase3,
    /// <summary><c>InstList_MotherBrainHead_Attacking_Bomb_Phase2</c>.</summary>
    BombPhase2,
    /// <summary><c>InstList_MotherBrainHead_Attacking_Bomb_Phase3</c>.</summary>
    BombPhase3,
    /// <summary><c>InstList_MotherBrainHead_Attacking_Laser</c>.</summary>
    Laser,
    /// <summary><c>InstList_MotherBrainHead_ChargingRainbowBeam_0</c>.</summary>
    ChargingRainbowBeam,
    /// <summary><c>InstList_MotherBrainHead_ChargingRainbowBeam_1</c>.</summary>
    ChargingRainbowBeamLoop,
}
