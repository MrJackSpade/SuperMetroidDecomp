namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's body, melting and skeleton animation programs, declared in the order the cartridge authors them
/// in bank $A4. The fight AI depends on that order: it asks whether the program it is running lies at or after
/// a given program, as the native routines compare instruction pointers.
/// </summary>
public enum CrocomireBodyProgram
{
    /// <summary><c>InstList_Crocomire_Initial</c>.</summary>
    Initial,
    /// <summary><c>UNUSED_InstList_Crocomire_ChargeForwardOneStep_A4BAEA</c>.</summary>
    UnusedChargeForwardOneStep,
    /// <summary><c>InstList_Crocomire_ProjectileAttack_0</c>.</summary>
    ProjectileAttack,
    /// <summary><c>InstList_Crocomire_ProjectileAttack_1</c>.</summary>
    ProjectileAttackLoop,
    /// <summary><c>InstList_Crocomire_StepForwardAfterDelay</c>.</summary>
    StepForwardAfterDelay,
    /// <summary><c>InstList_Crocomire_StepForward</c>.</summary>
    StepForward,
    /// <summary><c>InstList_Crocomire_StepBack</c>.</summary>
    StepBack,
    /// <summary><c>InstList_Crocomire_SteppingBack</c>.</summary>
    SteppingBack,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage</c>.</summary>
    WaitForFirstSecondDamage,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage_MovingClaws</c>.</summary>
    MovingClaws,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage_Roar</c>.</summary>
    Roar,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage_RoarCloseMouth_0</c>.</summary>
    RoarCloseMouth,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage_RoarCloseMouth_1</c>.</summary>
    RoarCloseMouthLoop,
    /// <summary><c>InstList_Crocomire_PowerBombReaction_MouthFullyOpen</c>.</summary>
    PowerBombReactionMouthFullyOpen,
    /// <summary><c>InstList_Crocomire_PowerBombReaction_MouthPartiallyOpen</c>.</summary>
    PowerBombReactionMouthPartiallyOpen,
    /// <summary><c>InstList_Crocomire_PowerBombReaction_MouthNotOpen_0</c>.</summary>
    PowerBombReactionMouthNotOpen,
    /// <summary><c>InstList_Crocomire_PowerBombReaction_MouthNotOpen_1</c>.</summary>
    PowerBombReactionMouthNotOpenLoop,
    /// <summary><c>InstList_CrocomireTongue_NearSpikeWallCharge_0</c>.</summary>
    NearSpikeWallCharge,
    /// <summary><c>InstList_CrocomireTongue_NearSpikeWallCharge_1</c>.</summary>
    NearSpikeWallChargeLoop,
    /// <summary><c>InstList_Crocomire_BackOffFromSpikeWall</c>.</summary>
    BackOffFromSpikeWall,
    /// <summary><c>InstList_Crocomire_Melting1_TopRow</c>.</summary>
    MeltingOneTopRow,
    /// <summary><c>InstList_Crocomire_Melting1_Top2Rows</c>.</summary>
    MeltingOneTopTwoRows,
    /// <summary><c>InstList_Crocomire_Melting1_Top3Rows</c>.</summary>
    MeltingOneTopThreeRows,
    /// <summary><c>InstList_Crocomire_Melting1_Top4Rows</c>.</summary>
    MeltingOneTopFourRows,
    /// <summary><c>InstList_Crocomire_Melting2_TopRow</c>.</summary>
    MeltingTwoTopRow,
    /// <summary><c>InstList_Crocomire_Melting2_Top2Rows</c>.</summary>
    MeltingTwoTopTwoRows,
    /// <summary><c>InstList_Crocomire_Melting2_Top3Rows</c>.</summary>
    MeltingTwoTopThreeRows,
    /// <summary><c>InstList_Crocomire_Melting2_Top4Rows</c>.</summary>
    MeltingTwoTopFourRows,
    /// <summary><c>InstList_CrocomireTongue_BridgeCollapsed</c>.</summary>
    BridgeCollapsed,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_Falling</c>.</summary>
    SkeletonFalling,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_FallsApart_0</c>.</summary>
    SkeletonFallsApart,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_1</c>.</summary>
    SkeletonStable,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_Dead</c>.</summary>
    Dead,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_FlowingDownTheRiver</c>.</summary>
    SkeletonFlowingDownRiver,

}
