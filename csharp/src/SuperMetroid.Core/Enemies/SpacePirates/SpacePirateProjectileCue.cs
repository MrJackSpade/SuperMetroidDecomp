namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A laser or claw animation cue: it fires the laser or throws the claw one way.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct SpacePirateProjectileCue(SpacePirateProjectileCueKind Kind);
