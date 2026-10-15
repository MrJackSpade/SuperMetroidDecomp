namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The Shaktool's saw-hand, arm and head animations as typed programs (#641), one per bank-$AA list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="ShaktoolAnimations"/>.
/// </summary>
internal enum ShaktoolProgram
{
    /// <summary><c>UNUSED_InstList_Shaktool_SawHand_Attack_PrimaryPiece_AAD9EA</c> at $AA:D9EA.</summary>
    SawHandAttackPrimaryPiece,
    /// <summary><c>UNUSED_InstList_Shaktool_SawHand_Attack_FinalPiece_AAD9F2</c> at $AA:D9F2.</summary>
    SawHandAttackFinalPiece,
    /// <summary><c>InstList_Shaktool_SawHand_HeadBob_PrimaryPiece</c> at $AA:D9FC.</summary>
    SawHandHeadBobPrimaryPiece,
    /// <summary><c>InstList_Shaktool_SawHand_HeadBob_FinalPiece</c> at $AA:DA04.</summary>
    SawHandHeadBobFinalPiece,
    /// <summary><c>InstList_Shaktool_SawHand_PrimaryPiece</c> at $AA:DA0E.</summary>
    SawHandPrimaryPiece,
    /// <summary><c>InstList_Shaktool_SawHand_FinalPiece</c> at $AA:DA1E.</summary>
    SawHandFinalPiece,
    /// <summary><c>UNUSED_InstList_Shaktool_ArmPiece_Attack_Back_AADA2E</c> at $AA:DA2E.</summary>
    ArmPieceAttackBack,
    /// <summary><c>UNUSED_InstList_Shaktool_ArmPiece_Attack_Front_AADA42</c> at $AA:DA42.</summary>
    ArmPieceAttackFront,
    /// <summary><c>InstList_Shaktool_ArmPiece_HeadBob_Back</c> at $AA:DA56.</summary>
    ArmPieceHeadBobBack,
    /// <summary><c>InstList_Shaktool_ArmPiece_HeadBob_Front</c> at $AA:DA62.</summary>
    ArmPieceHeadBobFront,
    /// <summary><c>InstList_Shaktool_ArmPiece_Normal</c> at $AA:DA72.</summary>
    ArmPieceNormal,
    /// <summary><c>UNUSED_InstList_Shaktool_Head_Attack_AADA7A</c> at $AA:DA7A.</summary>
    HeadAttack,
    /// <summary><c>InstList_Shaktool_Head_HeadBob</c> at $AA:DA90.</summary>
    HeadHeadBob,
    /// <summary><c>InstList_Shaktool_Head_AimingLeft</c> at $AA:DAA4.</summary>
    HeadAimingLeft,
    /// <summary><c>InstList_Shaktool_Head_AimingUpLeft</c> at $AA:DAAC.</summary>
    HeadAimingUpLeft,
    /// <summary><c>InstList_Shaktool_Head_AimingUp</c> at $AA:DAB4.</summary>
    HeadAimingUp,
    /// <summary><c>InstList_Shaktool_Head_AimingUpRight</c> at $AA:DABC.</summary>
    HeadAimingUpRight,
    /// <summary><c>InstList_Shaktool_Head_AimingRight</c> at $AA:DAC4.</summary>
    HeadAimingRight,
    /// <summary><c>InstList_Shaktool_Head_AimingDownRight</c> at $AA:DACC.</summary>
    HeadAimingDownRight,
    /// <summary><c>InstList_Shaktool_Head_AimingDown</c> at $AA:DAD4.</summary>
    HeadAimingDown,
    /// <summary><c>InstList_Shaktool_Head_AimingDownLeft</c> at $AA:DADC.</summary>
    HeadAimingDownLeft,
}
