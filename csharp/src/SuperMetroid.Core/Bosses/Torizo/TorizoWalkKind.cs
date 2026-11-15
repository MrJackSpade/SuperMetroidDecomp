namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which walk a <see cref="TorizoCue.Walk"/> cue is: which Torizo takes the step, and how it turns at a wall.</summary>
internal enum TorizoWalkKind
{
    /// <summary>Takes a Bomb Torizo walking step, turning at a wall.</summary>
    WalkBombTorizo,
    /// <summary>Takes a faceless Bomb Torizo walking step, turning at a wall.</summary>
    WalkFacelessBombTorizo,
    /// <summary>Takes a Golden Torizo walking step, turning at a wall.</summary>
    WalkGoldenTorizo,
}
