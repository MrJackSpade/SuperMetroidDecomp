namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's typed body programs (#641), in the authored order of their bank-$A6 lists. Each entry program first
/// redirects to its right-facing counterpart unless Ridley faces left, as the native lists do.
/// </summary>
internal enum RidleyBodyProgram
{
    /// <summary>The standing pose (<c>InstList_Ridley_FacingLeft_Initial</c>, $A6:E538).</summary>
    Initial,
    /// <summary>The right-facing standing pose ($A6:E542).</summary>
    InitialFacingRight,
    /// <summary>Ceres Ridley's lunge (<c>InstList_RidleyCeres_FacingLeft_Lunging</c>, $A6:E548).</summary>
    CeresLunge,
    /// <summary>The right-facing lunge ($A6:E576).</summary>
    CeresLungeFacingRight,
    /// <summary>Ceres Ridley reaching for the Baby Metroid (<c>InstList_RidleyCeres_RetrieveBabyMetroid</c>, $A6:E658).</summary>
    RetrieveBabyMetroid,
    /// <summary>The right-facing reach ($A6:E676).</summary>
    RetrieveBabyMetroidFacingRight,
    /// <summary>The opening roar (<c>InstList_Ridley_FacingLeft_OpeningRoar</c>, $A6:E690).</summary>
    OpeningRoar,
    /// <summary>The right-facing opening roar ($A6:E6AE).</summary>
    OpeningRoarFacingRight,
    /// <summary>The death roar (<c>InstList_Ridley_FacingLeft_DeathRoar</c>, $A6:E6C8).</summary>
    DeathRoar,
    /// <summary>The right-facing death roar ($A6:E6DE).</summary>
    DeathRoarFacingRight,
    /// <summary>Turning to face right (<c>InstList_Ridley_TurnFromLeftToRight</c>, $A6:E6F0).</summary>
    TurnFromLeftToRight,
    /// <summary>Turning to face left (<c>InstList_Ridley_TurnFromRightToLeft</c>, $A6:E706).</summary>
    TurnFromRightToLeft,
    /// <summary>Two fireball bursts (<c>InstList_Ridley_FacingLeft_Fireballing_0</c>, $A6:E73A).</summary>
    Fireballing,
    /// <summary>The fireballing tail that closes the mouth (<c>InstList_Ridley_FacingLeft_Fireballing_1</c>, $A6:E7AC).</summary>
    FireballingEnd,
    /// <summary>The right-facing fireball bursts (<c>InstList_Ridley_FacingRight_Fireballing_0</c>, $A6:E7B4).</summary>
    FireballingFacingRight,
    /// <summary>The right-facing fireballing tail (<c>InstList_Ridley_FacingRight_Fireballing_1</c>, $A6:E820).</summary>
    FireballingEndFacingRight,
    /// <summary>Taking off (<c>InstList_RidleyCeres_FacingLeft_TransitionToFlying</c>, $A6:E91D).</summary>
    TransitionToFlying,
    /// <summary>The right-facing take-off (<c>InstList_RidleyCeres_FacingRight_TransitionToFlying</c>, $A6:E945).</summary>
    /// <remarks>It calls the Lower Norfair lift-off, as the native list does.</remarks>
    TransitionToFlyingFacingRight,
}
