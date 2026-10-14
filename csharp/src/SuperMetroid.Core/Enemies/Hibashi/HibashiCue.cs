namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>One cue of the Hibashi's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Value">The eruption frame an activity-frame cue selects, zero through 21.</param>
internal readonly record struct HibashiCue(HibashiCueKind Kind, ushort Value = 0);
