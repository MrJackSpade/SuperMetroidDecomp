namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's body, melting and skeleton animations, each named for what it shows (#1938). His fight and death start them
/// by name and never ask where in an animation his body stands (#2180).
/// </summary>
public enum CrocomireBodySegment
{
    /// <summary>His idle sway before the fight starts.</summary>
    Initial,
    /// <summary>Swinging his claws and opening his mouth to spit, before his spitting lunges.</summary>
    ProjectileAttack,
    /// <summary>His spitting lunges, each led by the fight cue that spits a shot, looping until his volley is spent.</summary>
    ProjectileAttackSpitting,
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
    /// <summary>The Power Bomb reaction's charge: his steps forward, looping while the charge lasts.</summary>
    PowerBombReactionCharge,
    /// <summary>His charge near the spike wall, starting with a roar.</summary>
    NearSpikeWallCharge,
    /// <summary>His charge near the spike wall, its steps toward the wall, looping until he backs off.</summary>
    NearSpikeWallChargeSteps,
    /// <summary>Backing off from the spike wall.</summary>
    BackOffFromSpikeWall,
    /// <summary>His flesh melting off his skull, its top row above the acid.</summary>
    MeltingFleshTopRow,
    /// <summary>His flesh melting off his skull, its top two rows above the acid.</summary>
    MeltingFleshTopTwoRows,
    /// <summary>His flesh melting off his skull, its top three rows above the acid.</summary>
    MeltingFleshTopThreeRows,
    /// <summary>His flesh melting off his skull, its top four rows above the acid.</summary>
    MeltingFleshTopFourRows,
    /// <summary>His bared skull over the rest of him melting, its top row above the acid.</summary>
    MeltingBareSkullTopRow,
    /// <summary>His bared skull over the rest of him melting, its top two rows above the acid.</summary>
    MeltingBareSkullTopTwoRows,
    /// <summary>His bared skull over the rest of him melting, its top three rows above the acid.</summary>
    MeltingBareSkullTopThreeRows,
    /// <summary>His bared skull over the rest of him melting, its top four rows above the acid.</summary>
    MeltingBareSkullTopFourRows,
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
