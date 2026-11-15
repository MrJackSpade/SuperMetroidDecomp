
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>A speed boost counter after one completed run, and whether its countdown ran out.</summary>
/// <param name="Counter">The counter.</param>
/// <param name="CountdownExpired">Whether the countdown is now zero.</param>
public readonly record struct SpeedBoostCountdown(SamusSpeedBoostCounter Counter, bool CountdownExpired);
