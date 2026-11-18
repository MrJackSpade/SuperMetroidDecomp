namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's typed body animations (#641), in authored order. Most come in a left-facing and a right-facing segment, and
/// his owner starts the one for the way he faces (#2175, <see cref="RidleyFacedAnimation"/>).
/// </summary>
internal enum RidleyBodySegment
{
    /// <summary>The left-facing standing pose.</summary>
    StandingFacingLeft,
    /// <summary>The right-facing standing pose.</summary>
    StandingFacingRight,
    /// <summary>Ceres Ridley lunging facing left: his legs stretching out and drawing back in to standing.</summary>
    CeresLungeFacingLeft,
    /// <summary>Ceres Ridley lunging facing right: his legs stretching out and drawing back in to standing.</summary>
    CeresLungeFacingRight,
    /// <summary>Ceres Ridley reaching for the Baby Metroid, facing left.</summary>
    RetrieveBabyMetroidFacingLeft,
    /// <summary>The right-facing reach.</summary>
    RetrieveBabyMetroidFacingRight,
    /// <summary>Ridley facing left roaring as the fight opens: his mouth opening wide and closing again, then standing.</summary>
    OpeningRoarFacingLeft,
    /// <summary>Ridley facing right roaring as the fight opens: his mouth opening wide and closing again, then standing.</summary>
    OpeningRoarFacingRight,
    /// <summary>Ridley facing left roaring as he dies, resting on his open mouth.</summary>
    DeathRoarFacingLeft,
    /// <summary>Ridley facing right roaring as he dies, resting on his open mouth.</summary>
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
    TransitionToFlyingFacingLeft,
    /// <summary>The right-facing take-off.</summary>
    TransitionToFlyingFacingRight,
}
