namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's body, melting and skeleton animations. The fight AI asks which animation his body stands in, in the named
/// groups of <see cref="CrocomireBodyAnimationGroups"/>, never how the animations are ordered (#1886).
/// </summary>
public enum CrocomireBodySegment
{
    /// <summary>Crocomire animation: initial.</summary>
    Initial,
    /// <summary>Crocomire animation: projectile attack, part 0.</summary>
    ProjectileAttack,
    /// <summary>Crocomire animation: projectile attack, part 1.</summary>
    ProjectileAttackLoop,
    /// <summary>Crocomire animation: step forward after delay.</summary>
    StepForwardAfterDelay,
    /// <summary>Crocomire animation: step forward.</summary>
    StepForward,
    /// <summary>Crocomire animation: step back.</summary>
    StepBack,
    /// <summary>Crocomire animation: stepping back.</summary>
    SteppingBack,
    /// <summary>Crocomire animation: wait for first second damage.</summary>
    WaitForFirstSecondDamage,
    /// <summary>Crocomire animation: wait for first second damage, moving claws.</summary>
    MovingClaws,
    /// <summary>Crocomire animation: wait for first second damage, roar.</summary>
    Roar,
    /// <summary>Crocomire animation: wait for first second damage, roar close mouth, part 0.</summary>
    RoarCloseMouth,
    /// <summary>Crocomire animation: wait for first second damage, roar close mouth, part 1.</summary>
    RoarCloseMouthLoop,
    /// <summary>Crocomire animation: power Bomb reaction, mouth fully open.</summary>
    PowerBombReactionMouthFullyOpen,
    /// <summary>Crocomire animation: power Bomb reaction, mouth partially open.</summary>
    PowerBombReactionMouthPartiallyOpen,
    /// <summary>Crocomire animation: power Bomb reaction, mouth not open, part 0.</summary>
    PowerBombReactionMouthNotOpen,
    /// <summary>Crocomire animation: power Bomb reaction, mouth not open, part 1.</summary>
    PowerBombReactionMouthNotOpenLoop,
    /// <summary>Crocomire tongue animation: near spike wall charge, part 0.</summary>
    NearSpikeWallCharge,
    /// <summary>Crocomire tongue animation: near spike wall charge, part 1.</summary>
    NearSpikeWallChargeLoop,
    /// <summary>Crocomire animation: back off from spike wall.</summary>
    BackOffFromSpikeWall,
    /// <summary>Crocomire animation: melting 1, top row.</summary>
    MeltingOneTopRow,
    /// <summary>Crocomire animation: melting 1, top 2 rows.</summary>
    MeltingOneTopTwoRows,
    /// <summary>Crocomire animation: melting 1, top 3 rows.</summary>
    MeltingOneTopThreeRows,
    /// <summary>Crocomire animation: melting 1, top 4 rows.</summary>
    MeltingOneTopFourRows,
    /// <summary>Crocomire animation: melting 2, top row.</summary>
    MeltingTwoTopRow,
    /// <summary>Crocomire animation: melting 2, top 2 rows.</summary>
    MeltingTwoTopTwoRows,
    /// <summary>Crocomire animation: melting 2, top 3 rows.</summary>
    MeltingTwoTopThreeRows,
    /// <summary>Crocomire animation: melting 2, top 4 rows.</summary>
    MeltingTwoTopFourRows,
    /// <summary>Crocomire tongue animation: bridge collapsed.</summary>
    BridgeCollapsed,
    /// <summary>Crocomire corpse animation: skeleton, falling.</summary>
    SkeletonFalling,
    /// <summary>Crocomire corpse animation: skeleton, falls apart, part 0.</summary>
    SkeletonFallsApart,
    /// <summary>Crocomire corpse animation: skeleton, part 1.</summary>
    SkeletonStable,
    /// <summary>Crocomire corpse animation: skeleton, dead.</summary>
    Dead,
    /// <summary>Crocomire corpse animation: skeleton, flowing down the river.</summary>
    SkeletonFlowingDownRiver,

}
