namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>What a signal cue of the Hibashi's animations does: end the eruption, or play its sound.</summary>
internal enum HibashiCueKind
{
    /// <summary>Ends the eruption and hands the collision part back to waiting.</summary>
    FinishActivity,
    /// <summary>Plays the eruption sound.</summary>
    PlayEruptionSound,
}
