namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One cue of the turtles' animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct MamaTurtleCue(MamaTurtleCueKind Kind);
