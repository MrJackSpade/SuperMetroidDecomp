namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>One cue of Boyon's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct BoyonCue(BoyonCueKind Kind);
