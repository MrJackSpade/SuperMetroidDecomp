namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's typed body animations (#641), in authored order. Most come in a left-facing and a right-facing segment, and
/// his owner starts the one for the way he faces (#2175, <see cref="RidleyFacedAnimation"/>).
/// </summary>
internal enum RidleyBodySegment
{
    /// <summary>The left-facing standing pose.</summary>
    InitialFacingLeft,
    /// <summary>The right-facing standing pose.</summary>
    InitialFacingRight,
    /// <summary>Ceres Ridley's left-facing lunge.</summary>
    CeresLungeFacingLeft,
    /// <summary>The right-facing lunge.</summary>
    CeresLungeFacingRight,
    /// <summary>Ceres Ridley reaching for the Baby Metroid, facing left.</summary>
    RetrieveBabyMetroidFacingLeft,
    /// <summary>The right-facing reach.</summary>
    RetrieveBabyMetroidFacingRight,
    /// <summary>The left-facing opening roar.</summary>
    OpeningRoarFacingLeft,
    /// <summary>The right-facing opening roar.</summary>
    OpeningRoarFacingRight,
    /// <summary>The left-facing death roar.</summary>
    DeathRoarFacingLeft,
    /// <summary>The right-facing death roar.</summary>
    DeathRoarFacingRight,
    /// <summary>Turning to face right.</summary>
    TurnFromLeftToRight,
    /// <summary>Turning to face left.</summary>
    TurnFromRightToLeft,
    /// <summary>Two left-facing fireball bursts.</summary>
    FireballingFacingLeft,
    /// <summary>The left-facing fireballing tail that closes the mouth.</summary>
    FireballingEndFacingLeft,
    /// <summary>The right-facing fireball bursts.</summary>
    FireballingFacingRight,
    /// <summary>The right-facing fireballing tail.</summary>
    FireballingEndFacingRight,
    /// <summary>Taking off facing left.</summary>
    /// <remarks>It calls the Ceres lift-off, as the game's animation does.</remarks>
    TransitionToFlyingFacingLeft,
    /// <summary>The right-facing take-off.</summary>
    /// <remarks>It calls the Lower Norfair lift-off, as the game's animation does.</remarks>
    TransitionToFlyingFacingRight,
}
