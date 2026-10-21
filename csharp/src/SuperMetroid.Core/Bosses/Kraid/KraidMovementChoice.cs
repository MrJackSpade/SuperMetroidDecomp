namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's chosen horizontal target and the time before he chooses again.</summary>
/// <param name="TargetX">The target x.</param>
/// <param name="ThinkTimer">The think timer.</param>
public readonly record struct KraidMovementChoice(int TargetX, int ThinkTimer);
