namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>One cue of the Hibashi's animations: what it does, with the values it acts on.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Value">The eruption frame an activity-frame cue selects, zero through 21.</param>
internal readonly record struct HibashiCue(HibashiCueKind Kind, int Value = 0);
