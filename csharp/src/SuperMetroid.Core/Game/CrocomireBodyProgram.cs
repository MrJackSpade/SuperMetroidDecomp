namespace SuperMetroid.Core.Game;

/// <summary>
/// Crocomire's body, melting and skeleton animation programs, declared in the order the cartridge authors them
/// in bank $A4. The fight AI depends on that order: it asks whether the program it is running lies at or after
/// a given program, as the native routines compare instruction pointers.
/// </summary>
public enum CrocomireBodyProgram
{
    /// <summary><c>InstList_Crocomire_Initial</c> (<c>$A4:BADE</c>).</summary>
    Initial,
    /// <summary><c>UNUSED_InstList_Crocomire_ChargeForwardOneStep_A4BAEA</c> (<c>$A4:BAEA</c>).</summary>
    UnusedChargeForwardOneStep,
    /// <summary><c>InstList_Crocomire_ProjectileAttack_0</c> (<c>$A4:BB36</c>).</summary>
    ProjectileAttack,
    /// <summary><c>InstList_Crocomire_ProjectileAttack_1</c> (<c>$A4:BB94</c>).</summary>
    ProjectileAttackLoop,
    /// <summary><c>InstList_Crocomire_StepForwardAfterDelay</c> (<c>$A4:BBCA</c>).</summary>
    StepForwardAfterDelay,
    /// <summary><c>InstList_Crocomire_StepForward</c> (<c>$A4:BBCE</c>).</summary>
    StepForward,
    /// <summary><c>InstList_Crocomire_StepBack</c> (<c>$A4:BC30</c>).</summary>
    StepBack,
    /// <summary><c>InstList_Crocomire_SteppingBack</c> (<c>$A4:BC34</c>).</summary>
    SteppingBack,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage</c> (<c>$A4:BC56</c>).</summary>
    WaitForFirstSecondDamage,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage_MovingClaws</c> (<c>$A4:BCD8</c>).</summary>
    MovingClaws,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage_Roar</c> (<c>$A4:BD2A</c>).</summary>
    Roar,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage_RoarCloseMouth_0</c> (<c>$A4:BD8E</c>).</summary>
    RoarCloseMouth,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage_RoarCloseMouth_1</c> (<c>$A4:BDA2</c>).</summary>
    RoarCloseMouthLoop,
    /// <summary><c>InstList_Crocomire_PowerBombReaction_MouthFullyOpen</c> (<c>$A4:BDAE</c>).</summary>
    PowerBombReactionMouthFullyOpen,
    /// <summary><c>InstList_Crocomire_PowerBombReaction_MouthPartiallyOpen</c> (<c>$A4:BDB2</c>).</summary>
    PowerBombReactionMouthPartiallyOpen,
    /// <summary><c>InstList_Crocomire_PowerBombReaction_MouthNotOpen_0</c> (<c>$A4:BDB6</c>).</summary>
    PowerBombReactionMouthNotOpen,
    /// <summary><c>InstList_Crocomire_PowerBombReaction_MouthNotOpen_1</c> (<c>$A4:BE06</c>).</summary>
    PowerBombReactionMouthNotOpenLoop,
    /// <summary><c>InstList_CrocomireTongue_NearSpikeWallCharge_0</c> (<c>$A4:BE7E</c>).</summary>
    NearSpikeWallCharge,
    /// <summary><c>InstList_CrocomireTongue_NearSpikeWallCharge_1</c> (<c>$A4:BEEC</c>).</summary>
    NearSpikeWallChargeLoop,
    /// <summary><c>InstList_Crocomire_BackOffFromSpikeWall</c> (<c>$A4:BF3C</c>).</summary>
    BackOffFromSpikeWall,
    /// <summary><c>InstList_Crocomire_Melting1_TopRow</c> (<c>$A4:BF64</c>).</summary>
    MeltingOneTopRow,
    /// <summary><c>InstList_Crocomire_Melting1_Top2Rows</c> (<c>$A4:BF6C</c>).</summary>
    MeltingOneTopTwoRows,
    /// <summary><c>InstList_Crocomire_Melting1_Top3Rows</c> (<c>$A4:BF72</c>).</summary>
    MeltingOneTopThreeRows,
    /// <summary><c>InstList_Crocomire_Melting1_Top4Rows</c> (<c>$A4:BF78</c>).</summary>
    MeltingOneTopFourRows,
    /// <summary><c>InstList_Crocomire_Melting2_TopRow</c> (<c>$A4:BF7E</c>).</summary>
    MeltingTwoTopRow,
    /// <summary><c>InstList_Crocomire_Melting2_Top2Rows</c> (<c>$A4:BF86</c>).</summary>
    MeltingTwoTopTwoRows,
    /// <summary><c>InstList_Crocomire_Melting2_Top3Rows</c> (<c>$A4:BF8C</c>).</summary>
    MeltingTwoTopThreeRows,
    /// <summary><c>InstList_Crocomire_Melting2_Top4Rows</c> (<c>$A4:BF92</c>).</summary>
    MeltingTwoTopFourRows,
    /// <summary><c>InstList_CrocomireTongue_BridgeCollapsed</c> (<c>$A4:BFB0</c>).</summary>
    BridgeCollapsed,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_Falling</c> (<c>$A4:E14A</c>).</summary>
    SkeletonFalling,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_FallsApart_0</c> (<c>$A4:E158</c>).</summary>
    SkeletonFallsApart,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_1</c> (<c>$A4:E1C6</c>).</summary>
    SkeletonStable,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_Dead</c> (<c>$A4:E1CC</c>).</summary>
    Dead,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_FlowingDownTheRiver</c> (<c>$A4:E1D2</c>).</summary>
    SkeletonFlowingDownRiver,

}
