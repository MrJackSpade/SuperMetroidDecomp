namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which walk a <see cref="TorizoCue.Walk"/> cue is. One walking step of a Torizo, read from its walking table.</summary>
internal enum TorizoWalkKind
{
    /// <summary>Takes a Bomb Torizo walking step, turning at a wall.</summary>
    WalkBombTorizo,
    /// <summary>Takes a faceless Bomb Torizo walking step, turning at a wall.</summary>
    WalkFacelessBombTorizo,
    /// <summary>Takes a Golden Torizo walking step, turning at a wall.</summary>
    WalkGoldenTorizo,
}
