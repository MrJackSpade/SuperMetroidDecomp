namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>One cue of Boyon's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct BoyonCue(BoyonCueKind Kind);
