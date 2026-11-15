namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A laser or claw animation cue: it fires the laser or throws the claw one way.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Direction">The way the laser flies or the claw is thrown.</param>
internal readonly record struct SpacePirateProjectileCue(SpacePirateProjectileCueKind Kind, HorizontalDirection Direction);
