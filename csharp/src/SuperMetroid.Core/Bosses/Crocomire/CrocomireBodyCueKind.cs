namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>What a cue of Crocomire's body programs does.</summary>
internal enum CrocomireBodyCueKind
{
    /// <summary><c>Instruction_Crocomire_FightAI</c>: runs the fight-AI function, which may start another program.</summary>
    FightAi,
    /// <summary>Starts the projectile attack on a one-in-four random draw.</summary>
    MaybeStartProjectileAttack,
    /// <summary>Queues Crocomire's cry.</summary>
    Cry,
    /// <summary>Queues the big explosion sound.</summary>
    BigExplosionSound,
    /// <summary>Queues the skeleton-collapse sound.</summary>
    SkeletonCollapseSound,
    /// <summary>Shakes the screen and queues its sound.</summary>
    ShakeScreen,
    /// <summary>Moves four pixels left unless a mouth hit is pending.</summary>
    MoveLeft,
    /// <summary>Spawns random foot dust, then moves four pixels left unless a mouth hit is pending.</summary>
    MoveLeftWithDust,
    /// <summary>Moves four pixels left, backing off when the spike wall stops it, and otherwise spawns foot dust.</summary>
    MoveLeftTowardSpikeWall,
    /// <summary>Moves four pixels right while the body is on screen.</summary>
    MoveRightIfOnScreen,
    /// <summary>Moves four pixels right.</summary>
    MoveRight,
    /// <summary>Spawns random foot dust, then moves four pixels right while the body is on screen.</summary>
    MoveRightIfOnScreenWithDust,
    /// <summary>Spawns random foot dust, then moves four pixels right.</summary>
    MoveRightWithDust,
    /// <summary>Spawns a big dust cloud at <see cref="CrocomireBodyCue.DustOffset"/> pixels from the body.</summary>
    Dust,
}
