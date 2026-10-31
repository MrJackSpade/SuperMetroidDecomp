namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>One cue of Zoa's programs, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct ZoaCue(ZoaCueKind Kind);
