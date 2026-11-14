namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>What a cue of the Hibashi's animations does: size the collision part for an eruption step, end the eruption, or play its sound.</summary>
internal enum HibashiCueKind
{
    /// <summary>Moves and sizes the collision part for the cue's eruption step.</summary>
    EruptionStep,
    /// <summary>Ends the eruption and hands the collision part back to waiting.</summary>
    FinishActivity,
    /// <summary>Plays the eruption sound.</summary>
    PlayEruptionSound,
}
