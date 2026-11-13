namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's body, melting and skeleton animation programs, declared in the order the game authors them.
/// The fight AI depends on that order: it asks whether the program it is running lies at or after
/// a given program, as the game's fight compares animation positions.
/// </summary>
public enum CrocomireBodySegment
{
    /// <summary>Crocomire animation script: initial.</summary>
    Initial,
    /// <summary>Unused Crocomire animation script: charge forward one step.</summary>
    UnusedChargeForwardOneStep,
    /// <summary>Crocomire animation script: projectile attack, part 0.</summary>
    ProjectileAttack,
    /// <summary>Crocomire animation script: projectile attack, part 1.</summary>
    ProjectileAttackLoop,
    /// <summary>Crocomire animation script: step forward after delay.</summary>
    StepForwardAfterDelay,
    /// <summary>Crocomire animation script: step forward.</summary>
    StepForward,
    /// <summary>Crocomire animation script: step back.</summary>
    StepBack,
    /// <summary>Crocomire animation script: stepping back.</summary>
    SteppingBack,
    /// <summary>Crocomire animation script: wait for first second damage.</summary>
    WaitForFirstSecondDamage,
    /// <summary>Crocomire animation script: wait for first second damage, moving claws.</summary>
    MovingClaws,
    /// <summary>Crocomire animation script: wait for first second damage, roar.</summary>
    Roar,
    /// <summary>Crocomire animation script: wait for first second damage, roar close mouth, part 0.</summary>
    RoarCloseMouth,
    /// <summary>Crocomire animation script: wait for first second damage, roar close mouth, part 1.</summary>
    RoarCloseMouthLoop,
    /// <summary>Crocomire animation script: power Bomb reaction, mouth fully open.</summary>
    PowerBombReactionMouthFullyOpen,
    /// <summary>Crocomire animation script: power Bomb reaction, mouth partially open.</summary>
    PowerBombReactionMouthPartiallyOpen,
    /// <summary>Crocomire animation script: power Bomb reaction, mouth not open, part 0.</summary>
    PowerBombReactionMouthNotOpen,
    /// <summary>Crocomire animation script: power Bomb reaction, mouth not open, part 1.</summary>
    PowerBombReactionMouthNotOpenLoop,
    /// <summary>Crocomire tongue animation script: near spike wall charge, part 0.</summary>
    NearSpikeWallCharge,
    /// <summary>Crocomire tongue animation script: near spike wall charge, part 1.</summary>
    NearSpikeWallChargeLoop,
    /// <summary>Crocomire animation script: back off from spike wall.</summary>
    BackOffFromSpikeWall,
    /// <summary>Crocomire animation script: melting 1, top row.</summary>
    MeltingOneTopRow,
    /// <summary>Crocomire animation script: melting 1, top 2 rows.</summary>
    MeltingOneTopTwoRows,
    /// <summary>Crocomire animation script: melting 1, top 3 rows.</summary>
    MeltingOneTopThreeRows,
    /// <summary>Crocomire animation script: melting 1, top 4 rows.</summary>
    MeltingOneTopFourRows,
    /// <summary>Crocomire animation script: melting 2, top row.</summary>
    MeltingTwoTopRow,
    /// <summary>Crocomire animation script: melting 2, top 2 rows.</summary>
    MeltingTwoTopTwoRows,
    /// <summary>Crocomire animation script: melting 2, top 3 rows.</summary>
    MeltingTwoTopThreeRows,
    /// <summary>Crocomire animation script: melting 2, top 4 rows.</summary>
    MeltingTwoTopFourRows,
    /// <summary>Crocomire tongue animation script: bridge collapsed.</summary>
    BridgeCollapsed,
    /// <summary>Crocomire corpse animation script: skeleton, falling.</summary>
    SkeletonFalling,
    /// <summary>Crocomire corpse animation script: skeleton, falls apart, part 0.</summary>
    SkeletonFallsApart,
    /// <summary>Crocomire corpse animation script: skeleton, part 1.</summary>
    SkeletonStable,
    /// <summary>Crocomire corpse animation script: skeleton, dead.</summary>
    Dead,
    /// <summary>Crocomire corpse animation script: skeleton, flowing down the river.</summary>
    SkeletonFlowingDownRiver,

}
