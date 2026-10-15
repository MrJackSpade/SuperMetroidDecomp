namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>The bank-$A3 function pointer stored in a Sbug enemy's variable B.</summary>
public enum SbugEnemyFunction : ushort
{
    /// <summary><c>Function_Sbug_WaitForSamusToGetNear</c> at <c>$A3:A2D7</c>; selects an activation handler only when both axis distances are strictly below the spawn radius.</summary>
    WaitForSamus = 0xa2d7,
    /// <summary><c>Function_Sbug_Activate_0_MoveForwards</c> at <c>$A3:A301</c>; spends one AI invocation selecting continuous forward movement.</summary>
    ActivateMoveForward = 0xa301,
    /// <summary><c>Function_Sbug_Activate_1_MoveForwardsInAZigZag</c> at <c>$A3:A30B</c>; selects alternating side movement without moving on this invocation.</summary>
    ActivateZigZag = 0xa30b,
    /// <summary><c>Function_Sbug_Activate_3_MoveRandomly_WaitForSamusIfHitBlock</c> at <c>$A3:A315</c>; resets shared RNG to $000B and selects the collision-aware random-direction chooser.</summary>
    ActivateRandomUntilCollision = 0xa315,
    /// <summary><c>Function_Sbug_Activate_4_MoveRandomly_ReverseUntil6BlockAway</c> at <c>$A3:A325</c>; sets the $0200 long countdown, resets shared RNG to $000B, and selects its direction chooser.</summary>
    ActivateRandomAndReverseWhenFar = 0xa325,
    /// <summary><c>Function_Sbug_Activate_5_MoveForwards_WaitForSamus</c> at <c>$A3:A33B</c>; initializes the 32-update forward segment without moving yet.</summary>
    ActivateMoveForwardThenWait = 0xa33b,
    /// <summary><c>Function_Sbug_Activate_2_MoveAlong45DegreeAngleTowardsSamus</c> at <c>$A3:A34B</c>; computes and installs a fixed velocity and facing toward Samus without moving yet.</summary>
    ActivateMoveTowardSamus = 0xa34b,
    /// <summary><c>Function_Sbug_Activate_6_MoveAlong45DegreeAngleAwayFromSamus</c> at <c>$A3:A380</c>; computes and installs the opposite velocity and facing without moving yet.</summary>
    ActivateMoveAwayFromSamus = 0xa380,
    /// <summary><c>Function_Sbug_Activated_0_MoveForwards</c> at <c>$A3:A407</c>; adds the precomputed forward magnitudes with signs derived from the spawn angle.</summary>
    MoveForward = 0xa407,
    /// <summary><c>Function_Sbug_Activated_1_MoveForwardsInAZigZag</c> at <c>$A3:A40E</c>; selects side velocity and facing using bit $10 of the enemy frame counter.</summary>
    ZigZag = 0xa40e,
    /// <summary><c>Function_Sbug_Activated_2_MoveAlong45DegreeAngleTowardsSamus</c> at <c>$A3:A440</c>; applies the captured custom velocity without retargeting or block collision checks.</summary>
    MoveTowardSamus = 0xa440,
    /// <summary><c>Function_Sbug_Activated_6_MoveAlong45DegAngleAwayFromSamus</c> at <c>$A3:A447</c>; applies the captured away velocity without retargeting or block collision checks.</summary>
    MoveAwayFromSamus = 0xa447,
    /// <summary><c>Function_Sbug_Activated_5_MoveForwards_WaitForSamus</c> at <c>$A3:A44E</c>; decrements the segment timer before moving and returns to proximity waiting on signed underflow.</summary>
    MoveForwardThenWait = 0xa44e,
    /// <summary><c>Function_Sbug_MoveAlongCustomAngle_WaitForSamusIfHitBlock</c> at <c>$A3:A462</c>; runs a timed, collision-aware segment, waiting on collision or choosing another angle on timer underflow.</summary>
    MoveRandomlyUntilCollision = 0xa462,
    /// <summary><c>Function_Sbug_MoveStraightAlongAngle_ReverseUntil6BlocksAway</c> at <c>$A3:A476</c>; applies custom movement and, while the long countdown is nonnegative, checks segment expiry and the both-axis 96-pixel reversal condition.</summary>
    MoveStraightAndReverseWhenFar = 0xa476,
    /// <summary><c>Function_Sbug_Activated_4_MoveRandomly_ReverseUntil6BlocksAway</c> at <c>$A3:A4B6</c>; perturbs the custom angle using shared RNG, installs velocity and facing, and starts a 32-update reversal-aware segment.</summary>
    ChooseRandomDirectionAndReverseWhenFar = 0xa4b6,
    /// <summary><c>Function_Sbug_Activated_3_MoveRandomly_WaitForSamusIfHitBlock</c> at <c>$A3:A4F0</c>; perturbs the custom angle using shared RNG, installs velocity and facing, and starts a 32-update collision-aware segment.</summary>
    ChooseRandomDirectionUntilCollision = 0xa4f0,
}
