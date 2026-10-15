namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>What a cue of the Evir's programs does: one per bank-$A8 instruction its lists use.</summary>
internal enum EvirCueKind
{
    /// <summary>Moves the regenerating projectile's X offset one pixel toward the body's facing (<c>AdvanceRegenerationXOffset</c>, $A8:87B6).</summary>
    AdvanceRegenerationOffset,
    /// <summary>Ends regeneration and returns the projectile to idle (<c>FinishRegeneration</c>, $A8:87CB).</summary>
    FinishRegeneration,
    /// <summary>Queues the spit sound (<c>PlaySpitSFX</c>, $A8:878F).</summary>
    PlaySpitSound,
    /// <summary>Starts the regenerating projectile's X offset from the body (<c>SetInitialRegenerationXOffset</c>, $A8:879B).</summary>
    SetInitialRegenerationOffset,
}
