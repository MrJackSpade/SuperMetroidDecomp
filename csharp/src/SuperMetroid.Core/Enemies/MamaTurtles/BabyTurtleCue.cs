namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One cue of a baby turtle's animations: what it does (#2342).</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct BabyTurtleCue(BabyTurtleCueKind Kind);
