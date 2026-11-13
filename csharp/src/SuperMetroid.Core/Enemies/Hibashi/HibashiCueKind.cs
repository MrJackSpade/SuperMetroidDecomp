namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>What a cue of the Hibashi's programs does: one per kind of step its lists use.</summary>
internal enum HibashiCueKind
{
    /// <summary>Sets the collision part's Y offset and half-height for the eruption frame in Value.</summary>
    ActivityFrame,
    /// <summary>Ends the eruption and hands the collision part back to waiting.</summary>
    FinishActivity,
    /// <summary>Plays the eruption sound.</summary>
    PlayEruptionSound,
}
