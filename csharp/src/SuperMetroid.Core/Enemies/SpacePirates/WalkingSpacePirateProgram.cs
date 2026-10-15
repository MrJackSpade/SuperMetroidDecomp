namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The walking Space Pirates' walk, look, flinch and laser animations as typed programs (#641), one per bank-$B2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="WalkingSpacePirateAnimations"/>.
/// </summary>
internal enum WalkingSpacePirateProgram
{
    /// <summary><c>InstList_PirateWalking_Flinch_FacingLeft</c> at $B2:FB4C.</summary>
    FlinchFacingLeft,
    /// <summary><c>InstList_PirateWalking_Flinch_FacingRight</c> at $B2:FB58.</summary>
    FlinchFacingRight,
    /// <summary><c>InstList_PirateWalking_WalkingLeft_0</c> at $B2:FB64.</summary>
    WalkingLeft,
    /// <summary>The loop at $B2:FB68.</summary>
    WalkingLeftLoop,
    /// <summary><c>InstList_PirateWalking_FireLasersLeft</c> at $B2:FB8C.</summary>
    FireLasersLeft,
    /// <summary><c>InstList_PirateWalking_LookingAround_FacingLeft</c> at $B2:FBC6.</summary>
    LookingFacingLeft,
    /// <summary><c>InstList_PirateWalking_WalkingRight_0</c> at $B2:FBE6.</summary>
    WalkingRight,
    /// <summary>The loop at $B2:FBEA.</summary>
    WalkingRightLoop,
    /// <summary><c>InstList_PirateWalking_FireLasersRight</c> at $B2:FC0E.</summary>
    FireLasersRight,
    /// <summary><c>InstList_PirateWalking_LookingAround_FacingRight</c> at $B2:FC48.</summary>
    LookingFacingRight,
}
