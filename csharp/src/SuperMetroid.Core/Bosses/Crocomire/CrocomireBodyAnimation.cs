namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's body, melting and skeleton animations, each named for what it shows (#1938, #2399). His fight and death start
/// them by name and never ask where in an animation his body stands (#2180); each plays the
/// <see cref="CrocomireBodySegment"/> runs <see cref="CrocomireBodyAnimations.StartingWith"/> starts it with.
/// </summary>
public enum CrocomireBodyAnimation
{
    /// <summary>His idle sway before the fight starts.</summary>
    IdleSway,
    /// <summary>Swinging his claws and opening his mouth to spit, then his spitting lunges.</summary>
    ProjectileAttack,
    /// <summary>A pause before his step forward, then his step forward.</summary>
    StepForwardAfterDelay,
    /// <summary>His step forward.</summary>
    StepForward,
    /// <summary>The start of his step back, then his step back and his damage wait.</summary>
    StepBack,
    /// <summary>His step back, then his damage wait.</summary>
    SteppingBack,
    /// <summary>Waiting for a hit: standing, moving his claws, then roaring again and again.</summary>
    AwaitingHit,
    /// <summary>Waiting for a hit, moving his claws, then roaring again and again.</summary>
    MovingClaws,
    /// <summary>Waiting for a hit, roaring again and again.</summary>
    Roar,
    /// <summary>His roar closing his mouth, then roaring again and again.</summary>
    RoarCloseMouth,
    /// <summary>The Power Bomb reaction struck with his mouth fully open.</summary>
    PowerBombReactionMouthFullyOpen,
    /// <summary>The Power Bomb reaction struck with his mouth partly open.</summary>
    PowerBombReactionMouthPartiallyOpen,
    /// <summary>The Power Bomb reaction struck with his mouth closed: his claw swing, then his charge.</summary>
    PowerBombReactionMouthNotOpen,
    /// <summary>His charge near the spike wall, starting with a roar.</summary>
    NearSpikeWallCharge,
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
    /// <summary>His skeleton falling apart, then at rest.</summary>
    SkeletonFallsApart,
    /// <summary>His skeleton at rest after it falls apart.</summary>
    SkeletonStable,
    /// <summary>His dead skeleton.</summary>
    Dead,
    /// <summary>His skeleton flowing down the river.</summary>
    SkeletonFlowingDownRiver,
}
