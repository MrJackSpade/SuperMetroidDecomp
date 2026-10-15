namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's brain (head) programs in bank $A9 order, run by her dedicated brain list rather than an enemy
/// slot. The unreachable frames after the initial dummy and the unused flash at <c>$A9:9C7F</c> and
/// <c>$A9:9CE7</c> are not authored.
/// </summary>
public enum MotherBrainHeadProgram
{
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase2_0</c> (<c>$A9:9B7F</c>).</summary>
    StretchingPhase2,
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase2_1</c> (<c>$A9:9BAB</c>).</summary>
    StretchingPhase2Loop,
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase3_0</c> (<c>$A9:9BB3</c>).</summary>
    StretchingPhase3,
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase3_1</c> (<c>$A9:9BDF</c>).</summary>
    StretchingPhase3Loop,
    /// <summary><c>InstList_MotherBrainHead_HyperBeamRecoil_0</c> (<c>$A9:9BE7</c>).</summary>
    HyperBeamRecoil,
    /// <summary><c>InstList_MotherBrainHead_HyperBeamRecoil_1</c> (<c>$A9:9C0B</c>).</summary>
    HyperBeamRecoilLoop,
    /// <summary><c>InstList_MotherBrainHead_Initial</c> (<c>$A9:9C21</c>).</summary>
    Initial,
    /// <summary><c>InstList_MotherBrainHead_Decapitated_0</c> (<c>$A9:9C29</c>).</summary>
    Decapitated,
    /// <summary><c>InstList_MotherBrainHead_Decapitated_1</c> (<c>$A9:9C31</c>).</summary>
    DecapitatedLoop,
    /// <summary><c>InstList_MotherBrainHead_DyingDrool_0</c> (<c>$A9:9C39</c>).</summary>
    DyingDrool,
    /// <summary><c>InstList_MotherBrainHead_DyingDrool_1</c> (<c>$A9:9C47</c>).</summary>
    DyingDroolBurst,
    /// <summary><c>InstList_MotherBrainHead_DyingDrool_2</c> (<c>$A9:9C5F</c>).</summary>
    DyingDroolLoop,
    /// <summary><c>InstList_MotherBrainHead_FiringRainbowBeam</c> (<c>$A9:9C77</c>).</summary>
    FiringRainbowBeam,
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase2_0</c> (<c>$A9:9C87</c>).</summary>
    NeutralPhase2,
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase2_1</c> (<c>$A9:9C9F</c>).</summary>
    NeutralPhase2Hold,
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase3_0</c> (<c>$A9:9CB9</c>).</summary>
    NeutralPhase3,
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase3_1</c> (<c>$A9:9CD1</c>).</summary>
    NeutralPhase3Hold,
    /// <summary><c>InstList_MotherBrainHead_Corpse_0</c> (<c>$A9:9D25</c>).</summary>
    Corpse,
    /// <summary><c>InstList_MotherBrainHead_Corpse_1</c> (<c>$A9:9D35</c>).</summary>
    CorpseLoop,
    /// <summary><c>InstList_MotherBrainHead_Attacking_4OnionRings_Phase2</c> (<c>$A9:9D3D</c>).</summary>
    FourOnionRingsPhase2,
    /// <summary><c>InstList_MotherBrainHead_Attacking_2OnionRings_Phase2</c> (<c>$A9:9D7F</c>).</summary>
    TwoOnionRingsPhase2,
    /// <summary><c>InstList_MotherBrainHead_Attacking_BabyMetroid</c> (<c>$A9:9DB1</c>).</summary>
    AttackingBabyMetroid,
    /// <summary><c>InstList_MotherBrainHead_AttackingSamus_4OnionRings_Phase3</c> (<c>$A9:9DBB</c>).</summary>
    AttackingSamusFourOnionRingsPhase3,
    /// <summary><c>InstList_MotherBrainHead_Attacking_4OnionRings_Phase3</c> (<c>$A9:9DC1</c>).</summary>
    FourOnionRingsPhase3,
    /// <summary><c>InstList_MotherBrainHead_Attacking_Bomb_Phase2</c> (<c>$A9:9ECC</c>).</summary>
    BombPhase2,
    /// <summary><c>InstList_MotherBrainHead_Attacking_Bomb_Phase3</c> (<c>$A9:9F00</c>).</summary>
    BombPhase3,
    /// <summary><c>InstList_MotherBrainHead_Attacking_Laser</c> (<c>$A9:9F34</c>).</summary>
    Laser,
    /// <summary><c>InstList_MotherBrainHead_ChargingRainbowBeam_0</c> (<c>$A9:9F6C</c>).</summary>
    ChargingRainbowBeam,
    /// <summary><c>InstList_MotherBrainHead_ChargingRainbowBeam_1</c> (<c>$A9:9F7A</c>).</summary>
    ChargingRainbowBeamLoop,
}
