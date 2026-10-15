namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>What a cue of the Metroid's programs does: one per bank-$A3 instruction its lists use.</summary>
internal enum MetroidCueKind
{
    /// <summary>Plays the draining-Samus sound (<c>PlayDrainingSamusSFX</c>, $A3:EAA5).</summary>
    PlayDrainingSound,
    /// <summary>Plays one of eight cries chosen by the next random number (<c>PlayRandomMetroidSFX</c>, $A3:EAB1).</summary>
    PlayRandomCry,
}
