namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The laser animation's cue (#2554): it fires the laser one way and runs its flight at once, so the laser moves on the
/// update it fires.
/// </summary>
/// <param name="Direction">The way the laser flies.</param>
internal readonly record struct SpacePirateLaserCue(HorizontalDirection Direction);
