
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>A speed boost counter after one animation command, and whether its cadence expired.</summary>
/// <param name="Counter">The counter.</param>
/// <param name="CadenceExpired">The cadence expired.</param>
public readonly record struct SpeedBoostCountdown(SamusSpeedBoostCounter Counter, bool CadenceExpired);
