namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>One cue of the Kago's bugs animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct KagoBugProjectileCue(KagoBugProjectileCueKind Kind);
