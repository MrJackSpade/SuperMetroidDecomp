namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's typed body programs (#641), in the authored order of their lists. Each entry program first
/// redirects to its right-facing counterpart unless Ridley faces left, as the lists do.
/// </summary>
internal enum RidleyBodySegment
{
    /// <summary>The standing pose.</summary>
    Initial,
    /// <summary>The right-facing standing pose.</summary>
    InitialFacingRight,
    /// <summary>Ceres Ridley's lunge.</summary>
    CeresLunge,
    /// <summary>The right-facing lunge.</summary>
    CeresLungeFacingRight,
    /// <summary>Ceres Ridley reaching for the Baby Metroid.</summary>
    RetrieveBabyMetroid,
    /// <summary>The right-facing reach.</summary>
    RetrieveBabyMetroidFacingRight,
    /// <summary>The opening roar.</summary>
    OpeningRoar,
    /// <summary>The right-facing opening roar.</summary>
    OpeningRoarFacingRight,
    /// <summary>The death roar.</summary>
    DeathRoar,
    /// <summary>The right-facing death roar.</summary>
    DeathRoarFacingRight,
    /// <summary>Turning to face right.</summary>
    TurnFromLeftToRight,
    /// <summary>Turning to face left.</summary>
    TurnFromRightToLeft,
    /// <summary>Two fireball bursts.</summary>
    Fireballing,
    /// <summary>The fireballing tail that closes the mouth.</summary>
    FireballingEnd,
    /// <summary>The right-facing fireball bursts.</summary>
    FireballingFacingRight,
    /// <summary>The right-facing fireballing tail.</summary>
    FireballingEndFacingRight,
    /// <summary>Taking off.</summary>
    TransitionToFlying,
    /// <summary>The right-facing take-off.</summary>
    /// <remarks>It calls the Lower Norfair lift-off, as the list does.</remarks>
    TransitionToFlyingFacingRight,
}
