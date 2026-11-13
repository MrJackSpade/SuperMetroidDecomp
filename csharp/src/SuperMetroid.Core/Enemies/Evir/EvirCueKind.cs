namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>What a cue of the Evir's animations does: start, move or end the regenerating projectile, or queue the spit sound.</summary>
internal enum EvirCueKind
{
    /// <summary>Moves the regenerating projectile's X offset one pixel toward the body's facing.</summary>
    AdvanceRegenerationOffset,
    /// <summary>Ends regeneration and returns the projectile to idle.</summary>
    FinishRegeneration,
    /// <summary>Queues the spit sound.</summary>
    PlaySpitSound,
    /// <summary>Starts the regenerating projectile's X offset from the body.</summary>
    SetInitialRegenerationOffset,
}
