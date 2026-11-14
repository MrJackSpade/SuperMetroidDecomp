namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>One cue of the Hibashi's animations: what it does, with the eruption step it applies.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Step">The eruption step a rising cue applies to the collision part.</param>
internal readonly record struct HibashiCue(HibashiCueKind Kind, HibashiEruptionStep? Step = null);
