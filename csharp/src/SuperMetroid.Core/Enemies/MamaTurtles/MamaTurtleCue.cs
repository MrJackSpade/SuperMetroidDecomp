namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One cue of the turtles' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct MamaTurtleCue(MamaTurtleCueKind Kind);
