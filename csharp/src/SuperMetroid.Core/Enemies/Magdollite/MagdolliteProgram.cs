namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite head's idle, submerge and emerge animations, its body's pillar stacks and its overlay's throw and cap animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="MagdolliteAnimations"/>.
/// </summary>
internal enum MagdolliteProgram
{
    /// <summary><c>InstList_Magdollite_Idling_FacingLeft</c> at $A8:AC9C.</summary>
    LeftIdle,
    /// <summary><c>InstList_Magdollite_Slave2_ThrowFireballs_FacingLeft</c> at $A8:ACB0.</summary>
    LeftThrow,
    /// <summary><c>InstList_Magdollite_SplashIntoLavaAndFormBasePillar_Left_0</c> at $A8:ACDE.</summary>
    LeftSubmerge,
    /// <summary><c>InstList_Magdollite_SplashIntoLavaAndFormBasePillar_Left_1</c> at $A8:ACFE.</summary>
    LeftRaisePillar,
    /// <summary><c>InstList_Magdollite_UnformBasePillar_SplashBackToIdle_Left_0</c> at $A8:AD0C.</summary>
    LeftEmerge,
    /// <summary><c>InstList_Magdollite_UnformBasePillar_SplashBackToIdle_Left_1</c> at $A8:AD14.</summary>
    LeftLowerPillar,
    /// <summary><c>InstList_Magdollite_Idling_FacingRight</c> at $A8:AD3C.</summary>
    RightIdle,
    /// <summary><c>InstList_Magdollite_ThrowFireballs_FacingRight</c> at $A8:AD50.</summary>
    RightThrow,
    /// <summary><c>InstList_Magdollite_SplashIntoLavaAndFormBasePillar_Right_0</c> at $A8:AD7E.</summary>
    RightSubmerge,
    /// <summary><c>InstList_Magdollite_SplashIntoLavaAndFormBasePillar_Right_1</c> at $A8:AD9E.</summary>
    RightRaisePillar,
    /// <summary><c>InstList_Magdollite_UnformBasePillar_SplashBackToIdle_Right_0</c> at $A8:ADAC.</summary>
    RightEmerge,
    /// <summary><c>InstList_Magdollite_UnformBasePillar_SplashBackToIdle_Right_1</c> at $A8:ADB4.</summary>
    RightLowerPillar,
    /// <summary><c>InstList_Magdollite_Slave1_NarrowPillar_FacingLeft</c> at $A8:ADDC.</summary>
    PillarPhase0,
    /// <summary><c>InstList_Magdollite_Slave1_NarrowPillar_FacingRight</c> at $A8:ADE2.</summary>
    PillarPhase1,
    /// <summary><c>InstList_Magdollite_Slave1_3xPillarStack</c> at $A8:ADE8.</summary>
    PillarPhase2,
    /// <summary><c>InstList_Magdollite_Slave1_4xPillarStack</c> at $A8:ADEE.</summary>
    PillarPhase3,
    /// <summary><c>InstList_Magdollite_Slave1_5xPillarStack</c> at $A8:ADF4.</summary>
    PillarPhase4,
    /// <summary><c>InstList_Magdollite_Slave1_6xPillarStack</c> at $A8:ADFA.</summary>
    PillarPhase5,
    /// <summary><c>InstList_Magdollite_Slave1_7xPillarStack</c> at $A8:AE00.</summary>
    PillarPhase6,
    /// <summary><c>InstList_Magdollite_Slave1_8xPillarStack</c> at $A8:AE06.</summary>
    PillarPhase7,
    /// <summary><c>InstList_Magdollite_Slave2_PillarCap</c> at $A8:AE0C.</summary>
    PillarCap,
}
