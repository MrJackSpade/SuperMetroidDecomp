namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The claw animation's cue (#2554): it throws the claw one way, flying out and back from the next pass.</summary>
/// <param name="Direction">The way the claw is thrown.</param>
internal readonly record struct SpacePirateClawCue(HorizontalDirection Direction);
