namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>What a cue of the Hibashi's animations does: size the collision part for an eruption frame, end the eruption, or play its sound.</summary>
internal enum HibashiCueKind
{
    /// <summary>Sets the collision part's Y offset and half-height for the eruption frame in Value.</summary>
    ActivityFrame,
    /// <summary>Ends the eruption and hands the collision part back to waiting.</summary>
    FinishActivity,
    /// <summary>Plays the eruption sound.</summary>
    PlayEruptionSound,
}
