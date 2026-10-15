namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Bank-$A8 function pointers stored in native Magdollite variable F.</summary>
public enum MagdolliteEnemyFunction : ushort
{
    /// <summary>$A8:B11A, Function_Magdollite_WaitForSamusToGetNear: faces Samus and submerges when the hand cooldown has expired and Samus is within the configured horizontal range.</summary>
    HeadWaiting = 0xb11a,
    /// <summary>$A8:B175, Function_Magdollite_WaitUntilBasePillarFormed: waits for the head's submerge animation to finish, then releases the arm to grow.</summary>
    HeadWaitingForAttackAnimation = 0xb175,
    /// <summary>$A8:B193, Function_Magdollite_WaitForOtherPartsToFinish: waits for the arm to land, then selects the facing-dependent head emergence animation.</summary>
    HeadWaitingForBodyLanding = 0xb193,
    /// <summary>$A8:B1B8, Function_Magdollite_UnformBasePillarBackToHeadIdling: waits for emergence to finish before returning the head to idle.</summary>
    HeadWaitingForReturnAnimation = 0xb1b8,
    /// <summary>$A8:B1DD, Function_Magdollite_Slave1_WaitForAttackToBeTriggered: holds the arm dormant until the head clears its not-emerged flag, then initializes growth at its spawn Y.</summary>
    BodyDormant = 0xb1dd,
    /// <summary>$A8:B204, Function_Magdollite_Slave1_HandlePillarGrowth: adds the preceding head's upward 16.16 velocity and extends pillar segments until reaching Samus's height or the maximum rise.</summary>
    BodyRising = 0xb204,
    /// <summary>$A8:B291, Function_Magdollite_Slave1_GetEnemyIndex: native index-load/return stub that holds the grown arm stationary while the hand throws.</summary>
    NoOpAtApex = 0xb291,
    /// <summary>$A8:B295, Function_Magdollite_Slave1_HandlePillarShrinking: adds the head's downward 16.16 velocity, removes pillar segments, and releases the head when the arm lands.</summary>
    BodyFalling = 0xb295,
    /// <summary>$A8:B30D, Function_Magdollite_Slave2_Idling_WaitingForTrigger: waits for the head to begin forming the base pillar before tracking arm growth.</summary>
    OverlayWaitingForAttack = 0xb30d,
    /// <summary>$A8:B31F, Function_Magdollite_Slave2_SetSlave1ToShrinkAfterFireballs: waits for the throw animation to finish, restores the captured hand position, and starts arm shrinkage.</summary>
    OverlayWaitingForThrowAnimation = 0xb31f,
    /// <summary>$A8:B356, Function_Magdollite_Slave2_SetToThrowFireballsAfterGrowing: follows the growing arm cap and, at the apex, selects a Samus-facing throw animation and captures its origin.</summary>
    OverlayTrackingRisingBody = 0xb356,
    /// <summary>$A8:B3A7, Function_Magdollite_Slave2_GoBackToIdlingAfterPillarShrinks: follows the shrinking arm cap until the head returns to idle.</summary>
    OverlayTrackingFallingBody = 0xb3a7,
}
