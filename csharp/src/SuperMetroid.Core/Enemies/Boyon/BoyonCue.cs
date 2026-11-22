namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>One cue of Boyon's animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly partial record struct BoyonCue(BoyonCueKind Kind);
