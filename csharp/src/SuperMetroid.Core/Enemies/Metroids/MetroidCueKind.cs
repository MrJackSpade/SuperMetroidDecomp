namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>What a cue of the Metroid's programs does: one per kind of step its lists use.</summary>
internal enum MetroidCueKind
{
    /// <summary>Plays the draining-Samus sound.</summary>
    PlayDrainingSound,
    /// <summary>Plays one of eight cries chosen by the next random number.</summary>
    PlayRandomCry,
}
