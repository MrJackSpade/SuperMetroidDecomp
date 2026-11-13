namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One cue of the turtles' animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct MamaTurtleCue(MamaTurtleCueKind Kind);
