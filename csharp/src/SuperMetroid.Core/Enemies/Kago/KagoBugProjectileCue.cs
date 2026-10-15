namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>One cue of the Kago's bugs programs, with the operands its native command carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct KagoBugProjectileCue(KagoBugProjectileCueKind Kind);
