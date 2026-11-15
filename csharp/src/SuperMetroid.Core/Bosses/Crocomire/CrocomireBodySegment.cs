namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's body, melting and skeleton animations, each named for what it shows (#1938). The fight AI asks which animation his body stands in, in the named
/// groups of <see cref="CrocomireBodyAnimationGroups"/>, never how the animations are ordered (#1886).
/// </summary>
public enum CrocomireBodySegment
{
    /// <summary>His idle sway before the fight starts.</summary>
    Initial,
    /// <summary>Opening his mouth to spit, before the spit loop.</summary>
    ProjectileAttack,
    /// <summary>The spit loop, whose fight cues spit his volley.</summary>
    ProjectileAttackLoop,
    /// <summary>A pause before his step forward.</summary>
    StepForwardAfterDelay,
    /// <summary>His step forward.</summary>
    StepForward,
    /// <summary>The start of his step back.</summary>
    StepBack,
    /// <summary>His step back.</summary>
    SteppingBack,
    /// <summary>Waiting for a hit, standing.</summary>
    WaitForFirstSecondDamage,
    /// <summary>Waiting for a hit, moving his claws.</summary>
    MovingClaws,
    /// <summary>Waiting for a hit, roaring.</summary>
    Roar,
    /// <summary>His roar closing his mouth.</summary>
    RoarCloseMouth,
    /// <summary>His closed mouth after the roar, whose fight cues roar again.</summary>
    RoarCloseMouthLoop,
    /// <summary>The Power Bomb reaction struck with his mouth fully open.</summary>
    PowerBombReactionMouthFullyOpen,
    /// <summary>The Power Bomb reaction struck with his mouth partly open.</summary>
    PowerBombReactionMouthPartiallyOpen,
    /// <summary>The Power Bomb reaction struck with his mouth closed, raising dust.</summary>
    PowerBombReactionMouthNotOpen,
    /// <summary>The Power Bomb reaction's charge to the left.</summary>
    PowerBombReactionMouthNotOpenLoop,
    /// <summary>His charge near the spike wall, starting.</summary>
    NearSpikeWallCharge,
    /// <summary>His charge toward the spike wall.</summary>
    NearSpikeWallChargeLoop,
    /// <summary>Backing off from the spike wall.</summary>
    BackOffFromSpikeWall,
    /// <summary>The first melting pass, its top row.</summary>
    MeltingOneTopRow,
    /// <summary>The first melting pass, its top two rows.</summary>
    MeltingOneTopTwoRows,
    /// <summary>The first melting pass, its top three rows.</summary>
    MeltingOneTopThreeRows,
    /// <summary>The first melting pass, its top four rows.</summary>
    MeltingOneTopFourRows,
    /// <summary>The second melting pass, its top row.</summary>
    MeltingTwoTopRow,
    /// <summary>The second melting pass, its top two rows.</summary>
    MeltingTwoTopTwoRows,
    /// <summary>The second melting pass, its top three rows.</summary>
    MeltingTwoTopThreeRows,
    /// <summary>The second melting pass, its top four rows.</summary>
    MeltingTwoTopFourRows,
    /// <summary>Sinking after the bridge collapses under him.</summary>
    BridgeCollapsed,
    /// <summary>His skeleton falling.</summary>
    SkeletonFalling,
    /// <summary>His skeleton falling apart.</summary>
    SkeletonFallsApart,
    /// <summary>His skeleton at rest after it falls apart.</summary>
    SkeletonStable,
    /// <summary>His dead skeleton.</summary>
    Dead,
    /// <summary>His skeleton flowing down the river.</summary>
    SkeletonFlowingDownRiver,
}
